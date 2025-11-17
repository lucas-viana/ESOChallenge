using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Dtos
{
    public class FortniteApiResponse<T>
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    public class CosmeticDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public CosmeticTypeDto? Type { get; set; }
        
        [JsonPropertyName("rarity")]
        public CosmeticRarityDto? Rarity { get; set; }
        
        [JsonPropertyName("series")]
        public CosmeticSeriesDto? Series { get; set; }
        
        [JsonPropertyName("images")]
        public CosmeticImagesDto? Images { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("added")]
        public DateTime? Added { get; set; }
        
        [JsonPropertyName("shopHistory")]
        public ShopHistoryDto? ShopHistory { get; set; }
    }

    public class CosmeticTypeDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        
        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class CosmeticRarityDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        
        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class CosmeticImagesDto
    {
        [JsonPropertyName("smallIcon")]
        public string? SmallIcon { get; set; }
        
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
        
        [JsonPropertyName("featured")]
        public string? Featured { get; set; }
    }

    public class CosmeticSeriesDto
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }

    public class ShopHistoryDto
    {
        [JsonPropertyName("dates")]
        public List<DateTime>? Dates { get; set; }
    }

    // ========================================
    // DTOs para a resposta completa do /shop
    // ========================================
    
    /// <summary>
    /// Dados principais da loja
    /// </summary>
public class ShopData
{
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }
    
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }
    
    [JsonPropertyName("vbuckIcon")]
    public string? VbuckIcon { get; set; }
    
    [JsonPropertyName("entries")]
    public List<ShopEntry>? Entries { get; set; }
}    /// <summary>
    /// Representa uma seção da loja (Destaque, Diária, etc.)
    /// </summary>
    public class ShopSection
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("entries")]
        public List<ShopEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Entrada individual na loja (pode conter um item ou bundle)
    /// </summary>
public class ShopEntry
{
    [JsonPropertyName("regularPrice")]
    public int RegularPrice { get; set; }
    
    [JsonPropertyName("finalPrice")]
    public int FinalPrice { get; set; }

    [JsonPropertyName("devName")]
    public string DevName { get; set; } = string.Empty;

    [JsonPropertyName("bundle")]
    public BundleDto? Bundle { get; set; }

    [JsonPropertyName("brItems")]
    public List<CosmeticDto>? BrItems { get; set; }

    [JsonPropertyName("tracks")]
    public List<TrackDto>? Tracks { get; set; }

    [JsonPropertyName("cars")]
    public List<CarDto>? Cars { get; set; }

    [JsonPropertyName("newDisplayAsset")]
    public NewDisplayAssetDto? NewDisplayAsset { get; set; }
}

public class NewDisplayAssetDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("cosmeticId")]
    public string? CosmeticId { get; set; }
}

public class BundleDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("info")]
    public string Info { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }
}    /// <summary>
    /// Informações do banner de uma entrada
    /// </summary>
    public class BannerDto
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("backendValue")]
        public string? BackendValue { get; set; }
    }

    /// <summary>
    /// Representa uma música/track no bundle
    /// </summary>
    public class TrackDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("devName")]
        public string? DevName { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("artist")]
        public string? Artist { get; set; }

        [JsonPropertyName("albumArt")]
        public string? AlbumArt { get; set; }
    }

    /// <summary>
    /// Representa um carro/veículo no bundle
    /// </summary>
    public class CarDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("images")]
        public CosmeticImagesDto? Images { get; set; }
    }
}