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


// ... (logo após builder.Services.AddSwaggerGen();)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Lógica de conversão para o Render (Aceita postgres:// e postgresql://)
if (!string.IsNullOrEmpty(connectionString) && 
    (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://")))
{
    try 
    {
        // Parse da URL
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        // Reconstrói para o formato Key=Value que o Npgsql entende
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
    }
    catch (Exception ex)
    {
        // Loga o erro para ajudar no debug se a string estiver mal formatada
        Console.WriteLine($"Erro ao processar connection string: {ex.Message}");
        throw;
    }
}

// 3. Configura o DbContext
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

// Authentication & Authorization
// Order is important: Authentication BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
await app.RunAsync();