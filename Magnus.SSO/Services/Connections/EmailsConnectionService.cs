using Magnus.SSO.Models.DTOs.Connections;

namespace Magnus.SSO.Services.Connections
{
    public class EmailsConnectionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _emailsApiUrl;

        public EmailsConnectionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _emailsApiUrl = configuration["Magnus:URLS:Emails:Api"] + "/emails";
        }

        public async Task SendRegistrationEmail(RegistrationEmailDTO registrationEmailDTO)
            => await _httpClient.PostAsJsonAsync($"{_emailsApiUrl}/registration", registrationEmailDTO);

        public async Task SendResetPasswordEmail(ResetPasswordEmailDTO resetPasswordEmailDTO)
            => await _httpClient.PostAsJsonAsync($"{_emailsApiUrl}/reset-password", resetPasswordEmailDTO);
    }
}