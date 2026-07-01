using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SedaWears.Application.Features.Categories.Commands;

public record UpdateCategoryActiveStatusCommand(int Id, bool? IsActive, int? ShopId = null) : IRequest;

public class UpdateCategoryActiveStatusHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    UserManager<User> userManager) : IRequestHandler<UpdateCategoryActiveStatusCommand>
{
    public async Task Handle(UpdateCategoryActiveStatusCommand request, CancellationToken ct)
    {
        var user = currentUser.Id.HasValue ? await userManager.FindByIdAsync(currentUser.Id.Value.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

        if (request.ShopId.HasValue)
        {
            var shopExists = await dbContext.Shops
                .AnyAsync(s => s.Id == request.ShopId.Value, ct);

            if (!shopExists)
                throw new ShopNotFoundException();

            if (!isAdmin)
            {
                var isMember = await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId.Value, ct)
                               || await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId.Value, ct);

                if (!isMember)
                    throw new ShopNotFoundException();
            }
        }
        else if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can update global categories.");
        }

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.ShopId == request.ShopId, ct)
            ?? throw new CategoryNotFoundException();

        category.IsActive = request.IsActive!.Value;
        await dbContext.SaveChangesAsync(ct);
    }
}
