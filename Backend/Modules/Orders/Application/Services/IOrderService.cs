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
        // Método para procesar solicitudes de creación (evento entrante)
        Task<OrderDto> CreateOrderAsync(CreateOrderRequestContract createOrderRequest);
        Task<bool> AssignDeliveryUserAsync(Guid orderId, int userId);
        Task<bool> ReadyToShipAsync(Guid orderId);

        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
    }
}