using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Notifications.Application.Services;
using Notifications.Application.DTOs;
using Notifications.Domain.ValueObjects;
using Notifications.Domain.Entities;
using System.Security.Claims;

namespace Notifications.API.Controllers
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationApplicationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationApplicationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("device")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceTokenRequest request)
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no válido en el token" });
                }

                // Extraer rol del usuario del JWT
                var roleString = User.FindFirst(ClaimTypes.Role)?.Value 
                               ?? User.FindFirst("role")?.Value;

                if (!Enum.TryParse<UserRole>(roleString, true, out var userRole))
                {
                    return BadRequest(new { message = "Rol de usuario no válido" });
                }

                var deviceTokenRequest = new DeviceTokenRequest
                {
                    DeviceToken = request.DeviceToken,
                };

                var result = await _notificationService.RegisterDeviceTokenAsync(
                    userId, 
                    userRole, 
                    deviceTokenRequest
                );

                if (result.Success)
                {
                    return Ok(new DeviceTokenResponse
                    {
                        Success = true,
                        Message = "Dispositivo registrado exitosamente",
                        RegisteredAt = result.RegisteredAt
                    });
                }

                return BadRequest(new DeviceTokenResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando dispositivo para usuario");
                return StatusCode(500, new DeviceTokenResponse
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        [HttpDelete("device")]
        public async Task<IActionResult> UnregisterDevice()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value 
                           ?? User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no válido en el token" });
                }

                await _notificationService.DeactivateUserDevicesAsync(userId);

                return Ok(new { 
                    success = true, 
                    message = "Dispositivos desregistrados exitosamente" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error desregistrando dispositivos");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error interno del servidor" 
                });
            }
        }
    }
}