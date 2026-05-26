using SedaWears.Application.Features.Users.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Users.Projections;
using Microsoft.AspNetCore.Identity;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Users.Queries;

public record GetUserQuery(UserRole? Role = null, int? Id = null) : IRequest<UserDto>;

public class GetUserHandler(
    UserManager<User> userManager,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken ct)
    {
        var role = request.Role ?? originContext.CurrentRole;
        var userId = request.Id ?? currentUser.Id!.Value;

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await userManager.IsInRoleAsync(user, role.ToString()))
            throw new NotFoundException($"{role} not found.");

        return await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .ProjectToUser()
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"{role} not found.");
    }
}
