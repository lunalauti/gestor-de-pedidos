using Notifications.Application.DTOs;
using Notifications.Domain.Entities;
using Notifications.Domain.ValueObjects;
using Notifications.Domain.Interfaces;
using Orders.Domain.Repositories;
using Orders.Domain.Entities;

namespace Notifications.Application.Services
{
    public class NotificationApplicationService : INotificationApplicationService
    {
        private readonly IDeviceTokenRepository _tokenRepository;
        private readonly INotificationService _notificationService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<NotificationApplicationService> _logger;

        public NotificationApplicationService(
            IDeviceTokenRepository tokenRepository,
            INotificationService notificationService,
            IOrderRepository orderRepository,
            ILogger<NotificationApplicationService> logger)
        {
            _tokenRepository = tokenRepository;
            _notificationService = notificationService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<DeviceTokenResponse> RegisterDeviceTokenAsync(string userId, UserRole userRole, DeviceTokenRequest request)
        {
            try
            {
                var deviceToken = new DeviceToken
                {
                    UserId = userId,
                    Token = request.DeviceToken,
                    UserRole = userRole,
                    DeviceId = request.DeviceId,
                    Platform = request.Platform,
                    AppVersion = request.AppVersion
                };

                var savedToken = await _tokenRepository.SaveTokenAsync(deviceToken);

                return new DeviceTokenResponse
                {
                    Success = true,
                    Message = "Device token registrado exitosamente",
                    RegisteredAt = savedToken.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registrando device token para usuario {userId}");
                return new DeviceTokenResponse
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        public async Task<bool> SendOrderNotificationAsync(string eventType, UserRole targetRole, Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning($"No se encontró la orden con ID {orderId}");
                    return false;
                }

                var content = NotificationContent.CreateOrderNotification(
                    orderId, 
                    eventType, 
                    order.OrderNumber
                );

                var tokens = await _tokenRepository.GetActiveTokensByRoleAsync(targetRole);
                var deviceTokens = tokens.Select(t => t.Token).ToList();

                if (!deviceTokens.Any())
                {
                    _logger.LogWarning($"No hay tokens activos para el rol {targetRole}");
                    return false;
                }

                var result = await _notificationService.SendBulkNotificationAsync(deviceTokens, content);

                return result.SuccessCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando notificación de pedido {orderId}");
                return false;
            }
        }

        public async Task<bool> SendPersonalNotificationAsync(string userId, NotificationContent content)
        {
            try
            {
                var tokens = await _tokenRepository.GetTokensByUserIdAsync(userId);
                var deviceTokens = tokens.Select(t => t.Token).ToList();
                _logger.LogInformation($"token del usuario {userId}: {deviceTokens[0]}");

                if (!deviceTokens.Any())
                {
                    _logger.LogWarning($"No hay tokens activos para el usuario {userId}");
                    return false;
                }

                var successCount = 0;
                foreach (var token in deviceTokens)
                {
                    var result = await _notificationService.SendSingleNotificationAsync(token, content);
                    if (result)
                    {
                        successCount++;
                    }
                }
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando notificación personal al usuario {userId}");
                return false;
            }
        }

        public async Task DeactivateUserDevicesAsync(string userId)
        {
            try
            {
                await _tokenRepository.DeactivateUserTokensAsync(userId);
                _logger.LogInformation($"Dispositivos del usuario {userId} desactivados por logout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error desactivando dispositivos del usuario {userId}");
                throw;
            }
        }

        public async Task<bool> SendBroadcastNotificationAsync(NotificationContent content)
        {
            try
            {
                var tokens = await _tokenRepository.GetAllActiveTokensAsync();
                var deviceTokens = tokens.Select(t => t.Token).ToList();
                _logger.LogInformation($"Tokens activos: {string.Join(", ", deviceTokens)}");  // Corregido aquí

                if (!deviceTokens.Any())
                {
                    _logger.LogWarning("No hay tokens activos en el sistema");
                    return false;
                }

                var result = await _notificationService.SendBulkNotificationAsync(deviceTokens, content);
                return result.SuccessCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación broadcast");
                return false;
            }
        }
    }
}