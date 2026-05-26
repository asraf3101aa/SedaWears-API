using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SedaWears.Application.Features.Orders.Commands;
using SedaWears.Application.Features.Orders.Queries;
using SedaWears.Domain.Entities;

using SedaWears.Domain.Enums;

namespace SedaWears.Presentation.Controllers.Admin;

[ApiController]
[Route("admin/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class OrdersController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomerOrders([FromQuery] int customerId)
        => Ok(await mediator.Send(new GetCustomerOrdersQuery(customerId)));

    [HttpPatch("{orderId:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromQuery] int customerId, [FromBody] OrderStatus status)
    {
        await mediator.Send(new UpdateOrderStatusCommand(orderId, customerId, status));
        return Ok();
    }
}
