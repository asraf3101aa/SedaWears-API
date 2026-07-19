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
[Authorize(Roles = nameof(UserRole.Admin))]
public class ShopsController(ISender mediator) : ControllerBase
{
    #region Shops

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetShops(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopSortBy sortBy = ShopSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopsQuery(sortBy, sortOrder, search, pageNumber, pageSize), ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShop(int id, CancellationToken ct)
        => Ok(await mediator.Send(new GetShopQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> CreateShop(UpsertShopRequest? request, CancellationToken ct)
    {
        await mediator.Send(new CreateShopCommand(request?.Name, request?.SubdomainSlug, request?.Description, request?.LogoFileName, request?.BannerFileName), ct);
        return Ok(new { message = "Shop created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateShop(int id, UpsertShopRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopCommand(id, request?.Name, request?.SubdomainSlug, request?.Description, request?.LogoFileName, request?.BannerFileName), ct);
        return Ok(new { message = "Shop updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteShop(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopCommand(id), ct);
        return Ok(new { message = "Shop deleted successfully." });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateShopStatus(int id, UpdateShopActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateShopActiveStatusCommand(id, request?.IsActive), ct);
        var status = (request?.IsActive ?? false) ? "activated" : "deactivated";
        return Ok(new { message = $"Shop {status} successfully." });
    }

    #endregion

    #region Owners

    [HttpGet("{id:int}/owners")]
    public async Task<IActionResult> GetShopOwners(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopMemberSortBy sortBy = ShopMemberSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopMembersQuery(id, UserRole.Owner, pageNumber, pageSize, sortBy, sortOrder), ct));

    [HttpDelete("{id:int}/owners/{ownerId:int}")]
    public async Task<IActionResult> DeleteShopOwner(int id, int ownerId, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopMemberCommand(id, ownerId), ct);
        return Ok(new { message = "Owner removed successfully." });
    }

    [HttpGet("{id:int}/owners/invites")]
    public async Task<IActionResult> GetInvitedShopOwners(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetInvitedShopOwnersQuery(id, pageNumber, pageSize), ct));

    [HttpPost("{id:int}/owners/invites")]
    public async Task<IActionResult> InviteShopOwner(int id, ShopMemberInviteRequest? request, CancellationToken ct)
    {
        await mediator.Send(new InviteOwnerCommand(id, request?.Email?.Trim()), ct);
        return Ok(new { message = "Invitation sent successfully." });
    }

    [HttpPost("{id:int}/owners/invites/{invitationId:int}/resend")]
    public async Task<IActionResult> ResendInvitedShopOwner(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new ResendShopOwnerInvitationCommand(id, invitationId), ct);
        return Ok(new { message = "Invitation resent successfully." });
    }

    [HttpDelete("{id:int}/owners/invites/{invitationId:int}")]
    public async Task<IActionResult> DeleteInvitedShopOwner(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new DeleteInvitedShopMemberCommand(id, invitationId, UserRole.Owner), ct);
        return Ok(new { message = "Invitation deleted successfully." });
    }

    #endregion

    #region Managers

    [HttpGet("{id:int}/managers")]
    public async Task<IActionResult> GetShopManagers(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopMemberSortBy sortBy = ShopMemberSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetShopMembersQuery(id, UserRole.Manager, pageNumber, pageSize, sortBy, sortOrder), ct));

    [HttpDelete("{id:int}/managers/{managerId:int}")]
    public async Task<IActionResult> DeleteShopManager(int id, int managerId, CancellationToken ct)
    {
        await mediator.Send(new DeleteShopMemberCommand(id, managerId), ct);
        return Ok(new { message = "Manager removed successfully." });
    }

    [HttpGet("{id:int}/managers/invites")]
    public async Task<IActionResult> GetInvitedShopManagers(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetInvitedShopManagersQuery(id, pageNumber, pageSize), ct));

    [HttpPost("{id:int}/managers/invites")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> InviteShopManager(int id, ShopMemberInviteRequest? request, CancellationToken ct)
    {
        await mediator.Send(new InviteManagerCommand(id, request?.Email?.Trim()), ct);
        return Ok(new { message = "Invitation sent successfully." });
    }

    [HttpPost("{id:int}/managers/invites/{invitationId:int}/resend")]
    public async Task<IActionResult> ResendInvitedShopManager(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new ResendShopManagerInvitationCommand(id, invitationId), ct);
        return Ok(new { message = "Invitation resent successfully." });
    }

