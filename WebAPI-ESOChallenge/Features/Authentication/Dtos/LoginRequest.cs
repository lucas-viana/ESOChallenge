using System.ComponentModel.DataAnnotations;

namespace WebAPI_ESOChallenge.Features.Authentication.Dtos
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}