using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Wishlist.Commands;

public record RemoveFromWishlistCommand(int ProductId) : IRequest;

public class RemoveFromWishlistHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : IRequestHandler<RemoveFromWishlistCommand>
{
    public async Task Handle(RemoveFromWishlistCommand request, CancellationToken ct)
    {
        var userId = currentUser.Id;

        var item = await dbContext.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId, ct);

        if (item == null) return;

        dbContext.WishlistItems.Remove(item);
        await dbContext.SaveChangesAsync(ct);
    }
}
