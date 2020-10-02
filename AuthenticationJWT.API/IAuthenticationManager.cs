using AuthenticationJWT.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationJWT.API
{
    public interface IAuthenticationManager
    {
        Task<bool> ValidateUser(UserForAuthenticationDto userForAuthenticationDto);
        Task<bool> ValidateRefreshToken(string refreshToken);

        Task<AuthenticationModel> RefreshTokenAsync(string refreshToken);
        Task<AuthenticationModel> CreateTokenAsync();
    }
}
