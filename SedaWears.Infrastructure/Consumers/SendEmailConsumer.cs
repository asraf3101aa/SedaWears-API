using Microsoft.Extensions.Options;
using MassTransit;
using Resend;
using SedaWears.Application.Common.Events;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.Consumers;

public class SendEmailConsumer(IResend resend, IOptions<EmailConfig> configOptions) : IConsumer<SendEmailEvent>
{
    public async Task Consume(ConsumeContext<SendEmailEvent> context)
    {
        var message = context.Message;
        var fromEmail = message.UseNoReply ? configOptions.Value.NoReplyEmail : configOptions.Value.ContactEmail;

        var email = new EmailMessage
        {
            From = $"{configOptions.Value.FromName} <{fromEmail}>"
        };
        email.To.Add(message.To);
        email.Subject = message.Subject;
        email.HtmlBody = message.Body;

        await resend.EmailSendAsync(email);
    }
}
