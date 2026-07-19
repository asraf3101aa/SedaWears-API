using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id, int ShopId) : IRequest;

public class DeleteCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IUserService userService,
    IOriginContext originContext) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to delete categories for this shop.");

        var isAuthorized = originContext.OriginRole switch
        {
            UserRole.Admin => true,
            UserRole.Owner => await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId, ct),
            UserRole.Manager => await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId, ct),
            _ => false
        };

        if (!isAuthorized)
            throw new ShopNotFoundException();

        var deletedRowsCount = await dbContext.Categories
            .Where(c => c.Id == request.Id && c.ShopId == request.ShopId)
            .ExecuteDeleteAsync(ct);

        if (deletedRowsCount == 0)
            throw new CategoryNotFoundException();
    }
}
