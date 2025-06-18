namespace Connection.Domain.Events
{
    public abstract class OrderEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string EventType { get; protected set; } = string.Empty;
    }

    public class OrderCreatedEvent : OrderEvent
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<OrderItemEvent> Items { get; set; } = new List<OrderItemEvent>();

        public OrderCreatedEvent()
        {
            EventType = nameof(OrderCreatedEvent);
        }
    }

    public class OrderItemEvent
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class OrderStatusChangedEvent : OrderEvent
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;

        public OrderStatusChangedEvent()
        {
            EventType = nameof(OrderStatusChangedEvent);
        }
    }

}