using Magnus.SSO.Models.DTOs;
using Magnus.SSO.Services;
using Microsoft.AspNetCore.Mvc;

namespace magnus.sso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(UserDTO? user)
        {
            user = await _usersService.Add(user);
            if (user is null) return Conflict();
            else return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var loginStatus = await _usersService.Login(loginDTO);
            if (loginStatus.Item1)
            {
                SetAccessTokenInCookie(loginStatus.Item2);
                SetRefreshTokenInCookie(loginStatus.Item3);
                return Ok();
            }

            return Unauthorized();
        }

        private void SetAccessTokenInCookie(string accessToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddHours(1),
            };
            Response.Cookies.Append("access_token", accessToken, cookieOptions);
        }

        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddYears(1),
            };
            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
        }
    }
}