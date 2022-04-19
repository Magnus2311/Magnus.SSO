using Magnus.SSO.Database.Repositories;
using Magnus.SSO.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace magnus.sso.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SSOAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UsersRepository _usersRepo;
        private AuthorizationFilterContext _context;
        private string accessSecToken;

        public SSOAttribute() => _usersRepo = new UsersRepository(Startup.Configuration!);

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var handler = new JwtSecurityTokenHandler();
            _context = context;
            context.HttpContext.Request.Cookies.TryGetValue("access_token", out var accessToken);
            if (accessToken is not null && ValidateToken(accessToken))
            {
                var claims = ((JwtSecurityToken)handler.ReadToken(accessToken)).Claims;
                var nameClaim = claims.FirstOrDefault(claim => claim.Type.Contains("name"));
                if (nameClaim is not null)
                {
                    var username = nameClaim.Value;
                    var user = _usersRepo.GetByUsername(username).GetAwaiter().GetResult();
                    AppSettings.LoggedUser = user;
                    return;
                }
               
            }

            context.HttpContext.Request.Cookies.TryGetValue("refresh_token", out var refreshToken);
            if (refreshToken is not null && ValidateToken(refreshToken))
            {
                var claims = ((JwtSecurityToken)handler.ReadToken(refreshToken)).Claims;
                var nameClaim = claims.FirstOrDefault(claim => claim.Type.Contains("name"));
                if (nameClaim is not null)
                {
                    var username = nameClaim.Value;
                    var user = _usersRepo.GetByUsername(username).GetAwaiter().GetResult();
                    if (user.RefreshTokens.Any(rt => rt == refreshToken))
                    {
                        accessSecToken = user.GenerateJwtToken();
                        SetAccessToken(accessSecToken, context);
                        AppSettings.LoggedUser = user;
                        return;
                    }
                }
            }

            context.Result = new UnauthorizedResult();
        }

        private bool ValidateToken(string authToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                _context.HttpContext.User = (ClaimsPrincipal)principal;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = Startup.Configuration?["JWT_ValidIssuer"],
                ValidAudience = Startup.Configuration?["JWT_ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.Configuration?["JWT_Secret"] ?? ""))
            };
        }
        private static void SetAccessToken(string accessToken, AuthorizationFilterContext context)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(1),
            };
            context.HttpContext.Response.Cookies.Append("access_token", accessToken, cookieOptions);
        }
    }
}
