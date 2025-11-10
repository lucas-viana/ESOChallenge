using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI_ESOChallenge.Features.Cosmetics
{
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
                return Ok(new { success = true, data = cosmetics, count = cosmetics.Count() });
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
                return Ok(new { success = true, data = cosmetics, count = cosmetics.Count() });
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
                return Ok(new { success = true, data = cosmetics, count = cosmetics.Count() });
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
    }
}