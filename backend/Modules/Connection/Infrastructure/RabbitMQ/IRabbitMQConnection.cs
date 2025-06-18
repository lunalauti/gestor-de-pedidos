using RabbitMQ.Client;

namespace Connection.Infrastructure.RabbitMQ
{
    public interface IRabbitMQConnection
    {
        Task<bool> IsConnectedAsync();
        Task<IChannel> CreateChannelAsync();
        Task CloseAsync();
    }
}