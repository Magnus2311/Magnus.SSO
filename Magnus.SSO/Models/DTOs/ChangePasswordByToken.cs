namespace Magnus.SSO.Models.DTOs
{
    public class ChangePasswordByToken
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}