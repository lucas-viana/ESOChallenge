using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI_ESOChallenge.Features.Authentication.Dtos;
using WebAPI_ESOChallenge.Features.Authentication.Interfaces;
using WebAPI_ESOChallenge.Features.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace WebAPI_ESOChallenge.Features.Authentication.Services
{
    /// <summary>
    /// Serviço de autenticação seguindo SOLID principles
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager, 
            IJwtTokenGenerator jwtTokenGenerator,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Verificar se já existe um usuário com este email
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Errors = new List<string> { "Email já está em uso." } 
                };
            }

            var user = new ApplicationUser
            {
                UserName = request.Email, // Usar email como username
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var token = _jwtTokenGenerator.GenerateToken(user);
                var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryInMinutes"], out var minutes) ? minutes : 60;
                var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

                return new AuthResponse 
                { 
                    Token = token,
                    Email = user.Email ?? string.Empty,
                    ExpiresAt = expiresAt.ToString("o"), // ISO 8601 format
                    Success = true 
                };
            }

            return new AuthResponse 
            { 
                Success = false, 
                Errors = result.Errors.Select(e => e.Description).ToList() 
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Buscar usuário por email
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Errors = new List<string> { "Email ou senha inválidos." } 
                };
            }

            var token = _jwtTokenGenerator.GenerateToken(user);
            var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryInMinutes"], out var minutes) ? minutes : 60;
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            return new AuthResponse 
            { 
                Token = token,
                Email = user.Email ?? string.Empty,
                ExpiresAt = expiresAt.ToString("o"), // ISO 8601 format
                Success = true 
            };
        }
    }
}