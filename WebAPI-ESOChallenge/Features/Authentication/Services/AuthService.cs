using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI_ESOChallenge.Features.Authentication.Dtos;
using WebAPI_ESOChallenge.Features.Authentication.Interfaces;
using WebAPI_ESOChallenge.Features.Authentication.Models;
using Microsoft.AspNetCore.Identity;

namespace WebAPI_ESOChallenge.Features.Authentication.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var token = _jwtTokenGenerator.GenerateToken(user);
                return new AuthResponse { Token = token, Success = true };
            }

            return new AuthResponse { Success = false, Errors = result.Errors.Select(e => e.Description).ToList() };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new AuthResponse { Success = false, Errors = new List<string> { "Invalid username or password." } };
            }

            var token = _jwtTokenGenerator.GenerateToken(user);
            return new AuthResponse { Token = token, Success = true };
        }
    }
}