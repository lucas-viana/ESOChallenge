namespace WebAPI_ESOChallenge.Features.Cosmetics.Dtos;

/// <summary>
/// DTO de resposta paginada com metadados de filtros
/// </summary>
public class PaginatedCosmeticsResponse
{
    public List<CosmeticResponseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    
    // Metadados de filtros disponíveis (para popular o sidebar)
    public Dictionary<string, int> AvailableTypes { get; set; } = new();
    public Dictionary<string, int> AvailableRarities { get; set; } = new();
    public int MinPriceAvailable { get; set; }
    public int MaxPriceAvailable { get; set; }
}
