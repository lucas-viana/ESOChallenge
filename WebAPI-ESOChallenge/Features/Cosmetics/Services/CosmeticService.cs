using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebAPI_ESOChallenge.Features.Cosmetics.Dtos;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;
using WebAPI_ESOChallenge.Services.Interfaces;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Services;

public class CosmeticService : ICosmeticService
{
    private readonly IHttpClientService _httpClientService;
    private readonly ILogger<CosmeticService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public CosmeticService(
        IHttpClientService httpClientService, 
        ILogger<CosmeticService> logger,
        IConfiguration configuration)
    {
        _httpClientService = httpClientService;
        _logger = logger;
        _configuration = configuration;
        _baseUrl = _configuration["FortniteApi:BaseUrl"] ?? "https://fortnite-api.com/v2";
    }

    public async Task<IEnumerable<Cosmetic>> GetAllCosmeticsAsync()
    {
        try
        {
            _logger.LogInformation("Buscando todos os cosméticos da API do Fortnite");
            
            var url = $"{_baseUrl}/cosmetics/br";
            var response = await _httpClientService.GetAsync<FortniteApiResponse<List<CosmeticDto>>>(url);

            if (response?.Data == null || !response.Data.Any())
            {
                _logger.LogWarning("Nenhum cosmético encontrado na API");
                return Enumerable.Empty<Cosmetic>();
            }

            return response.Data
                .Where(dto => dto.Type != null && dto.Rarity != null)
                .Select(MapToCosmetic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os cosméticos");
            throw;
        }
    }

    public async Task<IEnumerable<Cosmetic>> GetNewCosmeticsAsync()
    {
        var url = $"{_baseUrl}/cosmetics/new";
        try
        {
            _logger.LogInformation("Buscando cosméticos novos da API do Fortnite");
            var response = await _httpClientService.GetAsync<FortniteApiResponse<CosmeticsListResponse>>(url);

            if (response?.Data?.Items?.Br == null || !response.Data.Items.Br.Any())
            {
                _logger.LogWarning("Nenhum cosmético novo encontrado ou resposta inválida da API.");
                return Enumerable.Empty<Cosmetic>();
            }

            var cosmetics = response.Data.Items.Br
                .Where(dto => dto.Type != null && dto.Rarity != null)
                .Select(MapToCosmetic)
                .ToList();
            
            _logger.LogInformation("{Count} cosméticos novos encontrados.", cosmetics.Count);
            return cosmetics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos novos");
            throw;
        }
    }

    public async Task<IEnumerable<Cosmetic>> GetShopCosmeticsAsync()
    {
        try
        {
            _logger.LogInformation("Buscando cosméticos da loja do Fortnite");
            
            var url = $"{_baseUrl}/shop";
            var response = await _httpClientService.GetAsync<FortniteApiResponse<ShopData>>(url);

            if (response?.Data?.Entries == null)
            {
                _logger.LogWarning("Nenhum dado da loja encontrado na API");
                return Enumerable.Empty<Cosmetic>();
            }

            _logger.LogInformation("Total de entries na loja: {Count}", response.Data.Entries.Count);

            // Dictionary para evitar duplicatas (mesmo cosmético pode aparecer em múltiplas entries)
            var uniqueCosmetics = new Dictionary<string, Cosmetic>();

            foreach (var entry in response.Data.Entries.Where(e => e.BrItems != null && e.BrItems.Any()))
            {
                foreach (var dto in entry.BrItems!.Where(dto => dto.Type != null && dto.Rarity != null))
                {
                    // Adiciona apenas se ainda não existir (evita duplicatas)
                    if (!uniqueCosmetics.ContainsKey(dto.Id))
                    {
                        var cosmetic = MapToCosmetic(dto);
                        // Marca como disponível na loja
                        cosmetic.IsAvailable = true;
                        // Atualiza o preço com o valor real da loja, se disponível
                        cosmetic.Price = entry.FinalPrice > 0 ? entry.FinalPrice : cosmetic.Price;
                        
                        uniqueCosmetics.Add(dto.Id, cosmetic);
                    }
                }
            }

            _logger.LogInformation("{Count} cosméticos únicos encontrados na loja", uniqueCosmetics.Count);
            return uniqueCosmetics.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos da loja");
            throw;
        }
    }

    public async Task<Cosmetic?> GetCosmeticByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Buscando cosmético {CosmeticId} da API do Fortnite", id);
            
            var url = $"{_baseUrl}/cosmetics/br/{id}";
            var response = await _httpClientService.GetAsync<FortniteApiResponse<CosmeticDto>>(url);

            if (response?.Data == null || response.Data.Type == null || response.Data.Rarity == null)
            {
                _logger.LogWarning("Cosmético {CosmeticId} não encontrado na API", id);
                return null;
            }

            return MapToCosmetic(response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosmético {CosmeticId}", id);
            throw;
        }
    }

    private static Cosmetic MapToCosmetic(CosmeticDto dto)
    {
        return new Cosmetic
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Type = new CosmeticType
            {
                Value = dto.Type?.Value ?? string.Empty,
                DisplayValue = dto.Type?.DisplayValue ?? string.Empty
            },
            Rarity = new CosmeticRarity
            {
                Value = dto.Rarity?.Value ?? string.Empty,
                DisplayValue = dto.Rarity?.DisplayValue ?? string.Empty
            },
            Series = dto.Series != null ? new CosmeticSeries
            {
                Value = dto.Series.Value ?? string.Empty,
                Image = dto.Series.Image ?? string.Empty
            } : null,
            Images = dto.Images != null ? new CosmeticImages
            {
                SmallIcon = dto.Images.SmallIcon,
                Icon = dto.Images.Icon,
                Featured = dto.Images.Featured
            } : null,
            Added = dto.Added,
            Price = GetCosmeticPrice(dto),
            IsAvailable = dto.ShopHistory?.Dates?.Any() ?? false
        };
    }

    private static int GetCosmeticPrice(CosmeticDto dto)
    {
        return dto.Rarity?.Value?.ToLower() switch
        {
            "common" => 800,
            "uncommon" => 800,
            "rare" => 1200,
            "epic" => 1500,
            "legendary" => 2000,
            "marvel" => 1500,
            "dc" => 1500,
            "icon" => 1500,
            "starwars" => 1500,
            _ => 1200
        };
    }
}