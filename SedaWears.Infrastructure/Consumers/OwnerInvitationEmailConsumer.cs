using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Infrastructure.Services;

namespace SedaWears.Infrastructure.Consumers;

public class OwnerInvitationEmailConsumer(ResendEmailSender emailSender) : IConsumer<OwnerInvitationEmailEvent>
{
    public async Task Consume(ConsumeContext<OwnerInvitationEmailEvent> context)
    {
        var message = context.Message;

        var subject = $"SedaWears Shop Owner Invitation for {message.ShopName}";
        var body = $"<p>You have been invited as a Shop Owner for <b>{message.ShopName}</b> on SedaWears.</p>" +
                   $"<p>Click <a href='{message.Url}'>here</a> to accept the invitation and set your password.</p>";

        await emailSender.SendAsync(message.To, subject, body);
    }
}
