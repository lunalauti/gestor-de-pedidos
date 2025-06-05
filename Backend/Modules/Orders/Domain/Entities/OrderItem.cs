namespace Orders.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public string ProductName { get; private set; }
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }

        protected OrderItem() { } // Para EF Core

        public OrderItem(Guid orderId, string productName, string productId, int quantity)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            ProductName = productName;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
