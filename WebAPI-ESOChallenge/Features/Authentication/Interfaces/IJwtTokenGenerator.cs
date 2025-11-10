using System.Threading.Tasks;
using WebAPI_ESOChallenge.Features.Authentication.Models;

namespace WebAPI_ESOChallenge.Features.Authentication.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser user);
        Task<string> GenerateTokenAsync(ApplicationUser user);
    }
}