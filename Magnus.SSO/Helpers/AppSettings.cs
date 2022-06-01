using magnus.sso;
using Magnus.SSO.Database.Models;

namespace Magnus.SSO.Helpers
{
    public class AppSettings
    {
        public static string ValidIssuer = Startup.Configuration?["Magnus:SSO:JWT:Issuer"] ?? string.Empty;
        public static string ValidAudience = Startup.Configuration?["Magnus:SSO:JWT:Audience"] ?? string.Empty;
        public static string Secret = Startup.Configuration?["Magnus:SSO:JWT:Secret"] ?? string.Empty;

        public User LoggedUser { get; set; } = new User();
    }
}
