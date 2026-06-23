using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;

namespace WebSaklaso.Jobs
{
    public class OrderStatusSyncService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrderStatusSyncService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timeoutDays = configuration.GetValue<int>("Jobs:OrderStatusSyncJob:ProcessingStatusTimeoutDays");
            var syncInterval = configuration.GetValue<int>("Jobs:OrderStatusSyncJob:ServiceIntervalHours");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var cutoff = DateTime.UtcNow.AddDays(-timeoutDays);

                    var orders = await context.Orders
                        .Where(o => o.Status == "Processing" && o.OrderDate < cutoff)
                        .ToListAsync(stoppingToken);
                    if(orders.Count > 0)
                    {
                        foreach (var order in orders)
                        {
                            order.Status = "Shipped";
                        }

                        await context.SaveChangesAsync(stoppingToken);

                        if (orders.Count > 0)
                            logger.LogInformation("Moved {Count} orders from Processing to Shipped", orders.Count);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during 'Processing' status cleanup");
                }

                await Task.Delay(TimeSpan.FromHours(syncInterval), stoppingToken);
            }
        }
    }
}
