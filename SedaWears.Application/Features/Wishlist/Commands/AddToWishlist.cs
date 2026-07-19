using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Wishlist.Commands;

public record AddToWishlistCommand(int ProductId) : IRequest;

public class AddToWishlistHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<AddToWishlistCommand>
{
    public async Task Handle(AddToWishlistCommand request, CancellationToken ct)
    {
        var userId = currentUser.Id;

        var productExists = await dbContext.Products.AnyAsync(p => p.Id == request.ProductId, ct);
        if (!productExists) throw new ProductNotFoundException();

        var alreadyInWishlist = await dbContext.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == request.ProductId, ct);

        if (alreadyInWishlist) return;

        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = request.ProductId
        };

        dbContext.WishlistItems.Add(item);
        await dbContext.SaveChangesAsync(ct);
    }
}
