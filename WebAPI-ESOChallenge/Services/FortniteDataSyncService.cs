using Microsoft.EntityFrameworkCore;
using WebAPI_ESOChallenge.Data;
using WebAPI_ESOChallenge.Features.Cosmetics.Interfaces;
using WebAPI_ESOChallenge.Features.Cosmetics.Models;

namespace WebAPI_ESOChallenge.Services;

/// <summary>
/// Background service que sincroniza dados da API do Fortnite com o banco de dados local.
/// Executa a cada 6 horas para manter os dados atualizados.
/// Seguindo o princípio da Responsabilidade Única (SOLID).
/// </summary>
public class FortniteDataSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FortniteDataSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(6);

    public FortniteDataSyncService(
        IServiceProvider serviceProvider,
        ILogger<FortniteDataSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Fortnite Data Sync Service iniciado");

        // Aguarda 5 segundos para garantir que o app está totalmente inicializado
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

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
            _logger.LogInformation("🔄 Iniciando sincronização de dados do Fortnite...");

            using var scope = _serviceProvider.CreateScope();
            var cosmeticService = scope.ServiceProvider.GetRequiredService<ICosmeticService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Buscar dados da loja atual (entidades para persistência)
            var shopCosmetics = await cosmeticService.GetShopCosmeticsForPersistenceAsync();
            
            if (shopCosmetics == null || !shopCosmetics.Any())
            {
                _logger.LogWarning("⚠️ Nenhum cosmético encontrado na loja");
                return;
            }

            var shopList = shopCosmetics.ToList();
            _logger.LogInformation("📦 {Count} cosméticos encontrados na loja", shopList.Count);

            // Nota: Removemos a busca de "new cosmetics" pois agora só trabalhamos com entidades 
            // para persistência no background service. Se necessário, pode-se adicionar outro 
            // método ForPersistence para novos cosméticos também.
            
            var allCosmetics = shopList;

            _logger.LogInformation("📊 Total de {Count} cosméticos únicos para sincronizar", allCosmetics.Count);

            // 2. Fazer upsert (insert ou update) no banco de dados
            var insertedCount = 0;
            var updatedCount = 0;

            foreach (var cosmetic in allCosmetics)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("⚠️ Sincronização cancelada");
                    return;
                }

                var existing = await dbContext.Cosmetics
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == cosmetic.Id, cancellationToken);

                if (existing == null)
                {
                    // Inserir novo cosmético
                    dbContext.Cosmetics.Add(cosmetic);
                    insertedCount++;
                }
                else
                {
                    // Atualizar cosmético existente
                    dbContext.Cosmetics.Update(cosmetic);
                    updatedCount++;
                }
            }

            // 5. Salvar todas as alterações
            var saved = await dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "✅ Sincronização concluída: {Inserted} novos, {Updated} atualizados, {Total} registros afetados",
                insertedCount, updatedCount, saved);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🛑 Sincronização cancelada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro durante a sincronização de dados do Fortnite");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Fortnite Data Sync Service parando...");
        await base.StopAsync(cancellationToken);
    }
}
