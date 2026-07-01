using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Domain.Enums;

namespace SedaWears.Application.Features.Cart.Commands;

public record UpdateCartItemCommand(int CartItemId, ProductSize? Size, int? Quantity) : ICommand;

public class UpdateCartItemHandler(IApplicationDbContext context, ICurrentUser currentUser) : ICommandHandler<UpdateCartItemCommand>
{
    public async Task Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        var userId = currentUser.Id;
        var item = await context.CartItems.FirstOrDefaultAsync(c => c.Id == request.CartItemId && c.UserId == userId, ct) 
            ?? throw new CartItemNotFoundException();
        
        if (request.Quantity.HasValue)
        {
            item.Quantity = request.Quantity.Value;
        }
        if (request.Size.HasValue)
        {
            item.Size = request.Size.Value;
        }
        
        await context.SaveChangesAsync(ct);
    }
}
