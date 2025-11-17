using WebAPI_ESOChallenge.Features.Authentication.Models;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Features.Purchases.Models;

/// <summary>
/// Represents the many-to-many relationship between users and their purchased cosmetics.
/// Tracks purchase and refund history.
/// </summary>
public class UserCosmetic
{
    public string UserId { get; set; } = string.Empty;
    public string CosmeticId { get; set; } = string.Empty;
    
    public int PurchasePrice { get; set; }
    public DateTime PurchasedAt { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    /// <summary>
    /// ID do bundle que contém este item (null se comprado individualmente)
    /// </summary>
    public string? BundleId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Cosmetic Cosmetic { get; set; } = null!;
}
