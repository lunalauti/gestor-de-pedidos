using Microsoft.AspNetCore.Mvc;
using Notification.Application.Services;
using Notification.Application.DTOs;
using Notification.Domain.ValueObjects;
using Notification.Domain.Entities;

namespace Notification.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestNotificationController : ControllerBase
    {
        private readonly INotificationApplicationService _notificationService;
        private readonly ILogger<TestNotificationController> _logger;

        public TestNotificationController(
            INotificationApplicationService notificationService,
            ILogger<TestNotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("register-test-token")]
        public async Task<IActionResult> RegisterTestToken()
        {
            try
            {
                var testToken = "eMfilBNTRLieOl4wkyRiVH:APA91bGTZOr8Yh4K31nUnuv8o_b8m-FwAfNOpyQjTt6QHjFKxJkUVhd2WUjefFft8fdBfhm7dyJ4uR8sPPHYNNOdatyMZpYi_C_P2aC2txq-Fmxfu-lBKms";
                
                var request = new DeviceTokenRequest
                {
                    DeviceToken = testToken,
                    DeviceId = "TEST_DEVICE_001",
                };

                var result = await _notificationService.RegisterDeviceTokenAsync(
                    "TEST_USER_001", 
                    UserRole.DELIVERY, 
                    request
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando token de prueba");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("send-test-notification")]
        public async Task<IActionResult> SendTestNotification()
        {
            try
            {
                var content = new NotificationContent(
                    "🚀 Notificación de Prueba",
                    "Esta es una notificación de prueba desde tu API",
                    new Dictionary<string, string>
                    {
                        { "test", "true" },
                        { "timestamp", DateTime.UtcNow.ToString() },
                        { "type", "test_notification" }
                    }
                );

                var result = await _notificationService.SendPersonalNotificationAsync(
                    "TEST_USER_001", 
                    content
                );

                return Ok(new { 
                    success = result, 
                    message = result ? "Notificación enviada exitosamente" : "Error enviando notificación",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de prueba");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("send-order-notification-test")]
        public async Task<IActionResult> SendOrderNotificationTest([FromBody] TestOrderNotificationRequest request)
        {
            try
            {
                var result = await _notificationService.SendOrderNotificationAsync(
                    request.OrderId ?? "TEST-ORDER-001",
                    request.EventType ?? "ORDER_RECEIVED",
                    request.TargetRole
                );

                return Ok(new { 
                    success = result, 
                    message = result ? "Notificación de pedido enviada" : "Error enviando notificación",
                    orderId = request.OrderId,
                    eventType = request.EventType,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de pedido de prueba");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("send-custom-notification")]
        public async Task<IActionResult> SendCustomNotification([FromBody] CustomNotificationRequest request)
        {
            try
            {
                var content = new NotificationContent(
                    request.Title,
                    request.Body,
                    request.Data ?? new Dictionary<string, string>()
                );

                var result = await _notificationService.SendPersonalNotificationAsync(
                    request.UserId ?? "TEST_USER_001",
                    content
                );

                return Ok(new { 
                    success = result, 
                    message = result ? "Notificación personalizada enviada" : "Error enviando notificación",
                    title = request.Title,
                    body = request.Body
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación personalizada");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("test-scenarios")]
        public IActionResult GetTestScenarios()
        {
            var scenarios = new
            {
                scenarios = new object[] // Explicitly specify object[] type
                {
                    new { 
                        name = "Nuevo Pedido",
                        endpoint = "/api/TestNotification/send-order-notification-test",
                        body = new { 
                            orderId = "ORD-12345",
                            eventType = "ORDER_RECEIVED",
                            targetRole = 1 // Add targetRole for consistency
                        }
                    },
                    new { 
                        name = "Pedido Listo",
                        endpoint = "/api/TestNotification/send-order-notification-test",
                        body = new { 
                            orderId = "ORD-12345",
                            eventType = "ORDER_READY",
                            targetRole = 1
                        }
                    },
                    new { 
                        name = "Pedido Asignado",
                        endpoint = "/api/TestNotification/send-order-notification-test",
                        body = new { 
                            orderId = "ORD-12345",
                            eventType = "ORDER_ASSIGNED",
                            targetRole = 1
                        }
                    },
                    new { 
                        name = "Notificación Personalizada",
                        endpoint = "/api/TestNotification/send-custom-notification",
                        body = new { 
                            title = "Mi Título Personalizado",
                            body = "Este es el mensaje de mi notificación",
                            userId = "TEST_USER_001", // Add userId for consistency
                            data = new Dictionary<string, string>
                            {
                                { "custom_field", "valor_personalizado" }
                            }
                        }
                    }
                },
                instructions = new[]
                {
                    "1. Primero ejecuta: POST /api/TestNotification/register-test-token",
                    "2. Luego prueba cualquiera de los endpoints de arriba",
                    "3. Verifica que tu app móvil reciba las notificaciones"
                }
            };

            return Ok(scenarios);
        }
    }

    public class TestOrderNotificationRequest
    {
        public string? OrderId { get; set; }
        public string? EventType { get; set; }
        public UserRole TargetRole { get; set; } = UserRole.DELIVERY;
    }

    public class CustomNotificationRequest
    {
        public string Title { get; set; } = "Título de Prueba";
        public string Body { get; set; } = "Cuerpo de la notificación";
        public string? UserId { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }
}