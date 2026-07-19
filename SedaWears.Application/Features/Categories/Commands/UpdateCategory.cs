using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(int Id, string? Name, string? Description, int ShopId) : IRequest;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .MinimumLength(5).WithMessage("Description must be at least 5 characters.")
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class UpdateCategoryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IUserService userService,
    IOriginContext originContext) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var user = await userService.FindByIdAsync(currentUser.Id, ct) ?? throw new UnauthorizedAccessException();

        if (!user.Roles.Contains(originContext.OriginRole))
            throw new ForbiddenException("You are not authorized to update categories for this shop.");

        var isAuthorized = originContext.OriginRole switch
        {
            UserRole.Admin => true,
            UserRole.Owner => await dbContext.ShopOwners.AnyAsync(so => so.UserId == currentUser.Id && so.ShopId == request.ShopId, ct),
            UserRole.Manager => await dbContext.ShopManagers.AnyAsync(sm => sm.UserId == currentUser.Id && sm.ShopId == request.ShopId, ct),
            _ => false
        };

        if (!isAuthorized)
            throw new ShopNotFoundException();

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.ShopId == request.ShopId, ct)
            ?? throw new CategoryNotFoundException();

        category.Name = request.Name!;
        category.Description = request.Description;

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new BadRequestException("Category with this name already exists.");
        }
    }
}
