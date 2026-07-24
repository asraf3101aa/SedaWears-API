using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Categories.Commands;
using SedaWears.Application.Features.Categories.Models;
using SedaWears.Application.Features.Categories.Queries;
using SedaWears.Application.Features.Invitations.Commands;
using SedaWears.Application.Features.Products.Commands;
using SedaWears.Application.Features.Products.Models;
using SedaWears.Application.Features.Products.Queries;
using SedaWears.Application.Features.Shops.Commands;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Queries;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("shops")]
public class ShopsController(ISender mediator) : ControllerBase
{
    #region Shops

    [HttpGet]
    public async Task<IActionResult> GetShops(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopSortField sortBy = ShopSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopsQuery(sortBy, sortOrder, search, pageNumber, pageSize), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShop(int id, CancellationToken ct)
        => Ok(await mediator.Send(new GetShopQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateShop(UpsertShopRequest? request, CancellationToken ct)
    {
        var shopId = await mediator.Send(new CreateShopCommand(request?.Name, request?.SubdomainSlug, request?.Description, request?.LogoFileName, request?.BannerFileName), ct);
        return CreatedAtAction(nameof(GetShop), new { id = shopId }, null);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Owner)}, {nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShop(int id, UpsertShopRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopCommand(id, request?.Name, request?.SubdomainSlug, request?.Description, request?.LogoFileName, request?.BannerFileName), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteShop(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateShopStatus(int id, UpdateShopActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopActiveStatusCommand(id, request?.IsActive), ct);
        return NoContent();
    }

    #endregion

    #region Owners

    [HttpGet("{id:int}/owners")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetShopOwners(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopMemberSortField sortBy = ShopMemberSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopMembersQuery(id, UserRole.Owner, pageNumber, pageSize, sortBy, sortOrder), ct));

    [HttpDelete("{id:int}/owners/{ownerId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteShopOwner(int id, int ownerId, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopMemberCommand(id, ownerId), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/owners/invites")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetInvitedShopOwners(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetInvitedShopOwnersQuery(id, pageNumber, pageSize), ct));

    [HttpPost("{id:int}/owners/invites")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> InviteShopOwner(int id, ShopMemberInviteRequest? request, CancellationToken ct)
    {
        await mediator.Send(new InviteOwnerCommand(id, request?.Email?.Trim()), ct);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("{id:int}/owners/invites/{invitationId:int}/resend")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ResendInvitedShopOwner(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new ResendShopOwnerInvitationCommand(id, invitationId), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/owners/invites/{invitationId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteInvitedShopOwner(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new DeleteInvitedShopMemberCommand(id, invitationId, UserRole.Owner), ct);
        return NoContent();
    }

    #endregion

    #region Managers

    [HttpGet("{id:int}/managers")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetShopManagers(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopMemberSortField sortBy = ShopMemberSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopMembersQuery(id, UserRole.Manager, pageNumber, pageSize, sortBy, sortOrder), ct));

    [HttpDelete("{id:int}/managers/{managerId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteShopManager(int id, int managerId, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopMemberCommand(id, managerId), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/managers/invites")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetInvitedShopManagers(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetInvitedShopManagersQuery(id, pageNumber, pageSize), ct));

    [HttpPost("{id:int}/managers/invites")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> InviteShopManager(int id, ShopMemberInviteRequest? request, CancellationToken ct)
    {
        await mediator.Send(new InviteManagerCommand(id, request?.Email?.Trim()), ct);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("{id:int}/managers/invites/{invitationId:int}/resend")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ResendInvitedShopManager(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new ResendShopManagerInvitationCommand(id, invitationId), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/managers/invites/{invitationId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteInvitedShopManager(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new DeleteInvitedShopMemberCommand(id, invitationId, UserRole.Manager), ct);
        return NoContent();
    }

    #endregion

    #region Categories

    [HttpGet("{shopId:int}/categories")]
    public async Task<IActionResult> GetShopCategories(
        int shopId,
        [FromQuery] CategorySortField sortBy = CategorySortField.DisplayOrder,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopCategoriesQuery(shopId, sortBy, sortOrder, search), ct));

    [HttpGet("{shopId:int}/categories/{id:int}")]
    public async Task<IActionResult> GetShopCategory(int shopId, int id, CancellationToken ct)
        => Ok(await mediator.Send(new GetShopCategoryQuery(id, shopId), ct));

    [HttpPost("{shopId:int}/categories")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateShopCategory(int shopId, [FromBody] ShopCategoryUpsertRequest? request, CancellationToken ct)
    {
        var categoryId = await mediator.Send(new CreateShopCategoryCommand(request?.Name, request?.Description, shopId), ct);
        return CreatedAtAction(nameof(GetShopCategory), new { shopId, id = categoryId }, null);
    }

    [HttpPut("{shopId:int}/categories/{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategory(int shopId, int id, [FromBody] ShopCategoryUpsertRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopCategoryCommand(id, request?.Name, request?.Description, shopId), ct);
        return NoContent();
    }

    [HttpDelete("{shopId:int}/categories/{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteShopCategory(int shopId, int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopCategoryCommand(id, shopId), ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/categories/{categoryId:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategoryStatus(int id, int categoryId, [FromBody] UpdateShopCategoryActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopCategoryActiveStatusCommand(categoryId, request?.IsActive, id), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/categories/reorder")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> ReorderShopCategories(int id, [FromBody] ReorderShopCategoriesRequest? request, CancellationToken ct)
    {
        var commandOrders = request?.Orders?.Select(o => new ReorderCategoryItem(o.Id!.Value, o.DisplayOrder!.Value)).ToList();
        await mediator.Send(new ReorderShopCategoriesCommand(commandOrders, id), ct);
        return NoContent();
    }

    #endregion

    #region Products

    [HttpGet("{id:int}/products")]
    public async Task<IActionResult> GetShopProducts(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ProductSortField sortBy = ProductSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopProductsQuery(id, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpGet("{shopId:int}/categories/{categoryId:int}/products")]
    public async Task<IActionResult> GetShopCategoryProducts(
        int shopId,
        int categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ProductSortField sortBy = ProductSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopCategoryProductsQuery(shopId, categoryId, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpGet("{shopId:int}/categories/{categoryId:int}/products/{id:int}")]
    public async Task<IActionResult> GetShopCategoryProduct(int shopId, int categoryId, int id, CancellationToken ct)
        => Ok(await mediator.Send(new GetShopCategoryProductQuery(id, shopId, categoryId), ct));

    [HttpPost("{shopId:int}/categories/{categoryId:int}/products")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateShopCategoryProduct(int shopId, int categoryId, ShopCategoryProductUpsertRequest? request, CancellationToken ct)
    {
        var command = new CreateShopCategoryProductCommand(
            shopId,
            categoryId,
            request?.Name,
            request?.Description,
            request?.Price,
            request?.Gender,
            request?.ImageFileNames
        );

        var productId = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetShopCategoryProduct), new { id = shopId, categoryId, productId }, null);
    }

    [HttpPut("{shopId:int}/categories/{categoryId:int}/products/{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategoryProduct(int shopId, int categoryId, int id, ShopCategoryProductUpsertRequest? request, CancellationToken ct)
    {
        var command = new UpdateShopCategoryProductCommand(
            shopId,
            categoryId,
            id,
            request?.Name,
            request?.Description,
            request?.Price,
            request?.Gender,
            request?.ImageFileNames
        );

        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPatch("{shopId:int}/categories/{categoryId:int}/products/{id:int}/sizes")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategoryProductSizes(int shopId, int categoryId, int id, UpdateShopCategoryProductSizesRequest? request, CancellationToken ct)
    {
        var command = new UpdateShopCategoryProductSizesCommand(
            shopId,
            categoryId,
            id,
            request?.Sizes?.Select(s => new ProductSizeDto(s.Size ?? 0, s.Stock ?? 0)).ToList()
        );

        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{shopId:int}/categories/{categoryId:int}/products/{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteShopCategoryProduct(int shopId, int categoryId, int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopCategoryProductCommand(shopId, categoryId, id), ct);
        return NoContent();
    }

    [HttpPatch("{shopId:int}/categories/{categoryId:int}/products/{id:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategoryProductStatus(int shopId, int categoryId, int id, UpdateShopCategoryProductActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopCategoryProductActiveStatusCommand(shopId, categoryId, id, request?.IsActive), ct);
        return NoContent();
    }

    #endregion
}
