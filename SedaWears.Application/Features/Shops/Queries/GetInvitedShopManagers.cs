using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetInvitedShopManagersQuery(
    int ShopId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<InvitedUserDto>>;

public class GetInvitedShopManagersHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvitedShopManagersQuery, PaginatedList<InvitedUserDto>>
{
    public async Task<PaginatedList<InvitedUserDto>> Handle(GetInvitedShopManagersQuery request, CancellationToken ct)
    {
        var query = dbContext.InvitedShopManagers
            .AsNoTracking()
            .Where(ism => ism.ShopId == request.ShopId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(ism => ism.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ism => new InvitedUserDto(ism.Id, ism.Email, ism.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<InvitedUserDto>(items, total, request.PageNumber, request.PageSize);
    }
}
