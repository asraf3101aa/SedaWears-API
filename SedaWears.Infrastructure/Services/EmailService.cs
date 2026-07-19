using MassTransit;
using SedaWears.Application.Common.Events;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Infrastructure.Services;

public class EmailService(IPublishEndpoint publishEndpoint) : IEmailService
{
    public async Task SendManagerInvitationEmailAsync(string to, string shopName, string url)
    {
        await publishEndpoint.Publish(new ManagerInvitationEmailEvent
        {
            To = to,
            ShopName = shopName,
            Url = url
        });
    }

    public async Task SendOwnerInvitationEmailAsync(string to, string shopName, string url)
    {
        await publishEndpoint.Publish(new OwnerInvitationEmailEvent
        {
            To = to,
            ShopName = shopName,
            Url = url
        });
    }

    public async Task SendAdminInvitationEmailAsync(string to, string url)
    {
        await publishEndpoint.Publish(new AdminInvitationEmailEvent
        {
            To = to,
            Url = url
        });
    }

    public async Task SendForgotPasswordEmailAsync(string to, string url)
    {
        await publishEndpoint.Publish(new ForgotPasswordEmailEvent
        {
            To = to,
            Url = url
        });
    }
}
