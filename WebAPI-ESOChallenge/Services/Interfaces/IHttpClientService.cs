using System.Net.Http;
using System.Threading.Tasks;

namespace WebAPI_ESOChallenge.Services.Interfaces
{
    public interface IHttpClientService
    {
        Task<T?> GetAsync<T>(string url);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
        Task<HttpResponseMessage> PutAsync(string url, HttpContent content);
        Task<HttpResponseMessage> DeleteAsync(string url);
    }
}