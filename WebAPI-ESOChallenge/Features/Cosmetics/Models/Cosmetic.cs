using System;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Models
{
    public class Cosmetic
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CosmeticType? Type { get; set; }
        public CosmeticRarity? Rarity { get; set; }
        public CosmeticSeries? Series { get; set; }
        public CosmeticImages? Images { get; set; }
        public DateTime? Added { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CosmeticType
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class CosmeticRarity
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class CosmeticSeries
    {
        public string Value { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    public class CosmeticImages
    {
        public string? SmallIcon { get; set; }
        public string? Icon { get; set; }
        public string? Featured { get; set; }
    }
}