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

    [JsonPropertyName("offerId")]
    public string OfferId { get; set; } = string.Empty;

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

    [JsonPropertyName("instruments")]
    public List<InstrumentDto>? Instruments { get; set; }

    [JsonPropertyName("legoKits")]
    public List<LegoKitDto>? LegoKits { get; set; }

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

        [JsonPropertyName("album")]
        public string? Album { get; set; }

        [JsonPropertyName("albumArt")]
        public string? AlbumArt { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }

        [JsonPropertyName("shopHistory")]
        public List<DateTime>? ShopHistory { get; set; }
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

        [JsonPropertyName("type")]
        public CosmeticTypeDto? Type { get; set; }

        [JsonPropertyName("rarity")]
        public CosmeticRarityDto? Rarity { get; set; }

        [JsonPropertyName("images")]
        public CarImagesDto? Images { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }

        [JsonPropertyName("shopHistory")]
        public List<DateTime>? ShopHistory { get; set; }
    }

    /// <summary>
    /// Imagens específicas de carros (small e large)
    /// </summary>
    public class CarImagesDto
    {
        [JsonPropertyName("small")]
        public string? Small { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("featured")]
        public string? Featured { get; set; }

        [JsonPropertyName("smallIcon")]
        public string? SmallIcon { get; set; }
    }

    /// <summary>
    /// Representa um instrumento musical no bundle
    /// </summary>
    public class InstrumentDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("type")]
        public CosmeticTypeDto? Type { get; set; }

        [JsonPropertyName("rarity")]
        public CosmeticRarityDto? Rarity { get; set; }

        [JsonPropertyName("images")]
        public InstrumentImagesDto? Images { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }

        [JsonPropertyName("shopHistory")]
        public List<DateTime>? ShopHistory { get; set; }
    }

    /// <summary>
    /// Imagens de instrumentos (small e large)
    /// </summary>
    public class InstrumentImagesDto
    {
        [JsonPropertyName("small")]
        public string? Small { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }
    }

    /// <summary>
    /// Representa um kit LEGO no bundle
    /// </summary>
    public class LegoKitDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public CosmeticTypeDto? Type { get; set; }

        [JsonPropertyName("images")]
        public LegoKitImagesDto? Images { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }

        [JsonPropertyName("shopHistory")]
        public List<DateTime>? ShopHistory { get; set; }
    }

    /// <summary>
    /// Imagens de LEGO kits (small, large e wide)
    /// </summary>
    public class LegoKitImagesDto
    {
        [JsonPropertyName("small")]
        public string? Small { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }

        [JsonPropertyName("wide")]
        public string? Wide { get; set; }
    }

    /// <summary>
    /// Representa um LEGO cosmético (categoria separada de legoKits)
    /// </summary>
    public class LegoDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("cosmeticId")]
        public string? CosmeticId { get; set; }

        [JsonPropertyName("soundLibraryTags")]
        public List<string>? SoundLibraryTags { get; set; }

        [JsonPropertyName("images")]
        public LegoKitImagesDto? Images { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }
    }

    /// <summary>
    /// Representa um Bean (Fall Guys cosmético)
    /// </summary>
    public class BeanDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("cosmeticId")]
        public string? CosmeticId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("gameplayTags")]
        public List<string>? GameplayTags { get; set; }

        [JsonPropertyName("images")]
        public BeanImagesDto? Images { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("added")]
        public DateTime Added { get; set; }
    }

    /// <summary>
    /// Imagens de Beans (small e large)
    /// </summary>
    public class BeanImagesDto
    {
        [JsonPropertyName("small")]
        public string? Small { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }
    }

    /// <summary>
    /// Estrutura de resposta do endpoint /cosmetics com todas as categorias
    /// </summary>
    public class AllCosmeticsData
    {
        [JsonPropertyName("br")]
        public List<CosmeticDto>? Br { get; set; }

        [JsonPropertyName("tracks")]
        public List<TrackDto>? Tracks { get; set; }

        [JsonPropertyName("instruments")]
        public List<InstrumentDto>? Instruments { get; set; }

        [JsonPropertyName("cars")]
        public List<CarDto>? Cars { get; set; }

        [JsonPropertyName("lego")]
        public List<LegoDto>? Lego { get; set; }

        [JsonPropertyName("legoKits")]
        public List<LegoKitDto>? LegoKits { get; set; }

        [JsonPropertyName("beans")]
        public List<BeanDto>? Beans { get; set; }
    }

    /// <summary>
    /// Estrutura de resposta do endpoint /cosmetics/new
    /// </summary>
    public class NewCosmeticsData
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("build")]
        public string? Build { get; set; }

        [JsonPropertyName("previousBuild")]
        public string? PreviousBuild { get; set; }

        [JsonPropertyName("hashes")]
        public NewCosmeticsHashes? Hashes { get; set; }

        [JsonPropertyName("lastAdditions")]
        public NewCosmeticsLastAdditions? LastAdditions { get; set; }

        [JsonPropertyName("items")]
        public AllCosmeticsData? Items { get; set; }
    }

    /// <summary>
    /// Hashes das últimas atualizações por categoria
    /// </summary>
    public class NewCosmeticsHashes
    {
        [JsonPropertyName("all")]
        public string? All { get; set; }

        [JsonPropertyName("br")]
        public string? Br { get; set; }

        [JsonPropertyName("tracks")]
        public string? Tracks { get; set; }

        [JsonPropertyName("instruments")]
        public string? Instruments { get; set; }

        [JsonPropertyName("cars")]
        public string? Cars { get; set; }

        [JsonPropertyName("lego")]
        public string? Lego { get; set; }

        [JsonPropertyName("legoKits")]
        public string? LegoKits { get; set; }

        [JsonPropertyName("beans")]
        public string? Beans { get; set; }
    }

    /// <summary>
    /// Data das últimas adições por categoria
    /// </summary>
    public class NewCosmeticsLastAdditions
    {
        [JsonPropertyName("all")]
        public DateTime? All { get; set; }

        [JsonPropertyName("br")]
        public DateTime? Br { get; set; }

        [JsonPropertyName("tracks")]
        public DateTime? Tracks { get; set; }

        [JsonPropertyName("instruments")]
        public DateTime? Instruments { get; set; }

        [JsonPropertyName("cars")]
        public DateTime? Cars { get; set; }

        [JsonPropertyName("lego")]
        public DateTime? Lego { get; set; }

        [JsonPropertyName("legoKits")]
        public DateTime? LegoKits { get; set; }

        [JsonPropertyName("beans")]
        public DateTime? Beans { get; set; }
    }
}