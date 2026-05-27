using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Profile.Queries;
using SedaWears.Application.Features.Shops.Queries;
using SedaWears.Domain.Enums;
using SedaWears.Application.Features.Shops.Models;

using SedaWears.Application.Features.Users.Commands;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Profile.Models;
using SedaWears.Application.Features.Profile.Commands;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("profile")]
public class ProfileController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProfile()
        => Ok(await mediator.Send(new GetMeQuery()));

    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        await mediator.Send(new UpdateProfileCommand(request.FirstName, request.LastName, request.Phone));
        return Ok(new { message = "Profile updated successfully." });
    }

    [HttpPatch("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordRequest request)
    {
        await mediator.Send(new ChangeUserPasswordCommand(request.OldPassword, request.NewPassword));
        return Ok(new { message = "Password changed successfully." });
    }

    [HttpGet("shops")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> GetMyShops(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ShopSortBy sortBy = ShopSortBy.CreatedAt,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        [FromQuery] string? search = null)
        => Ok(await mediator.Send(new GetMyShopsQuery(sortBy, sortOrder, search, pageNumber, pageSize)));
}
