using System.Text.Json.Serialization;

namespace WebAPI_ESOChallenge.Features.News.DTOs;

/// <summary>
/// Response from Fortnite API /v2/news endpoint
/// </summary>
public class NewsApiResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public NewsData? Data { get; set; }
}

/// <summary>
/// News data containing information for different game modes
/// </summary>
public class NewsData
{
    [JsonPropertyName("br")]
    public NewsGameMode? Br { get; set; }

    [JsonPropertyName("stw")]
    public NewsGameMode? Stw { get; set; }

    [JsonPropertyName("creative")]
    public NewsGameMode? Creative { get; set; }
}

/// <summary>
/// News information for a specific game mode (BR, STW, or Creative)
/// All fields are optional as the API returns them based on availability
/// </summary>
public class NewsGameMode
{
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("motds")]
    public List<Motd>? Motds { get; set; }

    [JsonPropertyName("messages")]
    public List<NewsMessage>? Messages { get; set; }
}

/// <summary>
/// Message of the Day (MOTD)
/// </summary>
public class Motd
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("tabTitle")]
    public string TabTitle { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("tileImage")]
    public string? TileImage { get; set; }

    [JsonPropertyName("sortingPriority")]
    public int SortingPriority { get; set; }

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("videoString")]
    public string? VideoString { get; set; }

    [JsonPropertyName("videoId")]
    public string? VideoId { get; set; }
}

/// <summary>
/// News message
/// </summary>
public class NewsMessage
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("adspace")]
    public string? Adspace { get; set; }
}
