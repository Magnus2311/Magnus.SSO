namespace Magnus.SSO.Emails
{
    public class GmailSmtpEmailsService : IEmailsService
    {
        private const string FROM_EMAIL = _configuration["SSO_EMAIL_SENDER"];
        private readonly string password = _configuration["SSO_EMAIL_PASSWORD"];
        private readonly SmtpClient _smtpClient;

        private readonly IConfiguration _configuration;

        public GmailSmtpEmailsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FROM_EMAIL, password),
                EnableSsl = true,
            };
        }

        public async Task ReSendRegistrationEmail(string url, string email, string token)
        {
            var message = new MailMessage(FROM_EMAIL, email, "New confirmation email from Life Mode", $"{url}/auth/confirmEmail/{email}&{token}");
            message.IsBodyHtml = true;
            await _smtpClient.SendMailAsync(message);
        }

        public async Task SendRegistrationEmail(string url, string email, string token, string template)
        {
            var message = new MailMessage(FROM_EMAIL, email)
            {
                Subject = "Welcome to Life Mode"
            };
            message.IsBodyHtml = true;
            message.AlternateViews.Add(GetEmbeddedImage("./Images/logo_transparent.png", template, token));

            await _smtpClient.SendMailAsync(message);
        }

        public async Task SendResetPasswordEmail(string url, string email, string token, string template)
        {
            var message = new MailMessage(FROM_EMAIL, email)
            {
                Subject = "Reset password for Life Mode"
            };
            message.IsBodyHtml = true;
            message.AlternateViews.Add(GetEmbeddedImage("./Images/logo_transparent.png", template, token));

            await _smtpClient.SendMailAsync(message);
        }

        private AlternateView GetEmbeddedImage(String filePath, string template, string token)
        {
            LinkedResource res = new LinkedResource(filePath);
            res.ContentType = new ContentType()
            {
                MediaType = "image/png",
                Name = "logo_transparent.png"
            };
            res.ContentId = Guid.NewGuid().ToString();
            string htmlBody = string.Format(template, res.ContentId, token);
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }
    }
}