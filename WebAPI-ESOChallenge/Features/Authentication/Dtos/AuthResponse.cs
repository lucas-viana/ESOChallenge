namespace WebAPI_ESOChallenge.Features.Authentication.Dtos;

public class AuthResponse
{
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}