using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Users.Models;
using System.Web;

namespace SedaWears.Application.Features.Shops.Queries;

public record GetInvitedShopOwnersQuery(
    int ShopId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<InvitedUserDto>>;

public class GetInvitedShopOwnersHandler(
    IApplicationDbContext dbContext,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) : IRequestHandler<GetInvitedShopOwnersQuery, PaginatedList<InvitedUserDto>>
{
    public async Task<PaginatedList<InvitedUserDto>> Handle(GetInvitedShopOwnersQuery request, CancellationToken ct)
    {
        var query = dbContext.InvitedShopOwners.AsNoTracking().Where(iso => iso.ShopId == request.ShopId);

        var total = await query.CountAsync(ct);
        var invitations = await query
            .OrderByDescending(iso => iso.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = invitations
            .Select(iso => new InvitedUserDto(
                iso.Id,
                iso.Email,
                iso.CreatedAt,
                new Uri($"{hostUrlsConfigOptions.Value.Owner}/accept-invitation?email={iso.Email}&token={HttpUtility.UrlEncode(iso.Token)}&shopId={iso.ShopId}")))
            .ToList();

        return new PaginatedList<InvitedUserDto>(items, total, request.PageNumber, request.PageSize);
    }
}
