using Microsoft.EntityFrameworkCore;
using WebAPI_ESOChallenge.Data;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Services;

/// <summary>
/// Background service que sincroniza dados da API do Fortnite com o banco de dados local.
/// Executa a cada 1 hora para manter os dados atualizados.
/// Sincroniza tanto os itens da loja quanto todos os cosméticos disponíveis.
/// Seguindo o princípio da Responsabilidade Única (SOLID).
/// </summary>
public class FortniteDataSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FortniteDataSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(1);

    public FortniteDataSyncService(
        IServiceProvider serviceProvider,
        ILogger<FortniteDataSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fortnite Data Sync Service iniciado");

        // Aguarda 5 segundos para garantir que o app está totalmente inicializado
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        // Executa imediatamente ao iniciar
        await SyncDataAsync(stoppingToken);

        // Depois continua executando no intervalo definido
        using var timer = new PeriodicTimer(_syncInterval);

        while (!stoppingToken.IsCancellationRequested && 
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncDataAsync(stoppingToken);
        }
    }

    private async Task SyncDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando sincronização completa do Fortnite (Loja + Todos os Cosméticos + Novos)...");

            using var scope = _serviceProvider.CreateScope();
            var cosmeticService = scope.ServiceProvider.GetRequiredService<ICosmeticService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // IMPORTANTE: Buscar itens novos ANTES de resetar flags
            // Isso garante que temos a lista atualizada da API antes de qualquer modificação
            _logger.LogInformation("Buscando itens novos da API...");
            var newCosmetics = await cosmeticService.GetNewCosmeticsForPersistenceAsync();
            var newCosmeticIds = newCosmetics?.Select(c => c.Id).ToHashSet() ?? new HashSet<string>();
            _logger.LogInformation("{Count} itens novos identificados pela API", newCosmeticIds.Count);

            // 1. Sincronizar TODOS os cosméticos (todas as categorias: br, tracks, cars, instruments, lego, legoKits, beans)
            _logger.LogInformation("Fase 1/3: Sincronizando todos os cosméticos...");
            var allCosmetics = await cosmeticService.GetAllCosmeticsForPersistenceAsync();
            
            if (allCosmetics == null || !allCosmetics.Any())
            {
                _logger.LogWarning("Nenhum cosmético encontrado na API");
            }
            else
            {
                var allCosmeticsList = allCosmetics.ToList();
                _logger.LogInformation("{Count} cosméticos encontrados (todas categorias)", allCosmeticsList.Count);

                // Marcar itens que estão na lista de novos ANTES de fazer upsert
                foreach (var cosmetic in allCosmeticsList)
                {
                    cosmetic.IsNew = newCosmeticIds.Contains(cosmetic.Id);
                }

                await UpsertCosmeticsAsync(dbContext, allCosmeticsList, cancellationToken, preserveIsNew: true);
            }

            // 2. Sincronizar itens da LOJA e atualizar flags IsInShop
            _logger.LogInformation("🛒 Fase 2/3: Sincronizando loja atual...");
            var shopCosmetics = await cosmeticService.GetShopCosmeticsForPersistenceAsync();
            
            if (shopCosmetics == null || !shopCosmetics.Any())
            {
                _logger.LogWarning("Nenhum cosmético encontrado na loja");
            }
            else
            {
                var shopList = shopCosmetics.ToList();
                _logger.LogInformation("{Count} itens na loja atual", shopList.Count);

                // Resetar IsInShop de todos os itens
                await dbContext.Cosmetics
                    .ExecuteUpdateAsync(c => c.SetProperty(x => x.IsInShop, false), cancellationToken);

                // Marcar itens da loja atual como IsInShop = true e atualizar preços
                await UpsertCosmeticsAsync(dbContext, shopList, cancellationToken, markAsInShop: true, preserveIsNew: true);
            }

            _logger.LogInformation("Sincronização completa finalizada - {Count} itens marcados como novos", newCosmeticIds.Count);

            _logger.LogInformation("Sincronização completa finalizada com sucesso!");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sincronização cancelada pelo sistema");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a sincronização de dados do Fortnite");
        }
    }

    /// <summary>
    /// Faz upsert (insert ou update) de uma lista de cosméticos no banco de dados
    /// </summary>
    private async Task UpsertCosmeticsAsync(
        ApplicationDbContext dbContext, 
        List<Cosmetic> cosmetics, 
        CancellationToken cancellationToken,
        bool markAsInShop = false,
        bool markAsNew = false,
        bool preserveIsNew = false)
    {
        var insertedCount = 0;
        var updatedCount = 0;
        
        // Buscar todas as entidades existentes de uma vez para otimizar
        var cosmeticIds = cosmetics.Select(c => c.Id).ToHashSet();
        var existingCosmetics = await dbContext.Cosmetics
            .Where(c => cosmeticIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        foreach (var cosmetic in cosmetics)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (!existingCosmetics.ContainsKey(cosmetic.Id))
            {
                // Inserir novo cosmético
                // Aplicar flags antes de adicionar
                if (markAsInShop)
                {
                    cosmetic.IsInShop = true;
                }

                // IMPORTANTE: Preservar IsNew do cosmético ou aplicar flag markAsNew
                if (markAsNew || cosmetic.IsNew)
                {
                    cosmetic.IsNew = true;
                }
                
                dbContext.Cosmetics.Add(cosmetic);
                insertedCount++;
            }
            else
            {
                // Atualizar cosmético existente
                var existing = existingCosmetics[cosmetic.Id];
                
                // Atualizar propriedades principais
                existing.Name = cosmetic.Name;
                existing.Description = cosmetic.Description;
                existing.Type = cosmetic.Type;
                existing.Rarity = cosmetic.Rarity;
                existing.Series = cosmetic.Series;
                existing.Images = cosmetic.Images;
                existing.Added = cosmetic.Added;
                existing.Price = cosmetic.Price;
                existing.IsBundle = cosmetic.IsBundle;
                existing.ContainedItemIds = cosmetic.ContainedItemIds;
                existing.BundleInfo = cosmetic.BundleInfo;
                
                // Atualizar flags condicionalmente
                if (markAsInShop)
                {
                    existing.IsInShop = true;
                }
                
                // IMPORTANTE: Gerenciar IsNew corretamente
                // Se preserveIsNew = true, copia o valor de cosmetic.IsNew (já foi marcado antes do upsert)
                // Se markAsNew = true, força IsNew = true
                // Caso contrário, mantém o valor existente no banco
                if (preserveIsNew)
                {
                    existing.IsNew = cosmetic.IsNew;
                }
                else if (markAsNew)
                {
                    existing.IsNew = true;
                }
                
                updatedCount++;
            }
        }

        // Salvar todas as alterações
        var saved = await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "💾 Upsert concluído: {Inserted} novos, {Updated} atualizados, {Total} registros salvos",
            insertedCount, updatedCount, saved);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fortnite Data Sync Service parando...");
        await base.StopAsync(cancellationToken);
    }
}
