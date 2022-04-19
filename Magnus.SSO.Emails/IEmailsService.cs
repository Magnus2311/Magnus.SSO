using System.Threading;

namespace Magnus.SSO.Emails
{
    public interface IEmailsService
    {
        Task SendRegistrationEmail(string url, string email, string token, string template);
        Task ReSendRegistrationEmail(string url, string email, string token);
        Task SendResetPasswordEmail(string url, string email, string token, string template);
    }
}