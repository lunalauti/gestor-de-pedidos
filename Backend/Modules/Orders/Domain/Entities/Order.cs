using System;
using Orders.Domain.Enums;
using Orders.Domain.Entities;

namespace Orders.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public string OrderNumber { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public OrderStatus OrderStatusId { get; private set; }
        public string CustomerName { get; private set; }
        public string CustomerEmail { get; private set; }
        public string Address { get; private set; }
        public string Phone { get; private set; }
        public List<OrderItem> Items { get; private set; }
        public int? DeliveryUserId { get; private set; }
        public string? DeliveryUserEmail { get; private set; }

        protected Order() { }

        public Order(string orderNumber, string customerName, string customerEmail, string address, string phone)
        {
            Id = Guid.NewGuid();
            OrderNumber = orderNumber ?? throw new ArgumentNullException(nameof(orderNumber));
            CustomerName = customerName ?? throw new ArgumentNullException(nameof(customerName));
            CustomerEmail = customerEmail ?? throw new ArgumentNullException(nameof(customerEmail));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Phone = phone ?? throw new ArgumentNullException(nameof(phone));
            OrderStatusId = OrderStatus.RECEIVED;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            Items = new List<OrderItem>();
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            if (OrderStatusId != newStatus)
            {
                OrderStatusId = newStatus;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void AddItem(string productName, string productId, int quantity)
        {
            var item = new OrderItem(Id, productName, productId, quantity);
            Items.Add(item);
        }

        public void AssignDeliveryUser(int userId, string userEmail)
        {
            if (userId <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor que cero", nameof(userId));
            
            if (string.IsNullOrEmpty(userEmail))
                throw new ArgumentException("El email del usuario no puede estar vacío", nameof(userEmail));

            DeliveryUserId = userId;
            DeliveryUserEmail = userEmail;
            UpdateStatus(OrderStatus.OUT_FOR_DELIVERY);
        }
    }
}