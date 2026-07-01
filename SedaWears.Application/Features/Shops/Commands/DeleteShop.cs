using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Shops.Commands;

public record DeleteShopCommand(int Id) : IRequest;

public class DeleteShopHandler(IApplicationDbContext dbContext) : IRequestHandler<DeleteShopCommand>
{
    public async Task Handle(DeleteShopCommand request, CancellationToken ct)
    {
        var shop = await dbContext.Shops
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct) ?? throw new ShopNotFoundException();

        dbContext.Shops.Remove(shop);
        await dbContext.SaveChangesAsync(ct);
    }
}
