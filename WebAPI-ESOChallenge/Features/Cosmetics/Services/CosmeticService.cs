using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WebAPI_ESOChallenge.Features.Cosmetics.Dtos;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;
using WebAPI_ESOChallenge.Services.Interfaces;
using WebAPI_ESOChallenge.Data;

namespace WebAPI_ESOChallenge.Features.Cosmetics.Services;

/// <summary>
/// Serviço responsável por gerenciar cosméticos do Fortnite
/// Implementa princípios SOLID e Clean Code
/// Responsabilidade: Toda lógica de negócio relacionada a cosméticos
/// </summary>
public class CosmeticService : ICosmeticService
{
    private readonly IHttpClientService _httpClientService;
    private readonly ILogger<CosmeticService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly string _baseUrl;

    public CosmeticService(
        IHttpClientService httpClientService, 
        ILogger<CosmeticService> logger,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _httpClientService = httpClientService;
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _baseUrl = _configuration["FortniteApi:BaseUrl"] ?? "https://fortnite-api.com/v2";
    }

    /// <summary>
    /// Busca TODOS os cosméticos disponíveis (não apenas Battle Royale)
    /// Atende requisito: /cosmetics - Listagem de todos os cosméticos do Fortnite
    /// Retorna DTOs prontos para serem consumidos pela API
    /// </summary>
    public async Task<IEnumerable<CosmeticResponseDto>> GetAllCosmeticsAsync()
    {
        try
        {
            var url = $"{_baseUrl}/cosmetics";
            _logger.LogInformation("Buscando todos os cosméticos do Fortnite (todas as categorias)");
            
            var response = await _httpClientService.GetAsync<FortniteApiResponse<Dictionary<string, List<CosmeticDto>>>>(url);

            if (response?.Data == null)
            {
                _logger.LogWarning("Nenhum cosmético encontrado na API");
                return Enumerable.Empty<CosmeticResponseDto>();
            }

            // Combinar TODAS as categorias: br, tracks, instruments, cars, lego, beans, etc.
            var allCosmetics = new List<Cosmetic>();
            
            foreach (var category in response.Data)
            {
                if (category.Value == null) continue;
                
                var categoryCosmetics = category.Value
                    .Where(dto => dto.Type != null && dto.Rarity != null)
                    .Select(MapToCosmetic);
                    
                allCosmetics.AddRange(categoryCosmetics);
                _logger.LogDebug("Categoria '{Category}': {Count} cosméticos", category.Key, category.Value.Count);
            }

            _logger.LogInformation("{Count} cosméticos encontrados em todas as categorias", allCosmetics.Count);
            
            // Mapear para DTOs de resposta
            var responseDtos = new List<CosmeticResponseDto>();
            foreach (var cosmetic in allCosmetics)
            {
                responseDtos.Add(await MapToResponseDtoAsync(cosmetic));
            }
            
            return responseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os cosméticos");
            throw;
        }
    }

    /// <summary>
    /// Busca os cosméticos novos/recentes
    /// Atende requisito: /cosmetics/new - Listagem de todos os cosméticos que são novos
    /// Retorna DTOs prontos para serem consumidos pela API
    /// </summary>
    public async Task<IEnumerable<CosmeticResponseDto>> GetNewCosmeticsAsync()
    {
        var url = $"{_baseUrl}/cosmetics/new";
        try
        {
            _logger.LogInformation("Buscando cosméticos novos da API do Fortnite");
            var response = await _httpClientService.GetAsync<FortniteApiResponse<CosmeticsListResponse>>(url);

            if (response?.Data?.Items?.Br == null || !response.Data.Items.Br.Any())
            {
                _logger.LogWarning("Nenhum cosmético novo encontrado");
                return Enumerable.Empty<CosmeticResponseDto>();
            }

            var cosmetics = response.Data.Items.Br
                .Where(dto => dto.Type != null && dto.Rarity != null)
                .Select(MapToCosmetic)
                .ToList();
            
            _logger.LogInformation("{Count} cosméticos novos encontrados", cosmetics.Count);
            
            // Mapear para DTOs de resposta
            var responseDtos = new List<CosmeticResponseDto>();
            foreach (var cosmetic in cosmetics)
            {
                responseDtos.Add(await MapToResponseDtoAsync(cosmetic));
            }
            
            return responseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos novos");
            throw;
        }
    }

