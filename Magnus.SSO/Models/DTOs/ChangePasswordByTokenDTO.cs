namespace Magnus.SSO.Models.DTOs
{
    public class ChangePasswordByTokenDTO
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}