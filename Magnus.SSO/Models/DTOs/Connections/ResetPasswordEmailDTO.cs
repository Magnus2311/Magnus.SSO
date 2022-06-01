using Magnus.SSO.Helpers;

namespace Magnus.SSO.Models.DTOs.Connections
{
    public class ResetPasswordEmailDTO
    {
        public SenderType SenderType { get; init; }
        public string? Username { get; init; }
        public string? Email { get; init; }
        public string? Token { get; init; }
    }
}