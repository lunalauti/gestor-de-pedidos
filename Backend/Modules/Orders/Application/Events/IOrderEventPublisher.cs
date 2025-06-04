namespace Orders.Application.Events
{
    public interface IOrderEventPublisher
    {
        Task PublishOrderCreatedAsync(Order order);
        Task PublishOrderStatusChangedAsync(Order order, OrderStatus oldStatus, OrderStatus newStatus);
    }
}