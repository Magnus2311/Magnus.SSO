using Magnus.SSO.Database.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Magnus.SSO.Helpers
{
    public static class UserExtensionMethods
    {
        public static string GenerateJwtToken(this User user, bool isRefreshToken = false)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Secret));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(credentials);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var expires = isRefreshToken ? DateTime.Now.AddYears(1) : DateTime.Now.AddHours(1);
            var payload = new JwtPayload(AppSettings.ValidIssuer, AppSettings.ValidAudience, authClaims, DateTime.Now, expires);

            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(secToken);
        }
    }
}
