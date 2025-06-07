using Connection.Domain.Services;
using Connection.MessageContracts;
using Orders.Application.Services;

namespace Connection.Infrastructure.Services
{
    public class MessageConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageConsumerService> _logger;

        public MessageConsumerService(
            IServiceProvider serviceProvider,
            ILogger<MessageConsumerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MessageConsumerService started");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

                // Consumir mensajes de create_order
                await consumer.StartConsumingAsync<OrderCreatedContract>("create_order", async (orderContract) =>
                {
                    using var handlerScope = _serviceProvider.CreateScope();
                    var orderService = handlerScope.ServiceProvider.GetRequiredService<IOrderService>();
                    
                    _logger.LogInformation("Processing create_order event for Order ID: {OrderId}", orderContract.OrderId);
                    
                    try
                    {
                        await orderService.CreateOrderAsync(orderContract);
                        _logger.LogInformation("Successfully processed create_order event for Order ID: {OrderId}", orderContract.OrderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing create_order event for Order ID: {OrderId}", orderContract.OrderId);
                        throw; // Re-throw para que RabbitMQ reencole el mensaje
                    }
                });

                // Mantener el servicio corriendo
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MessageConsumerService");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MessageConsumerService is stopping");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
                await consumer.StopConsumingAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MessageConsumerService");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}