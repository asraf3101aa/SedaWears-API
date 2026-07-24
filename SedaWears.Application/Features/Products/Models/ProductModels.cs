using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Products.Models;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    Gender Gender,
    List<ProductImageDto> Images,
    List<ProductSizeDto> Sizes,
    CategorySummary Category,
    DateTime CreatedAt,
    bool IsDeleted);

public record CategorySummary(int Id, string Name);
public record ProductSizeDto(ProductSize Size, int Stock);
public record ProductImageDto(string FileName, Uri Url);
