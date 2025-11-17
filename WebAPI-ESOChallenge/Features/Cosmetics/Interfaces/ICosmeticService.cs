using WebAPI_ESOChallenge.Features.Cosmetics.Dtos;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;

/// <summary>
/// Interface do serviço de cosméticos
/// Responsável por toda lógica de negócio relacionada a cosméticos
/// </summary>
public interface ICosmeticService
{
    // Métodos públicos que retornam DTOs (para API)
    Task<IEnumerable<CosmeticResponseDto>> GetAllCosmeticsAsync();
    Task<IEnumerable<CosmeticResponseDto>> GetNewCosmeticsAsync();
    Task<IEnumerable<CosmeticResponseDto>> GetShopCosmeticsAsync();
    Task<CosmeticResponseDto?> GetCosmeticByIdAsync(string id);
    
    // Métodos para persistência/administração (retornam entidades de domínio)
    Task<IEnumerable<Cosmetic>> GetShopCosmeticsForPersistenceAsync();
}