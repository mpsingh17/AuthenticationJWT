using AuthenticationJWT.Core.DTOs;
using AuthenticationJWT.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationJWT.API
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private User _user;

        public AuthenticationManager(UserManager<User> userManager, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuthenticationDto)
        {
            _user = await _userManager.FindByNameAsync(userForAuthenticationDto.UserName);

            return _user != null && await _userManager.CheckPasswordAsync(_user, userForAuthenticationDto.Password);
        }

        public async Task<AuthenticationModel> CreateTokenAsync()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            var authenticationModel = new AuthenticationModel();

            if (_user.RefreshTokens.Any(a => a.IsActive))
            {
                var activeRefreshToken = _user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                authenticationModel.RefreshToken = activeRefreshToken.Token;
                authenticationModel.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var refreshToken = CreateRefreshToken();
                authenticationModel.RefreshToken = refreshToken.Token;
                authenticationModel.RefreshTokenExpiration = refreshToken.Expires;
                _user.RefreshTokens.Add(refreshToken);
                _dbContext.Users.Update(_user);
                _dbContext.SaveChanges();
            }

            authenticationModel.JwtToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return authenticationModel;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("SecretKey").Value);

            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings.GetSection("ValidIssuer").Value,
                audience: jwtSettings.GetSection("ValidAudience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("Expires").Value)),
                signingCredentials: signingCredentials
            );

            return tokenOptions;
        }

        private RefreshToken CreateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.Now.AddDays(15),
                Created = DateTime.Now
            };
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            _user = await _dbContext.Users
                .Where(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken))
                .SingleOrDefaultAsync();

            var refreshTokenInDb = _user.RefreshTokens
                .Where(rt => rt.Token == refreshToken)
                .FirstOrDefault();

            return refreshToken != null && refreshTokenInDb.IsActive;
        }

        public async Task<AuthenticationModel> RefreshTokenAsync(string refreshToken)
        {
            var userRefreshToken = _user.RefreshTokens.Single(x => x.Token == refreshToken);

            // TODO: Handle if refresh token is not active.
            if (!userRefreshToken.IsActive)
            {
            }

            // TODO: Revoke all exisitng refresh tokens.

            // Revoke existing refresh token. The refresh token is used only once.
            userRefreshToken.Revoked = DateTime.Now;

            // Generate new refresh token.
            var newRefreshToken = CreateRefreshToken();
            _user.RefreshTokens.Add(newRefreshToken);
            _dbContext.Update(_user);
            _dbContext.SaveChanges();

            // Generate new jwt
            var authenticationModel = await CreateTokenAsync();
            authenticationModel.RefreshToken = newRefreshToken.Token;
            authenticationModel.RefreshTokenExpiration = newRefreshToken.Expires;
            
            return authenticationModel;
        }
    }
}
