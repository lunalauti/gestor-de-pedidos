using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Connection.Infrastructure.RabbitMQ
{
    public class RabbitMQConnection : IRabbitMQConnection, IDisposable
    {
        private readonly IConnection _connection;
        private readonly RabbitMQSettings _settings;

        public RabbitMQConnection(RabbitMQSettings settings)
        {
            _settings = settings;
            var factory = new ConnectionFactory()
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost
            };
            
            _connection = factory.CreateConnection();
        }

        public Task<bool> IsConnectedAsync()
        {
            return Task.FromResult(_connection?.IsOpen ?? false);
        }

        public IChannel CreateChannel()
        {
            return _connection.CreateModel();
        }

        public Task CloseAsync()
        {
            if (_connection != null && _connection.IsOpen)
            {
                _connection.Close();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}