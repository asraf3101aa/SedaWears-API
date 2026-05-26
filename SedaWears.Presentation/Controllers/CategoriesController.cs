using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Categories.Commands;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Queries;
using SedaWears.Domain.Enums;
using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
public class CategoriesController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int? shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetCategoriesQuery(shopId, pageNumber, pageSize, sortBy, sortOrder, search), ct));


    [HttpPost]
    public async Task<IActionResult> CreateCategory(UpsertCategoryRequest request, CancellationToken ct)
    {
        await mediator.Send(new CreateCategoryCommand(request.Name ?? string.Empty, request.Description, request.ShopId), ct);
        return Ok(new { message = "Category created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, UpsertCategoryRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryCommand(id, request.Name ?? string.Empty, request.Description, request.ShopId), ct);
        return Ok(new { message = "Category updated successfully." });
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id, int? shopId, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id, shopId), ct);
        return Ok(new { message = "Category deleted successfully." });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateCategoryStatus(int id, UpdateCategoryActiveStatusRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryActiveStatusCommand(id, request.IsActive!.Value, request.ShopId), ct);
        var status = request.IsActive!.Value ? "activated" : "deactivated";
        return Ok(new { message = $"Category {status} successfully." });
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderCategories(ReorderCategoriesRequest request, CancellationToken ct)
    {
        var commandOrders = request.Orders!.Select(o => new ReorderCategoryItem(o.Id!.Value, o.DisplayOrder!.Value)).ToList();
        await mediator.Send(new ReorderCategoriesCommand(commandOrders, request.ShopId), ct);
        return Ok(new { message = "Categories reordered successfully." });
    }
}
