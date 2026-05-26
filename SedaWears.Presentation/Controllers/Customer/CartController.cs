using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Cart.Models;
using SedaWears.Application.Features.Cart.Commands;
using SedaWears.Application.Features.Cart.Queries;
using SedaWears.Domain.Enums;

using Microsoft.AspNetCore.RateLimiting;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Controllers.Customer;

[ApiController]
[Route("profile/cart")]
[Authorize]
[EnableRateLimiting(nameof(RateLimitingPolicies.Global))]
public class CartController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart()
        => Ok(await mediator.Send(new GetCartQuery()));

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartRequest request)
    {
        await mediator.Send(new AddToCartCommand(request.ProductId, request.Size, request.Quantity));
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, UpdateCartRequest request)
    {
        await mediator.Send(new UpdateCartItemCommand(id, request.Size, request.Quantity));
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        await mediator.Send(new RemoveCartItemCommand(id));
        return Ok(new { message = "Item removed from cart successfully." });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await mediator.Send(new ClearCartCommand());
        return Ok(new { message = "Cart cleared successfully." });
    }
}
