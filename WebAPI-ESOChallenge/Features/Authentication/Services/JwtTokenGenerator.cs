using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using WebAPI_ESOChallenge.Features.Authentication.Interfaces;
using WebAPI_ESOChallenge.Features.Authentication.Models;

namespace WebAPI_ESOChallenge.Features.Authentication.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(ApplicationUser user)
        {
            // Ler configurações do appsettings.json
            var secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
            var issuer = _configuration["Jwt:Issuer"] ?? "WebAPI-ESOChallenge";
            var audience = _configuration["Jwt:Audience"] ?? "WebAPI-ESOChallenge";
            
            // Ler tempo de expiração (padrão 60 minutos)
            var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryInMinutes"], out var minutes) ? minutes : 60;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único do token
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var token = GenerateToken(user);
            return Task.FromResult(token);
        }
    }
}