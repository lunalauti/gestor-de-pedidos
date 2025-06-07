using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Connection.Infrastructure.RabbitMQ
{
    public class RabbitMQConnection : IRabbitMQConnection, IDisposable
    {
        private IConnection? _connection;
        private readonly RabbitMQSettings _settings;

        public RabbitMQConnection(RabbitMQSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private async Task InitializeConnectionAsync()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                ConnectionFactory factory = new ConnectionFactory();

                factory.UserName = _settings.UserName;
                factory.Password = _settings.Password;
                factory.VirtualHost = _settings.VirtualHost;
                factory.HostName = _settings.HostName;
                factory.Port = _settings.Port;

                _connection = await factory.CreateConnectionAsync();
            }
        }

        public async Task<bool> IsConnectedAsync()
        {
            if (_connection == null)
                await InitializeConnectionAsync();
            
            return _connection?.IsOpen ?? false;
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            if (_connection == null || !_connection.IsOpen)
                await InitializeConnectionAsync();
            
            return await _connection!.CreateChannelAsync();
        }

        public async Task CloseAsync()
        {
            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}