using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Shops.Commands;

public record UpdateShopActiveStatusCommand(int Id, bool? IsActive) : IRequest;

public class UpdateShopActiveStatusHandler(IApplicationDbContext dbContext) : IRequestHandler<UpdateShopActiveStatusCommand>
{
    public async Task Handle(UpdateShopActiveStatusCommand request, CancellationToken ct)
    {
        var shop = await dbContext.Shops.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new ShopNotFoundException();

        shop.IsActive = request.IsActive!.Value;

        await dbContext.SaveChangesAsync(ct);
    }
}
