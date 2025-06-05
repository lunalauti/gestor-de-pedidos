namespace Connection.Domain.Services
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string routingKey) where T : class;
        Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class;
    }
}