    /// <summary>
    /// Busca cosméticos atualmente à venda na loja
    /// Atende requisito: /shop - Listagem de todos os cosméticos que estão atualmente à venda
    /// Implementa lógica robusta para identificar e processar bundles
    /// Retorna DTOs prontos para serem consumidos pela API
    /// </summary>
    public async Task<IEnumerable<CosmeticResponseDto>> GetShopCosmeticsAsync()
    {
        try
        {
            _logger.LogInformation("Buscando cosméticos da loja do Fortnite");
            
            var shopUrl = $"{_baseUrl}/shop";
            var shopResponse = await _httpClientService.GetAsync<FortniteApiResponse<ShopData>>(shopUrl);

            if (shopResponse?.Data?.Entries == null)
            {
                _logger.LogWarning("Nenhum dado da loja encontrado na API");
                return Enumerable.Empty<CosmeticResponseDto>();
            }

            _logger.LogDebug("Total de entries na loja: {Count}", shopResponse.Data.Entries.Count);

            // Buscar todos os cosméticos para ter referência completa (necessário para resolver bundles)
            // Nota: Chama método interno que retorna entidades, não DTOs
            var allCosmeticsResponse = await GetAllCosmeticsInternalAsync();
            var allCosmetics = allCosmeticsResponse.ToDictionary(c => c.Id);

            var shopCosmetics = new Dictionary<string, Cosmetic>();

            foreach (var entry in shopResponse.Data.Entries)
            {
                ProcessShopEntry(entry, shopCosmetics, allCosmetics);
            }

            _logger.LogInformation("{Count} cosméticos únicos encontrados na loja (incluindo bundles)", shopCosmetics.Count);
            
            // Mapear para DTOs de resposta
            var responseDtos = new List<CosmeticResponseDto>();
            foreach (var cosmetic in shopCosmetics.Values)
            {
                responseDtos.Add(await MapToResponseDtoAsync(cosmetic));
            }
            
            return responseDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos da loja");
            throw;
        }
    }

    /// <summary>
    /// Busca cosméticos da loja retornando entidades de domínio para persistência
    /// Uso: AdminController e FortniteDataSyncService para salvar no banco de dados
    /// SOLID: Separação de preocupações - método específico para operações administrativas
    /// </summary>
    public async Task<IEnumerable<Cosmetic>> GetShopCosmeticsForPersistenceAsync()
    {
        try
        {
            var shopUrl = $"{_baseUrl}/shop";
            var shopResponse = await _httpClientService.GetAsync<FortniteApiResponse<ShopData>>(shopUrl);

            if (shopResponse?.Data?.Entries == null)
            {
                return Enumerable.Empty<Cosmetic>();
            }

            var allCosmeticsResponse = await GetAllCosmeticsInternalAsync();
            var allCosmetics = allCosmeticsResponse.ToDictionary(c => c.Id);

            var shopCosmetics = new Dictionary<string, Cosmetic>();

            foreach (var entry in shopResponse.Data.Entries)
            {
                ProcessShopEntry(entry, shopCosmetics, allCosmetics);
            }

            return shopCosmetics.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos da loja para persistência");
            throw;
        }
    }

    /// <summary>
    /// Busca um cosmético específico por ID
    /// Retorna DTO pronto para ser consumido pela API
    /// </summary>
    public async Task<CosmeticResponseDto?> GetCosmeticByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Buscando cosmético {CosmeticId}", id);
            
            var url = $"{_baseUrl}/cosmetics/br/{id}";
            var response = await _httpClientService.GetAsync<FortniteApiResponse<CosmeticDto>>(url);

            if (response?.Data == null || response.Data.Type == null || response.Data.Rarity == null)
            {
                _logger.LogWarning("Cosmético {CosmeticId} não encontrado", id);
                return null;
            }

