namespace Orders.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public int OrderNumber { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public int OrderStatusId { get; set; }
        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
        public string Address { get; private set; }
        public string Phone { get; private set; }
        public List<OrderItem> Items { get; private set; }
        // public decimal TotalAmount { get; private set; } HAY QUE DEFINIR SI SE NECESITA

        protected Order() { }

        public Order(string orderNumber, string customerName, string customerEmail, string address, string phone)
        {
            Id = Guid.NewGuid();
            OrderNumber = int.Parse(orderNumber ?? throw new ArgumentNullException(nameof(orderNumber)));
            CustomerName = customerName ?? throw new ArgumentNullException(nameof(customerName));
            CustomerEmail = customerEmail ?? throw new ArgumentNullException(nameof(customerEmail));
            Address = Address ?? throw new ArgumentNullException(nameof(Address));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
            OrderStatusId = OrderStatus.Received;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            Items = new List<OrderItem>();
        }

        public void UpdateStatus(OrderStatus newStatusId)
        {
            if (OrderStatusId != newStatusId)
            {
                OrderStatusId = newStatusId;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void AddItem(string productId, int quantity, string productName)
        {
            var item = new OrderItem(productId, quantity, productName);
            Items.Add(item);
        }
    }
}