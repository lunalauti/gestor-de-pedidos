using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Services;
using Notifications.Application.DTOs;
using Notifications.Domain.ValueObjects;
using Notifications.Domain.Entities;
using Notifications.Domain.Interfaces;

namespace Notifications.API.Controllers
{
    [ApiController]
    [Route("api/test/notification")]
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

        [HttpPost("send-test-notification")]
        public async Task<IActionResult> SendTestNotification()
        {
            try
            {
                var content = new NotificationContent(
                    "Notificación de Prueba",
                    "Esta es una notificación de prueba desde tu API",
                    new Dictionary<string, string>
                    {
                        { "test", "true" },
                        { "timestamp", DateTime.UtcNow.ToString() },
                        { "type", "test_notification" }
                    }
                );

                var result = await _notificationService.SendPersonalNotificationAsync(
                    "1", 
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