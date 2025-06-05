using RabbitMQ.Client;

namespace Connection.Infrastructure.RabbitMQ
{
    public interface IRabbitMQConnection
    {
        Task<bool> IsConnectedAsync();
        IChannel CreateChannel();
        Task CloseAsync();
    }
}