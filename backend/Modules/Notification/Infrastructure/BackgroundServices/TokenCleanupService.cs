namespace Notification.Infrastructure.BackgroundServices
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly IConfiguration _configuration;

        public TokenCleanupService(
            IServiceProvider serviceProvider, 
            ILogger<TokenCleanupService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalHours = _configuration.GetValue<int>("TokenCleanup:IntervalHours", 24);
            var retentionDays = _configuration.GetValue<int>("TokenCleanup:RetentionDays", 30);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var tokenRepository = scope.ServiceProvider.GetRequiredService<IDeviceTokenRepository>();
                    
                    var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                    var deletedCount = await tokenRepository.CleanupInactiveTokensAsync(cutoffDate);
                    
                    _logger.LogInformation($"Token cleanup completado: {deletedCount} tokens eliminados");
                    
                    await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en limpieza de tokens");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}