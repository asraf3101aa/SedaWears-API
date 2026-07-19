using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Infrastructure.Services;

namespace SedaWears.Infrastructure.Consumers;

public class ForgotPasswordEmailConsumer(ResendEmailSender emailSender) : IConsumer<ForgotPasswordEmailEvent>
{
    public async Task Consume(ConsumeContext<ForgotPasswordEmailEvent> context)
    {
        var message = context.Message;

        const string subject = "Reset Password";
        var body = $"<p>To reset your password, click <a href='{message.Url}'>here</a>.</p>";

        await emailSender.SendAsync(message.To, subject, body);
    }
}
