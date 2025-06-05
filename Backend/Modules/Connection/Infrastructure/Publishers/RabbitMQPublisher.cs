using Connection.Domain.Services;
using Connection.Infrastructure.RabbitMQ;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Connection.Infrastructure.Publishers
{
    public class RabbitMQPublisher : IMessagePublisher
    {
        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQSettings _settings;

        public RabbitMQPublisher(IRabbitMQConnection connection, RabbitMQSettings settings)
        {
            _connection = connection;
            _settings = settings;
        }

        public async Task PublishAsync<T>(T message, string routingKey) where T : class
        {
            await PublishAsync(message, _settings.ExchangeName, routingKey);
        }

        public async Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class
        {
            using var channel = _connection.CreateChannel();
            
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
            
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            
            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );
            
            await Task.CompletedTask;
        }
    }
}