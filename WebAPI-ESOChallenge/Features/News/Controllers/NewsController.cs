using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_ESOChallenge.Features.News.DTOs;
using WebAPI_ESOChallenge.Features.News.Interfaces;

namespace WebAPI_ESOChallenge.Features.News.Controllers;

/// <summary>
/// Controller for Fortnite news endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly ILogger<NewsController> _logger;

    public NewsController(
        INewsService newsService,
        ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the latest Fortnite news for all game modes (BR, STW, Creative)
    /// </summary>
    /// <returns>News data containing MOTDs and messages</returns>
    /// <response code="200">Returns the latest news</response>
    /// <response code="500">If there was an error fetching the news</response>
    [HttpGet]
    [ProducesResponseType(typeof(NewsApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NewsApiResponse>> GetNews()
    {
        try
        {
            _logger.LogInformation("GET /api/news - Fetching latest Fortnite news");

            var news = await _newsService.GetNewsAsync();

            if (news == null)
            {
                _logger.LogWarning("News service returned null");
                return StatusCode(500, new { message = "Failed to fetch news from Fortnite API" });
            }

            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetNews endpoint");
            return StatusCode(500, new { message = "An error occurred while fetching news" });
        }
    }
}
