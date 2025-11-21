using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Dtos;
using Microsoft.Extensions.Logging;

namespace WebAPI_ESOChallenge.Features.Cosmetics
{
    /// <summary>
    /// Controller responsável apenas por receber requisições HTTP
    /// e delegar para o serviço (Single Responsibility Principle)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CosmeticsController : ControllerBase
    {
        private readonly ICosmeticService _cosmeticService;
        private readonly ILogger<CosmeticsController> _logger;

        public CosmeticsController(ICosmeticService cosmeticService, ILogger<CosmeticsController> logger)
        {
            _cosmeticService = cosmeticService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCosmetics()
        {
            try
            {
                _logger.LogInformation("Requisição recebida para obter todos os cosméticos");
                var cosmetics = await _cosmeticService.GetAllCosmeticsAsync();
                var cosmeticsList = cosmetics.ToList();
                return Ok(new { success = true, data = cosmeticsList, count = cosmeticsList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar requisição de todos os cosméticos");
                return StatusCode(500, new { success = false, message = "Erro ao buscar cosméticos", error = ex.Message });
            }
        }

        [HttpGet("new")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNewCosmetics()
        {
            try
            {
                _logger.LogInformation("Requisição recebida para obter cosméticos novos");
                var cosmetics = await _cosmeticService.GetNewCosmeticsAsync();
                var cosmeticsList = cosmetics.ToList();
                return Ok(new { success = true, data = cosmeticsList, count = cosmeticsList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar requisição de cosméticos novos");
                return StatusCode(500, new { success = false, message = "Erro ao buscar cosméticos novos", error = ex.Message });
            }
        }

        [HttpGet("shop")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetShopCosmetics()
        {
            try
            {
                _logger.LogInformation("Requisição recebida para obter cosméticos em promoção");
                var cosmetics = await _cosmeticService.GetShopCosmeticsAsync();
                var cosmeticsList = cosmetics.ToList();
                return Ok(new { success = true, data = cosmeticsList, count = cosmeticsList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar requisição de cosméticos em promoção");
                return StatusCode(500, new { success = false, message = "Erro ao buscar cosméticos em promoção", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCosmeticById(string id)
        {
            try
            {
                _logger.LogInformation("Requisição recebida para obter cosmético {CosmeticId}", id);
                var cosmetic = await _cosmeticService.GetCosmeticByIdAsync(id);
                
                if (cosmetic == null)
                {
                    return NotFound(new { success = false, message = $"Cosmético com ID {id} não encontrado" });
                }

                return Ok(new { success = true, data = cosmetic });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar requisição do cosmético {CosmeticId}", id);
                return StatusCode(500, new { success = false, message = "Erro ao buscar cosmético", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca cosméticos com filtros avançados, paginação e ordenação
        /// POST /api/cosmetics/search
        /// </summary>
        [HttpPost("search")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchCosmetics([FromBody] CosmeticFilterRequest filters)
        {
            try
            {
                _logger.LogInformation("Requisição de busca recebida - Página: {Page}, Busca: {Search}", 
                    filters.Page, filters.SearchTerm ?? "nenhuma");
                
                var result = await _cosmeticService.SearchCosmeticsAsync(filters);
                
                return Ok(new 
                { 
                    success = true, 
                    data = result.Items,
                    pagination = new 
                    {
                        totalCount = result.TotalCount,
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalPages = result.TotalPages,
                        hasPreviousPage = result.HasPreviousPage,
                        hasNextPage = result.HasNextPage
                    },
                    filters = new
                    {
                        availableTypes = result.AvailableTypes,
                        availableRarities = result.AvailableRarities,
                        minPriceAvailable = result.MinPriceAvailable,
                        maxPriceAvailable = result.MaxPriceAvailable
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar busca de cosméticos");
                return StatusCode(500, new { success = false, message = "Erro ao buscar cosméticos", error = ex.Message });
            }
        }
    }
}