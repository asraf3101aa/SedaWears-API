using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetInvitedAdminsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    InvitedAdminsSortBy SortBy = InvitedAdminsSortBy.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc) : IRequest<PaginatedList<InvitedUserDto>>;

public class GetInvitedAdminsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetInvitedAdminsQuery, PaginatedList<InvitedUserDto>>
{
    public async Task<PaginatedList<InvitedUserDto>> Handle(GetInvitedAdminsQuery request, CancellationToken ct)
    {
        var query = dbContext.InvitedAdmins.AsNoTracking();

        var desc = request.SortOrder == SortOrder.Desc;
        query = request.SortBy switch
        {
            InvitedAdminsSortBy.Email => desc
                ? query.OrderByDescending(ia => ia.Email)
                : query.OrderBy(ia => ia.Email),
            _ => desc
                ? query.OrderByDescending(ia => ia.CreatedAt)
                : query.OrderBy(ia => ia.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ia => new InvitedUserDto(ia.Id, ia.Email, ia.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<InvitedUserDto>(items, total, request.PageNumber, request.PageSize);
    }
}
