using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SedaWears.Application.Features.PromoCodes.Queries;

namespace SedaWears.Presentation.Controllers.Customer;

[ApiController]
[Route("promocodes")]
[AllowAnonymous]
public class PromoCodesController(ISender mediator) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<IActionResult> ValidatePromoCode([FromBody] ValidatePromoCodeQuery query)
    {
        var result = await mediator.Send(query);
        if (!result.IsValid)
            return BadRequest(result);

        return Ok(result);
    }
}
