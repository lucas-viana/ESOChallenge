namespace WebAPI_ESOChallenge.Features.Authentication.Dtos;

/// <summary>
/// Response DTO para autenticação
/// Contém token JWT e informações do usuário
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}