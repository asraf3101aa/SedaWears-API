using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Products.Models;

public record UpsertProductRequest(string? Name, string? Description, decimal? Price, Gender? Gender, List<string>? ImageFileNames);
public record UpdateProductSizesRequest(List<ProductSizeUpsertRequest>? Sizes);
public record ProductSizeUpsertRequest(ProductSize? Size, int? Stock);
public record UpdateProductActiveStatusRequest(bool? IsActive);
