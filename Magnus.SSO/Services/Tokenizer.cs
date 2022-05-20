using Magnus.SSO.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Magnus.SSO.Services
{
    public class Tokenizer
    {
        private JwtHeader _header;

        public Tokenizer(IConfiguration configuration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _header = new JwtHeader(credentials);
        }

        public string CreateRegistrationToken(string email)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var expires = DateTime.Now.AddDays(2);
            var payload = new JwtPayload(AppSettings.ValidIssuer, AppSettings.ValidAudience, authClaims, DateTime.Now, expires);

            var secToken = new JwtSecurityToken(_header, payload);
            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(secToken);
        }

        public IEnumerable<KeyValuePair<string, string>> DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            SecurityToken validatedToken;
            IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            var claimsIdentity = (ClaimsPrincipal)principal;
            foreach (var claim in claimsIdentity.Claims)
                yield return new KeyValuePair<string, string>(claim.Type, claim.Value);

        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                if (principal != null)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = AppSettings.ValidIssuer,
                ValidAudience = AppSettings.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Secret))
            };
        }
    }
}