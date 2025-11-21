using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using WebAPI_ESOChallenge.Features.News.DTOs;
using WebAPI_ESOChallenge.Features.News.Interfaces;
using WebAPI_ESOChallenge.Services.Interfaces;

namespace WebAPI_ESOChallenge.Features.News.Services;

/// <summary>
/// Service for Fortnite news operations
/// </summary>
public class NewsService : INewsService
{
    private readonly IHttpClientService _httpClientService;
    private readonly ILogger<NewsService> _logger;
    private readonly string _baseUrl;

    public NewsService(
        IHttpClientService httpClientService,
        ILogger<NewsService> logger,
        IConfiguration configuration)
    {
        _httpClientService = httpClientService;
        _logger = logger;
        _baseUrl = configuration["FortniteApi:BaseUrl"] ?? "https://fortnite-api.com/v2";
    }

    /// <summary>
    /// Gets the latest Fortnite news from the API
    /// </summary>
    public async Task<NewsApiResponse?> GetNewsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Fortnite news from API");

            var response = await _httpClientService.GetAsync<NewsApiResponse>($"{_baseUrl}/news");

            if (response == null)
            {
                _logger.LogWarning("News API returned null response");
                return null;
            }

            _logger.LogInformation("Successfully fetched news. Status: {Status}", response.Status);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Fortnite news");
            throw;
        }
    }
}
