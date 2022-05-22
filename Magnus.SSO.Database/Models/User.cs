using MongoDB.Bson;

namespace Magnus.SSO.Database.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Version { get; set; } = 1;
        public bool IsConfirmed { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;
        public HashSet<string> RefreshTokens = new HashSet<string>();
        public List<Login> Logins { get; set; } = new List<Login>();
    }
}
