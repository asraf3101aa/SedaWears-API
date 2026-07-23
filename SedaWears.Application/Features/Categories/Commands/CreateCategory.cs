using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string? Name, string? Description, int ShopId) : IRequest<int>
{
    public string? Name { get; init; } = Name?.Trim();
    public string? Description { get; init; } = Description?.Trim();
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
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

public class CreateCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IUserService userService,
    IOriginContext originContext) : IRequestHandler<CreateCategoryCommand, int>
{
    private const int MaxCategoriesPerShop = 7;

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to create categories for this shop.");

        var isAuthorized = originContext.OriginRole switch
        {
            UserRole.Admin => true,
            UserRole.Owner => await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId, ct),
            UserRole.Manager => await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId, ct),
            _ => false
        };

        if (!isAuthorized)
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
