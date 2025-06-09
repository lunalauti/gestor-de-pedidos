namespace Notification.Domain.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendSingleNotificationAsync(string deviceToken, NotificationContent content);
        Task<NotificationResult> SendBulkNotificationAsync(List<string> deviceTokens, NotificationContent content);
        Task<bool> ValidateTokenAsync(string deviceToken);
    }

    public class NotificationResult
    {
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> FailedTokens { get; set; } = new();
        public List<string> InvalidTokens { get; set; } = new();
    }
}
