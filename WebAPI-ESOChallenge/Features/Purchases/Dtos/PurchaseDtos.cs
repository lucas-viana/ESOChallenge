namespace WebAPI_ESOChallenge.Features.Purchases.Dtos;

public class PurchaseRequest
{
    public string CosmeticId { get; set; } = string.Empty;
}

public class PurchaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RemainingVBucks { get; set; }
    public PurchasedCosmeticDto? PurchasedCosmetic { get; set; }
}

public class RefundRequest
{
    public string CosmeticId { get; set; } = string.Empty;
}

public class RefundResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RefundedAmount { get; set; }
    public int RemainingVBucks { get; set; }
}

public class PurchasedCosmeticDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PurchasePrice { get; set; }
    public DateTime PurchasedAt { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? BundleId { get; set; }
    public CosmeticImagesDto? Images { get; set; }
    public string? Type { get; set; }
    public string? Rarity { get; set; }
}

public class CosmeticImagesDto
{
    public string? Icon { get; set; }
    public string? Featured { get; set; }
}

public class PurchaseHistoryItemDto
{
    public string CosmeticId { get; set; } = string.Empty;
    public string CosmeticName { get; set; } = string.Empty;
    public int Price { get; set; }
    public DateTime PurchasedAt { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class PurchaseHistoryResponse
{
    public List<PurchaseHistoryItemDto> Purchases { get; set; } = new();
    public int TotalSpent { get; set; }
    public int TotalRefunded { get; set; }
}

public class UserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public List<PurchasedCosmeticDto> OwnedCosmetics { get; set; } = new();
    public int TotalItems { get; set; }
}
