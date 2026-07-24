namespace SedaWears.Application.Features.Categories.Models;

public record UpsertCategoryRequest(int ShopId, string? Name, string? Description);
public record UpdateShopCategoryActiveStatusRequest(int ShopId, bool? IsActive);
public record CategoryOrderRequest(int? Id, int? DisplayOrder);
public record ReorderShopCategoriesRequest(List<CategoryOrderRequest>? Orders, int ShopId);
public record ShopCategoryUpsertRequest(string? Name, string? Description);
