using magnus.sso.Helpers;
using Magnus.SSO.Helpers;
using Magnus.SSO.Models.DTOs;
using Magnus.SSO.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace magnus.sso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly AppSettings _appSettings;

        public UsersController(UsersService usersService, AppSettings appSettings)
        {
            _usersService = usersService;
            _appSettings = appSettings;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Add(UserDTO? user)
        {
            user = await _usersService.Add(user);
            if (user is null) return Conflict();
            else return Ok(user);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var userDTO = await _usersService.ConfirmEmail(token);
            return Redirect(userDTO is not null ? $"{userDTO.CallbackUrl}?username={userDTO.Username}" : "");
        }

        [HttpGet("check-username-availability")]
        public bool CheckUsernameAvailability(string username)
            => _usersService.IsUsernameAvailable(username);


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            loginDTO.URL = Request.GetDisplayUrl();
            loginDTO.IP = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP unavailable";
            var loginStatus = await _usersService.Login(loginDTO);
            if (loginStatus.IsSuccessful)
            {
                return Ok(loginStatus.User);
            }

            return Unauthorized();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var user = _appSettings.LoggedUser;

            user.RefreshTokens.Remove(refreshToken);
            await _usersService.Update(user);

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            await _usersService.ResetPassword(resetPasswordDTO);

            return Ok();
        }

        [HttpGet("reset-password-confirmation")]
        public async Task<IActionResult> ResetPasswordConfirmation(string token)
        {
            var userDTO = await _usersService.RedirectByToken(token);
            return Redirect(userDTO is not null ? $"{userDTO.CallbackUrl}?token={token}&username={userDTO.Username}" : "");
        }

        [HttpPost("change-password-by-token")]
        public async Task<IActionResult> ChangePasswordByToken(ChangePasswordByTokenDTO changePasswordByToken)
        {
            await _usersService.ChangePasswordByToken(changePasswordByToken.Token, changePasswordByToken.NewPassword);
            return Ok();
        }

        [SSO]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordByToken)
        {
            return Ok(new ChangePasswordResponseDTO
            {
                IsChangeSuccessful = await _usersService.ChangePassword(changePasswordByToken)
            });
        }

        [SSO]
        [HttpGet("try-login")]
        public IActionResult TryLogin(string accessToken)
            => Ok(new { AccessToken = accessToken });

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken(string accessToken)
            => Ok(await _usersService.ValidateToken(accessToken));

        [HttpGet("validate-token-string")]
        public async Task<string> ValidateTokenString(string accessToken)
            => await _usersService.ValidateToken(accessToken);

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailDTO user)
        {
            await _usersService.ResendConfirmationEmail(user);
            return Ok();
        }

        private void SetAccessTokenInCookie(string accessToken)
        {
            var cookieOptions = new CookieOptions
            {
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