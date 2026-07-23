using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.PromoCodes.Commands;

public record CreatePromoCodeCommand(
    string Code,
    string? Description,
    PromoCodeDiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? LimitPerUser,
    int? ShopId) : IRequest<int>
{
    public string Code { get; init; } = Code.Trim();
    public string? Description { get; init; } = Description?.Trim();
}

public class CreatePromoCodeValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promo code is required.")
            .MaximumLength(50).WithMessage("Promo code must not exceed 50 characters.")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Promo code must contain only uppercase letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("Percentage discount must not exceed 100%.")
            .When(x => x.DiscountType == PromoCodeDiscountType.Percentage);

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount must be at least 0.")
            .When(x => x.MinimumOrderAmount.HasValue);

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Max discount amount must be at least 0.")
            .When(x => x.MaxDiscountAmount.HasValue);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after start date.");

        RuleFor(x => x.LimitPerUser)
            .GreaterThan(0).WithMessage("Limit per user must be greater than 0.")
            .When(x => x.LimitPerUser.HasValue);
    }
}

public class CreatePromoCodeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<CreatePromoCodeCommand, int>
{
    public async Task<int> Handle(CreatePromoCodeCommand request, CancellationToken ct)
    {
        var user = true ? await userManager.FindByIdAsync(currentUser.Id.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (request.ShopId.HasValue)
        {
            var shopExists = await dbContext.Shops.AnyAsync(s => s.Id == request.ShopId.Value, ct);
            if (!shopExists)
                throw new ShopNotFoundException();

            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId.Value, ct);

                if (!isMember)
                    throw new ForbiddenException("You are not authorized to create promo codes for this shop.");
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can create global promo codes.");
        }

        var codeUpper = request.Code.ToUpperInvariant();

        // Check uniqueness based on shop vs global
        bool exists;
        if (request.ShopId.HasValue)
        {
            exists = await dbContext.PromoCodes
                .AnyAsync(p => p.Code.ToUpper() == codeUpper && p.ShopId == request.ShopId.Value, ct);
        }
        else
        {
            exists = await dbContext.PromoCodes
                .AnyAsync(p => p.Code.ToUpper() == codeUpper && p.ShopId == null, ct);
        }

        if (exists)
            throw new BadRequestException($"Promo code '{codeUpper}' already exists.");

        var promoCode = new PromoCode
        {
            Code = codeUpper,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate.ToUniversalTime(),
            EndDate = request.EndDate.ToUniversalTime(),
            LimitPerUser = request.LimitPerUser,
            ShopId = request.ShopId,
            IsActive = true
        };

        dbContext.PromoCodes.Add(promoCode);
        await dbContext.SaveChangesAsync(ct);

        return promoCode.Id;
    }
}
