using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;
using SedaWears.Application.Features.Shops.Models;
using SedaWears.Application.Features.Shops.Queries;
using SedaWears.Application.Features.Shops.Commands;
using SedaWears.Application.Features.Invitations.Commands;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Shop;

[ApiController]
[Route("shops/{shopId:int}/[controller]")]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
public class ManagersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> InviteShopManager(int shopId, [FromBody] ShopMemberInviteRequest request)
    {
        await mediator.Send(new InviteManagerCommand(shopId, request.Email?.Trim()));
        return Ok(new { message = "Manager invited successfully." });
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> GetShopManagers(
        int shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
        => Ok(await mediator.Send(new GetShopMembersQuery(shopId, UserRole.Manager, pageNumber, pageSize, sortBy, sortOrder)));

    [HttpGet("invited")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> GetInvitedShopManagers(
        int shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        => Ok(await mediator.Send(new GetInvitedShopManagersQuery(shopId, pageNumber, pageSize)));

    [HttpGet("{managerId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> GetShopManager([FromRoute] int shopId, [FromRoute] int managerId)
        => Ok(await mediator.Send(new GetShopMemberQuery(shopId, managerId)));

    [HttpPatch("{managerId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> UpdateShopManager([FromRoute] int shopId, [FromRoute] int managerId, [FromBody] UpdateShopMemberRequest request)
    {
        await mediator.Send(new UpdateShopMemberCommand(
            shopId,
            managerId,
            request.FirstName?.Trim() ?? string.Empty,
            request.LastName?.Trim() ?? string.Empty));
        return Ok(new { message = "Manager updated successfully." });
    }

    [HttpDelete("{managerId:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> DeleteShopManager([FromRoute] int shopId, [FromRoute] int managerId)
    {
        await mediator.Send(new DeleteShopMemberCommand(shopId, managerId));
        return Ok(new { message = "Manager removed from shop successfully." });
    }

    [HttpPost("{managerId:int}/resend-invitation")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> ResendShopManagerInvitation(int shopId, int managerId)
    {
        await mediator.Send(new ResendShopManagerInvitationCommand(shopId, managerId));
        return Ok(new { message = "Shop manager invitation resent successfully." });
    }

    [HttpDelete("{managerId:int}/invitation")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)}")]
    public async Task<IActionResult> DeleteInvitedShopManager(int shopId, int managerId)
    {
        await mediator.Send(new DeleteInvitedShopMemberCommand(shopId, managerId, UserRole.Manager));
        return Ok(new { message = "Invitation deleted successfully." });
    }
}
