using MediatR;
using Microsoft.EntityFrameworkCore;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common;
using ZiggyCreatures.Caching.Fusion;

namespace SedaWears.Application.Features.Profile.Commands;

public record DeleteAddressCommand(int AddressId) : IRequest;

public class DeleteAddressCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IFusionCache fusionCache) : 
    IRequestHandler<DeleteAddressCommand>
{
    public async Task Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id ?? throw new UnauthorizedAccessException();
        var address = await dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId, cancellationToken)
            ?? throw new AddressNotFoundException();

        dbContext.Addresses.Remove(address);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        await fusionCache.RemoveAsync(CacheKeys.ProfileAddresses(userId), token: cancellationToken);
    }
}
