using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetInvitedShopOwnersQuery(
    int ShopId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<InvitedUserDto>>;

public class GetInvitedShopOwnersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvitedShopOwnersQuery, PaginatedList<InvitedUserDto>>
{
    public async Task<PaginatedList<InvitedUserDto>> Handle(GetInvitedShopOwnersQuery request, CancellationToken ct)
    {
        var query = dbContext.InvitedShopOwners
            .AsNoTracking()
            .Where(iso => iso.ShopId == request.ShopId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(iso => iso.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(iso => new InvitedUserDto(iso.Id, iso.Email, iso.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<InvitedUserDto>(items, total, request.PageNumber, request.PageSize);
    }
}
