using Microsoft.Extensions.Options;
using Resend;
using SedaWears.Application.Common.Settings;

namespace SedaWears.Infrastructure.Services;

public class ResendEmailSender(IResend resend, IOptions<EmailConfig> configOptions)
{
    public async Task SendAsync(string to, string subject, string htmlBody, bool useNoReply = true)
    {
        var fromEmail = useNoReply ? configOptions.Value.NoReplyEmail : configOptions.Value.ContactEmail;

        var email = new EmailMessage
        {
            From = $"{configOptions.Value.FromName} <{fromEmail}>"
        };
        email.To.Add(to);
        email.Subject = subject;
        email.HtmlBody = htmlBody;

        await resend.EmailSendAsync(email);
    }
}
