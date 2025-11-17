using Microsoft.EntityFrameworkCore;
using WebAPI_ESOChallenge.Data;
using WebAPI_ESOChallenge.Features.Purchases.Dtos;
using WebAPI_ESOChallenge.Features.Purchases.Interfaces;
using WebAPI_ESOChallenge.Features.Purchases.Models;

namespace WebAPI_ESOChallenge.Features.Purchases.Services;

public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(
        ApplicationDbContext context,
        ILogger<PurchaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseResponse> PurchaseCosmeticAsync(string userId, string cosmeticId)
    {
        try
        {
            // Validar se o cosmético existe
            var cosmetic = await _context.Cosmetics
                .FirstOrDefaultAsync(c => c.Id == cosmeticId);

            if (cosmetic == null)
            {
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Cosmético não encontrado"
                };
            }

            // Validar se o usuário existe
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Usuário não encontrado"
                };
            }

            // Verificar se o item já foi comprado (AsNoTracking para não interferir com operações futuras)
            var existingPurchaseCheck = await _context.UserCosmetics
                .AsNoTracking()
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);

            // Se já possui e não foi reembolsado, não pode comprar novamente
            if (existingPurchaseCheck != null && !existingPurchaseCheck.IsRefunded)
            {
                return new PurchaseResponse
                {
                    Success = false,
                    Message = "Você já possui este cosmético",
                    RemainingVBucks = user.VBucks
                };
            }

            // Verificar saldo
            var price = cosmetic.Price;
            if (user.VBucks < price)
            {
                return new PurchaseResponse
                {
                    Success = false,
                    Message = $"V-Bucks insuficientes. Você tem {user.VBucks} e o item custa {price}",
                    RemainingVBucks = user.VBucks
                };
            }

            // Realizar a compra (transaction para garantir atomicidade)
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Deduzir V-Bucks
                user.VBucks -= price;

                // Se for um bundle, processar todos os itens contidos
                if (cosmetic.IsBundle && cosmetic.ContainedItemIds.Any())
                {
                    _logger.LogInformation("Processando bundle {BundleId} com {Count} itens inclusos", 
                        cosmeticId, cosmetic.ContainedItemIds.Count);

                    // Verificar se precisa reativar ou adicionar o bundle
                    if (existingPurchaseCheck != null && existingPurchaseCheck.IsRefunded)
                    {
                        // Reativar o bundle
                        var bundleToReactivate = await _context.UserCosmetics
                            .FirstAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);
                        
                        bundleToReactivate.PurchasePrice = price;
                        bundleToReactivate.PurchasedAt = DateTime.UtcNow;
                        bundleToReactivate.IsRefunded = false;
                        bundleToReactivate.RefundedAt = null;
                    }
                    else
                    {
                        // Registrar a compra do bundle principal (para histórico)
                        _context.UserCosmetics.Add(new UserCosmetic
                        {
                            UserId = userId,
                            CosmeticId = cosmeticId,
                            PurchasePrice = price,
                            PurchasedAt = DateTime.UtcNow,
                            IsRefunded = false,
                            BundleId = null // Bundle em si não tem bundle pai
                        });
                    }

                    // Filtrar IDs únicos e excluir o próprio bundle ID para evitar duplicação
                    var itemIds = cosmetic.ContainedItemIds
                        .Where(id => id != cosmeticId) // Não incluir o próprio bundle
                        .Distinct()
                        .ToList();

                    // Verificar quais itens existem no banco de dados
                    var existingCosmetics = await _context.Cosmetics
                        .AsNoTracking()
                        .Where(c => itemIds.Contains(c.Id))
                        .Select(c => c.Id)
                        .ToListAsync();

                    var validItemIds = itemIds.Where(id => existingCosmetics.Contains(id)).ToList();
                    var invalidItemIds = itemIds.Except(validItemIds).ToList();

                    if (invalidItemIds.Any())
                    {
                        _logger.LogWarning(
                            "Bundle {BundleId} contém {Count} itens que não existem no banco: {Items}",
                            cosmeticId, invalidItemIds.Count, string.Join(", ", invalidItemIds));
                    }

                    // Buscar todos os itens já comprados de uma vez (melhor performance)
                    var existingItems = await _context.UserCosmetics
                        .AsNoTracking()
                        .Where(uc => uc.UserId == userId && validItemIds.Contains(uc.CosmeticId))
                        .ToListAsync();

                    var existingItemIds = existingItems
                        .Where(i => !i.IsRefunded)
                        .Select(i => i.CosmeticId)
                        .ToHashSet();

                    var refundedItemIds = existingItems
                        .Where(i => i.IsRefunded)
                        .Select(i => i.CosmeticId)
                        .ToHashSet();

                    foreach (var itemId in validItemIds)
                    {
                        if (existingItemIds.Contains(itemId))
                        {
                            // Item já possuído, pular
                            _logger.LogDebug("Item {ItemId} já possuído pelo usuário", itemId);
                            continue;
                        }
                        
                        if (refundedItemIds.Contains(itemId))
                        {
                            // Reativar compra - precisa buscar novamente para tracking
                            var itemToReactivate = await _context.UserCosmetics
                                .FirstAsync(uc => uc.UserId == userId && uc.CosmeticId == itemId);
                            
                            itemToReactivate.PurchasePrice = 0; // Preço já pago no bundle
                            itemToReactivate.PurchasedAt = DateTime.UtcNow;
                            itemToReactivate.IsRefunded = false;
                            itemToReactivate.RefundedAt = null;
                            itemToReactivate.BundleId = cosmeticId; // Associar ao bundle
                            _logger.LogDebug("Reativando item {ItemId} do bundle", itemId);
                        }
                        else
                        {
                            // Adicionar novo item
                            _context.UserCosmetics.Add(new UserCosmetic
                            {
                                UserId = userId,
                                CosmeticId = itemId,
                                PurchasePrice = 0, // Preço já pago no bundle
                                PurchasedAt = DateTime.UtcNow,
                                IsRefunded = false,
                                BundleId = cosmeticId // Rastrear de qual bundle veio
                            });
                            _logger.LogDebug("Adicionando novo item {ItemId} do bundle", itemId);
                        }
                    }
                }
                else // Compra de item individual
                {
                    if (existingPurchaseCheck != null && existingPurchaseCheck.IsRefunded)
                    {
                        // Reativar o item - buscar com tracking
                        var itemToReactivate = await _context.UserCosmetics
                            .FirstAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId);
                        
                        itemToReactivate.PurchasePrice = price;
                        itemToReactivate.PurchasedAt = DateTime.UtcNow;
                        itemToReactivate.IsRefunded = false;
                        itemToReactivate.RefundedAt = null;
                    }
                    else
                    {
                        var userCosmetic = new UserCosmetic
                        {
                            UserId = userId,
                            CosmeticId = cosmeticId,
                            PurchasePrice = price,
                            PurchasedAt = DateTime.UtcNow,
                            IsRefunded = false,
                            BundleId = null // Compra individual
                        };
                        _context.UserCosmetics.Add(userCosmetic);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "User {UserId} purchased cosmetic {CosmeticId} for {Price} V-Bucks", 
                    userId, cosmeticId, price);

                return new PurchaseResponse
                {
                    Success = true,
                    Message = "Compra realizada com sucesso!",
                    RemainingVBucks = user.VBucks,
                    PurchasedCosmetic = new PurchasedCosmeticDto
                    {
                        Id = cosmetic.Id,
                        Name = cosmetic.Name,
                        Description = cosmetic.Description,
                        PurchasePrice = price,
                        PurchasedAt = DateTime.UtcNow, // Usar o tempo atual
                        IsRefunded = false,
                        Images = new CosmeticImagesDto
                        {
                            Icon = cosmetic.Images?.Icon,
                            Featured = cosmetic.Images?.Featured
                        },
                        Type = cosmetic.Type?.DisplayValue,
                        Rarity = cosmetic.Rarity?.DisplayValue
                    }
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing cosmetic {CosmeticId} for user {UserId}", cosmeticId, userId);
            return new PurchaseResponse
            {
                Success = false,
                Message = "Erro ao processar compra. Tente novamente."
            };
        }
    }

    public async Task<List<PurchasedCosmeticDto>> GetUserCosmeticsAsync(string userId)
    {
        try
        {
            var userCosmetics = await _context.UserCosmetics
                .Include(uc => uc.Cosmetic)
                    .ThenInclude(c => c.Images)
                .Include(uc => uc.Cosmetic)
                    .ThenInclude(c => c.Type)
                .Include(uc => uc.Cosmetic)
                    .ThenInclude(c => c.Rarity)
                .Where(uc => uc.UserId == userId && !uc.IsRefunded)
                .OrderByDescending(uc => uc.PurchasedAt)
                .ToListAsync();

            return userCosmetics.Select(uc => new PurchasedCosmeticDto
            {
                Id = uc.Cosmetic.Id,
                Name = uc.Cosmetic.Name,
                Description = uc.Cosmetic.Description,
                PurchasePrice = uc.PurchasePrice,
                PurchasedAt = uc.PurchasedAt,
                IsRefunded = uc.IsRefunded,
                RefundedAt = uc.RefundedAt,
                BundleId = uc.BundleId,
                Images = new CosmeticImagesDto
                {
                    Icon = uc.Cosmetic.Images?.Icon,
                    Featured = uc.Cosmetic.Images?.Featured
                },
                Type = uc.Cosmetic.Type?.DisplayValue,
                Rarity = uc.Cosmetic.Rarity?.DisplayValue
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cosmetics for user {UserId}", userId);
            return new List<PurchasedCosmeticDto>();
        }
    }

    public async Task<RefundResponse> RefundCosmeticAsync(string userId, string cosmeticId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new RefundResponse
                {
                    Success = false,
                    Message = "Usuário não encontrado"
                };
            }

            var userCosmetic = await _context.UserCosmetics
                .Include(uc => uc.Cosmetic)
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId && !uc.IsRefunded);

            if (userCosmetic == null)
            {
                return new RefundResponse
                {
                    Success = false,
                    Message = "Você não possui este item ou ele já foi reembolsado",
                    RemainingVBucks = user.VBucks
                };
            }

            // Verificar se o item foi adquirido através de um bundle
            if (!string.IsNullOrEmpty(userCosmetic.BundleId))
            {
                return new RefundResponse
                {
                    Success = false,
                    Message = "Itens adquiridos em bundles não podem ser reembolsados individualmente. Reembolse o bundle completo.",
                    RemainingVBucks = user.VBucks
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Marcar como reembolsado
                userCosmetic.IsRefunded = true;
                userCosmetic.RefundedAt = DateTime.UtcNow;

                // Devolver V-Bucks
                user.VBucks += userCosmetic.PurchasePrice;

                // Se for um bundle, também reembolsar todos os itens inclusos
                if (userCosmetic.Cosmetic.IsBundle && userCosmetic.Cosmetic.ContainedItemIds.Any())
                {
                    _logger.LogInformation("Reembolsando bundle {BundleId} com {Count} itens inclusos", 
                        cosmeticId, userCosmetic.Cosmetic.ContainedItemIds.Count);

                    var bundleItems = await _context.UserCosmetics
                        .Where(uc => uc.UserId == userId 
                                  && userCosmetic.Cosmetic.ContainedItemIds.Contains(uc.CosmeticId)
                                  && !uc.IsRefunded)
                        .ToListAsync();

                    foreach (var item in bundleItems)
                    {
                        item.IsRefunded = true;
                        item.RefundedAt = DateTime.UtcNow;
                        _logger.LogDebug("Reembolsando item {ItemId} do bundle", item.CosmeticId);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "User {UserId} refunded cosmetic {CosmeticId} for {Price} V-Bucks", 
                    userId, cosmeticId, userCosmetic.PurchasePrice);

                return new RefundResponse
                {
                    Success = true,
                    Message = "Reembolso realizado com sucesso!",
                    RefundedAmount = userCosmetic.PurchasePrice,
                    RemainingVBucks = user.VBucks
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding cosmetic {CosmeticId} for user {UserId}", cosmeticId, userId);
            return new RefundResponse
            {
                Success = false,
                Message = "Erro ao processar reembolso. Tente novamente."
            };
        }
    }

    public async Task<PurchaseHistoryResponse> GetPurchaseHistoryAsync(string userId)
    {
        try
        {
            var purchases = await _context.UserCosmetics
                .Include(uc => uc.Cosmetic)
                    .ThenInclude(c => c.Images)
                .Where(uc => uc.UserId == userId)
                .OrderByDescending(uc => uc.PurchasedAt)
                .ToListAsync();

            var historyItems = purchases.Select(uc => new PurchaseHistoryItemDto
            {
                CosmeticId = uc.CosmeticId,
                CosmeticName = uc.Cosmetic.Name,
                Price = uc.PurchasePrice,
                PurchasedAt = uc.PurchasedAt,
                IsRefunded = uc.IsRefunded,
                RefundedAt = uc.RefundedAt,
                ThumbnailUrl = uc.Cosmetic.Images?.SmallIcon ?? uc.Cosmetic.Images?.Icon
            }).ToList();

            var totalSpent = purchases
                .Where(p => !p.IsRefunded)
                .Sum(p => p.PurchasePrice);

            var totalRefunded = purchases
                .Where(p => p.IsRefunded)
                .Sum(p => p.PurchasePrice);

            return new PurchaseHistoryResponse
            {
                Purchases = historyItems,
                TotalSpent = totalSpent,
                TotalRefunded = totalRefunded
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchase history for user {UserId}", userId);
            return new PurchaseHistoryResponse();
        }
    }

    public async Task<int> GetUserVBucksAsync(string userId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.VBucks ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V-Bucks for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<bool> UserOwnsItemAsync(string userId, string cosmeticId)
    {
        try
        {
            return await _context.UserCosmetics
                .AnyAsync(uc => uc.UserId == userId && uc.CosmeticId == cosmeticId && !uc.IsRefunded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking ownership for user {UserId} and cosmetic {CosmeticId}", userId, cosmeticId);
            return false;
        }
    }
}
