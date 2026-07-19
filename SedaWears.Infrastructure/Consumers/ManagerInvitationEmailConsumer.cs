using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Infrastructure.Services;

namespace SedaWears.Infrastructure.Consumers;

public class ManagerInvitationEmailConsumer(ResendEmailSender emailSender) : IConsumer<ManagerInvitationEmailEvent>
{
    public async Task Consume(ConsumeContext<ManagerInvitationEmailEvent> context)
    {
        var message = context.Message;

        var subject = $"SedaWears Shop Manager Invitation for {message.ShopName}";
        var body = $"<p>You have been invited as a Shop Manager for <b>{message.ShopName}</b> on SedaWears.</p>" +
                   $"<p>Click <a href='{message.Url}'>here</a> to accept the invitation and set your password.</p>";

        await emailSender.SendAsync(message.To, subject, body);
    }
}
