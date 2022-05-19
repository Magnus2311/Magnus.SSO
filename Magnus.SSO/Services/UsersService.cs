using System.Security.Claims;
using AutoMapper;
using Magnus.SSO.Database.Models;
using Magnus.SSO.Database.Repositories;
using Magnus.SSO.Helpers;
using Magnus.SSO.Models.DTOs;
using Magnus.SSO.Models.DTOs.Connections;
using Magnus.SSO.Services.Connections;

namespace Magnus.SSO.Services
{
    public class UsersService
    {
        private readonly UsersRepository _usersRepository;
        private readonly UrlsService _urlsService;
        private readonly IMapper _mapper;
        private readonly HashService _hashService;
        private readonly Tokenizer _tokenizer;
        private readonly EmailsConnectionService _emailsConnectionService;

        public UsersService(UsersRepository usersRepository,
            UrlsService urlsService,
            Tokenizer tokenizer,
            IMapper mapper,
            HashService hashService,
            EmailsConnectionService emailsConnectionService)
        {
            _usersRepository = usersRepository;
            _urlsService = urlsService;
            _tokenizer = tokenizer;
            _mapper = mapper;
            _hashService = hashService;
            _emailsConnectionService = emailsConnectionService;
        }

        public async Task<UserDTO?> ConfirmEmail(string token)
        {
            if (!_tokenizer.ValidateToken(token))
                return null;

            var claims = _tokenizer.DecodeToken(token).ToDictionary(x => x.Key, x => x.Value);
            var email = claims[ClaimTypes.Email];

            var callback = await _urlsService.GetCallbackByToken(token);
            if (callback != null)
            {
                var user = await _usersRepository.GetByEmail(email);
                user.IsConfirmed = true;
                await _usersRepository.Update(user);
                var userDTO = _mapper.Map<UserDTO>(user);
                userDTO.CallbackUrl = callback;
                return userDTO;
            }

            return null;
        }

        public async Task<UserDTO?> Add(UserDTO? userDTO)
        {
            if (userDTO is null) return null;

            var allUsers = await _usersRepository.GetAll();
            var user = _mapper.Map<User>(userDTO);
            if (allUsers.Any(u => u.Username.ToUpperInvariant() == user.Username.ToUpperInvariant())) return null;
            if (allUsers.Any(u => u.Email.ToUpperInvariant() == user.Email.ToUpperInvariant())) return null;

            user.Password = _hashService.Hash(userDTO.Password);
            await _usersRepository.Add(user);
            user = await _usersRepository.GetByUsername(user.Username);

            var token = _tokenizer.CreateRegistrationToken(user.Email);
            await _urlsService.Add(new Callback()
            {
                CallbackUrl = userDTO.CallbackUrl,
                Token = token
            });

            await _emailsConnectionService.SendRegistrationEmail(new RegistrationEmailDTO()
            {
                Email = userDTO.Email,
                SenderType = userDTO.SenderType,
                Token = token
            });

            return _mapper.Map<UserDTO>(user);
        }

        public bool IsUsernameAvailable(string username)
        {
            var usernames = _usersRepository.GetAllUsernames().Select(u => u.ToUpperInvariant());
            return !usernames.Contains(username.ToUpperInvariant());
        }

        public async Task<(bool, string, string)> Login(LoginDTO loginDTO)
        {
            User? user = null;
            if (!string.IsNullOrEmpty(loginDTO.Username))
                user = await _usersRepository.GetByUsername(loginDTO.Username);
            else if (!string.IsNullOrEmpty(loginDTO.Email))
                user = await _usersRepository.GetByEmail(loginDTO.Email);

            if (user == null) return (false, string.Empty, string.Empty);
            if (!_hashService.VerifyPassword(user.Password, loginDTO.Password)) return (false, string.Empty, string.Empty);

            var accessToken = user.GenerateJwtToken();
            var refreshToken = user.GenerateJwtToken(true);
            user.Logins.Add(new Login
            {
                IP = loginDTO.IP,
                URL = loginDTO.URL
            });
            user.RefreshTokens.Add(refreshToken);
            await _usersRepository.Update(user);
            return (true, accessToken, refreshToken);
        }

        public async Task Update(User user)
            => await _usersRepository.Update(user);
    }
}
