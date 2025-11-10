using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;

public interface ICosmeticService
{
    Task<IEnumerable<Cosmetic>> GetAllCosmeticsAsync();
    Task<IEnumerable<Cosmetic>> GetNewCosmeticsAsync();
    Task<IEnumerable<Cosmetic>> GetShopCosmeticsAsync();
    Task<Cosmetic?> GetCosmeticByIdAsync(string id);
}