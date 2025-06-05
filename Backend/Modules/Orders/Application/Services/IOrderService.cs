using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orders.Application.DTOs;
using Orders.Domain.Enums;

namespace Orders.Application.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    }
}