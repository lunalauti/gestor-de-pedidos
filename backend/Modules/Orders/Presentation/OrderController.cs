using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Orders.Application.Services;
using Orders.Application.DTOs;
using Orders.Domain.Enums;
using System.ComponentModel.DataAnnotations;

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
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
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
                    return NotFound($"Order with ID {orderId} not found or could not be updated");
                }

                _logger.LogInformation("Order {OrderId} marked as ready to ship", orderId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as ready to ship", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{orderId:guid}/out-for-delivery")]
        public async Task<IActionResult> OutForDelivery(Guid orderId, [FromBody] AssignDeliveryUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _orderService.AssignDeliveryUserAsync(orderId, request.UserId);
                
                if (!result)
                {
                    return NotFound($"Order with ID {orderId} not found or delivery user with ID {request.UserId} not found");
                }

                _logger.LogInformation("Order {OrderId} assigned to delivery user {UserId} and marked as out for delivery", orderId, request.UserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning delivery user {UserId} to order {OrderId}", request?.UserId, orderId);
                return StatusCode(500, "Internal server error");
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