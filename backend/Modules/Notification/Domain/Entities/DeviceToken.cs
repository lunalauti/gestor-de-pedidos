using Notification.Domain.ValueObjects;

namespace Notification.Domain.Entities
{
    public class DeviceToken
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UserRole UserRole { get; set; }
        public string? DeviceId { get; set; }
        public string? Platform { get; set; }
        public string? AppVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
        public bool IsActive { get; set; } = true;
    }
}