using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Projections;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Queries;

public record GetCategoryQuery(int Id, int ShopId) : IRequest<CategoryDto>;

public class GetCategoryHandler(
    IApplicationDbContext dbContext,
    IUserService userService,
    ICurrentUser currentUser,
    IOriginContext originContext) : IRequestHandler<GetCategoryQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryQuery request, CancellationToken ct)
    {
        if (request.ShopId == 1 && originContext.OriginRole is UserRole.Owner or UserRole.Manager)
            throw new CategoryNotFoundException();

        var query = dbContext.Categories
            .Where(c => c.Id == request.Id && c.ShopId == request.ShopId);

        if (originContext.OriginRole == UserRole.Customer)
        {
            query = query.Where(c => c.IsActive && c.Shop.IsActive);
        }
        else
        {
            var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

            if (!user.Roles.Contains(originContext.OriginRole))
                throw new ForbiddenException("You are not authorized to view this category.");

            var userId = currentUser.Id;
            query = originContext.OriginRole switch
            {
                UserRole.Admin => query,
                UserRole.Owner => query.Where(c => c.Shop.Owners.Any(o => o.UserId == userId)),
                UserRole.Manager => query.Where(c => c.Shop.Managers.Any(m => m.UserId == userId)),
                _ => throw new ForbiddenException("You are not authorized to view this category.")
            };
        }

        return await query
            .AsNoTracking()
            .ProjectToCategory()
            .FirstOrDefaultAsync(ct) ?? throw new CategoryNotFoundException();
    }
}
