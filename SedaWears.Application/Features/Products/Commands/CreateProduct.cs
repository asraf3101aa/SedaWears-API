using MediatR;
using FluentValidation;
using SedaWears.Domain.Enums;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Products.Commands;

public record CreateProductCommand(
    string? Name,
    string? Description,
    decimal? Price,
    Gender? Gender,
    int? CategoryId,
    List<string>? ImageFileNames,
    int? ShopId = null) : IRequest;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
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

        RuleFor(x => x.ImageFileNames)
            .NotEmpty().WithMessage("At least one image must be provided.");
    }
}

public class CreateProductHandler(IApplicationDbContext dbContext) : IRequestHandler<CreateProductCommand>
{
    public async Task Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (request.ShopId.HasValue)
        {
            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId!.Value && c.ShopId == request.ShopId, ct);

            if (!categoryExists)
            {
                throw new CategoryNotFoundException();
            }
        }

        var product = new Product
        {
            Name = request.Name!,
            Description = request.Description,
            Price = request.Price!.Value,
            CategoryId = request.CategoryId!.Value,
            Gender = request.Gender!.Value,
            Images = request.ImageFileNames!.Select((fileName, index) => new ProductImage
            {
                FileName = fileName,
                Order = index
            }).ToList()
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(ct);
    }
}
