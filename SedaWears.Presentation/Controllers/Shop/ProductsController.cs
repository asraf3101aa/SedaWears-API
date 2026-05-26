using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Products.Commands;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Queries;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Shop;

[ApiController]
[Route("shops/{shopId:int}/[controller]")]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
public class ProductsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        int shopId,
        [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(categoryId, shopId, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpPost]
    public async Task<IActionResult> CreateProduct(int shopId, UpsertProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Gender,
            request.CategoryId,
            request.Images,
            shopId
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int shopId, int id, UpsertProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Gender,
            request.CategoryId,
            request.Images,
            shopId
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int shopId, int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id, shopId), ct);
        return Ok(new { message = "Product deleted successfully." });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateProductStatus(int shopId, int id, UpdateProductActiveStatusRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateProductActiveStatusCommand(id, request.IsActive!.Value, shopId), ct);
        var status = request.IsActive!.Value ? "activated" : "deactivated";
        return Ok(new { message = $"Product {status} successfully." });
    }
}
