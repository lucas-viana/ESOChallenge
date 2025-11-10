using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebAPI_ESOChallenge.Features.Authentication.Interfaces;
using WebAPI_ESOChallenge.Features.Authentication.Services;
using WebAPI_ESOChallenge.Features.Authentication.Models;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Services;
using WebAPI_ESOChallenge.Services.Interfaces;
using WebAPI_ESOChallenge.Services;
using WebAPI_ESOChallenge.Data;
using WebAPI_ESOChallenge.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CORS Configuration (Clean Code - usando Extension Method)
// ========================================
builder.Services.AddCorsConfiguration(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

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

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICosmeticService, CosmeticService>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// ========================================
// Configure the HTTP request pipeline
// ========================================

// Enable CORS - MUST be before Authentication/Authorization
// CORS needs to process OPTIONS requests before any auth middleware
app.UseCorsPolicy(app.Environment);

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI ESO Challenge V1");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

// Security middleware
app.UseHttpsRedirection();

// Authentication & Authorization
// Order is important: Authentication BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
app.Run();