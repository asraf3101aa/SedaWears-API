using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Users.Models;
using System.Web;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetInvitedShopManagersQuery(
    int ShopId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<InvitedUserDto>>;

public class GetInvitedShopManagersHandler(
    IApplicationDbContext dbContext,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) : IRequestHandler<GetInvitedShopManagersQuery, PaginatedList<InvitedUserDto>>
{
    public async Task<PaginatedList<InvitedUserDto>> Handle(GetInvitedShopManagersQuery request, CancellationToken ct)
    {
        var query = dbContext.InvitedShopManagers.AsNoTracking().Where(ism => ism.ShopId == request.ShopId);

        var total = await query.CountAsync(ct);
        var invitations = await query
            .OrderByDescending(ism => ism.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = invitations
            .Select(ism => new InvitedUserDto(
                ism.Id,
                ism.Email,
                ism.CreatedAt,
                new Uri($"{hostUrlsConfigOptions.Value.Manager}/accept-invitation?email={ism.Email}&token={HttpUtility.UrlEncode(ism.Token)}&shopId={ism.ShopId}")))
            .ToList();

        return new PaginatedList<InvitedUserDto>(items, total, request.PageNumber, request.PageSize);
    }
}
