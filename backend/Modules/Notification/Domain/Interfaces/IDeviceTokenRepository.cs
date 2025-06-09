namespace Notification.Domain.Interfaces
{
    public interface IDeviceTokenRepository
    {
        Task<DeviceToken> SaveTokenAsync(DeviceToken deviceToken);
        Task<List<DeviceToken>> GetActiveTokensByRoleAsync(UserRole role);
        Task<List<DeviceToken>> GetTokensByUserIdAsync(string userId);
        Task<DeviceToken> GetTokenByValueAsync(string tokenValue);
        Task DeactivateTokenAsync(string tokenValue);
        Task DeactivateUserTokensAsync(string userId);
        Task<int> CleanupInactiveTokensAsync(DateTime cutoffDate);
        Task<bool> TokenExistsAsync(string tokenValue);
    }
}