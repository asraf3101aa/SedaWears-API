using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using SedaWears.Application.Features.Orders.Models;

namespace SedaWears.Application.Features.Orders.Commands;

public record CheckoutAddress(string? FirstName, string? LastName, string? Phone, string? Street, string? City, string? ZipCode);
public record CheckoutItem(int? ProductId, int? Quantity);

public record CheckoutCommand(
    string? CustomerEmail,
    CheckoutAddress? ShippingAddress,
    List<CheckoutItem>? Items,
    string? PromoCode) : ICommand<CheckoutDto>;

public class CheckoutValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Your cart is empty.");

        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(x => x.ProductId)
                .NotNull().WithMessage("Invalid product selected.")
                .GreaterThan(0).WithMessage("Invalid product selected.");
            item.RuleFor(x => x.Quantity)
                .NotNull().WithMessage("Quantity must be at least 1.")
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        });

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.");

        RuleFor(x => x.ShippingAddress!.FirstName)
            .NotEmpty().When(x => x.ShippingAddress != null).WithMessage("First name is required.");
        RuleFor(x => x.ShippingAddress!.LastName)
            .NotEmpty().When(x => x.ShippingAddress != null).WithMessage("Last name is required.");
        RuleFor(x => x.ShippingAddress!.Phone)
            .NotEmpty().When(x => x.ShippingAddress != null).WithMessage("Phone number is required.");
        RuleFor(x => x.ShippingAddress!.Street)
            .NotEmpty().When(x => x.ShippingAddress != null).WithMessage("Street address is required.");
        RuleFor(x => x.ShippingAddress!.City)
            .NotEmpty().When(x => x.ShippingAddress != null).WithMessage("City is required.");
        RuleFor(x => x.ShippingAddress!.ZipCode)
            .NotEmpty().When(x => x.ShippingAddress != null).WithMessage("Zip code is required.");
    }
}

public class CheckoutHandler(IApplicationDbContext context, ICurrentUser currentUser) : ICommandHandler<CheckoutCommand, CheckoutDto>
{
    public async Task<CheckoutDto> Handle(CheckoutCommand request, CancellationToken ct)
    {
        int? userId = null;
        int? guestId = null;

        if (currentUser.Id.HasValue)
        {
            userId = currentUser.Id!.Value;
        }
        else
        {
            if (string.IsNullOrEmpty(request.CustomerEmail)) throw new BadRequestException("Email is required for guest checkout.");

            var registeredUserExists = await context.Users.AnyAsync(u => u.Email == request.CustomerEmail, ct);
            if (registeredUserExists)
            {
                throw new BadRequestException("This email is registered. Please log in to complete checkout.");
            }

            var guest = await context.Guests.FirstOrDefaultAsync(g => g.Email == request.CustomerEmail, ct);
            if (guest == null)
            {
                guest = new Guest
                {
                    Email = request.CustomerEmail,
                    FirstName = request.ShippingAddress?.FirstName ?? string.Empty,
                    LastName = request.ShippingAddress?.LastName ?? string.Empty,
                    Phone = request.ShippingAddress?.Phone ?? string.Empty,
                    Street = request.ShippingAddress?.Street ?? string.Empty,
                    City = request.ShippingAddress?.City ?? string.Empty,
                    ZipCode = request.ShippingAddress?.ZipCode ?? string.Empty
                };
                context.Guests.Add(guest);
                await context.SaveChangesAsync(ct);
            }
            guestId = guest.Id;
        }

        if (userId.HasValue && request.ShippingAddress != null)
        {
            var street = request.ShippingAddress.Street ?? string.Empty;
            var city = request.ShippingAddress.City ?? string.Empty;
            var zipCode = request.ShippingAddress.ZipCode ?? string.Empty;

            var existingAddress = await context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId.Value &&
                    a.Street == street &&
                    a.City == city &&
                    a.ZipCode == zipCode, ct);

            if (existingAddress == null)
            {
                var addressCount = await context.Addresses.CountAsync(a => a.UserId == userId.Value, ct);
                if (addressCount >= 5)
                    throw new BadRequestException("Users can have at most 5 addresses saved.");

                context.Addresses.Add(new Address
                {
                    UserId = userId.Value,
                    FullName = $"{request.ShippingAddress.FirstName} {request.ShippingAddress.LastName}".Trim(),
                    Email = request.CustomerEmail ?? string.Empty,
                    Phone = request.ShippingAddress.Phone ?? string.Empty,
                    Street = street,
                    City = city,
                    ZipCode = zipCode,
                    Label = "Shipping"
                });
            }
        }

