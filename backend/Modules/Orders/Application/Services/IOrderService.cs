using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orders.Application.DTOs;
using Orders.Domain.Enums;
using Connection.MessageContracts;

namespace Orders.Application.Services
{
    public interface IOrderService
    {
        // Método para procesar solicitudes
        Task<OrderDto> CreateOrderAsync(CreateOrderRequestContract createOrderRequest);
        Task<bool> AssignDeliveryUserAsync(Guid orderId, int userId);
        Task<bool> ReadyToShipAsync(Guid orderId);
        Task<bool> DeliveredAsync(Guid orderId);
        Task<bool> DeliveryFailedAsync(Guid orderId);
        
        // Métodos reutilizables
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<OrderDto>> GetOrdersByDeliveryUserAsync(OrderStatus status, int deliveryUserId);
    }
}