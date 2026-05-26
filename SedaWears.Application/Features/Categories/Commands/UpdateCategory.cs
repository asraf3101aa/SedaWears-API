using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(int Id, string Name, string? Description, int? ShopId = null) : IRequest;

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
    UserManager<User> userManager) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var user = currentUser.Id.HasValue ? await userManager.FindByIdAsync(currentUser.Id.Value.ToString()) : null;
        var isAdmin = user != null && await userManager.IsInRoleAsync(user, nameof(UserRole.Admin));

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
            throw new ForbiddenException("Only administrators can update global categories.");
        }

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.ShopId == request.ShopId, ct)
            ?? throw new NotFoundException("Category not found.");

        var nameExists = await dbContext.Categories
            .AnyAsync(c => c.Id != request.Id &&
                           EF.Functions.ILike(c.Name, request.Name) &&
                           c.ShopId == request.ShopId, ct);

        if (nameExists)
            throw new BadRequestException("Category with this name already exists.");

        category.Name = request.Name;
        category.Description = request.Description;

        await dbContext.SaveChangesAsync(ct);
    }
}
