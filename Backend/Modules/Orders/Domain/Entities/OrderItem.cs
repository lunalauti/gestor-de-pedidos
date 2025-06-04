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

        public OrderItem(Guid orderId, int quantity, string productName, string productId)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
            ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
            
            Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be greater than 0");
        }
    }
}
