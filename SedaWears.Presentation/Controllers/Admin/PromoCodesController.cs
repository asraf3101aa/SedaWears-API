using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.PromoCodes.Commands;
using SedaWears.Application.Features.PromoCodes.Queries;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/promocodes")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class PromoCodesController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPromoCodes([FromQuery] int? shopId, [FromQuery] bool? isActive)
        => Ok(await mediator.Send(new GetPromoCodesQuery(shopId, isActive)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPromoCode(int id)
        => Ok(await mediator.Send(new GetPromoCodeByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create(CreatePromoCodeCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetPromoCode), new { id }, new { id, message = "Promo code created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePromoCodeCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch.");

        await mediator.Send(command);
        return Ok(new { message = "Promo code updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeletePromoCodeCommand(id));
        return Ok(new { message = "Promo code deleted successfully." });
    }
}
