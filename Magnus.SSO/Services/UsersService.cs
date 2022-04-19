using AutoMapper;
using Magnus.SSO.Database.Models;
using Magnus.SSO.Database.Repositories;
using Magnus.SSO.Helpers;
using Magnus.SSO.Models.DTOs;

namespace Magnus.SSO.Services
{
    public class UsersService
    {
        private readonly UsersRepository _usersRepository;
        private readonly IMapper _mapper;

        public UsersService(UsersRepository usersRepository,
            IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper;
        }

        public async Task<UserDTO?> Add(UserDTO? userDTO)
        {
            if (userDTO is null) return null;

            var allUsers = await _usersRepository.GetAll();
            var user = _mapper.Map<User>(userDTO);
            if (allUsers.Any(u => u.Username.ToUpperInvariant() == user.Username.ToUpperInvariant())) return null;
            if (allUsers.Any(u => u.Email.ToUpperInvariant() == user.Email.ToUpperInvariant())) return null;

            await _usersRepository.Add(user);
            user = await _usersRepository.GetByUsername(user.Username);

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<(bool, string, string)> Login(LoginDTO loginDTO)
        {
            User? user = null;
            if (!string.IsNullOrEmpty(loginDTO.Username))
                user = await _usersRepository.GetByUsername(loginDTO.Username);
            else if (!string.IsNullOrEmpty(loginDTO.Email))
                user = await _usersRepository.GetByEmail(loginDTO.Username);

            if (user == null) return (false, string.Empty, string.Empty);

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
    }
}
