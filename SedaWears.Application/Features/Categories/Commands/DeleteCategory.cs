using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SedaWears.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id, int? ShopId = null) : IRequest;

public class DeleteCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var currentUserId = currentUser.Id;
        if (!currentUserId.HasValue)
            throw new UnauthorizedAccessException();

        var user = await userManager.FindByIdAsync(currentUserId.Value.ToString()) ?? throw new UnauthorizedAccessException();
        var isAdmin = await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (request.ShopId.HasValue)
        {
            var shopExists = await dbContext.Shops
                .AnyAsync(s => s.Id == request.ShopId.Value, ct);

            if (!shopExists)
                throw new NotFoundException("Shop not found.");

            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId.Value, ct);

                if (!isMember)
                    throw new NotFoundException("Shop not found.");
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can delete global categories.");
        }

        var deletedRowsCount = await dbContext.Categories
            .Where(c => c.Id == request.Id && c.ShopId == request.ShopId)
            .ExecuteDeleteAsync(ct);

        if (deletedRowsCount == 0)
            throw new NotFoundException("Category not found.");
    }
}
