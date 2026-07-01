using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Queries;
using SedaWears.Application.Features.Shops.Commands;
using SedaWears.Application.Features.Invitations.Commands;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Shop;

[ApiController]
[Route("shops/{shopId:int}/[controller]")]
public class OwnersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> InviteShopOwner(int shopId, [FromBody] ShopMemberInviteRequest? request)
    {
        await mediator.Send(new InviteOwnerCommand(shopId, request?.Email?.Trim()));
        return Ok(new { message = "Owner added to shop successfully." });
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> GetShopOwners(
        int shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopMemberSortBy sortBy = ShopMemberSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc)
        => Ok(await mediator.Send(new GetShopMembersQuery(shopId, UserRole.Owner, pageNumber, pageSize, sortBy, sortOrder)));

    [HttpGet("invited")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> GetInvitedShopOwners(
        int shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await mediator.Send(new GetInvitedShopOwnersQuery(shopId, pageNumber, pageSize)));

    [HttpGet("{ownerId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> GetShopOwner([FromRoute] int shopId, [FromRoute] int ownerId)
        => Ok(await mediator.Send(new GetShopOwnerQuery(shopId, ownerId)));

    [HttpPatch("{ownerId:int}")]
    [Authorize(Roles = nameof(UserRole.Owner))]
    public async Task<IActionResult> UpdateShopOwner([FromRoute] int shopId, [FromRoute] int ownerId, [FromBody] UpdateShopMemberRequest? request)
    {
        await mediator.Send(new UpdateShopMemberCommand(
            shopId,
            ownerId,
            request?.FirstName?.Trim() ?? string.Empty,
            request?.LastName?.Trim() ?? string.Empty));
        return Ok(new { message = "Owner updated successfully." });
    }

    [HttpDelete("{ownerId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteShopOwner([FromRoute] int shopId, [FromRoute] int ownerId)
    {
        await mediator.Send(new DeleteShopMemberCommand(shopId, ownerId));
        return Ok(new { message = "Owner removed from shop successfully." });
    }

    [HttpPost("{ownerId:int}/resend-invitation")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> ResendShopOwnerInvitation(int shopId, int ownerId)
    {
        await mediator.Send(new ResendShopOwnerInvitationCommand(shopId, ownerId));
        return Ok(new { message = "Shop owner invitation resent successfully." });
    }

    [HttpDelete("{ownerId:int}/invitation")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> DeleteInvitedShopOwner(int shopId, int ownerId)
    {
        await mediator.Send(new DeleteInvitedShopMemberCommand(shopId, ownerId, UserRole.Owner));
        return Ok(new { message = "Invitation deleted successfully." });
    }
}
