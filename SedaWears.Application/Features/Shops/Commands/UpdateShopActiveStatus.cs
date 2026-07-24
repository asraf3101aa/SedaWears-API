using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Shops.Commands;

public record UpdateShopActiveStatusCommand(int Id, bool? IsActive) : IRequest;

public class UpdateShopActiveStatusHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IUserService userService) : IRequestHandler<UpdateShopActiveStatusCommand>
{
    public async Task Handle(UpdateShopActiveStatusCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(UserRole.Admin))
            throw new ForbiddenException("You are not authorized to update the shop active status.");

        await dbContext.Shops
           .Where(s => s.Id == request.Id && !s.IsDeleted)
           .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, request.IsActive!.Value), ct);
    }
}
