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
    [Authorize(Roles = nameof(UserRole.Admin))]
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

    [HttpGet("{id:int}/categories")]
    public async Task<IActionResult> GetCategories(
        int id,
        [FromQuery] CategorySortField sortBy = CategorySortField.DisplayOrder,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetCategoriesQuery(id, sortBy, sortOrder, search), ct));

    [HttpGet("{id:int}/categories/{categoryId:int}")]
    public async Task<IActionResult> GetCategory(int id, int categoryId, CancellationToken ct)
        => Ok(await mediator.Send(new GetCategoryQuery(categoryId, id), ct));

    [HttpPost("{id:int}/categories")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateCategory(int id, [FromBody] ShopUpsertCategoryRequest? request, CancellationToken ct)
    {
        var categoryId = await mediator.Send(new CreateCategoryCommand(request?.Name, request?.Description, id), ct);
        return CreatedAtAction(nameof(GetCategory), new { id, categoryId }, null);
    }

    [HttpPut("{id:int}/categories/{categoryId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateCategory(int id, int categoryId, [FromBody] ShopUpsertCategoryRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryCommand(categoryId, request?.Name, request?.Description, id), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/categories/{categoryId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteCategory(int id, int categoryId, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(categoryId, id), ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/categories/{categoryId:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateCategoryStatus(int id, int categoryId, [FromBody] UpdateCategoryActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryActiveStatusCommand(categoryId, request?.IsActive, id), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/categories/reorder")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> ReorderCategories(int id, [FromBody] ReorderCategoriesRequest? request, CancellationToken ct)
    {
        var commandOrders = request?.Orders?.Select(o => new ReorderCategoryItem(o.Id!.Value, o.DisplayOrder!.Value)).ToList();
        await mediator.Send(new ReorderCategoriesCommand(commandOrders, id), ct);
        return NoContent();
    }

    #endregion

    #region Products

    [HttpGet("{id:int}/categories/{categoryId:int}/products")]
    public async Task<IActionResult> GetProducts(
        int id,
        int categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ProductSortField sortBy = ProductSortField.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Asc,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(id, categoryId, pageNumber, pageSize, sortBy, sortOrder, search), ct));

    [HttpGet("{id:int}/categories/{categoryId:int}/products/{productId:int}")]
    public async Task<IActionResult> GetProduct(int id, int categoryId, int productId, CancellationToken ct)
        => Ok(await mediator.Send(new GetProductQuery(productId, id, categoryId), ct));

    [HttpPost("{id:int}/categories/{categoryId:int}/products")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> CreateProduct(int id, int categoryId, UpsertProductRequest? request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            id,
            categoryId,
            request?.Name,
            request?.Description,
            request?.Price,
            request?.Gender,
            request?.ImageFileNames
        );

        var productId = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetProduct), new { id, categoryId, productId }, null);
    }

    [HttpPut("{id:int}/categories/{categoryId:int}/products/{productId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateProduct(int id, int categoryId, int productId, UpsertProductRequest? request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            categoryId,
            productId,
            request?.Name,
            request?.Description,
            request?.Price,
            request?.Gender,
            request?.ImageFileNames
        );

        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/categories/{categoryId:int}/products/{productId:int}/sizes")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateProductSizes(int id, int categoryId, int productId, UpdateProductSizesRequest? request, CancellationToken ct)
    {
        var command = new UpdateProductSizesCommand(
            id,
            categoryId,
            productId,
            request?.Sizes?.Select(s => new ProductSizeDto(s.Size ?? 0, s.Stock ?? 0)).ToList()
        );

        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/categories/{categoryId:int}/products/{productId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> DeleteProduct(int id, int categoryId, int productId, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id, categoryId, productId), ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/categories/{categoryId:int}/products/{productId:int}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateProductStatus(int id, int categoryId, int productId, UpdateProductActiveStatusRequest? request, CancellationToken ct)
    {
        await mediator.Send(new UpdateProductActiveStatusCommand(id, categoryId, productId, request?.IsActive), ct);
        return NoContent();
    }

    #endregion
}
