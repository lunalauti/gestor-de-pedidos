using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Orders.Application.Services;
using Orders.Application.DTOs;
using Orders.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Notifications.Domain.ValueObjects;

namespace Orders.API.Controllers
{
    [ApiController]
    [Route("api/order")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no válido en el token" });
                }

                var roleString = User.FindFirst(ClaimTypes.Role)?.Value 
                               ?? User.FindFirst("role")?.Value;

                if (!Enum.TryParse<UserRole>(roleString, true, out var userRole))
                {
                    return BadRequest(new { message = "Rol de usuario no válido" });
                }

                List<OrderDto> orders;

                switch (userRole)
                {
                    case UserRole.WAREHOUSE_OPERATOR:
                        orders = await _orderService.GetAllOrdersAsync();
                        break;

                    case UserRole.DELIVERY:
                        var readyToShipOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.READY_TO_SHIP);
                        var outForDeliveryOrders = await _orderService.GetOrdersByDeliveryUserAsync(
                            OrderStatus.OUT_FOR_DELIVERY, 
                            int.Parse(userId)
                        );
                        orders = readyToShipOrders.Concat(outForDeliveryOrders).ToList();
                        break;

                    default:
                        return BadRequest(new { message = "Rol no soportado" });
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{orderId:guid}/ready-to-ship")]
        public async Task<IActionResult> ReadyToShip(Guid orderId)
        {
            try
            {
                var result = await _orderService.ReadyToShipAsync(orderId);
                
                if (!result)
                {
                    return NotFound($"No se encontró la orden con ID {orderId} o no se pudo actualizar");
                }

                _logger.LogInformation("Order {OrderId} marked as ready to ship", orderId);

                var message = new { Message = $"Orden {orderId} lista para entregar" };
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as ready to ship", orderId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{orderId:guid}/out-for-delivery")]
        public async Task<IActionResult> OutForDelivery(Guid orderId)
        {
            var userId = User.FindFirst("id")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuario no válido en el token" });
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest(new { message = "ID de usuario inválido en el token" });
            }

            var roleString = User.FindFirst(ClaimTypes.Role)?.Value 
                            ?? User.FindFirst("role")?.Value;

            if (!Enum.TryParse<UserRole>(roleString, true, out var userRole))
            {
                return BadRequest(new { message = "Rol de usuario no válido" });
            }

            if (userRole != UserRole.DELIVERY)
                return Unauthorized(new { message = "Solo un delivery puede asignarse ordenes" });

            try
            {

                var result = await _orderService.AssignDeliveryUserAsync(orderId, userIdInt);
                
                if (!result)
                {
                    return NotFound($"No se encontró la orden con ID {orderId} o el repartidor con ID {userId}");
                }

                _logger.LogInformation("Order {OrderId} assigned to delivery user {userId} and marked as out for delivery", orderId, userId);
                
                var message = new { Message = $"Orden {orderId} en camino" };
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning delivery user {userId} to order {OrderId}", userId, orderId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{orderId:guid}/delivered")]
        public async Task<IActionResult> Delivered(Guid orderId)
        {
            try
            {
                var result = await _orderService.DeliveredAsync(orderId);
                
                if (!result)
                {
                    return NotFound($"No se encontró la orden con ID {orderId} o no se pudo actualizar");
                }

                _logger.LogInformation("Order {OrderId} marked as delivered", orderId);

                var message = new { Message = $"Orden {orderId} entregada" };
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as delivered", orderId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{orderId:guid}/delivery-failed")]
        public async Task<IActionResult> DeliveryFailed(Guid orderId)
        {
            try
            {
                var result = await _orderService.DeliveryFailedAsync(orderId);
                
                if (!result)
                {
                    return NotFound($"No se encontró la orden con ID {orderId} o no se pudo actualizar");
                }

                _logger.LogInformation("Order {OrderId} marked as failed", orderId);

                var message = new { Message = $"Orden {orderId} con entrega fallida" };
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} with delivery failed", orderId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    public class AssignDeliveryUserRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }
    }
}