using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common;
using SedaWears.Domain.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Categories.Commands;

public record UpdateShopCategoryCommand(int Id, string? Name, string? Description, int ShopId) : IRequest
{
    public string? Name { get; init; } = Name?.Trim();
    public string? Description { get; init; } = Description?.Trim();
}

public class UpdateShopCategoryValidator : AbstractValidator<UpdateShopCategoryCommand>
{
    public UpdateShopCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(5).WithMessage("Description must be at least 5 characters.")
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters.");
    }
}

public class UpdateShopCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IUserService userService,
    IOriginContext originContext,
    IFusionCache fusionCache) : IRequestHandler<UpdateShopCategoryCommand>
{
    public async Task Handle(UpdateShopCategoryCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to update categories for this shop.");

        var shopQuery = dbContext.Shops.AsNoTracking().Where(s => s.Id == request.ShopId);
        var userId = currentUser.Id;

        bool shopExists = originContext.OriginRole switch
        {
            UserRole.Admin => await shopQuery.AnyAsync(s => !s.IsDeleted, ct),
            UserRole.Owner => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Owners.Any(o => o.UserId == userId), ct),
            UserRole.Manager => await shopQuery.AnyAsync(s => s.Id != 1 && !s.IsDeleted && s.Managers.Any(m => m.UserId == userId), ct),
            _ => false
        };

        if (!shopExists)
            throw new ShopNotFoundException();

        var categoryQuery = dbContext.Categories.Where(c => c.Id == request.Id && c.ShopId == request.ShopId && !c.IsDeleted);

        var category = await categoryQuery.FirstOrDefaultAsync(ct)
            ?? throw new CategoryNotFoundException();

        category.Name = request.Name!;
        category.Description = request.Description!;

        try
        {
            await dbContext.SaveChangesAsync(ct);
            await fusionCache.RemoveAsync(CacheKeys.Category(request.Id), token: ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new BadRequestException("Category with this name already exists.");
        }
    }
}
