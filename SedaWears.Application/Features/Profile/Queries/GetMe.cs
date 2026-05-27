using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Projections;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Application.Features.Profile.Queries;

public record GetMeQuery : IRequest<UserDto>;

public class GetMeHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    OpeninaryConfig config) : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userId = currentUser.Id ?? throw new UnauthorizedAccessException("User is not authenticated.");

        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .ProjectToUser(config.BaseUrl)
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedAccessException("User not found.");
    }
}
