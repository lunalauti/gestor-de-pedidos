using Orders.Domain.Enums;
namespace Connection.MessageContracts
{
    // Evento ENTRANTE: Sistema externo solicita crear orden
    public class CreateOrderRequestContract
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<CreateOrderItemContract> Items { get; set; } = new();
    }

    // Evento SALIENTE: Tu sistema confirma que la orden fue creada
    public class OrderCreatedContract
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemContract> Items { get; set; } = new();
    }

    // Evento SALIENTE: Notificar cambio de estado de orden
    public class OrderStatusUpdateContract
    {
        public string OrderId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Para el evento entrante
    public class CreateOrderItemContract
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    // Para el evento saliente
    public class OrderItemContract
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}