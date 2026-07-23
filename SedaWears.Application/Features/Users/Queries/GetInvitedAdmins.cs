using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Domain.Enums;
using System.Web;

namespace SedaWears.Application.Features.Users.Queries;

public record GetInvitedAdminsQuery(
    int PageNumber,
    int PageSize,
    InvitedAdminsSortField SortBy,
    SortOrder SortOrder) : IRequest<PaginatedList<InvitedUserDto>>;

public class GetInvitedAdminsHandler(
    IApplicationDbContext dbContext,
    IOptions<HostUrlsConfig> hostUrlsConfigOptions) : IRequestHandler<GetInvitedAdminsQuery, PaginatedList<InvitedUserDto>>
{
    public async Task<PaginatedList<InvitedUserDto>> Handle(GetInvitedAdminsQuery request, CancellationToken ct)
    {
        var query = dbContext.InvitedAdmins.AsNoTracking();

        var desc = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            InvitedAdminsSortField.Email => desc
                ? query.OrderByDescending(ia => ia.Email)
                : query.OrderBy(ia => ia.Email),
            _ => desc
                ? query.OrderByDescending(ia => ia.CreatedAt)
                : query.OrderBy(ia => ia.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var invitations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = invitations
            .Select(ia => new InvitedUserDto(
                ia.Id,
                ia.Email,
                ia.CreatedAt,
                new Uri($"{hostUrlsConfigOptions.Value.Admin}/accept-invitation?email={ia.Email}&token={HttpUtility.UrlEncode(ia.Token)}")))
            .ToList();

        return new PaginatedList<InvitedUserDto>(items, total, request.PageNumber, request.PageSize);
    }
}
