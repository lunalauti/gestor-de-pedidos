namespace Notifications.Application.DTOs
{
    public class DeviceTokenRequest
    {
        public string DeviceToken { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? Platform { get; set; }
        public string? AppVersion { get; set; }
    }

    public class DeviceTokenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}