using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Queries;
using SedaWears.Application.Features.Shops.Commands;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Shop;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
public class ShopsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetShops(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopSortBy sortBy = ShopSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        [FromQuery] string? search = null)
        => Ok(await mediator.Send(new GetShopsQuery(sortBy, sortOrder, search, pageNumber, pageSize)));

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetShop(int id)
        => Ok(await mediator.Send(new GetShopQuery(id)));

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Create(UpsertShopRequest request)
    {
        await mediator.Send(new CreateShopCommand(
            request.Name,
            request.SubdomainSlug,
            request.Description,
            request.LogoFileName,
            request.BannerFileName));
        return Ok(new { message = "Shop created successfully." });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShop(int id, UpsertShopRequest request)
    {
        await mediator.Send(new UpdateShopCommand(id, request.Name, request.SubdomainSlug, request.Description, request.LogoFileName, request.BannerFileName));
        return Ok(new { message = "Shop updated successfully." });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateShopStatus(int id, UpdateShopActiveStatusRequest request)
    {
        await mediator.Send(new UpdateShopActiveStatusCommand(id, request.IsActive));
        var status = request.IsActive ? "activated" : "deactivated";
        return Ok(new { message = $"Shop {status} successfully." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteShop(int id)
    {
        await mediator.Send(new DeleteShopCommand(id));
        return Ok(new { message = "Shop deleted successfully." });
    }
}
