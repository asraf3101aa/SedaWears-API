namespace SedaWears.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendManagerInvitationEmailAsync(string to, string shopName, string url);
    Task SendOwnerInvitationEmailAsync(string to, string shopName, string url);
    Task SendAdminInvitationEmailAsync(string to, string url);
    Task SendForgotPasswordEmailAsync(string to, string url);
}
