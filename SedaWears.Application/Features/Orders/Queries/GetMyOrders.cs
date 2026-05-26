using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Orders.Queries;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record OrderDto(
    int Id,
    string Status,
    decimal TotalAmount,
    List<OrderItemDto> Items)
{
    public static OrderDto ToOrderDto(Order order) => new(
        order.Id,
        order.Status.ToString(),
        order.TotalAmount,
        order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.Product?.Name ?? "Unknown",
            i.Quantity,
            i.UnitPrice,
            i.UnitPrice * i.Quantity)).ToList());
}

public record GetMyOrdersQuery() : IRequest<List<OrderDto>>;

public record GetCustomerOrdersQuery(int userId) : IRequest<List<OrderDto>>;

public class GetMyOrdersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser) : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id!.Value;
        return await GetOrdersInternal(userId, context, cancellationToken);
    }

    internal static async Task<List<OrderDto>> GetOrdersInternal(int userId, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var orders = await context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.Id)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderDto.ToOrderDto).ToList();
    }
}

public class GetCustomerOrdersQueryHandler(IApplicationDbContext context) : IRequestHandler<GetCustomerOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        return await GetMyOrdersQueryHandler.GetOrdersInternal(request.userId, context, cancellationToken);
    }
}
