using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI_ESOChallenge.Features.Purchases.Dtos;
using WebAPI_ESOChallenge.Features.Purchases.Interfaces;

namespace WebAPI_ESOChallenge.Features.Purchases;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(
        IPurchaseService purchaseService,
        ILogger<PurchasesController> logger)
    {
        _purchaseService = purchaseService;
        _logger = logger;
    }

    /// <summary>
    /// Purchase a cosmetic item
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> PurchaseCosmetic([FromBody] PurchaseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado" });
        }

        var result = await _purchaseService.PurchaseCosmeticAsync(userId, request.CosmeticId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all cosmetics owned by the current user
    /// </summary>
    [HttpGet("my-cosmetics")]
    public async Task<ActionResult<List<PurchasedCosmeticDto>>> GetMyCosmetics()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado" });
        }

        var cosmetics = await _purchaseService.GetUserCosmeticsAsync(userId);
        return Ok(cosmetics);
    }

    /// <summary>
    /// Refund a previously purchased cosmetic
    /// </summary>
    [HttpPost("refund")]
    public async Task<ActionResult<RefundResponse>> RefundCosmetic([FromBody] RefundRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado" });
        }

        var result = await _purchaseService.RefundCosmeticAsync(userId, request.CosmeticId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get purchase history for the current user
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<PurchaseHistoryResponse>> GetPurchaseHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado" });
        }

        var history = await _purchaseService.GetPurchaseHistoryAsync(userId);
        return Ok(history);
    }

    /// <summary>
    /// Get V-Bucks balance for the current user
    /// </summary>
    [HttpGet("vbucks")]
    public async Task<ActionResult<object>> GetVBucks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado" });
        }

        var vbucks = await _purchaseService.GetUserVBucksAsync(userId);
        return Ok(new { vbucks });
    }

    /// <summary>
    /// Get public profile of any user (their owned cosmetics)
    /// </summary>
    [HttpGet("users/{userId}/profile")]
    [AllowAnonymous]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string userId)
    {
        var cosmetics = await _purchaseService.GetUserCosmeticsAsync(userId);
        
        // You might want to get user info from UserManager here
        return Ok(new UserProfileDto
        {
            UserId = userId,
            OwnedCosmetics = cosmetics,
            TotalItems = cosmetics.Count
        });
    }
}
