using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Repositories;
using Orders.Application.Events;
using Connection.MessageContracts;

namespace Orders.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;

        public OrderService(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        // CAMBIO: Ahora recibe CreateOrderRequestContract (sin OrderId, CreatedAt)
        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequestContract createOrderRequest)
        {
            // Crear la orden en la base de datos
            var order = new Order(
                createOrderRequest.OrderNumber,
                createOrderRequest.CustomerName,
                createOrderRequest.CustomerEmail,
                createOrderRequest.Address,
                createOrderRequest.Phone
            );

            foreach (var itemDto in createOrderRequest.Items)
            {
                order.AddItem(itemDto.ProductName, itemDto.ProductId, itemDto.Quantity);
            }

            await _orderRepository.AddAsync(order);
            
            // PUBLICAR: Confirmar que la orden fue creada (con OrderId, CreatedAt)
            await _eventPublisher.PublishOrderCreatedAsync(order);

            return MapToDto(order);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            var oldStatus = order.OrderStatusId;
            order.UpdateStatus(newStatus);
            await _orderRepository.UpdateAsync(order);
            
            await _eventPublisher.PublishOrderStatusChangedAsync(order, oldStatus, newStatus);
            return true;
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Address = order.Address,
                Phone = order.Phone,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderStatusId = (int)order.OrderStatusId,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                }).ToList()
            };
        }
    }
}