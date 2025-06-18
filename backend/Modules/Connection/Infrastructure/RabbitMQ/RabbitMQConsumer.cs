using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Connection.Domain.Services;
using Connection.Infrastructure.RabbitMQ;

namespace Connection.Infrastructure.RabbitMQ
{
    public class RabbitMQConsumer : IMessageConsumer, IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly RabbitMQSettings _settings;
        private IChannel? _channel;
        private string? _consumerTag;
        private bool _isConsuming;

        public RabbitMQConsumer(IRabbitMQConnection connection, RabbitMQSettings settings)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task StartConsumingAsync<T>(string queueName, Func<T, Task> handler) where T : class
        {
            if (_isConsuming)
            {
                throw new InvalidOperationException("Consumer is already running");
            }

            _channel = await _connection.CreateChannelAsync();
            
            // Configurar QoS para procesar un mensaje a la vez
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            // Declarar la cola (asegurar que existe)
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Crear el consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                try
                {
                    var message = JsonSerializer.Deserialize<T>(messageJson);
                    if (message != null)
                    {
                        await handler(message);
                        
                        // Acknowledge el mensaje solo si se procesó correctamente
                        await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        // Rechazar mensaje si no se pudo deserializar
                        await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (JsonException jsonEx)
                {
                    // Error de deserialización - no reencolar
                    Console.WriteLine($"JSON deserialization error for queue {queueName}: {jsonEx.Message}");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
                }
                catch (Exception ex)
                {
                    // Error de procesamiento - reencolar para reintentar
                    Console.WriteLine($"Processing error for queue {queueName}: {ex.Message}");
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
                }
            };

            // Iniciar el consumo
            _consumerTag = await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false, // Manual acknowledgment
                consumer: consumer);

            _isConsuming = true;
            Console.WriteLine($"Started consuming from queue: {queueName}");
        }

        public async Task StopConsumingAsync()
        {
            if (!_isConsuming || _channel == null || string.IsNullOrEmpty(_consumerTag))
            {
                return;
            }

            try
            {
                await _channel.BasicCancelAsync(_consumerTag);
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
                
                _channel = null;
                _consumerTag = null;
                _isConsuming = false;
                
                Console.WriteLine("Stopped consuming messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping consumer: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                StopConsumingAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing consumer: {ex.Message}");
            }
        }
    }
}