using magnus.sso;

namespace Magnus.SSO.Helpers
{
    public class AppSettings
    {
        public static string ValidIssuer = Startup.Configuration?["JWT_ValidIssuer"] ?? string.Empty;
        public static string ValidAudience = Startup.Configuration?["JWT_ValidAudience"] ?? string.Empty;
        public static string Secret = Startup.Configuration?["JWT_Secret"] ?? string.Empty;
    }
}
