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

                // CAMBIO: Ahora consume CreateOrderRequestContract (solicitudes de creación)
                await consumer.StartConsumingAsync<CreateOrderRequestContract>("create_order", async (orderRequest) =>
                {
                    using var handlerScope = _serviceProvider.CreateScope();
                    var orderService = handlerScope.ServiceProvider.GetRequiredService<IOrderService>();
                    
                    _logger.LogInformation("Processing create_order request for Order Number: {OrderNumber}", orderRequest.OrderNumber);
                    
                    try
                    {
                        // El OrderService creará la orden Y publicará el evento de confirmación
                        var createdOrder = await orderService.CreateOrderAsync(orderRequest);
                        _logger.LogInformation("Successfully processed create_order request. Created Order ID: {OrderId}", createdOrder.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing create_order request for Order Number: {OrderNumber}", orderRequest.OrderNumber);
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