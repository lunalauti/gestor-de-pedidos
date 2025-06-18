using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Application.Events;
using Connection.Domain.Services;
using Connection.MessageContracts;
using RabbitMQ.Client;

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
                    OrderId = item.OrderId,
                    ProductName = item.ProductName,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                }).ToList()
            };

            await _messagePublisher.PublishAsync(contract, "order.created");
        }

        public async Task PublishOrderStatusUpdateAsync(Guid orderId, string orderNumber, OrderStatus status, DateTime updatedAt)
        {
            var contract = new OrderStatusUpdateContract
            {
                OrderId = orderId.ToString(),
                OrderNumber = orderNumber.ToString(),
                Status = status,
                UpdatedAt = updatedAt
            };

            await _messagePublisher.PublishAsync(contract, "update_status");
        }
    }
}