namespace Notification.Domain.Entities
{
    public class DeviceToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public UserRole UserRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
        public bool IsActive { get; set; } = true;
        public string DeviceId { get; set; } // Identificador único del dispositivo
        public string Platform { get; set; } // Android, iOS
        public string AppVersion { get; set; }
    }

    public enum UserRole
    {
        DEPOSIT,
        DELIVERY
    }
}