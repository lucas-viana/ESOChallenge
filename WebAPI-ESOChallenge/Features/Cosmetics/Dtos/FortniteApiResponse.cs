using System;
using System.Collections.Generic;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Dtos
{
    public class FortniteApiResponse<T>
    {
        public int Status { get; set; }
        public T? Data { get; set; }
    }

    public class CosmeticDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CosmeticTypeDto? Type { get; set; }
        public CosmeticRarityDto? Rarity { get; set; }
        public CosmeticSeriesDto? Series { get; set; }
        public CosmeticImagesDto? Images { get; set; }
        public string? Description { get; set; }
        public DateTime? Added { get; set; }
        public ShopHistoryDto? ShopHistory { get; set; }
    }

    public class CosmeticTypeDto
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class CosmeticRarityDto
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class CosmeticImagesDto
    {
        public string? SmallIcon { get; set; }
        public string? Icon { get; set; }
        public string? Featured { get; set; }
    }

    public class CosmeticSeriesDto
    {
        public string? Value { get; set; }
        public string? Image { get; set; }
    }

    public class ShopHistoryDto
    {
        public List<DateTime>? Dates { get; set; }
    }

    public class ShopResponse
    {
        public List<ShopEntryDto>? Entries { get; set; }
    }

    public class ShopEntryDto
    {
        public List<CosmeticDto>? Items { get; set; }
    }
}