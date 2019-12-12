using System.Threading.Tasks;
using JWTAPI.Core.Models;
using JWTAPI.Core.Security.Hashing;
using JWTAPI.Core.Security.Tokens;
using JWTAPI.Core.Services;
using JWTAPI.Core.Services.Communication;
using Microsoft.AspNetCore.Identity;

namespace JWTAPI.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordHasher<User> _passwordHasherIdentity;
        private readonly ITokenHandler _tokenHandler;
        
        public AuthenticationService(IUserService userService, IPasswordHasher passwordHasher, ITokenHandler tokenHandler, IPasswordHasher<User> passwordHasherIdentity)
        {
            _tokenHandler = tokenHandler;
            _passwordHasher = passwordHasher;
            _userService = userService;
            _passwordHasherIdentity = passwordHasherIdentity;
        }

        public async Task<TokenResponse> CreateAccessTokenAsync(string email, string password)
        {
            var user = await _userService.FindByEmailAsync(email);
            if (user == null || _passwordHasherIdentity.VerifyHashedPassword(user, user.Password, password) == 0)
            //if (user == null || !_passwordHasher.PasswordMatches(password, user.Password))
            {
                return new TokenResponse(false, "Invalid credentials.", null);
            }

            var token = _tokenHandler.CreateAccessToken(user);

            return new TokenResponse(true, null, token);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string userEmail)
        {
            var token = _tokenHandler.TakeRefreshToken(refreshToken);

            if (token == null)
            {
                return new TokenResponse(false, "Invalid refresh token.", null);
            }

            if (token.IsExpired())
            {
                return new TokenResponse(false, "Expired refresh token.", null);
            }

            var user = await _userService.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return new TokenResponse(false, "Invalid refresh token.", null);
            }

            var accessToken = _tokenHandler.CreateAccessToken(user);
            return new TokenResponse(true, null, accessToken);
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _tokenHandler.RevokeRefreshToken(refreshToken);
        }
    }
}