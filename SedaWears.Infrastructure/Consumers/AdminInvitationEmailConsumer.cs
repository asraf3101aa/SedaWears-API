using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Infrastructure.Services;

namespace SedaWears.Infrastructure.Consumers;

public class AdminInvitationEmailConsumer(ResendEmailSender emailSender) : IConsumer<AdminInvitationEmailEvent>
{
    public async Task Consume(ConsumeContext<AdminInvitationEmailEvent> context)
    {
        var message = context.Message;

        const string subject = "SedaWears Admin Invitation";
        var body = $"<p>You have been invited as an <b>Admin</b> to the SedaWears platform.</p>" +
                   $"<p>Click <a href='{message.Url}'>here</a> to accept the invitation and set up your account password.</p>";

        await emailSender.SendAsync(message.To, subject, body);
    }
}
