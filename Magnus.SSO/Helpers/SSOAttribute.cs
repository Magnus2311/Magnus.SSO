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
        private AuthorizationFilterContext? _context;
        private string accessSecToken;

        public SSOAttribute()
        {
            _usersRepo = new UsersRepository(Startup.Configuration!);
            accessSecToken = string.Empty;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var handler = new JwtSecurityTokenHandler();
            _context = context;
            if (context.HttpContext.Request.Cookies.TryGetValue("access_token", out var accessToken)
                && await ValidateToken(accessToken)) return;

            if (context.HttpContext.Request.Cookies.TryGetValue("refresh_token", out var token)
                && await ValidateToken(token))
            {
                var claims = ((JwtSecurityToken)handler.ReadToken(token)).Claims;
                if (claims != null)
                {
                    var userClaim = claims.FirstOrDefault(claim => claim.Type.Contains("name"));
                    if (userClaim != null)
                    {
                        var username = userClaim.Value;
                        var user = await _usersRepo.GetByUsername(username);
                        if (user != null && user.RefreshTokens.Any(rt => rt == token))
                        {
                            accessSecToken = user.GenerateJwtToken();
                            SetAccessToken(accessSecToken, context);
                            return;
                        }
                    }
                }
            }

            context.Result = new UnauthorizedResult();
        }

        private async Task<bool> ValidateToken(string? authToken)
        {
            if (authToken is null) return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                if (_context != null)
                {
                    _context.HttpContext.User = (ClaimsPrincipal)principal;
                    var user = await _usersRepo.GetByUsername(_context.HttpContext.User?.Identity?.Name ?? "");
                    AppSettings.LoggedUser = user;
                    return true;
                }

                return false;
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