        var order = new Order 
        { 
            UserId = userId, 
            GuestId = guestId, 
            Status = OrderStatus.Pending, 
            TotalAmount = 0 
        };

        bool shopIdSet = false;

        foreach (var item in request.Items ?? [])
        {
            var productId = item.ProductId ?? 0;
            var quantity = item.Quantity ?? 0;
            
            var product = await context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId, ct);
                
            if (product == null) throw new ProductNotFoundException();

            if (!shopIdSet && product.Category?.ShopId != null)
            {
                order.ShopId = product.Category.ShopId.Value;
                shopIdSet = true;
            }

            order.Items.Add(new OrderItem
            {
                Product = product,
                Quantity = quantity,
                UnitPrice = product.Price
            });
            order.TotalAmount += product.Price * quantity;
        }

        decimal subtotal = order.TotalAmount;
        decimal discountAmount = 0;
        int? appliedPromoCodeId = null;

        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            var codeUpper = request.PromoCode.Trim().ToUpperInvariant();
            var promoCode = await context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == codeUpper && p.IsActive, ct);

            if (promoCode == null)
            {
                throw new BadRequestException("Invalid promo code.");
            }

            // Validate shop specific
            if (promoCode.ShopId.HasValue && promoCode.ShopId.Value != order.ShopId)
            {
                throw new BadRequestException("This promo code is not valid for this shop.");
            }

            var now = DateTime.UtcNow;
            if (now < promoCode.StartDate)
            {
                throw new BadRequestException("This promo code is not active yet.");
            }

            if (now > promoCode.EndDate)
            {
                throw new BadRequestException("This promo code has expired.");
            }
            // Check minimum order amount
            if (promoCode.MinimumOrderAmount.HasValue && subtotal < promoCode.MinimumOrderAmount.Value)
            {
                throw new BadRequestException($"Minimum order amount of {promoCode.MinimumOrderAmount.Value:C} required to use this promo code.");
            }

            // Check limit per user/email
            if (promoCode.LimitPerUser.HasValue)
            {
                int usageCount = 0;
                if (userId.HasValue)
                {
                    usageCount = await context.Orders
                        .CountAsync(o => o.PromoCodeId == promoCode.Id && o.UserId == userId && o.Status != OrderStatus.Cancelled, ct);
                }
                else
                {
                    usageCount = await context.Orders
                        .CountAsync(o => o.PromoCodeId == promoCode.Id && 
                            ((o.Guest != null && o.Guest.Email == request.CustomerEmail) || (o.User != null && o.User.Email == request.CustomerEmail)) && 
                            o.Status != OrderStatus.Cancelled, ct);
                }

                if (usageCount >= promoCode.LimitPerUser.Value)
                {
                    throw new BadRequestException("You have reached the usage limit for this promo code.");
                }
            }

            // Calculate discount
            if (promoCode.DiscountType == PromoCodeDiscountType.Percentage)
            {
                discountAmount = subtotal * (promoCode.DiscountValue / 100m);
                if (promoCode.MaxDiscountAmount.HasValue)
                {
                    discountAmount = Math.Min(discountAmount, promoCode.MaxDiscountAmount.Value);
                }
            }
            else if (promoCode.DiscountType == PromoCodeDiscountType.FixedAmount)
            {
                discountAmount = Math.Min(promoCode.DiscountValue, subtotal);
            }

            discountAmount = Math.Round(discountAmount, 2);
            appliedPromoCodeId = promoCode.Id;
        }

        order.PromoCodeId = appliedPromoCodeId;
        order.DiscountAmount = discountAmount;
        order.TotalAmount = subtotal - discountAmount;

        context.Orders.Add(order);
        await context.SaveChangesAsync(ct);

        return new CheckoutDto(order.Id, order.Status.ToString(), order.TotalAmount);
    }
}
