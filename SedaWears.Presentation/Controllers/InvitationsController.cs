using MediatR;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Invitations.Commands;
using SedaWears.Application.Features.Invitations.Queries;
using SedaWears.Application.Features.Auth.Models;
using SedaWears.Application.Common.Settings;


namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("invitations")]
public class InvitationsController(ISender mediator) : ControllerBase
{
    [HttpPost("accept-owner")]
    public async Task<IActionResult> AcceptShopOwnerInvitation(AcceptShopMemberInvitationRequest? request)
    {
        await mediator.Send(new AcceptShopOwnerInvitationCommand(
            request?.ShopId,
            request?.Email?.Trim(),
            request?.Token?.Trim(),
            request?.FirstName?.Trim(),
            request?.LastName?.Trim(),
            request?.Password));

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("accept-manager")]
    public async Task<IActionResult> AcceptShopManagerInvitation(AcceptShopMemberInvitationRequest? request)
    {
        await mediator.Send(new AcceptShopManagerInvitationCommand(
            request?.ShopId,
            request?.Email?.Trim(),
            request?.Token?.Trim(),
            request?.FirstName?.Trim(),
            request?.LastName?.Trim(),
            request?.Password));

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("accept-admin")]
    public async Task<IActionResult> AcceptAdminInvitation(AcceptAdminInvitationRequest? request)
    {
        await mediator.Send(new AcceptAdminInvitationCommand(
            request?.Email?.Trim(),
            request?.Token?.Trim(),
            request?.FirstName?.Trim(),
            request?.LastName?.Trim(),
            request?.Password));

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpGet("owner-details")]
    public async Task<IActionResult> GetShopOwnerInvitationDetails([FromQuery] int? shopId, [FromQuery] string? email, [FromQuery] string? token)
    {
        var response = await mediator.Send(new GetShopOwnerInvitationDetailsQuery(shopId, email?.Trim(), token?.Trim()));
        return Ok(response);
    }

    [HttpGet("manager-details")]
    public async Task<IActionResult> GetShopManagerInvitationDetails([FromQuery] int? shopId, [FromQuery] string? email, [FromQuery] string? token)
    {
        var response = await mediator.Send(new GetShopManagerInvitationDetailsQuery(shopId, email?.Trim(), token?.Trim()));
        return Ok(response);
    }

    [HttpGet("admin-details")]
    public async Task<IActionResult> GetAdminInvitationDetails([FromQuery] string? email, [FromQuery] string? token)
    {
        var response = await mediator.Send(new GetAdminInvitationDetailsQuery(email?.Trim(), token?.Trim()));
        return Ok(response);
    }
}
