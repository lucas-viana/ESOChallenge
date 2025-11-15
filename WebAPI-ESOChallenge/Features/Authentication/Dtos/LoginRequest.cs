using System.ComponentModel.DataAnnotations;

namespace WebAPI_ESOChallenge.Features.Authentication.Dtos
{
    /// <summary>
    /// Request DTO para login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}