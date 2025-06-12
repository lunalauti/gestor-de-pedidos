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
using Notifications.Domain.Interfaces;
using Notifications.Domain.ValueObjects;
using Users.Application.Interfaces;

namespace Orders.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEventPublisher _eventPublisher;
        private readonly INotificationApplicationService _notificationService;
        private readonly IUserQueries _userQueries;

        public OrderService(
            IOrderRepository orderRepository, 
            IOrderEventPublisher eventPublisher,
            INotificationApplicationService notificationService,
            IUserQueries userQueries)
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
            _notificationService = notificationService;
            _userQueries = userQueries;
        }

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
            
            // Enviar notificación a los usuarios con rol warehouse_operator
            await _notificationService.SendOrderNotificationAsync("ORDER_RECEIVED", UserRole.WAREHOUSE_OPERATOR, order.Id);

            return _MapToDto(order);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? _MapToDto(order) : null;
        }

        public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order != null ? _MapToDto(order) : null;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(_MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(_MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetOrdersByDeliveryUserAsync(OrderStatus status, int deliveryUserId)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Where(o => o.DeliveryUserId == deliveryUserId)
                        .Select(_MapToDto)
                        .ToList();
        }

        public async Task<bool> AssignDeliveryUserAsync(Guid orderId, int userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            var user = await _userQueries.GetUserByIdAsync(userId);
            if (user == null) return false;

            var oldStatus = order.OrderStatusId;
            order.AssignDeliveryUser(userId, user.Email);
            await _orderRepository.UpdateAsync(order);

            var content = NotificationContent.CreateOrderNotification(
                orderId, 
                "ORDER_ASSIGNED", 
                order.OrderNumber
            );
            
            await _eventPublisher.PublishOrderStatusChangedAsync(order, oldStatus, OrderStatus.OUT_FOR_DELIVERY);
            await _notificationService.SendBroadcastNotificationAsync(content);

            return true;
        }

        private async Task<bool> ChangeOrderStatusAsync(Guid orderId, OrderStatus newStatus, string notificationType, UserRole? notificationRole = null)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            var oldStatus = order.OrderStatusId;
            order.UpdateStatus(newStatus);
            await _orderRepository.UpdateAsync(order);

            await _eventPublisher.PublishOrderStatusChangedAsync(order, oldStatus, newStatus);
            
            if (notificationRole.HasValue)
            {
                await _notificationService.SendOrderNotificationAsync(notificationType, notificationRole.Value, order.Id);
            } else {
                // Broadcast
                var content = NotificationContent.CreateOrderNotification(
                    orderId, 
                    notificationType, 
                    order.OrderNumber
                );
            
                await _notificationService.SendBroadcastNotificationAsync(content);
            }

            return true;
        }

        public async Task<bool> ReadyToShipAsync(Guid orderId)
        {
            return await ChangeOrderStatusAsync(
                orderId, 
                OrderStatus.READY_TO_SHIP, 
                "ORDER_READY", 
                UserRole.DELIVERY
            );
        }

        public async Task<bool> DeliveredAsync(Guid orderId)
        {
            return await ChangeOrderStatusAsync(
                orderId,
                OrderStatus.DELIVERED,
                "ORDER_DELIVERED"
            );
        }

        public async Task<bool> DeliveryFailedAsync(Guid orderId)
        {
            return await ChangeOrderStatusAsync(
                orderId,
                OrderStatus.DELIVERY_FAILED,
                "ORDER_FAILED"
            );
        }

        private static OrderDto _MapToDto(Order order)
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
                DeliveryUserId = order.DeliveryUserId,
                DeliveryUserEmail = order.DeliveryUserEmail,
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