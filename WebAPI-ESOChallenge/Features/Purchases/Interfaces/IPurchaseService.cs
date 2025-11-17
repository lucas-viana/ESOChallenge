using WebAPI_ESOChallenge.Features.Purchases.Dtos;

namespace WebAPI_ESOChallenge.Features.Purchases.Interfaces;

/// <summary>
/// Service for handling cosmetic purchases, refunds, and user inventory management
/// </summary>
public interface IPurchaseService
{
    /// <summary>
    /// Purchase a cosmetic item for the authenticated user
    /// </summary>
    Task<PurchaseResponse> PurchaseCosmeticAsync(string userId, string cosmeticId);
    
    /// <summary>
    /// Get all cosmetics owned by a user (not refunded)
    /// </summary>
    Task<List<PurchasedCosmeticDto>> GetUserCosmeticsAsync(string userId);
    
    /// <summary>
    /// Refund a previously purchased cosmetic
    /// </summary>
    Task<RefundResponse> RefundCosmeticAsync(string userId, string cosmeticId);
    
    /// <summary>
    /// Get purchase history including refunds
    /// </summary>
    Task<PurchaseHistoryResponse> GetPurchaseHistoryAsync(string userId);
    
    /// <summary>
    /// Get user's current V-Bucks balance
    /// </summary>
    Task<int> GetUserVBucksAsync(string userId);
    
    /// <summary>
    /// Check if a user owns a specific cosmetic (not refunded)
    /// </summary>
    Task<bool> UserOwnsItemAsync(string userId, string cosmeticId);
}
