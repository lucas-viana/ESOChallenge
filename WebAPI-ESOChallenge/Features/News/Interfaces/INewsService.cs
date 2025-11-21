using WebAPI_ESOChallenge.Features.News.DTOs;

namespace WebAPI_ESOChallenge.Features.News.Interfaces;

/// <summary>
/// Service interface for Fortnite news operations
/// </summary>
public interface INewsService
{
    /// <summary>
    /// Gets the latest Fortnite news from the API
    /// </summary>
    /// <returns>News data containing BR, STW, and Creative news</returns>
    Task<NewsApiResponse?> GetNewsAsync();
}
