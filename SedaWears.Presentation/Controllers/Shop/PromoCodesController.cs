using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SedaWears.Application.Features.PromoCodes.Commands;
using SedaWears.Application.Features.PromoCodes.Queries;
using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Shop;

[ApiController]
[Route("shops/{shopId:int}/promocodes")]
[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Owner)},{nameof(UserRole.Manager)}")]
public class PromoCodesController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPromoCodes(int shopId, [FromQuery] bool? isActive)
        => Ok(await mediator.Send(new GetPromoCodesQuery(shopId, isActive)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPromoCode(int shopId, int id)
    {
        var promoCode = await mediator.Send(new GetPromoCodeByIdQuery(id));
        if (promoCode.ShopId != shopId)
            return NotFound("Promo code not found in this shop.");

        return Ok(promoCode);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int shopId, CreatePromoCodeCommand command)
    {
        if (command.ShopId != shopId)
            return BadRequest("Shop ID mismatch between route and request body.");

        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetPromoCode), new { shopId, id }, new { id, message = "Promo code created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int shopId, int id, UpdatePromoCodeCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch.");

        // Additional sanity check that the promo code belongs to the shop
        var promoCode = await mediator.Send(new GetPromoCodeByIdQuery(id));
        if (promoCode.ShopId != shopId)
            return NotFound("Promo code not found in this shop.");

        await mediator.Send(command);
        return Ok(new { message = "Promo code updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int shopId, int id)
    {
        var promoCode = await mediator.Send(new GetPromoCodeByIdQuery(id));
        if (promoCode.ShopId != shopId)
            return NotFound("Promo code not found in this shop.");

        await mediator.Send(new DeletePromoCodeCommand(id));
        return Ok(new { message = "Promo code deleted successfully." });
    }
}
