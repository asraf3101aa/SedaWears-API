using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Users.Models;
using SedaWears.Application.Features.Profile.Models;
using SedaWears.Application.Features.Profile.Commands;
using SedaWears.Application.Features.Profile.Queries;
using SedaWears.Domain.Enums;
using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Controllers.Customer;

[ApiController]
[Route("profile/addresses")]
[Authorize(Roles = nameof(UserRole.Customer))]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
public class AddressController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AddressDto>>> GetAddresses()
    {
        return await mediator.Send(new GetAddressesQuery());
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> AddAddress(AddressRequest request)
    {
        return await mediator.Send(new AddAddressCommand(
            request.Label, request.FullName, request.Email, request.Phone, request.Street, request.City, request.ZipCode));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        await mediator.Send(new DeleteAddressCommand(id));
        return Ok(new { message = "Address deleted successfully." });
    }
}
