namespace WebAPI_ESOChallenge.Configuration;

/// <summary>
/// Configurações de CORS da aplicação
/// Seguindo o princípio da Responsabilidade Única (SOLID)
/// </summary>
public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Configurações da API do Fortnite
/// </summary>
public class FortniteApiSettings
{
    public string BaseUrl { get; set; } = "https://fortnite-api.com/v2";
}

/// <summary>
/// Configurações de JWT
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 60;
}
