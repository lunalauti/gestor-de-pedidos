using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Notification.Infrastructure.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly IDeviceTokenRepository _tokenRepository;

        public FirebaseNotificationService(
            ILogger<FirebaseNotificationService> logger,
            IConfiguration configuration,
            IDeviceTokenRepository tokenRepository)
        {
            _logger = logger;
            _tokenRepository = tokenRepository;
            
            try
            {
                var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];
                
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(serviceAccountPath)
                    });
                }
                
                _firebaseMessaging = FirebaseMessaging.DefaultInstance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inicializando Firebase Admin SDK");
                throw;
            }
        }

        public async Task<bool> SendSingleNotificationAsync(string deviceToken, NotificationContent content)
        {
            try
            {
                var message = CreateMessage(deviceToken, content);
                var response = await _firebaseMessaging.SendAsync(message);
                
                _logger.LogInformation($"Notificación enviada exitosamente: {response}");
                
                // Actualizar último uso del token
                await UpdateTokenLastUsed(deviceToken);
                
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, $"Error FCM enviando notificación: {ex.Message}");
                
                // Si el token es inválido, desactivarlo
                if (IsInvalidTokenError(ex))
                {
                    await _tokenRepository.DeactivateTokenAsync(deviceToken);
                    _logger.LogWarning($"Token inválido desactivado: {deviceToken.Substring(0, 10)}...");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado enviando notificación");
                return false;
            }
        }

        public async Task<NotificationResult> SendBulkNotificationAsync(List<string> deviceTokens, NotificationContent content)
        {
            var result = new NotificationResult { TotalSent = deviceTokens.Count };
            
            if (!deviceTokens.Any())
            {
                return result;
            }

            try
            {
                var messages = deviceTokens.Select(token => CreateMessage(token, content)).ToList();
                var response = await _firebaseMessaging.SendAllAsync(messages);
                
                result.SuccessCount = response.SuccessCount;
                result.FailureCount = response.FailureCount;
                
                // Procesar errores y tokens inválidos
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var individualResponse = response.Responses[i];
                    if (!individualResponse.IsSuccess)
                    {
                        var token = deviceTokens[i];
                        result.FailedTokens.Add(token);
                        
                        if (individualResponse.Exception is FirebaseMessagingException fmEx && IsInvalidTokenError(fmEx))
                        {
                            result.InvalidTokens.Add(token);
                            await _tokenRepository.DeactivateTokenAsync(token);
                        }
                    }
                }
                
                _logger.LogInformation($"Notificaciones masivas: {result.SuccessCount}/{result.TotalSent} exitosas");
                
                if (result.InvalidTokens.Any())
                {
                    _logger.LogWarning($"Desactivados {result.InvalidTokens.Count} tokens inválidos");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones masivas");
                result.FailureCount = result.TotalSent;
                result.FailedTokens = deviceTokens;
                return result;
            }
        }

        public async Task<bool> ValidateTokenAsync(string deviceToken)
        {
            try
            {
                var message = new Message()
                {
                    Token = deviceToken,
                    Data = new Dictionary<string, string> { { "validation", "true" } }
                };

                await _firebaseMessaging.SendAsync(message, dryRun: true);
                return true;
            }
            catch (FirebaseMessagingException)
            {
                return false;
            }
        }

        private Message CreateMessage(string deviceToken, NotificationContent content)
        {
            return new Message()
            {
                Token = deviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = content.Title,
                    Body = content.Body
                },
                Data = content.Data,
                Android = new AndroidConfig()
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification()
                    {
                        Icon = "ic_notification",
                        Color = "#FF6B35",
                        Sound = "default",
                        ChannelId = "order_updates"
                    }
                }
            };
        }

        private bool IsInvalidTokenError(FirebaseMessagingException ex)
        {
            // Updated error codes for newer Firebase Admin SDK versions
            return ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                ex.MessagingErrorCode == MessagingErrorCode.SenderIdMismatch ||
                (ex.ErrorCode != null && (
                    ex.ErrorCode.Equals("INVALID_REGISTRATION_TOKEN") ||
                    ex.ErrorCode.Equals("REGISTRATION_TOKEN_NOT_REGISTERED") ||
                    ex.ErrorCode.Equals("INVALID_ARGUMENT")
                ));
        }

        private async Task UpdateTokenLastUsed(string deviceToken)
        {
            try
            {
                var token = await _tokenRepository.GetTokenByValueAsync(deviceToken);
                if (token != null)
                {
                    token.LastUsed = DateTime.UtcNow;
                    await _tokenRepository.SaveTokenAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error actualizando último uso del token");
            }
        }
    }
}