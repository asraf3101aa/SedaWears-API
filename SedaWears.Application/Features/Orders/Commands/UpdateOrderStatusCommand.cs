using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Entities;

namespace SedaWears.Application.Features.Orders.Commands;

public record UpdateOrderStatusCommand(int OrderId, int CustomerId, OrderStatus Status) : IRequest;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("A valid customer identifier is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status provided.");
    }
}

public class UpdateOrderStatusCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateOrderStatusCommand>
{
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await context.Orders
            .AsNoTracking()
            .Select(o => new { o.Id, o.UserId })
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (existingOrder.UserId != request.CustomerId)
            throw new ForbiddenException("Order does not belong to this customer.");

        await context.Orders
            .Where(o => o.Id == request.OrderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, request.Status), cancellationToken);
    }
}
