using System;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Models
{
    /// <summary>
    /// Entidade de domínio que representa um cosmético do Fortnite
    /// </summary>
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

    /// <summary>
    /// Tipo do cosmético (Outfit, Pickaxe, etc.)
    /// Entidade de valor - EF Core requer ID para rastreamento
    /// </summary>
    public class CosmeticType
    {
        public int Id { get; set; } // Primary Key obrigatória para EF Core
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Raridade do cosmético (Common, Rare, Epic, Legendary)
    /// </summary>
    public class CosmeticRarity
    {
        public int Id { get; set; } // Primary Key obrigatória para EF Core
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Série do cosmético (Marvel, DC, etc.)
    /// </summary>
    public class CosmeticSeries
    {
        public int Id { get; set; } // Primary Key obrigatória para EF Core
        public string Value { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    /// <summary>
    /// URLs das imagens do cosmético
    /// </summary>
    public class CosmeticImages
    {
        public int Id { get; set; } // Primary Key obrigatória para EF Core
        public string? SmallIcon { get; set; }
        public string? Icon { get; set; }
        public string? Featured { get; set; }
    }
}