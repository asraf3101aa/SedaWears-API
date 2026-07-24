using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Products.Models;

public record ShopCategoryProductUpsertRequest(string? Name, string? Description, decimal? Price, Gender? Gender, List<string>? ImageFileNames);
public record UpdateShopCategoryProductSizesRequest(List<ProductSizeUpsertRequest>? Sizes);
public record ProductSizeUpsertRequest(ProductSize? Size, int? Stock);
public record UpdateShopCategoryProductActiveStatusRequest(bool? IsActive);
