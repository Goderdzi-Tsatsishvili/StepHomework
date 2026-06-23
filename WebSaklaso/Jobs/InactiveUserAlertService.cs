using WebSaklaso.Data;
using Microsoft.EntityFrameworkCore;
using WebSaklaso.Service.Contracts;

namespace WebSaklaso.Jobs
{
    public class InactiveUserAlertService(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<InactiveUserAlertService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timeoutDays = configuration.GetValue<int>("Jobs:InactiveUserAlertJob:InactivityTimeoutDays");

            var alertInterval = configuration.GetValue<int>("Jobs:InactiveUserAlertJob:ServiceIntervalHours");

            var adminEmail = configuration.GetValue<string>("Jobs:InactiveUserAlertJob:AdminEmail");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var cutoff = DateTime.UtcNow.AddDays(-timeoutDays);

                    var inactiveCustomers = await context.Customers
                        .Where(u => u.LastLoginDate != null && u.LastLoginDate < cutoff)
                        .ToListAsync(stoppingToken);

                    if (inactiveCustomers.Count > 0)
                    {
                        var rows = "";

                        foreach (var user in inactiveCustomers)
                        {
                            rows += $"""
                            <tr>
                                <td style="padding:8px;border-bottom:1px solid #ddd;">
                                    {user.CustomerName}
                                </td>

                                <td style="padding:8px;border-bottom:1px solid #ddd;">
                                    {user.Email}
                                </td>

                                <td style="padding:8px;border-bottom:1px solid #ddd;">
                                    {(user.LastLoginDate.HasValue
                                        ? user.LastLoginDate.Value.ToString("yyyy-MM-dd")
                                        : "Never")}
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

                            <h2 style="color:#f0ad4e;">
                                👤 Inactive Customer Alert
                            </h2>

                            <p>
                                The following customers have been inactive for more than {timeoutDays} days:
                            </p>

                            <table style="
                                width:100%;
                                border-collapse:collapse;">

                                <thead>
                                    <tr>
                                        <th style="padding:8px;border-bottom:1px solid #ddd;text-align:left;">
                                            Username
                                        </th>

                                        <th style="padding:8px;border-bottom:1px solid #ddd;text-align:left;">
                                            Email
                                        </th>

                                        <th style="padding:8px;border-bottom:1px solid #ddd;text-align:left;">
                                            Last Active
                                        </th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {rows}
                                </tbody>

                            </table>

                            <p style="margin-top:20px;">
                                Consider re-engagement campaigns or notifications.
                            </p>

                            <hr>

                            <small>
                                Automated customer activity monitoring system
                            </small>

                        </div>

                        </body>
                        </html>
                        """;

                        await emailService.Send(
                            adminEmail,
                            "Inactive Users Alert",
                            emailBody);

                        logger.LogInformation("sent inactive users alert for {count} users", inactiveCustomers.Count);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "error during inactive users alert");
                }

                await Task.Delay(TimeSpan.FromHours(alertInterval), stoppingToken);
            }
        }
    }
}
