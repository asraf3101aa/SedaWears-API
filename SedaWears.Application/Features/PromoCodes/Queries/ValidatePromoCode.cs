using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.PromoCodes.Queries;

public record ValidatePromoCodeResultDto(
    bool IsValid,
    string Message,
    decimal DiscountAmount,
    string? DiscountType,
    decimal DiscountValue);

public record ValidatePromoCodeQuery(
    string Code,
    int ShopId,
    decimal OrderSubtotal,
    string? CustomerEmail) : IRequest<ValidatePromoCodeResultDto>;

public class ValidatePromoCodeValidator : AbstractValidator<ValidatePromoCodeQuery>
{
    public ValidatePromoCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promo code is required.");

        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("Invalid shop ID.");

        RuleFor(x => x.OrderSubtotal)
            .GreaterThanOrEqualTo(0).WithMessage("Order subtotal must be greater than or equal to 0.");
    }
}

public class ValidatePromoCodeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser) : IRequestHandler<ValidatePromoCodeQuery, ValidatePromoCodeResultDto>
{
    public async Task<ValidatePromoCodeResultDto> Handle(ValidatePromoCodeQuery request, CancellationToken ct)
    {
        var codeUpper = request.Code.Trim().ToUpperInvariant();

        // 1. Find the active promo code
        var promoCode = await dbContext.PromoCodes
            .FirstOrDefaultAsync(p => p.Code.ToUpper() == codeUpper && p.IsActive, ct);

        if (promoCode == null)
        {
            return new ValidatePromoCodeResultDto(false, "Invalid promo code.", 0, null, 0);
        }

        // 2. Validate shop ownership/matching
        if (promoCode.ShopId.HasValue && promoCode.ShopId.Value != request.ShopId)
        {
            return new ValidatePromoCodeResultDto(false, "This promo code is not valid for this shop.", 0, null, 0);
        }

        // 3. Validate dates
        var now = DateTime.UtcNow;
        if (now < promoCode.StartDate)
        {
            return new ValidatePromoCodeResultDto(false, "This promo code is not active yet.", 0, null, 0);
        }

        if (now > promoCode.EndDate)
        {
            return new ValidatePromoCodeResultDto(false, "This promo code has expired.", 0, null, 0);
        }

        // 4. Validate minimum order amount
        if (promoCode.MinimumOrderAmount.HasValue && request.OrderSubtotal < promoCode.MinimumOrderAmount.Value)
        {
            return new ValidatePromoCodeResultDto(false, $"Minimum order amount of {promoCode.MinimumOrderAmount.Value:C} required to use this promo code.", 0, null, 0);
        }

        // 5. Validate limit per user/email
        if (promoCode.LimitPerUser.HasValue)
        {
            int usageCount = 0;
            
            if (currentUser.Id.HasValue)
            {
                var userId = currentUser.Id.Value;
                usageCount = await dbContext.Orders
                    .CountAsync(o => o.PromoCodeId == promoCode.Id && o.UserId == userId && o.Status != OrderStatus.Cancelled, ct);
            }
            else if (!string.IsNullOrEmpty(request.CustomerEmail))
            {
                usageCount = await dbContext.Orders
                    .CountAsync(o => o.PromoCodeId == promoCode.Id && 
                        ((o.Guest != null && o.Guest.Email == request.CustomerEmail) || (o.User != null && o.User.Email == request.CustomerEmail)) && 
                        o.Status != OrderStatus.Cancelled, ct);
            }

            if (usageCount >= promoCode.LimitPerUser.Value)
            {
                return new ValidatePromoCodeResultDto(false, "You have reached the usage limit for this promo code.", 0, null, 0);
            }
        }

        // 7. Calculate discount
        decimal discountAmount = 0;
        if (promoCode.DiscountType == PromoCodeDiscountType.Percentage)
        {
            discountAmount = request.OrderSubtotal * (promoCode.DiscountValue / 100m);
            if (promoCode.MaxDiscountAmount.HasValue)
            {
                discountAmount = Math.Min(discountAmount, promoCode.MaxDiscountAmount.Value);
            }
        }
        else if (promoCode.DiscountType == PromoCodeDiscountType.FixedAmount)
        {
            discountAmount = Math.Min(promoCode.DiscountValue, request.OrderSubtotal);
        }

        discountAmount = Math.Round(discountAmount, 2);

        return new ValidatePromoCodeResultDto(true, "Promo code applied successfully.", discountAmount, promoCode.DiscountType.ToString(), promoCode.DiscountValue);
    }
}
