namespace Connection.MessageContracts
{
    public class OrderCreatedContract
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<OrderItemContract> Items { get; set; } = new();
    }

    public class OrderStatusChangedContract
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }

    public class OrderItemContract
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}