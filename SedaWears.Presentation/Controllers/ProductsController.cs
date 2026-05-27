using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Products.Commands;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Queries;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
public class ProductsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int? shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ProductSortBy sortBy = ProductSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(categoryId, shopId, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id, CancellationToken ct)
        => Ok(await mediator.Send(new GetProductQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateProduct(UpsertProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Gender,
            request.CategoryId,
            request.Images
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product created successfully." });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateProduct(int id, UpsertProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Gender,
            request.CategoryId,
            request.Images
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product updated successfully." });
    }

    [HttpPatch("{id:int}/sizes")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateProductSizes(int id, UpdateProductSizesRequest request, CancellationToken ct)
    {
        var command = new UpdateProductSizesCommand(
            id,
            request.Sizes!.Select(s => new ProductSizeDto(s.Size!.Value, s.Stock!.Value)).ToList()
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product sizes updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        return Ok(new { message = "Product deleted successfully." });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateProductStatus(int id, UpdateProductActiveStatusRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateProductActiveStatusCommand(id, request.IsActive!.Value), ct);
        var status = request.IsActive!.Value ? "activated" : "deactivated";
        return Ok(new { message = $"Product {status} successfully." });
    }
}
