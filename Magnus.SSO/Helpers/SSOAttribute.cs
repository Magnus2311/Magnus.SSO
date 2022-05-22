using Magnus.SSO.Database.Repositories;
using Magnus.SSO.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

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
            var httpType = context.HttpContext.Request.Method.ToUpperInvariant();

            var accessToken = string.Empty;
            var refreshToken = string.Empty;

            if (httpType == "POST")
            {
                var bodyStream = new StreamReader(context.HttpContext.Request.Body);
                bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
                var bodyText = bodyStream.ReadToEnd();
                var body = JsonSerializer.Deserialize<dynamic>(bodyText);

                accessToken = body?.accessToken;
                refreshToken = body?.refreshToken;
            }

            if (httpType == "GET")
            {
                accessToken = context.HttpContext.Request.Query["accessToken"];
                refreshToken = context.HttpContext.Request.Query["refreshToken"];
            }

            _context = context;
            var handler = new JwtSecurityTokenHandler();
            if (!string.IsNullOrEmpty(accessToken)
                && await ValidateToken(accessToken)) return;

            if (!string.IsNullOrEmpty(refreshToken)
                && await ValidateToken(refreshToken))
            {
                var claims = ((JwtSecurityToken)handler.ReadToken(refreshToken)).Claims;
                if (claims != null)
                {
                    var userClaim = claims.FirstOrDefault(claim => claim.Type.Contains("name"));
                    if (userClaim != null)
                    {
                        var username = userClaim.Value;
                        var user = await _usersRepo.GetByUsername(username);
                        if (user != null && user.RefreshTokens.Any(rt => rt == refreshToken))
                        {
                            accessSecToken = user.GenerateJwtToken();
                            SetAccessToken(accessSecToken, context);
                            AppSettings.LoggedUser = user;
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
                ValidIssuer = AppSettings.ValidIssuer,
                ValidAudience = AppSettings.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Secret ?? ""))
            };
        }
        private static void SetAccessToken(string accessToken, AuthorizationFilterContext context)
        {
            QueryBuilder queryBuilder = new QueryBuilder();
            var modifiedValue = HttpUtility.UrlDecode(accessToken);
            queryBuilder.Add("accessToken", modifiedValue);
            context.HttpContext.Request.QueryString = queryBuilder.ToQueryString();
        }
    }
}
