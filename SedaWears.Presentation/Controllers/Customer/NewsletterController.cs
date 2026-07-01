using MediatR;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Newsletter.Commands;
using SedaWears.Application.Features.Newsletter.Queries;

namespace SedaWears.Presentation.Controllers.Customer;

public record NewsletterRequest(string? Email);
public record UnsubscribeConfirmationRequest(string? Token);

[ApiController]
[Route("customer/[controller]")]
public class NewsletterController(ISender mediator) : ControllerBase
{
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterRequest? request)
    {
        await mediator.Send(new SubscribeCommand(request?.Email ?? string.Empty));
        return Ok("Thank you for subscribing to Luga Store updates!");
    }

    [HttpGet("validate-unsubscribe/{token}")]
    public async Task<IActionResult> ValidateUnsubscribe(string token)
    {
        var email = await mediator.Send(new ValidateUnsubscribeQuery(token));
        if (email == null) return NotFound("Invalid unsubscribe link.");
        return Ok(new { email });
    }

    [HttpPost("confirm-unsubscribe")]
    public async Task<IActionResult> ConfirmUnsubscribe([FromBody] UnsubscribeConfirmationRequest? request)
    {
        await mediator.Send(new ConfirmUnsubscribeCommand(request?.Token ?? string.Empty));
        return Ok("You have been successfully unsubscribed.");
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] NewsletterRequest? request)
    {
        await mediator.Send(new UnsubscribeCommand(request?.Email ?? string.Empty));
        return Ok("You have been unsubscribed.");
    }
}