            var cosmetic = MapToCosmetic(response.Data);
            return await MapToResponseDtoAsync(cosmetic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosmético {CosmeticId}", id);
            throw;
        }
    }

    #region Private Methods - Single Responsibility Principle

    /// <summary>
    /// Processa uma entry da loja, identificando se é bundle ou item individual
    /// Aplica Single Responsibility Principle
    /// </summary>
    private void ProcessShopEntry(
        ShopEntry entry, 
        Dictionary<string, Cosmetic> shopCosmetics, 
        Dictionary<string, Cosmetic> allCosmetics)
    {
        // CASO 1: Bundle com lista de itens explícita (brItems)
        if (IsExplicitBundle(entry))
        {
            ProcessExplicitBundle(entry, shopCosmetics);
            return;
        }

        // CASO 2: Bundle sem lista de itens (usa NewDisplayAsset para identificar)
        if (IsImplicitBundle(entry))
        {
            ProcessImplicitBundle(entry, shopCosmetics, allCosmetics);
            return;
        }

        // CASO 3: Itens individuais normais
        if (entry.BrItems != null)
        {
            ProcessIndividualItems(entry, shopCosmetics);
        }
    }

    /// <summary>
    /// Verifica se é um bundle com lista explícita de itens
    /// Clean Code: nome descritivo que expressa intenção
    /// </summary>
    private static bool IsExplicitBundle(ShopEntry entry) =>
        entry.Bundle != null && 
        entry.BrItems != null && 
        entry.BrItems.Count > 1;

    /// <summary>
    /// Verifica se é um bundle sem lista de itens (identificado por NewDisplayAsset)
    /// Clean Code: nome descritivo que expressa intenção
    /// </summary>
    private static bool IsImplicitBundle(ShopEntry entry) =>
        entry.Bundle != null && 
        (entry.BrItems == null || entry.BrItems.Count == 0) && 
        entry.NewDisplayAsset?.CosmeticId != null;

    /// <summary>
    /// Processa bundle com lista explícita de itens
    /// Exemplo: Bundle que contém multiple BrItems, Tracks e Cars
    /// </summary>
    private void ProcessExplicitBundle(ShopEntry entry, Dictionary<string, Cosmetic> shopCosmetics)
    {
        var bundleDto = entry.BrItems!.First();
        var bundleId = $"BUNDLE_{bundleDto.Id}";

        if (shopCosmetics.ContainsKey(bundleId)) return;

        // Primeiro, adicionar os itens individuais do bundle ao banco
        if (entry.BrItems != null)
        {
            foreach (var dto in entry.BrItems.Where(dto => dto.Type != null && dto.Rarity != null))
            {
                if (!shopCosmetics.ContainsKey(dto.Id))
                {
                    var cosmetic = MapToCosmetic(dto);
                    cosmetic.IsAvailable = true;
                    cosmetic.Price = 0; // Itens de bundle não têm preço individual na loja
                    shopCosmetics.Add(dto.Id, cosmetic);
                }
            }
        }

        // Adicionar Tracks (músicas) ao banco
        if (entry.Tracks != null)
        {
            foreach (var track in entry.Tracks)
            {
                if (!shopCosmetics.ContainsKey(track.Id))
                {
                    var cosmetic = MapTrackToCosmetic(track);
                    cosmetic.IsAvailable = true;
                    cosmetic.Price = 0;
                    shopCosmetics.Add(track.Id, cosmetic);
                }
            }
        }

        // Adicionar Cars (veículos) ao banco
        if (entry.Cars != null)
        {
            foreach (var car in entry.Cars)
            {
                if (!shopCosmetics.ContainsKey(car.Id))
                {
                    var cosmetic = MapCarToCosmetic(car);
                    cosmetic.IsAvailable = true;
                    cosmetic.Price = 0;
                    shopCosmetics.Add(car.Id, cosmetic);
                }
            }
        }

        // Coletar IDs de todos os tipos de itens do bundle
        var containedItemIds = new List<string>();
        
        // Itens BR (skins, pickaxes, etc)
        if (entry.BrItems != null)
        {
            containedItemIds.AddRange(entry.BrItems.Select(i => i.Id));
        }
        
        // Tracks (músicas)
        if (entry.Tracks != null)
        {
            containedItemIds.AddRange(entry.Tracks.Select(t => t.Id));
        }
        
        // Cars (veículos)
        if (entry.Cars != null)
        {
            containedItemIds.AddRange(entry.Cars.Select(c => c.Id));
        }

        var bundle = CreateBundleCosmetic(
            bundleId,
            entry.Bundle!.Name,
            entry.Bundle.Info,
            entry.FinalPrice,
            entry.Bundle.Image ?? bundleDto.Images?.Featured,
            bundleDto.Images?.Icon,
            containedItemIds,
            entry.Bundle.Image
        );

        shopCosmetics.Add(bundleId, bundle);
        _logger.LogDebug("Bundle explícito processado: {BundleId} com {Count} itens (BrItems: {BrCount}, Tracks: {TrackCount}, Cars: {CarCount})", 
            bundleId, bundle.ContainedItemIds.Count, 
            entry.BrItems?.Count ?? 0, 
            entry.Tracks?.Count ?? 0, 
            entry.Cars?.Count ?? 0);
    }

    /// <summary>
    /// Processa bundle sem lista explícita (extrai do devName)
    /// Exemplo: Bundle CHROMAKOPIAN que não tem BrItems mas tem devName descritivo
    /// </summary>
    private void ProcessImplicitBundle(
        ShopEntry entry, 
        Dictionary<string, Cosmetic> shopCosmetics,
        Dictionary<string, Cosmetic> allCosmetics)
    {
        var mainCosmeticId = entry.NewDisplayAsset!.CosmeticId!;
        var bundleId = $"BUNDLE_{mainCosmeticId}";

        if (shopCosmetics.ContainsKey(bundleId)) return;

        // Extrair IDs dos itens do devName (formato: "1 x Item1, 1 x Item2")
        var containedIds = ExtractItemIdsFromDevName(entry.DevName, allCosmetics);
        
        // Se não conseguiu extrair do devName, usar apenas o item principal
        if (containedIds.Count == 0 && allCosmetics.ContainsKey(mainCosmeticId))
        {
            containedIds.Add(mainCosmeticId);
        }

        if (containedIds.Count == 0)
        {
            _logger.LogWarning("Bundle implícito sem itens identificados: {BundleId}, DevName: {DevName}", bundleId, entry.DevName);
            return;
        }

        var mainCosmetic = allCosmetics.GetValueOrDefault(mainCosmeticId);
        var bundle = CreateBundleCosmetic(
            bundleId,
            entry.Bundle!.Name,
            entry.Bundle.Info,
            entry.FinalPrice,
            entry.Bundle.Image ?? mainCosmetic?.Images?.Featured,
            mainCosmetic?.Images?.Icon,
            containedIds,
            entry.Bundle.Image
        );

        shopCosmetics.Add(bundleId, bundle);
        _logger.LogDebug("Bundle implícito processado: {BundleId} com {Count} itens", bundleId, bundle.ContainedItemIds.Count);
    }

    /// <summary>
    /// Extrai IDs de cosméticos do campo devName
    /// Exemplo: "[VIRTUAL]1 x CHROMAKOPIA Tyler, 1 x Dynamite" -> ["CID_...", "CID_..."]
    /// Clean Code: método com responsabilidade única e bem definida
    /// </summary>
    private List<string> ExtractItemIdsFromDevName(string devName, Dictionary<string, Cosmetic> allCosmetics)
    {
        var itemIds = new List<string>();
        
        if (string.IsNullOrEmpty(devName)) return itemIds;

        // Remover prefixo [VIRTUAL] e split por vírgula
        var cleanDevName = devName.Replace("[VIRTUAL]", "").Trim();
        var parts = cleanDevName.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            // Extrair nome do item (após "x ")
            // Padrão: "1 x CHROMAKOPIA Tyler for -1 MtxCurrency"
            var itemNameMatch = Regex.Match(part, @"x\s+([^,]+?)(?:\s+for|$)");
            if (!itemNameMatch.Success) continue;

            var itemName = itemNameMatch.Groups[1].Value.Trim();
            
            // Buscar o cosmético pelo nome (case-insensitive)
            var matchingCosmetic = allCosmetics.Values
                .FirstOrDefault(c => c.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (matchingCosmetic != null)
            {
                itemIds.Add(matchingCosmetic.Id);
                _logger.LogDebug("Item '{ItemName}' identificado como ID: {ItemId}", itemName, matchingCosmetic.Id);
            }
            else
            {
                _logger.LogWarning("Item '{ItemName}' não encontrado no catálogo de cosméticos", itemName);
            }
        }

        return itemIds.Distinct().ToList();
    }

    /// <summary>
    /// Processa itens individuais da loja
    /// </summary>
    private void ProcessIndividualItems(ShopEntry entry, Dictionary<string, Cosmetic> shopCosmetics)
    {
        foreach (var dto in entry.BrItems!.Where(dto => dto.Type != null && dto.Rarity != null))
        {
            if (shopCosmetics.ContainsKey(dto.Id)) continue;

            var cosmetic = MapToCosmetic(dto);
            cosmetic.IsAvailable = true;
            cosmetic.Price = entry.FinalPrice > 0 ? entry.FinalPrice : cosmetic.Price;
            
            shopCosmetics.Add(dto.Id, cosmetic);
        }
    }

    /// <summary>
    /// Factory method para criar um cosmético bundle
    /// Aplica DRY (Don't Repeat Yourself) e Single Responsibility
    /// </summary>
    private static Cosmetic CreateBundleCosmetic(
        string id,
        string name,
        string description,
        int price,
        string? featuredImage,
        string? iconImage,
        List<string> containedItemIds,
        string? bundleImage = null)
    {
        return new Cosmetic
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            IsBundle = true,
            IsAvailable = true,
            Images = new CosmeticImages 
            { 
                Featured = featuredImage,
                Icon = iconImage
            },
            Rarity = new CosmeticRarity { Value = "legendary", DisplayValue = "Pacote" },
            Type = new CosmeticType { Value = "bundle", DisplayValue = "Pacote" },
            ContainedItemIds = containedItemIds,
            BundleInfo = bundleImage != null ? new BundleInfo
            {
                Name = name,
                Info = description,
                Image = bundleImage
            } : null
        };
    }

    /// <summary>
    /// Mapeia DTO da API para modelo de domínio
    /// Clean Code: método simples e focado em transformação de dados
    /// </summary>
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

    /// <summary>
    /// Mapeia TrackDto (música) para modelo de domínio Cosmetic
    /// </summary>
    private static Cosmetic MapTrackToCosmetic(TrackDto track)
    {
        return new Cosmetic
        {
            Id = track.Id,
            Name = track.Title ?? track.DevName ?? "Unknown Track",
            Description = track.Artist ?? string.Empty,
            Type = new CosmeticType
            {
                Value = "track",
                DisplayValue = "Música"
            },
            Rarity = new CosmeticRarity
            {
                Value = "uncommon",
                DisplayValue = "Incomum"
            },
            Images = track.AlbumArt != null ? new CosmeticImages
            {
                Icon = track.AlbumArt,
                Featured = track.AlbumArt
            } : null,
            Added = DateTime.UtcNow,
            Price = 0,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Mapeia CarDto (veículo) para modelo de domínio Cosmetic
    /// </summary>
    private static Cosmetic MapCarToCosmetic(CarDto car)
    {
        return new Cosmetic
        {
            Id = car.Id,
            Name = car.Name ?? "Unknown Car",
            Description = car.Description ?? string.Empty,
            Type = new CosmeticType
            {
                Value = "car",
                DisplayValue = "Veículo"
            },
            Rarity = new CosmeticRarity
            {
                Value = "rare",
                DisplayValue = "Raro"
            },
            Images = car.Images != null ? new CosmeticImages
            {
                SmallIcon = car.Images.SmallIcon,
                Icon = car.Images.Icon,
                Featured = car.Images.Featured
            } : null,
            Added = DateTime.UtcNow,
            Price = 0,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Calcula preço base de um cosmético baseado em sua raridade
    /// Clean Code: lógica clara com pattern matching
    /// </summary>
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

    /// <summary>
    /// Versão interna que retorna entidades de domínio (usado internamente para processamento de bundles)
    /// Clean Code: Separação entre métodos públicos (que retornam DTOs) e privados (que trabalham com entidades)
    /// </summary>
    private async Task<IEnumerable<Cosmetic>> GetAllCosmeticsInternalAsync()
    {
        var url = $"{_baseUrl}/cosmetics";
        var response = await _httpClientService.GetAsync<FortniteApiResponse<Dictionary<string, List<CosmeticDto>>>>(url);

        if (response?.Data == null)
        {
            return Enumerable.Empty<Cosmetic>();
        }

        var allCosmetics = new List<Cosmetic>();
        
        foreach (var category in response.Data)
        {
            if (category.Value == null) continue;
            
            var categoryCosmetics = category.Value
                .Where(dto => dto.Type != null && dto.Rarity != null)
                .Select(MapToCosmetic);
                
            allCosmetics.AddRange(categoryCosmetics);
        }

        return allCosmetics;
    }

    /// <summary>
    /// Mapeia entidade de domínio Cosmetic para DTO de resposta da API
    /// Responsável por buscar dados adicionais de bundles no banco de dados
    /// SOLID: Single Responsibility - focado apenas em transformação de dados e enriquecimento de bundles
    /// </summary>
    private async Task<CosmeticResponseDto> MapToResponseDtoAsync(Cosmetic cosmetic)
    {
        var dto = new CosmeticResponseDto
        {
            Id = cosmetic.Id,
            Name = cosmetic.Name,
            Description = cosmetic.Description,
            Type = cosmetic.Type != null ? new TypeDto 
            { 
                Value = cosmetic.Type.Value, 
                DisplayValue = cosmetic.Type.DisplayValue 
            } : null,
            Rarity = cosmetic.Rarity != null ? new RarityDto 
            { 
                Value = cosmetic.Rarity.Value, 
                DisplayValue = cosmetic.Rarity.DisplayValue 
            } : null,
            Series = cosmetic.Series != null ? new SeriesDto 
            { 
                Value = cosmetic.Series.Value, 
                Image = cosmetic.Series.Image 
            } : null,
            Images = cosmetic.Images != null ? new ImagesDto 
            { 
                SmallIcon = cosmetic.Images.SmallIcon,
                Icon = cosmetic.Images.Icon,
                Featured = cosmetic.Images.Featured
            } : null,
            Added = cosmetic.Added,
            Price = cosmetic.Price,
            IsAvailable = cosmetic.IsAvailable,
            IsBundle = cosmetic.IsBundle,
            ContainedItemIds = cosmetic.ContainedItemIds,
            Bundle = cosmetic.BundleInfo != null ? new BundleInfoDto
            {
                Name = cosmetic.BundleInfo.Name,
                Info = cosmetic.BundleInfo.Info,
                Image = cosmetic.BundleInfo.Image
            } : null
        };

        // Se for bundle, buscar imagens e dados dos itens filhos no banco de dados
        // Responsabilidade: Enriquecimento de dados de bundles com informações persistidas
        if (cosmetic.IsBundle && cosmetic.ContainedItemIds != null && cosmetic.ContainedItemIds.Any())
        {
            try
            {
                var childCosmetics = await _context.Cosmetics
                    .AsNoTracking()
                    .Where(c => cosmetic.ContainedItemIds.Contains(c.Id))
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Type,
                        c.Rarity,
                        Image = c.Images != null ? (c.Images.Icon ?? c.Images.Featured ?? c.Images.SmallIcon) : null
                    })
                    .ToListAsync();

                dto.ContainedItemsImages = childCosmetics
                    .Select(c => c.Image)
                    .Where(img => !string.IsNullOrEmpty(img))
                    .ToList()!;

                dto.ContainedItems = childCosmetics
                    .Select(c => new ContainedItemDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = c.Type != null ? new TypeDto
                        {
                            Value = c.Type.Value,
                            DisplayValue = c.Type.DisplayValue
                        } : null,
                        Rarity = c.Rarity != null ? new RarityDto
                        {
                            Value = c.Rarity.Value,
                            DisplayValue = c.Rarity.DisplayValue
                        } : null,
                        Image = c.Image
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar dados dos itens do bundle {BundleId}", cosmetic.Id);
                dto.ContainedItemsImages = new List<string>();
                dto.ContainedItems = new List<ContainedItemDto>();
            }
        }

        return dto;
    }

    #endregion
}

