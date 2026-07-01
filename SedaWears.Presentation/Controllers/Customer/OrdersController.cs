using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Orders.Models;
using SedaWears.Application.Features.Orders.Commands;
using SedaWears.Application.Features.Orders.Queries;

using SedaWears.Application.Common.Settings;

namespace SedaWears.Presentation.Controllers.Customer;

[ApiController]
[Route("orders")]
public class OrdersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Checkout([FromBody] CreateOrderRequest? request)
    {
        var address = request?.ShippingAddress is { } a
            ? new CheckoutAddress(a.FirstName, a.LastName, a.Phone, a.Street, a.City, a.ZipCode)
            : null;

        var items = request?.Items?.Select(i => new CheckoutItem(i.ProductId, i.Quantity)).ToList();

        var result = await mediator.Send(new CheckoutCommand(request?.CustomerEmail, address, items, request?.PromoCode));
        return Ok(result);
    }

    [HttpGet("/profile/orders")]
    [Authorize]
    public async Task<IActionResult> GetMyHistory()
    {
        var orders = await mediator.Send(new GetMyOrdersQuery());
        return Ok(orders);
    }
}
