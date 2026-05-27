using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Features.Wishlist.Queries;

public record WishlistDto(
    int ProductId, 
    string ProductName, 
    decimal ProductPrice, 
    string? ProductImage, 
    DateTime AddedAt);

public record GetWishlistQuery() : IRequest<List<WishlistDto>>;

public class GetWishlistHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, OpeninaryConfig config) : IRequestHandler<GetWishlistQuery, List<WishlistDto>>
{
    public async Task<List<WishlistDto>> Handle(GetWishlistQuery request, CancellationToken ct)
    {
        var userId = currentUser.Id ?? throw new UnauthorizedAccessException();

        return await dbContext.WishlistItems
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistDto(
                w.ProductId,
                w.Product.Name,
                w.Product.Price,
                w.Product.Images.OrderBy(i => i.Order).Select(i => string.IsNullOrEmpty(i.FileName) ? null : config.BaseUrl + "/t/" + i.FileName).FirstOrDefault(),
                w.CreatedAt))
            .ToListAsync(ct);
    }
}
