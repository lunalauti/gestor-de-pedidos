namespace Orders.Infrastructure.Events
{
    public class OrderEventPublisher : IOrderEventPublisher
    {
        private readonly IMessagePublisher _messagePublisher;

        public OrderEventPublisher(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public async Task PublishOrderCreatedAsync(Order order)
        {
            var contract = new OrderCreatedContract
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Address = order.Address,
                Phone = order.Phone,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemContract
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                }).ToList()
            };

            await _messagePublisher.PublishAsync(contract, "order.created");
        }

        public async Task PublishOrderStatusChangedAsync(Order order, OrderStatus oldStatus, OrderStatus newStatus)
        {
            var contract = new OrderStatusChangedContract
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                ChangedAt = DateTime.UtcNow
            };

            await _messagePublisher.PublishAsync(contract, "order.status.changed");
        }

    }
}