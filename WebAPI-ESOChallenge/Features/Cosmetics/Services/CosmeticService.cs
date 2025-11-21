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
            
            var response = await _httpClientService.GetAsync<FortniteApiResponse<AllCosmeticsData>>(url);

            if (response?.Data == null)
            {
                _logger.LogWarning("Nenhum cosmético encontrado na API");
                return Enumerable.Empty<CosmeticResponseDto>();
            }

            var allCosmetics = new List<Cosmetic>();
            var data = response.Data;
            
            // Categoria BR (Battle Royale)
            if (data.Br != null)
            {
                var brCosmetics = data.Br
                    .Where(dto => dto.Type != null && dto.Rarity != null)
                    .Select(MapToCosmetic);
                allCosmetics.AddRange(brCosmetics);
                _logger.LogDebug("Categoria 'br': {Count} cosméticos", data.Br.Count);
            }
            
            // Categoria Tracks (Músicas)
            if (data.Tracks != null)
            {
                var tracks = data.Tracks.Select(MapTrackToCosmetic);
                allCosmetics.AddRange(tracks);
                _logger.LogDebug("Categoria 'tracks': {Count} cosméticos", data.Tracks.Count);
            }
            
            // Categoria Instruments (Instrumentos)
            if (data.Instruments != null)
            {
                var instruments = data.Instruments.Select(MapInstrumentToCosmetic);
                allCosmetics.AddRange(instruments);
                _logger.LogDebug("Categoria 'instruments': {Count} cosméticos", data.Instruments.Count);
            }
            
            // Categoria Cars (Veículos)
            if (data.Cars != null)
            {
                var cars = data.Cars.Select(MapCarToCosmetic);
                allCosmetics.AddRange(cars);
                _logger.LogDebug("Categoria 'cars': {Count} cosméticos", data.Cars.Count);
            }
            
            // Categoria Lego
            if (data.Lego != null)
            {
                var legos = data.Lego.Select(MapLegoToCosmetic);
                allCosmetics.AddRange(legos);
                _logger.LogDebug("Categoria 'lego': {Count} cosméticos", data.Lego.Count);
            }
            
            // Categoria LegoKits
            if (data.LegoKits != null)
            {
                var legoKits = data.LegoKits.Select(MapLegoKitToCosmetic);
                allCosmetics.AddRange(legoKits);
                _logger.LogDebug("Categoria 'legoKits': {Count} cosméticos", data.LegoKits.Count);
            }
            
            // Categoria Beans (Fall Guys)
            if (data.Beans != null)
            {
                var beans = data.Beans.Select(MapBeanToCosmetic);
                allCosmetics.AddRange(beans);
                _logger.LogDebug("Categoria 'beans': {Count} cosméticos", data.Beans.Count);
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
    /// Busca os cosméticos novos/recentes de TODAS as categorias
    /// Atende requisito: /cosmetics/new - Listagem de todos os cosméticos que são novos
    /// Retorna DTOs prontos para serem consumidos pela API
    /// </summary>
    public async Task<IEnumerable<CosmeticResponseDto>> GetNewCosmeticsAsync()
    {
        var url = $"{_baseUrl}/cosmetics/new";
        try
        {
            _logger.LogInformation("Buscando cosméticos novos da API do Fortnite (todas as categorias)");
            var response = await _httpClientService.GetAsync<FortniteApiResponse<NewCosmeticsData>>(url);

            if (response?.Data?.Items == null)
            {
                _logger.LogWarning("Nenhum cosmético novo encontrado");
                return Enumerable.Empty<CosmeticResponseDto>();
            }

            var allNewCosmetics = new List<Cosmetic>();
            var items = response.Data.Items;
            
            // Categoria BR (Battle Royale)
            if (items.Br != null && items.Br.Any())
            {
                var brCosmetics = items.Br
                    .Where(dto => dto.Type != null && dto.Rarity != null)
                    .Select(MapToCosmetic);
                allNewCosmetics.AddRange(brCosmetics);
                _logger.LogDebug("Novos na categoria 'br': {Count} cosméticos", items.Br.Count);
            }
            
            // Categoria Tracks (Músicas)
            if (items.Tracks != null && items.Tracks.Any())
            {
                var tracks = items.Tracks.Select(MapTrackToCosmetic);
                allNewCosmetics.AddRange(tracks);
                _logger.LogDebug("Novos na categoria 'tracks': {Count} cosméticos", items.Tracks.Count);
            }
            
            // Categoria Instruments (Instrumentos)
            if (items.Instruments != null && items.Instruments.Any())
            {
                var instruments = items.Instruments.Select(MapInstrumentToCosmetic);
                allNewCosmetics.AddRange(instruments);
                _logger.LogDebug("Novos na categoria 'instruments': {Count} cosméticos", items.Instruments.Count);
            }
            
            // Categoria Cars (Veículos)
            if (items.Cars != null && items.Cars.Any())
            {
                var cars = items.Cars.Select(MapCarToCosmetic);
                allNewCosmetics.AddRange(cars);
                _logger.LogDebug("Novos na categoria 'cars': {Count} cosméticos", items.Cars.Count);
            }
            
            // Categoria Lego
            if (items.Lego != null && items.Lego.Any())
            {
                var legos = items.Lego.Select(MapLegoToCosmetic);
                allNewCosmetics.AddRange(legos);
                _logger.LogDebug("Novos na categoria 'lego': {Count} cosméticos", items.Lego.Count);
            }
            
            // Categoria LegoKits
            if (items.LegoKits != null && items.LegoKits.Any())
            {
                var legoKits = items.LegoKits.Select(MapLegoKitToCosmetic);
                allNewCosmetics.AddRange(legoKits);
                _logger.LogDebug("Novos na categoria 'legoKits': {Count} cosméticos", items.LegoKits.Count);
            }
            
            // Categoria Beans (Fall Guys)
            if (items.Beans != null && items.Beans.Any())
            {
                var beans = items.Beans.Select(MapBeanToCosmetic);
                allNewCosmetics.AddRange(beans);
                _logger.LogDebug("Novos na categoria 'beans': {Count} cosméticos", items.Beans.Count);
            }
            
            _logger.LogInformation("{Count} cosméticos novos encontrados em todas as categorias", allNewCosmetics.Count);
            
            // Mapear para DTOs de resposta
            var responseDtos = new List<CosmeticResponseDto>();
            foreach (var cosmetic in allNewCosmetics)
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
            
            var shopCosmetics = await GetShopCosmeticsInternalAsync();

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
            var shopCosmetics = await GetShopCosmeticsInternalAsync();
            return shopCosmetics.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos da loja para persistência");
            throw;
        }
    }

    /// <summary>
    /// Busca cosméticos novos retornando entidades de domínio para persistência
    /// Uso: Sincronização de novos itens no banco de dados
    /// </summary>
    public async Task<IEnumerable<Cosmetic>> GetNewCosmeticsForPersistenceAsync()
    {
        var url = $"{_baseUrl}/cosmetics/new";
        try
        {
            _logger.LogInformation("Buscando cosméticos novos para persistência");
            var response = await _httpClientService.GetAsync<FortniteApiResponse<NewCosmeticsData>>(url);

            if (response?.Data?.Items == null)
            {
                _logger.LogWarning("Nenhum cosmético novo encontrado para persistência");
                return Enumerable.Empty<Cosmetic>();
            }

            var allNewCosmetics = new List<Cosmetic>();
            var items = response.Data.Items;
            
            // Processar todas as categorias
            if (items.Br != null && items.Br.Any())
            {
                allNewCosmetics.AddRange(items.Br
                    .Where(dto => dto.Type != null && dto.Rarity != null)
                    .Select(MapToCosmetic));
            }
            
            if (items.Tracks != null && items.Tracks.Any())
            {
                allNewCosmetics.AddRange(items.Tracks.Select(MapTrackToCosmetic));
            }
            
            if (items.Instruments != null && items.Instruments.Any())
            {
                allNewCosmetics.AddRange(items.Instruments.Select(MapInstrumentToCosmetic));
            }
            
            if (items.Cars != null && items.Cars.Any())
            {
                allNewCosmetics.AddRange(items.Cars.Select(MapCarToCosmetic));
            }
            
            if (items.Lego != null && items.Lego.Any())
            {
                allNewCosmetics.AddRange(items.Lego.Select(MapLegoToCosmetic));
            }
            
            if (items.LegoKits != null && items.LegoKits.Any())
            {
                allNewCosmetics.AddRange(items.LegoKits.Select(MapLegoKitToCosmetic));
            }
            
            if (items.Beans != null && items.Beans.Any())
            {
                allNewCosmetics.AddRange(items.Beans.Select(MapBeanToCosmetic));
            }

            // Marcar todos os itens como novos
            foreach (var cosmetic in allNewCosmetics)
            {
                cosmetic.IsNew = true;
            }

            _logger.LogInformation("{Count} cosméticos novos prontos para persistência", allNewCosmetics.Count);
            return allNewCosmetics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos novos para persistência");
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
    /// Lógica central para buscar e processar cosméticos da loja.
    /// Unifica a busca para evitar duplicação de código entre os métodos públicos.
    /// IMPORTANTE: Antes de processar a nova loja, reseta IsInShop de TODOS os itens no banco
    /// para garantir que apenas itens atualmente na loja estejam disponíveis para compra.
    /// </summary>
    private async Task<Dictionary<string, Cosmetic>> GetShopCosmeticsInternalAsync()
    {
        var shopUrl = $"{_baseUrl}/shop";
        var shopResponse = await _httpClientService.GetAsync<FortniteApiResponse<ShopData>>(shopUrl);

        if (shopResponse?.Data?.Entries == null || !shopResponse.Data.Entries.Any())
        {
            _logger.LogWarning("Nenhum dado da loja encontrado na API");
            return new Dictionary<string, Cosmetic>();
        }

        _logger.LogDebug("Total de entries na loja: {Count}", shopResponse.Data.Entries.Count);
        
        // IMPORTANTE: Resetar IsInShop de todos os cosméticos no banco antes de processar a nova loja
        // Isso garante que apenas itens retornados pela API atual estarão disponíveis
        await ResetShopAvailabilityAsync();
        
        var shopCosmetics = new Dictionary<string, Cosmetic>();

        foreach (var entry in shopResponse.Data.Entries)
        {
            ProcessShopEntry(entry, shopCosmetics);
        }

        return shopCosmetics;
    }

    /// <summary>
    /// Reseta a flag IsInShop de todos os cosméticos no banco de dados.
    /// Chamado antes de processar a nova loja para garantir que apenas itens atuais estejam disponíveis.
    /// </summary>
    private async Task ResetShopAvailabilityAsync()
    {
        try
        {
            var itemsToUpdate = await _context.Cosmetics
                .Where(c => c.IsInShop)
                .ToListAsync();

            if (itemsToUpdate.Any())
            {
                _logger.LogInformation("Resetando IsInShop de {Count} itens antes de processar nova loja", itemsToUpdate.Count);
                
                foreach (var item in itemsToUpdate)
                {
                    item.IsInShop = false;
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao resetar disponibilidade da loja. Continuando com processamento...");
            // Não lançar exceção - permite continuar o processamento mesmo se o reset falhar
        }
    }

    /// <summary>
    /// Processa uma entry da loja, identificando se é bundle ou item individual.
    /// Nova lógica simplificada baseada na estrutura real da API.
    /// </summary>
    private void ProcessShopEntry(ShopEntry entry, Dictionary<string, Cosmetic> shopCosmetics)
    {
        var containedItems = new List<Cosmetic>();

        // 1. Coletar todos os itens de todas as categorias dentro da entry
        if (entry.BrItems != null)
        {
            foreach (var dto in entry.BrItems.Where(dto => dto.Type != null && dto.Rarity != null))
            {
                containedItems.Add(MapToCosmetic(dto));
            }
        }
        
        if (entry.Tracks != null)
        {
            foreach (var track in entry.Tracks)
            {
                containedItems.Add(MapTrackToCosmetic(track));
            }
        }
        
        if (entry.Cars != null)
        {
            foreach (var car in entry.Cars)
            {
                containedItems.Add(MapCarToCosmetic(car));
            }
        }
        
        if (entry.Instruments != null)
        {
            foreach (var instrument in entry.Instruments)
            {
                containedItems.Add(MapInstrumentToCosmetic(instrument));
            }
        }
        
        if (entry.LegoKits != null)
        {
            foreach (var legoKit in entry.LegoKits)
            {
                containedItems.Add(MapLegoKitToCosmetic(legoKit));
            }
        }

        // Filtrar itens inválidos que podem vir da API
        containedItems = containedItems.Where(c => c.Type != null && c.Rarity != null).ToList();
        
        if (!containedItems.Any())
        {
            _logger.LogWarning("Entry sem itens válidos: OfferId={OfferId}, DevName={DevName}", entry.OfferId, entry.DevName);
            return;
        }

        // 2. Decidir se é um bundle ou um item individual
        bool isBundle = entry.Bundle != null || containedItems.Count > 1;

        if (isBundle)
        {
            // É um bundle - processar como tal
            var containedItemIds = containedItems.Select(c => c.Id).ToList();
            ProcessBundle(entry, containedItems, containedItemIds, shopCosmetics);
        }
        else
        {
            // É um item individual
            ProcessIndividualItem(entry, containedItems[0], shopCosmetics);
        }
    }

    /// <summary>
    /// Processa um bundle da loja
    /// </summary>
    private void ProcessBundle(
        ShopEntry entry, 
        List<Cosmetic> containedItems, 
        List<string> containedItemIds, 
        Dictionary<string, Cosmetic> shopCosmetics)
    {
        // Gerar ID único para o bundle
        var bundleId = entry.Bundle?.Name != null 
            ? $"BUNDLE_{entry.Bundle.Name.Replace(" ", "").Replace("'", "")}" 
            : $"BUNDLE_{entry.OfferId}";

        if (shopCosmetics.ContainsKey(bundleId))
        {
            return;
        }

        // Adicionar os itens contidos no bundle à lista principal (se ainda não estiverem)
        // IMPORTANTE: Os itens do bundle NÃO devem ter IsInShop = true para não aparecerem individualmente na loja
        foreach (var item in containedItems.Where(item => !shopCosmetics.ContainsKey(item.Id)))
        {
            item.IsInShop = false; // Itens de bundle não aparecem individualmente na loja
            item.Price = 0; // Sem preço individual
            shopCosmetics.Add(item.Id, item);
        }

        // Criar e adicionar o cosmético do bundle
        var mainItem = containedItems[0];
        var bundleCosmetic = CreateBundleCosmetic(
            bundleId,
            entry.Bundle?.Name ?? $"Bundle {mainItem.Name}",
            entry.Bundle?.Info ?? $"Pacote contendo {containedItems.Count} itens",
            entry.FinalPrice,
            entry.Bundle?.Image ?? mainItem.Images?.Featured,
            mainItem.Images?.Icon,
            containedItemIds,
            entry.Bundle?.Image
        );
        
        shopCosmetics.Add(bundleId, bundleCosmetic);
        
        _logger.LogDebug(
            "Bundle processado: {BundleId} com {Count} itens (BrItems: {BrCount}, Tracks: {TrackCount}, Cars: {CarCount}, Instruments: {InstrumentCount}, LegoKits: {LegoCount})", 
            bundleId, 
            containedItemIds.Count,
            entry.BrItems?.Count ?? 0,
            entry.Tracks?.Count ?? 0,
            entry.Cars?.Count ?? 0,
            entry.Instruments?.Count ?? 0,
            entry.LegoKits?.Count ?? 0
        );
    }

    /// <summary>
    /// Processa um item individual da loja
    /// </summary>
    private static void ProcessIndividualItem(
        ShopEntry entry, 
        Cosmetic cosmetic, 
        Dictionary<string, Cosmetic> shopCosmetics)
    {
        if (shopCosmetics.ContainsKey(cosmetic.Id))
        {
            return;
        }

        cosmetic.IsInShop = true;
        cosmetic.Price = entry.FinalPrice > 0 ? entry.FinalPrice : cosmetic.Price;
        shopCosmetics.Add(cosmetic.Id, cosmetic);
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
            IsInShop = true,
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
            IsInShop = dto.ShopHistory?.Dates?.Any() ?? false
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
            Added = track.Added != default ? track.Added : DateTime.UtcNow,
            Price = 0,
            IsInShop = true
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
            Type = car.Type != null ? new CosmeticType
            {
                Value = car.Type.Value,
                DisplayValue = car.Type.DisplayValue
            } : new CosmeticType { Value = "car", DisplayValue = "Veículo" },
            Rarity = car.Rarity != null ? new CosmeticRarity
            {
                Value = car.Rarity.Value,
                DisplayValue = car.Rarity.DisplayValue
            } : new CosmeticRarity { Value = "rare", DisplayValue = "Raro" },
            Images = car.Images != null ? new CosmeticImages
            {
                SmallIcon = car.Images.Small,
                Icon = car.Images.Icon ?? car.Images.Large,
                Featured = car.Images.Featured ?? car.Images.Large
            } : null,
            Added = car.Added != default ? car.Added : DateTime.UtcNow,
            Price = 0,
            IsInShop = true
        };
    }

    /// <summary>
    /// Mapeia InstrumentDto (instrumento) para modelo de domínio Cosmetic
    /// </summary>
    private static Cosmetic MapInstrumentToCosmetic(InstrumentDto instrument)
    {
        return new Cosmetic
        {
            Id = instrument.Id,
            Name = instrument.Name ?? "Unknown Instrument",
            Description = instrument.Description ?? string.Empty,
            Type = instrument.Type != null ? new CosmeticType
            {
                Value = instrument.Type.Value,
                DisplayValue = instrument.Type.DisplayValue
            } : new CosmeticType { Value = "instrument", DisplayValue = "Instrumento" },
            Rarity = instrument.Rarity != null ? new CosmeticRarity
            {
                Value = instrument.Rarity.Value,
                DisplayValue = instrument.Rarity.DisplayValue
            } : new CosmeticRarity { Value = "uncommon", DisplayValue = "Incomum" },
            Images = instrument.Images != null ? new CosmeticImages
            {
                SmallIcon = instrument.Images.Small,
                Icon = instrument.Images.Large,
                Featured = instrument.Images.Large
            } : null,
            Added = instrument.Added != default ? instrument.Added : DateTime.UtcNow,
            Price = 0,
            IsInShop = true
        };
    }

    /// <summary>
    /// Mapeia LegoKitDto (Kit LEGO) para modelo de domínio Cosmetic
    /// </summary>
    private static Cosmetic MapLegoKitToCosmetic(LegoKitDto legoKit)
    {
        return new Cosmetic
        {
            Id = legoKit.Id,
            Name = legoKit.Name ?? "Unknown LEGO Kit",
            Description = string.Empty,
            Type = legoKit.Type != null ? new CosmeticType
            {
                Value = legoKit.Type.Value,
                DisplayValue = legoKit.Type.DisplayValue
            } : new CosmeticType { Value = "legokit", DisplayValue = "Kit LEGO" },
            Rarity = new CosmeticRarity
            {
                Value = "lego",
                DisplayValue = "LEGO"
            },
            Images = legoKit.Images != null ? new CosmeticImages
            {
                SmallIcon = legoKit.Images.Small,
                Icon = legoKit.Images.Large,
                Featured = legoKit.Images.Wide ?? legoKit.Images.Large
            } : null,
            Added = legoKit.Added != default ? legoKit.Added : DateTime.UtcNow,
            Price = 0,
            IsInShop = true
        };
    }

    /// <summary>
    /// Mapeia LegoDto (LEGO cosmético) para modelo de domínio Cosmetic
    /// </summary>
    private static Cosmetic MapLegoToCosmetic(LegoDto lego)
    {
        return new Cosmetic
        {
            Id = lego.Id,
            Name = lego.CosmeticId ?? lego.Id,
            Description = string.Empty,
            Type = new CosmeticType
            {
                Value = "lego",
                DisplayValue = "LEGO"
            },
            Rarity = new CosmeticRarity
            {
                Value = "lego",
                DisplayValue = "LEGO"
            },
            Images = lego.Images != null ? new CosmeticImages
            {
                SmallIcon = lego.Images.Small,
                Icon = lego.Images.Large,
                Featured = lego.Images.Wide ?? lego.Images.Large
            } : null,
            Added = lego.Added != default ? lego.Added : DateTime.UtcNow,
            Price = 0,
            IsInShop = false
        };
    }

    /// <summary>
    /// Mapeia BeanDto (Fall Guys) para modelo de domínio Cosmetic
    /// </summary>
    private static Cosmetic MapBeanToCosmetic(BeanDto bean)
    {
        return new Cosmetic
        {
            Id = bean.Id,
            Name = bean.Name ?? "Unknown Bean",
            Description = bean.Gender ?? string.Empty,
            Type = new CosmeticType
            {
                Value = "bean",
                DisplayValue = "Bean (Fall Guys)"
            },
            Rarity = new CosmeticRarity
            {
                Value = "uncommon",
                DisplayValue = "Incomum"
            },
            Images = bean.Images != null ? new CosmeticImages
            {
                SmallIcon = bean.Images.Small,
                Icon = bean.Images.Large,
                Featured = bean.Images.Large
            } : null,
            Added = bean.Added != default ? bean.Added : DateTime.UtcNow,
            Price = 0,
            IsInShop = false
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
    /// Retorna todos os cosméticos como entidades de domínio para persistência
    /// Usado pelo FortniteDataSyncService para sincronizar itens de bundles
    /// </summary>
    public async Task<IEnumerable<Cosmetic>> GetAllCosmeticsForPersistenceAsync()
    {
        return await GetAllCosmeticsInternalAsync();
    }

    /// <summary>
    /// Versão interna que retorna entidades de domínio (usado internamente para processamento de bundles)
    /// Clean Code: Separação entre métodos públicos (que retornam DTOs) e privados (que trabalham com entidades)
    /// </summary>
    private async Task<IEnumerable<Cosmetic>> GetAllCosmeticsInternalAsync()
    {
        var url = $"{_baseUrl}/cosmetics";
        var response = await _httpClientService.GetAsync<FortniteApiResponse<AllCosmeticsData>>(url);

        if (response?.Data == null)
        {
            return Enumerable.Empty<Cosmetic>();
        }

        var allCosmetics = new List<Cosmetic>();
        var data = response.Data;
        
        // Processar todas as categorias
        if (data.Br != null)
        {
            var brCosmetics = data.Br
                .Where(dto => dto.Type != null && dto.Rarity != null)
                .Select(MapToCosmetic);
            allCosmetics.AddRange(brCosmetics);
        }
        
        if (data.Tracks != null)
        {
            var tracks = data.Tracks.Select(MapTrackToCosmetic);
            allCosmetics.AddRange(tracks);
        }
        
        if (data.Instruments != null)
        {
            var instruments = data.Instruments.Select(MapInstrumentToCosmetic);
            allCosmetics.AddRange(instruments);
        }
        
        if (data.Cars != null)
        {
            var cars = data.Cars.Select(MapCarToCosmetic);
            allCosmetics.AddRange(cars);
        }
        
        if (data.Lego != null)
        {
            var legos = data.Lego.Select(MapLegoToCosmetic);
            allCosmetics.AddRange(legos);
        }
        
        if (data.LegoKits != null)
        {
            var legoKits = data.LegoKits.Select(MapLegoKitToCosmetic);
            allCosmetics.AddRange(legoKits);
        }
        
        if (data.Beans != null)
        {
            var beans = data.Beans.Select(MapBeanToCosmetic);
            allCosmetics.AddRange(beans);
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
            IsInShop = cosmetic.IsInShop,
            IsNew = cosmetic.IsNew,
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

    #region Busca Avançada com Filtros e Paginação

    /// <summary>
    /// Busca cosméticos com filtros avançados, paginação e ordenação
    /// Atende requisito: Busca por nome, tipo, raridade, data, disponibilidade e promoção
    /// </summary>
    public async Task<PaginatedCosmeticsResponse> SearchCosmeticsAsync(CosmeticFilterRequest filters)
    {
        try
        {
            _logger.LogInformation("Buscando cosméticos com filtros - Página: {Page}, Busca: {Search}", 
                filters.Page, filters.SearchTerm ?? "nenhuma");

            var query = _context.Cosmetics.AsQueryable();
            
            // Busca textual (nome ou descrição)
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var searchLower = filters.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(searchLower) || 
                    c.Description.ToLower().Contains(searchLower));
            }
            
            // Filtros por tipo (OR - qualquer um dos tipos selecionados)
            if (filters.Types?.Any() == true)
            {
                var typesLower = filters.Types.Select(t => t.ToLower()).ToList();
                query = query.Where(c => c.Type != null && typesLower.Contains(c.Type.Value.ToLower()));
            }
            
            // Filtros por raridade (OR - qualquer uma das raridades selecionadas)
            if (filters.Rarities?.Any() == true)
            {
                var raritiesLower = filters.Rarities.Select(r => r.ToLower()).ToList();
                query = query.Where(c => c.Rarity != null && raritiesLower.Contains(c.Rarity.Value.ToLower()));
            }
            
            // Filtros de data (converter para UTC para compatibilidade com PostgreSQL)
            if (filters.AddedAfter.HasValue)
            {
                var addedAfterUtc = filters.AddedAfter.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(filters.AddedAfter.Value, DateTimeKind.Utc)
                    : filters.AddedAfter.Value.ToUniversalTime();
                query = query.Where(c => c.Added >= addedAfterUtc);
            }
            
            if (filters.AddedBefore.HasValue)
            {
                var addedBeforeUtc = filters.AddedBefore.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(filters.AddedBefore.Value, DateTimeKind.Utc)
                    : filters.AddedBefore.Value.ToUniversalTime();
                query = query.Where(c => c.Added <= addedBeforeUtc);
            }
            
            // Apenas novos (retornados pelo endpoint /cosmetics/new)
            if (filters.OnlyNew == true)
            {
                query = query.Where(c => c.IsNew);
            }
            
            // Apenas disponíveis para compra
            if (filters.OnlyAvailable == true)
            {
                query = query.Where(c => c.IsInShop);
            }
            
            // Apenas em promoção (preço > 0 e disponível)
            if (filters.OnlyInShop == true)
            {
                query = query.Where(c => c.Price > 0 && c.IsInShop);
            }
            
            // Excluir bundles ou gerenciar exibição de bundles
            if (filters.ExcludeBundles == true)
            {
                // Excluir bundles completamente
                query = query.Where(c => !c.IsBundle);
            }
            else
            {
                // IMPORTANTE: Quando bundles estão incluídos, devemos OCULTAR os itens individuais
                // que fazem parte de bundles para evitar duplicação (mesma regra da loja)
                // Como ContainedItemIds é [NotMapped], precisamos carregar em memória primeiro
                var bundleItems = await _context.Cosmetics
                    .Where(c => c.IsBundle && c.ContainedItemIdsJson != null && c.ContainedItemIdsJson != "[]")
                    .Select(c => c.ContainedItemIdsJson)
                    .ToListAsync();
                
                // Deserializar e coletar todos os IDs únicos
                var bundleItemIds = bundleItems
                    .SelectMany(json => System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>())
                    .Distinct()
                    .ToHashSet();
                
                // Incluir apenas: bundles OU itens que não estão em nenhum bundle
                if (bundleItemIds.Any())
                {
                    query = query.Where(c => c.IsBundle || !bundleItemIds.Contains(c.Id));
                }
            }
            
            // Filtros de preço (validar valores positivos)
            if (filters.MinPrice.HasValue && filters.MinPrice.Value >= 0)
            {
                query = query.Where(c => c.Price >= filters.MinPrice.Value);
            }
            
            if (filters.MaxPrice.HasValue && filters.MaxPrice.Value >= 0)
            {
                query = query.Where(c => c.Price <= filters.MaxPrice.Value);
            }
            
            // Total de itens antes da paginação
            var totalCount = await query.CountAsync();
            
            // IMPORTANTE: Calcular metadados de filtros ANTES da paginação, mas DEPOIS dos filtros aplicados
            // Isso garante que as contagens reflitam apenas os itens que atendem aos critérios atuais
            // Otimização: usar GroupBy diretamente no banco de dados em vez de carregar tudo em memória
            var availableTypes = await query
                .Where(c => c.Type != null)
                .GroupBy(c => c.Type!.Value)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
            
            var availableRarities = await query
                .Where(c => c.Rarity != null)
                .GroupBy(c => c.Rarity!.Value)
                .Select(g => new { Rarity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rarity, x => x.Count);
            
            var minPrice = await query.AnyAsync() ? await query.MinAsync(c => c.Price) : 0;
            var maxPrice = await query.AnyAsync() ? await query.MaxAsync(c => c.Price) : 0;
            
            // Ordenação
            query = filters.SortBy.ToLower() switch
            {
                "name" => filters.SortOrder == "asc" 
                    ? query.OrderBy(c => c.Name) 
                    : query.OrderByDescending(c => c.Name),
                "price" => filters.SortOrder == "asc" 
                    ? query.OrderBy(c => c.Price) 
                    : query.OrderByDescending(c => c.Price),
                "rarity" => filters.SortOrder == "asc" 
                    ? query.OrderBy(c => c.Rarity!.Value) 
                    : query.OrderByDescending(c => c.Rarity!.Value),
                _ => filters.SortOrder == "asc" 
                    ? query.OrderBy(c => c.Added) 
                    : query.OrderByDescending(c => c.Added)
            };
            
            // Paginação
            var cosmetics = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();
            
            // Mapear para DTOs
            var items = new List<CosmeticResponseDto>();
            foreach (var cosmetic in cosmetics)
            {
                items.Add(await MapToResponseDtoAsync(cosmetic));
            }
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);
            
            _logger.LogInformation("Busca concluída: {Total} itens encontrados, Página {Page}/{TotalPages}", 
                totalCount, filters.Page, totalPages);
            
            return new PaginatedCosmeticsResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = filters.Page > 1,
                HasNextPage = filters.Page < totalPages,
                AvailableTypes = availableTypes,
                AvailableRarities = availableRarities,
                MinPriceAvailable = minPrice,
                MaxPriceAvailable = maxPrice
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cosméticos com filtros");
            throw;
        }
    }

    #endregion
}


