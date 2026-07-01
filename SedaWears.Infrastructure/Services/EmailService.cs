using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Infrastructure.Services;

public class EmailService(IPublishEndpoint publishEndpoint) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body, bool useNoReply = true)
    {
        await publishEndpoint.Publish(new SendEmailEvent
        {
            To = to,
            Subject = subject,
            Body = body,
            UseNoReply = useNoReply
        });
    }
}
