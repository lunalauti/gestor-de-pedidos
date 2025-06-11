using Notifications.Domain.Interfaces;
using Notifications.Domain.ValueObjects;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace Notifications.Infrastructure.Services
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
                _logger.LogInformation($"Loading Firebase service account from: {serviceAccountPath}");
                
                if (string.IsNullOrEmpty(serviceAccountPath))
                {
                    throw new InvalidOperationException("Firebase:ServiceAccountPath configuration is missing");
                }
                
                if (!File.Exists(serviceAccountPath))
                {
                    throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountPath}");
                }

                // Read and validate the service account JSON
                var serviceAccountJson = File.ReadAllText(serviceAccountPath);
                _logger.LogDebug($"Service account JSON length: {serviceAccountJson.Length} characters");
                
                // Parse JSON to validate structure
                JsonDocument jsonDoc;
                try
                {
                    jsonDoc = JsonDocument.Parse(serviceAccountJson);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Invalid JSON in service account file: {ex.Message}");
                }

                // Validate required fields
                if (!jsonDoc.RootElement.TryGetProperty("project_id", out var projectIdElement))
                {
                    throw new InvalidOperationException("Service account JSON missing 'project_id' field");
                }
                
                var projectId = projectIdElement.GetString();
                if (string.IsNullOrEmpty(projectId))
                {
                    throw new InvalidOperationException("Service account 'project_id' field is empty");
                }
                
                _logger.LogInformation($"Firebase Project ID from service account: {projectId}");
                
                // Validate other required fields
                var requiredFields = new[] { "type", "private_key", "client_email" };
                foreach (var field in requiredFields)
                {
                    if (!jsonDoc.RootElement.TryGetProperty(field, out _))
                    {
                        throw new InvalidOperationException($"Service account JSON missing required field: {field}");
                    }
                }
                
                if (FirebaseApp.DefaultInstance == null)
                {
                    var credential = GoogleCredential.FromJson(serviceAccountJson);
                    
                    var app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential,
                        ProjectId = projectId 
                    });
                    
                    _logger.LogInformation($"Firebase app created successfully for project: {app.Options.ProjectId}");
                    
                    // Double-check the project ID was set correctly
                    if (string.IsNullOrEmpty(app.Options.ProjectId))
                    {
                        throw new InvalidOperationException("Firebase app project ID is still empty after initialization");
                    }
                }
                else
                {
                    _logger.LogInformation($"Using existing Firebase app for project: {FirebaseApp.DefaultInstance.Options.ProjectId}");
                }
                
                _firebaseMessaging = FirebaseMessaging.DefaultInstance;
                _logger.LogInformation("Firebase messaging service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error initializing Firebase Admin SDK");
                throw;
            }
        }

        public async Task<bool> SendSingleNotificationAsync(string deviceToken, NotificationContent content)
        {
            try
            {
                var message = new Message()
                {
                    Notification = new Notification
                    {
                        Title = content.Title,
                        Body = content.Body,
                    },
                    Data = content.Data,
                    Token = deviceToken
                };
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
                // Log Firebase app details for debugging
                var app = FirebaseApp.DefaultInstance;
                _logger.LogInformation($"Using Firebase Project: {app.Options.ProjectId}");
                
                var messages = deviceTokens.Select(token => CreateMessage(token, content)).ToList();
                
                // Log message details
                _logger.LogInformation($"Sending {messages.Count} messages");
                _logger.LogInformation($"First message sample: {System.Text.Json.JsonSerializer.Serialize(messages.FirstOrDefault())}");
                
                var response = await _firebaseMessaging.SendAllAsync(messages);
                
                result.SuccessCount = response.SuccessCount;
                result.FailureCount = response.FailureCount;
                
                // Process errors with detailed logging
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var individualResponse = response.Responses[i];
                    if (!individualResponse.IsSuccess)
                    {
                        var token = deviceTokens[i];
                        result.FailedTokens.Add(token);
                        
                        _logger.LogError($"Failed to send to token {token.Substring(0, 10)}...: {individualResponse.Exception?.Message}");
                        
                        if (individualResponse.Exception is FirebaseMessagingException fmEx)
                        {
                            _logger.LogError($"Firebase error code: {fmEx.MessagingErrorCode}, Error: {fmEx.ErrorCode}");
                            
                            if (IsInvalidTokenError(fmEx))
                            {
                                result.InvalidTokens.Add(token);
                                await _tokenRepository.DeactivateTokenAsync(token);
                            }
                        }
                    }
                }
                
                _logger.LogInformation($"Bulk notifications: {result.SuccessCount}/{result.TotalSent} successful");
                return result;
            }
            catch (FirebaseMessagingException fmEx)
            {
                _logger.LogError(fmEx, $"Firebase Messaging Exception: {fmEx.MessagingErrorCode} - {fmEx.ErrorCode}");
                _logger.LogError($"Firebase exception details: {fmEx.Message}");
                
                result.FailureCount = result.TotalSent;
                result.FailedTokens = deviceTokens;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending bulk notifications");
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