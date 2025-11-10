using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAPI_ESOChallenge.Configuration;

namespace WebAPI_ESOChallenge.Extensions;

/// <summary>
/// Extension methods para configuração de CORS
/// Seguindo o princípio de Extensão Aberta/Modificação Fechada (Open/Closed Principle)
/// </summary>
public static class CorsExtensions
{
    private const string DevelopmentPolicyName = "DevelopmentPolicy";
    private const string ProductionPolicyName = "ProductionPolicy";

    /// <summary>
    /// Adiciona e configura os serviços de CORS
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Lê as configurações do appsettings.json
        var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>() 
            ?? new CorsSettings();

        services.AddCors(options =>
        {
            // Policy para Development
            options.AddPolicy(DevelopmentPolicyName, policy =>
            {
                var allowedOrigins = corsSettings.AllowedOrigins.Length > 0
                    ? corsSettings.AllowedOrigins
                    : GetDefaultDevelopmentOrigins();

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithExposedHeaders("Content-Disposition"); // Para downloads
            });

            // Policy para Production
            options.AddPolicy(ProductionPolicyName, policy =>
            {
                var allowedOrigins = corsSettings.AllowedOrigins
                    .Where(origin => !origin.Contains("localhost"))
                    .ToArray();

                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With")
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
                        .WithExposedHeaders("Content-Disposition");
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Aplica a policy de CORS apropriada baseada no ambiente
    /// </summary>
    public static IApplicationBuilder UseCorsPolicy(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        var policyName = environment.IsDevelopment() 
            ? DevelopmentPolicyName 
            : ProductionPolicyName;

        return app.UseCors(policyName);
    }

    /// <summary>
    /// Retorna as origens padrão para desenvolvimento
    /// </summary>
    private static string[] GetDefaultDevelopmentOrigins()
    {
        return new[]
        {
            "http://localhost:5173",    // Vite dev server
            "http://localhost:5174",    // Vite alternative port
            "http://localhost:3000",    // React/Next.js default
            "http://localhost:4200",    // Angular default
            "https://localhost:5173",   // HTTPS Vite
            "https://localhost:5174",   // HTTPS alternative
        };
    }
}
