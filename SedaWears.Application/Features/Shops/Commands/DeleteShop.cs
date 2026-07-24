using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Shops.Commands;

public record DeleteShopCommand(int Id) : IRequest;

public class DeleteShopHandler(IApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, ICurrentUser currentUser, IUserService userService) : IRequestHandler<DeleteShopCommand>
{
    public async Task Handle(DeleteShopCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(UserRole.Admin))
            throw new ForbiddenException("You are not authorized to delete a shop.");

        var affectedUserIds = await dbContext.ShopOwners.Where(so => so.ShopId == request.Id).Select(so => so.UserId)
            .Union(dbContext.ShopManagers.Where(sm => sm.ShopId == request.Id).Select(sm => sm.UserId))
            .ToListAsync(ct);

        var updatedRows = await dbContext.Shops
            .Where(s => s.Id == request.Id && !s.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.IsActive, false), ct);

        if (updatedRows > 0)
        {
            await publishEndpoint.Publish(new ShopDeletedEvent(request.Id, affectedUserIds), ct);
        }

    }
}
