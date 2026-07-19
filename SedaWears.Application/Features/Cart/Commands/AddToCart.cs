using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Cart.Commands;

public record AddToCartCommand(int ProductId, ProductSize? Size, int? Quantity) : ICommand;

public class AddToCartHandler(IApplicationDbContext context, ICurrentUser currentUser) : ICommandHandler<AddToCartCommand>
{
    public async Task Handle(AddToCartCommand request, CancellationToken ct)
    {
        var userId = currentUser.Id;
        var productExists = await context.Products.AnyAsync(p => p.Id == request.ProductId, ct);
        if (!productExists) throw new ProductNotFoundException();

        var existing = await context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId && c.Size == request.Size, ct);

        if (existing != null)
        {
            existing.Quantity += request.Quantity!.Value;
        }
        else
        {
            context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Size = request.Size!.Value,
                Quantity = request.Quantity!.Value
            });
        }

        await context.SaveChangesAsync(ct);
    }
}
