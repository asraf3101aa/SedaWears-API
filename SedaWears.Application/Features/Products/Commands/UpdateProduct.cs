using MediatR;
using FluentValidation;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Products.Commands;

public record UpdateProductCommand(
    int ShopId,
    int CategoryId,
    int Id,
    string? Name,
    string? Description,
    decimal? Price,
    Gender? Gender,
    List<string>? ImageFileNames) : IRequest
{
    public string? Name { get; init; } = Name?.Trim();
    public string? Description { get; init; } = Description?.Trim();
}

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("A valid shop identifier is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A valid category identifier is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price is required.")
            .GreaterThan(0).WithMessage("Product price must be greater than zero.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .IsInEnum().WithMessage("A valid gender must be specified.");

    }
}

public class UpdateProductHandler(IApplicationDbContext dbContext, IFusionCache fusionCache) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct) ?? throw new ProductNotFoundException();

        if (product.Category.ShopId != request.ShopId)
        {
            throw new ProductNotFoundException();
        }

        var categoryExists = await dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId && c.ShopId == request.ShopId, ct);

        if (!categoryExists)
        {
            throw new CategoryNotFoundException();
        }

        product.Name = request.Name!;
        product.Description = request.Description;
        product.Price = request.Price!.Value;
        product.CategoryId = request.CategoryId;
        product.Gender = request.Gender!.Value;

        if (request.ImageFileNames is { Count: > 0 })
        {
            product.Images.Clear();
            foreach (var (fileName, index) in request.ImageFileNames!.Select((v, i) => (v, i)))
            {
                product.Images.Add(new ProductImage { FileName = fileName, Order = index });
            }
        }

        await dbContext.SaveChangesAsync(ct);
        await fusionCache.RemoveAsync(CacheKeys.Product(request.Id), token: ct);
    }
}
