using MediatR;
using FluentValidation;
using SedaWears.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Products.Commands;

public record UpdateProductCommand(
    int? Id,
    string? Name,
    string? Description,
    decimal? Price,
    Gender? Gender,
    int? CategoryId,
    List<string>? ImageFileNames,
    int? ShopId = null) : IRequest;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("A valid product identifier is required.")
            .GreaterThan(0).WithMessage("A valid product identifier is required.");

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

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.")
            .GreaterThan(0).WithMessage("A valid category identifier is required.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .IsInEnum().WithMessage("A valid gender must be specified.");

    }
}

public class UpdateProductHandler(IApplicationDbContext dbContext) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id!.Value, ct) ?? throw new NotFoundException("Product not found");

        if (request.ShopId.HasValue)
        {
            if (product.Category.ShopId != request.ShopId)
            {
                throw new NotFoundException("Product not found.");
            }

            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId!.Value && c.ShopId == request.ShopId, ct);

            if (!categoryExists)
            {
                throw new NotFoundException("Category not found.");
            }
        }

        product.Name = request.Name!;
        product.Description = request.Description;
        product.Price = request.Price!.Value;
        product.CategoryId = request.CategoryId!.Value;
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
    }
}
