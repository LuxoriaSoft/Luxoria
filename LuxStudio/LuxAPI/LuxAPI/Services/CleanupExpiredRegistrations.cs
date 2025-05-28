using LuxAPI.DAL;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class CleanupExpiredRegistrations : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<CleanupExpiredRegistrations> _logger;

    public CleanupExpiredRegistrations(IServiceProvider provider, ILogger<CleanupExpiredRegistrations> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            var expired = db.PendingRegistrations.Where(p => p.ExpiresAt < now).ToList();

            if (expired.Any())
            {
                db.PendingRegistrations.RemoveRange(expired);
                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Purge : {Count} enregistrements expirés supprimés", expired.Count);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
