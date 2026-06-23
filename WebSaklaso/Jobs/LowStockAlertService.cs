using Microsoft.EntityFrameworkCore;
using WebSaklaso.Data;
using WebSaklaso.Entities;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Jobs
{
    //2. LowStockAlertService.cs
    //Your Product has a Quantity field.
    //When it hits zero(or a configured threshold), 
    //nobody currently gets notified.This service emails the Admin.
    public class LowStockAlertService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<LowStockAlertService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var alertAmount = configuration.GetValue<int>("Jobs:LowStockAlertJob:QuantityForAlert");

            var alertInterval = configuration.GetValue<int>("Jobs:LowStockAlertJob:ServiceIntervalHours");

            var adminEmail = configuration.GetValue<string>("Jobs:LowStockAlertJob:AdminEmail");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var productsByQuantity = await context.Products
                        .Where(p => p.Quantity <= alertAmount)
                        .ToListAsync(stoppingToken);

                    if (productsByQuantity.Count > 0)
                    {
                        var rows = "";

                        foreach (var product in productsByQuantity)
                        {
                            rows += $"""
                                <tr>
                                    <td style="padding:8px;border-bottom:1px solid #ddd;">
                                        {product.ProductName}
                                    </td>

                                    <td style="padding:8px;border-bottom:1px solid #ddd;">
                                        {product.Quantity}
                                    </td>
                                </tr>
                                """;
                        }

                        var emailBody = $"""
                            <!DOCTYPE html>
                            <html>
                            <body style="font-family:Arial,sans-serif;">

                            <div style="
                                max-width:600px;
                                margin:auto;
                                padding:20px;
                                border:1px solid #ddd;
                                border-radius:8px;">

                                <h2 style="color:#d9534f;">
                                    ⚠️ Low Stock Alert
                                </h2>


                                <p>
                                    The following products have reached the low stock threshold:
                                </p>


                                <table style="
                                    width:100%;
                                    border-collapse:collapse;">

                                    <thead>
                                        <tr>

                                            <th style="
                                                border-bottom:1px solid #ddd;
                                                text-align:left;
                                                padding:8px;">
                                                Product
                                            </th>


                                            <th style="
                                                border-bottom:1px solid #ddd;
                                                text-align:left;
                                                padding:8px;">
                                                Remaining Quantity
                                            </th>

                                        </tr>
                                    </thead>


                                    <tbody>

                                        {rows}

                                    </tbody>

                                </table>


                                <p style="margin-top:20px;">
                                    Please restock these products.
                                </p>


                                <hr>

                                <small>
                                    Automated inventory monitoring system
                                </small>


                            </div>

                            </body>
                            </html>
                            """;

                        await emailService.Send(
                            adminEmail,
                            "Low Stock Alert",
                            emailBody);

                        logger.LogInformation("Sent low stock alert for {Count} products", productsByQuantity.Count);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during low stock alert sending");
                }

                await Task.Delay(TimeSpan.FromHours(alertInterval), stoppingToken);
            }
        }
    }
}
