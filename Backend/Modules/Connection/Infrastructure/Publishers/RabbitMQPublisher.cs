using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Connection.Domain.Services;
using Connection.Infrastructure.RabbitMQ;

namespace Connection.Infrastructure.Publishers
{
    public class RabbitMQPublisher : IMessagePublisher, IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQSettings _settings;
        private IChannel? _channel;

        public RabbitMQPublisher(IRabbitMQConnection connection, RabbitMQSettings settings)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task PublishAsync<T>(T message, string routingKey) where T : class
        {
            await PublishAsync(message, _settings.ExchangeName, routingKey);
        }

        public async Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class
        {
            if (_channel == null)
            {
                _channel = await _connection.CreateChannelAsync();
                
                await _channel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null);
            }

            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);
        }

        public async Task CloseAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
                _channel = null;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}