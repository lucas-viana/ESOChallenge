using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI_ESOChallenge.Data;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;

namespace WebAPI_ESOChallenge.Features.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ICosmeticService _cosmeticService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ICosmeticService cosmeticService,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _cosmeticService = cosmeticService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Força sincronização imediata dos dados da loja
        /// </summary>
        [HttpPost("sync")]
        [AllowAnonymous] // Remover em produção
        public async Task<IActionResult> ForceSyncShop()
        {
            try
            {

                // Buscar cosméticos da loja (entidades para persistência)
                var shopCosmetics = await _cosmeticService.GetShopCosmeticsForPersistenceAsync();
                var shopList = shopCosmetics.ToList();

                _logger.LogInformation("📦 {Count} cosméticos encontrados na loja", shopList.Count);

                // Fazer upsert no banco
                var insertedCount = 0;
                var updatedCount = 0;

                foreach (var cosmetic in shopList)
                {
                    var existing = await _context.Cosmetics
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == cosmetic.Id);

                    if (existing == null)
                    {
                        _context.Cosmetics.Add(cosmetic);
                        insertedCount++;
                    }
                    else
                    {
                        _context.Cosmetics.Update(cosmetic);
                        updatedCount++;
                    }
                }

                var saved = await _context.SaveChangesAsync();

                var message = $"✅ Sincronização concluída: {insertedCount} novos, {updatedCount} atualizados, {saved} registros afetados";
                _logger.LogInformation(message);

                return Ok(new
                {
                    success = true,
                    message,
                    totalCosmetics = shopList.Count,
                    inserted = insertedCount,
                    updated = updatedCount,
                    saved
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante sincronização forçada");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro ao sincronizar dados",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Força sincronização de TODOS os cosméticos (todas as categorias)
        /// Usado para popular o banco com todos os itens disponíveis na API
        /// </summary>
        [HttpPost("sync-all")]
        [AllowAnonymous] // Remover em produção
        public async Task<IActionResult> ForceSyncAllCosmetics()
        {
            try
            {
                _logger.LogInformation("🔄 Iniciando sincronização de TODOS os cosméticos...");

                // Buscar TODOS os cosméticos da API (entidades para persistência)
                var allCosmetics = await _cosmeticService.GetAllCosmeticsForPersistenceAsync();
                var cosmeticsList = allCosmetics.ToList();

                _logger.LogInformation("📦 {Count} cosméticos encontrados em todas as categorias", cosmeticsList.Count);

                // Fazer upsert no banco
                var insertedCount = 0;
                var updatedCount = 0;

                // Buscar todos os IDs existentes de uma vez para otimizar
                var allIds = cosmeticsList.Select(c => c.Id).ToHashSet();
                var existingIdsList = await _context.Cosmetics
                    .Where(c => allIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();
                var existingIds = existingIdsList.ToHashSet();

                foreach (var cosmetic in cosmeticsList)
                {
                    if (!existingIds.Contains(cosmetic.Id))
                    {
                        _context.Cosmetics.Add(cosmetic);
                        insertedCount++;
                    }
                    else
                    {
                        _context.Cosmetics.Update(cosmetic);
                        updatedCount++;
                    }
                }

                var saved = await _context.SaveChangesAsync();

                var message = $"✅ Sincronização completa: {insertedCount} novos, {updatedCount} atualizados, {saved} registros afetados";
                _logger.LogInformation(message);

                return Ok(new
                {
                    success = true,
                    message,
                    totalCosmetics = cosmeticsList.Count,
                    inserted = insertedCount,
                    updated = updatedCount,
                    saved
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante sincronização completa de todos os cosméticos");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro ao sincronizar todos os cosméticos",
                    error = ex.Message
                });
            }
        }
    }
}
