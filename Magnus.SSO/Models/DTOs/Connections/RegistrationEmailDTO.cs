using Magnus.SSO.Helpers;

namespace Magnus.SSO.Models.DTOs.Connections
{
    public class RegistrationEmailDTO
    {
        public SenderType SenderType { get; init; }
        public string? Email { get; init; }
        public string? Token { get; init; }
    }
}