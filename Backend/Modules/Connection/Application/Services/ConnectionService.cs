using Connection.Infrastructure.RabbitMQ;

namespace Connection.Application.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly IRabbitMQConnection _rabbitConnection;

        public ConnectionService(IRabbitMQConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                return await _rabbitConnection.IsConnectedAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<ConnectionStatus> GetConnectionStatusAsync()
        {
            try
            {
                var isConnected = await _rabbitConnection.IsConnectedAsync();
                return new ConnectionStatus
                {
                    IsConnected = isConnected,
                    Status = isConnected ? "Connected" : "Disconnected",
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ConnectionStatus
                {
                    IsConnected = false,
                    Status = "Error",
                    LastChecked = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}