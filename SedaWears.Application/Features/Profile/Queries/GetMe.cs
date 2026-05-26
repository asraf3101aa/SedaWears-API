using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;

namespace SedaWears.Application.Features.Profile.Queries;

public record GetMeQuery : IRequest<UserDto>;

public class GetMeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser) : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userId = currentUser.Id ?? throw new UnauthorizedAccessException("User is not authenticated.");

        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .ProjectToUser()
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedAccessException("User not found.");
    }
}
