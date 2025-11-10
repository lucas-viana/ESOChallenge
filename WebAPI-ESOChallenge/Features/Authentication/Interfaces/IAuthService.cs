using System.Threading.Tasks;
using WebAPI_ESOChallenge.Features.Authentication.Dtos;

namespace WebAPI_ESOChallenge.Features.Authentication.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}