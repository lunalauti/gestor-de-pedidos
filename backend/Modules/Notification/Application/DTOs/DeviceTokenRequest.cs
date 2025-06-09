namespace Notification.Application.DTOs
{
    public class DeviceTokenRequest
    {
        [Required]
        public string DeviceToken { get; set; }
        
        public string DeviceId { get; set; }
    }

    public class DeviceTokenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
