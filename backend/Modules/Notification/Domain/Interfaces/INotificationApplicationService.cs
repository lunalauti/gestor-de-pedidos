using Notifications.Application.DTOs;
using Notifications.Domain.Entities;
using Notifications.Domain.ValueObjects;
using Notifications.Domain.Interfaces;

namespace Notifications.Domain.Interfaces 
{
    public interface INotificationApplicationService
    {
        Task<DeviceTokenResponse> RegisterDeviceTokenAsync(string userId, UserRole userRole, DeviceTokenRequest request);
        Task<bool> SendOrderNotificationAsync(string eventType, UserRole targetRole, Guid orderId);
        Task<bool> SendPersonalNotificationAsync(string userId, NotificationContent content);
        Task DeactivateUserDevicesAsync(string userId);
        Task<bool> SendBroadcastNotificationAsync(NotificationContent content);
    }
}