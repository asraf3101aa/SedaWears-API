using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Users.Queries;
using SedaWears.Application.Features.Users.Commands;
using SedaWears.Application.Features.Invitations.Commands;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("users")]
public class UsersController(ISender mediator) : ControllerBase
{
    [HttpGet("admins")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAdmins(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UsersSortBy sortBy = UsersSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc)
        => Ok(await mediator.Send(new GetAdminsQuery(pageNumber, pageSize, sortBy, sortOrder)));

    [HttpGet("admins/invited")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetInvitedAdmins(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] InvitedAdminsSortBy sortBy = InvitedAdminsSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc)
        => Ok(await mediator.Send(new GetInvitedAdminsQuery(pageNumber, pageSize, sortBy, sortOrder)));

    [HttpPost("admins/invite")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> InviteAdmin([FromBody] InviteAdminRequest request)
    {
        await mediator.Send(new InviteAdminCommand(request.Email?.Trim()));
        return Ok(new { Message = "Invitation sent successfully." });
    }

    [HttpGet("customers")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UsersSortBy sortBy = UsersSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc)
        => Ok(await mediator.Send(new GetCustomersQuery(pageNumber, pageSize, sortBy, sortOrder)));

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile(int id)
        => Ok(await mediator.Send(new GetUserQuery(Id: id)));

    [HttpPatch("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        await mediator.Send(new UpdateUserCommand(
            id,
            request.FirstName?.Trim(),
            request.LastName?.Trim()));
        return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("admins/{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        await mediator.Send(new DeleteAdminCommand(id));
        return Ok(new { message = "Admin deleted successfully." });
    }

    [HttpDelete("customers/{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await mediator.Send(new DeleteCustomerCommand(id));
        return Ok(new { message = "Customer deleted successfully." });
    }

    [HttpPost("admins/{id:int}/resend-invitation")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ResendAdminInvitation(int id)
    {
        await mediator.Send(new ResendAdminInvitationCommand(id));
        return Ok(new { message = "Invitation resent successfully." });
    }

    [HttpDelete("admins/{id:int}/invitation")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteInvitedAdmin(int id)
    {
        await mediator.Send(new DeleteInvitedAdminCommand(id));
        return Ok(new { message = "Invitation deleted successfully." });
    }
}
