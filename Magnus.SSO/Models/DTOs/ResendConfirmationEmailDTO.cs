using Magnus.SSO.Helpers;

namespace Magnus.SSO.Models.DTOs
{
    public class ResendConfirmationEmailDTO
    {
        public SenderType SenderType { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }
}