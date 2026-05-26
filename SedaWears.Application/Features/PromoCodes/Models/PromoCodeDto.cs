using System;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.PromoCodes.Models;

public record PromoCodeDto(
    int Id,
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaxDiscountAmount,
    DateTime StartDate,
    DateTime EndDate,
    int? LimitPerUser,
    bool IsActive,
    int? ShopId,
    DateTime CreatedAt)
{
    public static PromoCodeDto FromEntity(PromoCode promoCode) => new(
        promoCode.Id,
        promoCode.Code,
        promoCode.Description,
        promoCode.DiscountType.ToString(),
        promoCode.DiscountValue,
        promoCode.MinimumOrderAmount,
        promoCode.MaxDiscountAmount,
        promoCode.StartDate,
        promoCode.EndDate,
        promoCode.LimitPerUser,
        promoCode.IsActive,
        promoCode.ShopId,
        promoCode.CreatedAt
    );
}
