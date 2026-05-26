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

public record UpdatePromoCodeCommand(
    int Id,
    string? Description,
    PromoCodeDiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? LimitPerUser,
    bool IsActive) : IRequest;

public class UpdatePromoCodeValidator : AbstractValidator<UpdatePromoCodeCommand>
{
    public UpdatePromoCodeValidator()
    {
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

public class UpdatePromoCodeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<UpdatePromoCodeCommand>
{
    public async Task Handle(UpdatePromoCodeCommand request, CancellationToken ct)
    {
        var promoCode = await dbContext.PromoCodes
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException("Promo code not found.");

        var user = currentUser.Id.HasValue ? await userManager.FindByIdAsync(currentUser.Id.Value.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (promoCode.ShopId.HasValue)
        {
            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == promoCode.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == promoCode.ShopId.Value, ct);

                if (!isMember)
                    throw new ForbiddenException("You are not authorized to update this shop's promo codes.");
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can update global promo codes.");
        }

        // Update mutable fields (Code and ShopId are immutable for security and data integrity)
        promoCode.Description = request.Description;
        promoCode.DiscountType = request.DiscountType;
        promoCode.DiscountValue = request.DiscountValue;
        promoCode.MinimumOrderAmount = request.MinimumOrderAmount;
        promoCode.MaxDiscountAmount = request.MaxDiscountAmount;
        promoCode.StartDate = request.StartDate.ToUniversalTime();
        promoCode.EndDate = request.EndDate.ToUniversalTime();
        promoCode.LimitPerUser = request.LimitPerUser;
        promoCode.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(ct);
    }
}
