using Microsoft.EntityFrameworkCore;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Data;
using Notification.Domain.Entities;
using Notification.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Notification.Infrastructure.Repositories
{
    public class DeviceTokenRepository : IDeviceTokenRepository
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<DeviceTokenRepository> _logger;

        public DeviceTokenRepository(NotificationDbContext context, ILogger<DeviceTokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DeviceToken> SaveTokenAsync(DeviceToken deviceToken)
        {
            try
            {
                // Buscar token existente
                var existingToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.Token == deviceToken.Token);

                if (existingToken != null)
                {
                    // Actualizar token existente
                    existingToken.LastUsed = DateTime.UtcNow;
                    existingToken.IsActive = true;
                    existingToken.UserId = deviceToken.UserId; // Actualizar usuario por si cambió
                    existingToken.UserRole = deviceToken.UserRole;
                    existingToken.AppVersion = deviceToken.AppVersion;
                    
                    _context.DeviceTokens.Update(existingToken);
                    await _context.SaveChangesAsync();
                    return existingToken;
                }

                // Desactivar tokens antiguos del mismo dispositivo
                if (!string.IsNullOrEmpty(deviceToken.DeviceId))
                {
                    var oldDeviceTokens = await _context.DeviceTokens
                        .Where(dt => dt.DeviceId == deviceToken.DeviceId && dt.IsActive)
                        .ToListAsync();

                    foreach (var oldToken in oldDeviceTokens)
                    {
                        oldToken.IsActive = false;
                    }
                }

                // Crear nuevo token
                deviceToken.CreatedAt = DateTime.UtcNow;
                deviceToken.LastUsed = DateTime.UtcNow;
                deviceToken.IsActive = true;

                await _context.DeviceTokens.AddAsync(deviceToken);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Nuevo device token guardado para usuario {deviceToken.UserId}");
                return deviceToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error guardando device token para usuario {deviceToken.UserId}");
                throw;
            }
        }

        public async Task<List<DeviceToken>> GetActiveTokensByRoleAsync(UserRole role)
        {
            return await _context.DeviceTokens
                .Where(dt => dt.UserRole == role && dt.IsActive)
                .OrderByDescending(dt => dt.LastUsed)
                .ToListAsync();
        }

        public async Task<List<DeviceToken>> GetTokensByUserIdAsync(string userId)
        {
            return await _context.DeviceTokens
                .Where(dt => dt.UserId == userId && dt.IsActive)
                .OrderByDescending(dt => dt.LastUsed)
                .ToListAsync();
        }

        public async Task<DeviceToken?> GetTokenByValueAsync(string tokenValue)
        {
            return await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.Token == tokenValue);
        }

        public async Task DeactivateTokenAsync(string tokenValue)
        {
            var token = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.Token == tokenValue);

            if (token != null)
            {
                token.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Token desactivado: {tokenValue.Substring(0, 10)}...");
            }
        }

        public async Task DeactivateUserTokensAsync(string userId)
        {
            var tokens = await _context.DeviceTokens
                .Where(dt => dt.UserId == userId && dt.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsActive = false;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Desactivados {tokens.Count} tokens del usuario {userId}");
        }

        public async Task<int> CleanupInactiveTokensAsync(DateTime cutoffDate)
        {
            var inactiveTokens = await _context.DeviceTokens
                .Where(dt => !dt.IsActive || dt.LastUsed < cutoffDate)
                .ToListAsync();

            _context.DeviceTokens.RemoveRange(inactiveTokens);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Limpieza completada: {inactiveTokens.Count} tokens eliminados");
            return inactiveTokens.Count;
        }

        public async Task<bool> TokenExistsAsync(string tokenValue)
        {
            return await _context.DeviceTokens
                .AnyAsync(dt => dt.Token == tokenValue && dt.IsActive);
        }
    }
}