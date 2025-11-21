using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Dtos
{
    /// <summary>
    /// DTO de resposta para cosméticos, incluindo informações de bundles
    /// </summary>
    public class CosmeticResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public TypeDto? Type { get; set; }

        [JsonPropertyName("rarity")]
        public RarityDto? Rarity { get; set; }

        [JsonPropertyName("series")]
        public SeriesDto? Series { get; set; }

        [JsonPropertyName("images")]
        public ImagesDto? Images { get; set; }

        [JsonPropertyName("added")]
        public DateTime? Added { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("isInShop")]
        public bool IsInShop { get; set; }

        [JsonPropertyName("isNew")]
        public bool IsNew { get; set; }

        [JsonPropertyName("isBundle")]
        public bool IsBundle { get; set; }

        [JsonPropertyName("containedItemIds")]
        public List<string>? ContainedItemIds { get; set; }

        [JsonPropertyName("containedItemsImages")]
        public List<string>? ContainedItemsImages { get; set; }

        [JsonPropertyName("containedItems")]
        public List<ContainedItemDto>? ContainedItems { get; set; }

        [JsonPropertyName("bundle")]
        public BundleInfoDto? Bundle { get; set; }
    }

    public class TypeDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class RarityDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class SeriesDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;
    }

    public class ImagesDto
    {
        [JsonPropertyName("smallIcon")]
        public string? SmallIcon { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("featured")]
        public string? Featured { get; set; }
    }

    public class BundleInfoDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("info")]
        public string Info { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }

    /// <summary>
    /// DTO simplificado para itens contidos em bundles
    /// </summary>
    public class ContainedItemDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public TypeDto? Type { get; set; }

        [JsonPropertyName("rarity")]
        public RarityDto? Rarity { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }
}
