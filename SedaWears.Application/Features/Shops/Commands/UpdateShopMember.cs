using MediatR;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace SedaWears.Application.Features.Shops.Commands;

public record UpdateShopMemberCommand(int ShopId, int UserId, string FirstName, string LastName) : IRequest;

public class UpdateShopMemberHandler(IApplicationDbContext dbContext) : IRequestHandler<UpdateShopMemberCommand>
{
    public async Task Handle(UpdateShopMemberCommand request, CancellationToken ct)
    {
        var user = await dbContext.ShopOwners
            .Where(so => so.ShopId == request.ShopId && so.UserId == request.UserId)
            .Select(so => so.User)
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            user = await dbContext.ShopManagers
                .Where(sm => sm.ShopId == request.ShopId && sm.UserId == request.UserId)
                .Select(sm => sm.User)
                .FirstOrDefaultAsync(ct);
        }

        if (user == null)
            throw new ShopNotFoundException();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await dbContext.SaveChangesAsync(ct);
    }
}
