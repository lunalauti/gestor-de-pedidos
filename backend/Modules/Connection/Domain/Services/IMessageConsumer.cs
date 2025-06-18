namespace Connection.Domain.Services
{
    public interface IMessageConsumer
    {
        Task StartConsumingAsync<T>(string queueName, Func<T, Task> handler) where T : class;
        Task StopConsumingAsync();
    }
}