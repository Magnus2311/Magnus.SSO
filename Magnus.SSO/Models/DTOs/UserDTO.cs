namespace Magnus.SSO.Models.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Version { get; set; } = 1;
        public bool IsDeleted { get; set; }
    }
}
