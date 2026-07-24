using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Commands;

public record CreateShopCategoryCommand(string? Name, string? Description, int ShopId) : IRequest<int>
{
    public string? Name { get; init; } = Name?.Trim();
    public string? Description { get; init; } = Description?.Trim();
}

public class CreateShopCategoryValidator : AbstractValidator<CreateShopCategoryCommand>
{
    public CreateShopCategoryValidator()
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

public class CreateShopCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IUserService userService,
    IOriginContext originContext) : IRequestHandler<CreateShopCategoryCommand, int>
{
    private const int MaxCategoriesPerShop = 7;

    public async Task<int> Handle(CreateShopCategoryCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to create categories for this shop.");

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

        var categoryCount = await dbContext.Categories.CountAsync(c => c.ShopId == request.ShopId, ct);
        if (categoryCount >= MaxCategoriesPerShop)
            throw new BadRequestException($"A shop cannot have more than {MaxCategoriesPerShop} categories.");

        var finalOrder = (await dbContext.Categories
            .Where(c => c.ShopId == request.ShopId)
            .MaxAsync(c => (int?)c.DisplayOrder, ct) ?? 0) + 1;

        var category = new Category
        {
            Name = request.Name!,
            Description = request.Description!,
            DisplayOrder = finalOrder,
            ShopId = request.ShopId
        };
        dbContext.Categories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new ShopNotFoundException();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new BadRequestException("Category with this name already exists.");
        }

        return category.Id;
    }
}
