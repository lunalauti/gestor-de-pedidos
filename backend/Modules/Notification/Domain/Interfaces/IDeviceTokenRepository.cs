using Notifications.Domain.Entities;
using Notifications.Domain.ValueObjects;

namespace Notifications.Domain.Interfaces
{
    public interface IDeviceTokenRepository
    {
        Task<DeviceToken> SaveTokenAsync(DeviceToken deviceToken);
        Task<List<DeviceToken>> GetActiveTokensByRoleAsync(UserRole role);
        Task<List<DeviceToken>> GetAllActiveTokensAsync();
        Task<List<DeviceToken>> GetTokensByUserIdAsync(string userId);
        Task<DeviceToken?> GetTokenByValueAsync(string tokenValue);
        Task DeactivateTokenAsync(string tokenValue);
        Task DeactivateUserTokensAsync(string userId);
        Task<int> CleanupInactiveTokensAsync(DateTime cutoffDate);
        Task<bool> TokenExistsAsync(string tokenValue);
    }
}