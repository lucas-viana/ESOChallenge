namespace WebAPI_ESOChallenge.Features.Cosmetics.Dtos;

/// <summary>
/// DTO para requisição de cosméticos com filtros, paginação e ordenação
/// </summary>
public class CosmeticFilterRequest
{
    // Paginação
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    
    // Busca textual (nome ou descrição)
    public string? SearchTerm { get; set; }
    
    // Filtros por tipo (outfit, pickaxe, emote, etc.)
    public List<string>? Types { get; set; }
    
    // Filtros por raridade (common, rare, epic, legendary, etc.)
    public List<string>? Rarities { get; set; }
    
    // Filtros por data de inclusão
    public DateTime? AddedAfter { get; set; }
    public DateTime? AddedBefore { get; set; }
    
    // Filtros de disponibilidade
    public bool? OnlyNew { get; set; }          // Apenas novos (últimos 30 dias)
    public bool? OnlyAvailable { get; set; }    // Apenas disponíveis para compra
    public bool? OnlyInShop { get; set; }       // Apenas em promoção na loja
    public bool? ExcludeBundles { get; set; }   // Excluir bundles da busca
    
    // Filtros de preço (V-Bucks)
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    
    // Ordenação
    public string SortBy { get; set; } = "added";  // "name", "price", "rarity", "added"
    public string SortOrder { get; set; } = "desc"; // "asc", "desc"
}
