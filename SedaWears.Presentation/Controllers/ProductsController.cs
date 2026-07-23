using MediatR;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Queries;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int shopId,
        [FromQuery] int categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ProductSortField sortBy = ProductSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(shopId, categoryId, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id, [FromQuery] int shopId, [FromQuery] int categoryId, CancellationToken ct)
        => Ok(await mediator.Send(new GetProductQuery(id, shopId, categoryId), ct));
}