    [HttpDelete("{id:int}/managers/invites/{invitationId:int}")]
    public async Task<IActionResult> DeleteInvitedShopManager(int id, int invitationId, CancellationToken ct)
    {
        await mediator.Send(new DeleteInvitedShopMemberCommand(id, invitationId, UserRole.Manager), ct);
        return Ok(new { message = "Invitation deleted successfully." });
    }

    #endregion

    #region Categories

    [HttpGet("{id:int}/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShopCategories(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CategorySortBy sortBy = CategorySortBy.DisplayOrder,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetCategoriesQuery(id, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpPost("{id:int}/categories")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateShopCategory(int id, [FromBody] ShopUpsertCategoryRequest? request, CancellationToken ct)
    {
        await mediator.Send(new CreateCategoryCommand(request?.Name, request?.Description, id), ct);
        return Ok(new { message = "Category created successfully." });
    }

    [HttpPut("{id:int}/categories/{categoryId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategory(int id, int categoryId, [FromBody] ShopUpsertCategoryRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryCommand(categoryId, request?.Name, request?.Description, id), ct);
        return Ok(new { message = "Category updated successfully." });
    }

    [HttpDelete("{id:int}/categories/{categoryId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteShopCategory(int id, int categoryId, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(categoryId, id), ct);
        return Ok(new { message = "Category deleted successfully." });
    }

    [HttpPatch("{id:int}/categories/{categoryId:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopCategoryStatus(int id, int categoryId, [FromBody] UpdateCategoryActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryActiveStatusCommand(categoryId, request?.IsActive, id), ct);
        var status = (request?.IsActive ?? false) ? "activated" : "deactivated";
        return Ok(new { message = $"Category {status} successfully." });
    }

    [HttpPost("{id:int}/categories/reorder")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> ReorderShopCategories(int id, [FromBody] ReorderCategoriesRequest? request, CancellationToken ct)
    {
        var commandOrders = request?.Orders?.Select(o => new ReorderCategoryItem(o.Id!.Value, o.DisplayOrder!.Value)).ToList();
        await mediator.Send(new ReorderCategoriesCommand(commandOrders, id), ct);
        return Ok(new { message = "Categories reordered successfully." });
    }

    #endregion

    #region Products

    [HttpGet("{id:int}/products")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShopProducts(
        int id,
        [FromQuery] int? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ProductSortBy sortBy = ProductSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(categoryId, id, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpGet("{id:int}/products/{productId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShopProduct(int id, int productId, CancellationToken ct)
        => Ok(await mediator.Send(new GetProductQuery(productId), ct));

    [HttpPost("{id:int}/products")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateShopProduct(int id, UpsertProductRequest? request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request?.Name,
            request?.Description,
            request?.Price,
            request?.Gender,
            request?.CategoryId,
            request?.Images,
            id
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product created successfully." });
    }

    [HttpPut("{id:int}/products/{productId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopProduct(int id, int productId, UpsertProductRequest? request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            productId,
            request?.Name,
            request?.Description,
            request?.Price,
            request?.Gender,
            request?.CategoryId,
            request?.Images,
            id
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product updated successfully." });
    }

    [HttpPatch("{id:int}/products/{productId:int}/sizes")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopProductSizes(int id, int productId, UpdateProductSizesRequest? request, CancellationToken ct)
    {
        var command = new UpdateProductSizesCommand(
            productId,
            request?.Sizes?.Select(s => new ProductSizeDto(s.Size ?? 0, s.Stock ?? 0)).ToList()
        );

        await mediator.Send(command, ct);
        return Ok(new { message = "Product sizes updated successfully." });
    }

    [HttpDelete("{id:int}/products/{productId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteShopProduct(int id, int productId, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(productId, id), ct);
        return Ok(new { message = "Product deleted successfully." });
    }

    [HttpPatch("{id:int}/products/{productId:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopProductStatus(int id, int productId, UpdateProductActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateProductActiveStatusCommand(productId, request?.IsActive, id), ct);
        var status = (request?.IsActive ?? false) ? "activated" : "deactivated";
        return Ok(new { message = $"Product {status} successfully." });
    }

    #endregion
}
