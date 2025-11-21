using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using WebAPI_ESOChallenge.Data;
using WebAPI_ESOChallenge.Extensions;
using WebAPI_ESOChallenge.Features.Authentication.Interfaces;
using WebAPI_ESOChallenge.Features.Authentication.Models;
using WebAPI_ESOChallenge.Features.Authentication.Services;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Services;
using WebAPI_ESOChallenge.Features.Purchases.Interfaces;
using WebAPI_ESOChallenge.Features.Purchases.Services;
using WebAPI_ESOChallenge.Features.News.Interfaces;
using WebAPI_ESOChallenge.Features.News.Services;
using WebAPI_ESOChallenge.Services;
using WebAPI_ESOChallenge.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CORS Configuration (Clean Code - usando Extension Method)
// ========================================
builder.Services.AddCorsConfiguration(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================================
// Database Configuration
// ========================================
// Prioriza variável de ambiente (Docker) sobre appsettings.json
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Se a string de conexão vier no formato URL (postgres://), converte para formato Npgsql
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    try 
    {
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        var builderNpgsql = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Prefer,
            Pooling = true
        };
        
        connectionString = builderNpgsql.ToString();
        Console.WriteLine($" Connection string convertida do formato URL para Npgsql");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Erro ao processar connection string: {ex.Message}");
        throw;
    }
}

Console.WriteLine($"Conectando ao banco de dados...");

// Configura o DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ... (resto do código)
// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ========================================
// JWT Authentication Configuration
// ========================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "WebAPI-ESOChallenge";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "WebAPI-ESOChallenge";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Mudar para true em produção
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Remove delay de expiração do token
    };
});

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICosmeticService, CosmeticService>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddHttpClient();

// Register background services
builder.Services.AddHostedService<FortniteDataSyncService>();

var app = builder.Build();

// ========================================
// Database Migration & Seeding
// ========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Verificando migrations do banco de dados...");
        
        
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations aplicadas com sucesso!");
        }
        else
        {
            logger.LogInformation("Banco de dados está atualizado. Nenhuma migration pendente.");
        }
        
        // Verifica se o banco está acessível
        var canConnect = await context.Database.CanConnectAsync();
        if (canConnect)
        {
            logger.LogInformation(" Conexão com o banco de dados estabelecida com sucesso!");
        }
        else
        {
            logger.LogError(" Não foi possível conectar ao banco de dados!");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, " Erro ao executar migrations do banco de dados");
        throw;
    }
}

// ========================================
// Configure the HTTP request pipeline
// ========================================

// Enable CORS - MUST be before Authentication/Authorization
// CORS needs to process OPTIONS requests before any auth middleware
app.UseCorsPolicy(app.Environment);

// 1. Exceções detalhadas APENAS em desenvolvimento (Segurança)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI ESO Challenge V1");
    options.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
await app.RunAsync();