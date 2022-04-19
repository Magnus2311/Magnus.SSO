namespace Magnus.SSO.Models.DTOs
{
    public class LoginDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;  
    }
}
