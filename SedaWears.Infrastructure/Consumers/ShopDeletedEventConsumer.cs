using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace SedaWears.Infrastructure.Consumers;

public class ShopDeletedEventConsumer(IUserService userService) : IConsumer<ShopDeletedEvent>
{
    public async Task Consume(ConsumeContext<ShopDeletedEvent> context)
    {
        var message = context.Message;

        foreach (var userId in message.AffectedUserIds)
        {
            await userService.SyncMemberRoleAsync(userId, context.CancellationToken);
        }
    }
}
