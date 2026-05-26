namespace SedaWears.Application.Features.Categories.Models;

public record UpsertCategoryRequest(int? ShopId, string? Name, string? Description);
public record UpdateCategoryActiveStatusRequest(int? ShopId, bool? IsActive);
public record CategoryOrderRequest(int? Id, int? DisplayOrder);
public record ReorderCategoriesRequest(List<CategoryOrderRequest>? Orders, int? ShopId);
public record ShopUpsertCategoryRequest(string? Name, string? Description);
