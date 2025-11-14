using CC.Aplication.Helpers;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Helpers;
using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para recuperación de contraseñas con envío dual (SMS + Email)
    /// </summary>
    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<User> _userManager;
        private readonly IGeneralSettingsRepository _settingsRepo;
        private readonly IPasswordResetTokenRepository _resetTokenRepo;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            UserManager<User> userManager,
            IGeneralSettingsRepository settingsRepo,
            IPasswordResetTokenRepository resetTokenRepo,
            ISmsSender smsSender,
            IEmailSender emailSender,
            ILogger<PasswordResetService> logger)
        {
            _userManager = userManager;
            _settingsRepo = settingsRepo;
            _resetTokenRepo = resetTokenRepo;
            _smsSender = smsSender;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(
            ForgotPasswordRequest request,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Solicitud de recuperación de contraseña para: {UserNameOrEmail}", request.UserNameOrEmail);

            try
            {
                // Buscar usuario por username o email
                var user = await _userManager.FindByNameAsync(request.UserNameOrEmail)
                    ?? await _userManager.FindByEmailAsync(request.UserNameOrEmail);

                // Por seguridad, siempre retornamos éxito aunque el usuario no exista
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("Intento de reset para usuario inexistente: {UserNameOrEmail}", request.UserNameOrEmail);
                    
                    return new ForgotPasswordResponse
                    {
                        Success = true,
                        Message = "Si el usuario existe, recibirá un código de verificación en su email y/o teléfono registrado."
                    };
                }

                // Verificar que el usuario tenga email o teléfono
                if (string.IsNullOrWhiteSpace(user.Email) && string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    _logger.LogWarning("Usuario {UserId} no tiene email ni teléfono registrado", user.Id);
                    
                    return new ForgotPasswordResponse
                    {
                        Success = true,
                        Message = "Si el usuario existe, recibirá un código de verificación en su email y/o teléfono registrado."
                    };
                }

                // Invalidar tokens anteriores del usuario
                await _resetTokenRepo.InvalidateUserTokensAsync(user.Id);

                // Generar código de 6 dígitos
                var verificationCode = GenerateVerificationCode(6);
                var codeHash = HashCode(verificationCode);
                
                var ttlMinutes = await SettingsHelper.GetIntSettingAsync(_settingsRepo, "PasswordResetTtlMinutes", 15);

                // Crear token de reset
                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    CodeHash = codeHash,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes),
                    IsUsed = false,
                    FailedAttempts = 0,
                    DeliveredToEmail = false,
                    DeliveredToSms = false
                };

                resetToken = await _resetTokenRepo.AddAsync(resetToken);

                _logger.LogInformation("Token de reset creado para usuario {UserId} con ID {TokenId}", 
                    user.Id, resetToken.Id);

                // Preparar mensaje
                var template = await SettingsHelper.GetStringSettingAsync(
                    _settingsRepo,
                    "PasswordResetMessageTemplate",
                    "Su código de recuperación de contraseña es: {CODE}. Válido por {MINUTES} minutos.");

                var message = template
                    .Replace("{CODE}", verificationCode, StringComparison.OrdinalIgnoreCase)
                    .Replace("{MINUTES}", ttlMinutes.ToString(), StringComparison.OrdinalIgnoreCase);

                var emailSubject = "Recuperación de Contraseña - Portal Pacientes LaCardio";

                // Enviar por ambos canales (mejor práctica de seguridad)
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    try
                    {
                        await _emailSender.SendAsync(user.Email!, emailSubject, message, ct);
                        resetToken.DeliveredToEmail = true;
                        _logger.LogInformation("Código de reset enviado por email a {Email}", 
                            MaskingHelper.MaskEmail(user.Email));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar email de reset a {UserId}", user.Id);
                    }
                }

                if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    try
                    {
                        await _smsSender.SendAsync(user.PhoneNumber!, message, ct);
                        resetToken.DeliveredToSms = true;
                        _logger.LogInformation("Código de reset enviado por SMS a {Phone}", 
                            MaskingHelper.MaskPhone(user.PhoneNumber));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar SMS de reset a {UserId}", user.Id);
                    }
                }

                // Actualizar estado de entregas
                await _resetTokenRepo.UpdateAsync(resetToken);

                return new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "Código de verificación enviado. Revise su email y/o teléfono.",
                    MaskedEmail = MaskingHelper.MaskEmail(user.Email),
                    MaskedPhone = MaskingHelper.MaskPhone(user.PhoneNumber),
                    ResetTokenId = resetToken.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de reset para {UserNameOrEmail}", request.UserNameOrEmail);
                throw;
            }
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(
            ResetPasswordRequest request,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Intento de reset de contraseña con token {TokenId}", request.ResetTokenId);

            try
            {
                // Buscar token con usuario
                var resetToken = await _resetTokenRepo.FindByIdAsync(request.ResetTokenId);

                if (resetToken == null)
                {
                    _logger.LogWarning("Token de reset {TokenId} no encontrado", request.ResetTokenId);
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Token de recuperación inválido o expirado"
                    };
                }

                // Verificar expiración
                if (resetToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token de reset {TokenId} expirado", request.ResetTokenId);
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "El código de recuperación ha expirado. Solicite uno nuevo."
                    };
                }

                // Verificar si ya fue usado
                if (resetToken.IsUsed)
                {
                    _logger.LogWarning("Token de reset {TokenId} ya fue usado", request.ResetTokenId);
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Este código ya fue utilizado. Solicite uno nuevo."
                    };
                }

                // Verificar intentos fallidos (máximo 5)
                if (resetToken.FailedAttempts >= 5)
                {
                    _logger.LogWarning("Token de reset {TokenId} bloqueado por múltiples intentos fallidos", request.ResetTokenId);
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Código bloqueado por múltiples intentos fallidos. Solicite uno nuevo."
                    };
                }

                // Verificar código
                var codeHash = HashCode(request.VerificationCode);
                if (resetToken.CodeHash != codeHash)
                {
                    resetToken.FailedAttempts++;
                    await _resetTokenRepo.UpdateAsync(resetToken);

                    _logger.LogWarning("Código incorrecto para token {TokenId}. Intentos fallidos: {FailedAttempts}", 
                        request.ResetTokenId, resetToken.FailedAttempts);

                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = $"Código incorrecto. Intentos restantes: {5 - resetToken.FailedAttempts}"
                    };
                }

                // Código válido, resetear contraseña
                var user = await _userManager.FindByIdAsync(resetToken.UserId.ToString());
                if (user == null)
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                // Generar token de Identity para reset
                var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, identityResetToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al resetear contraseña para usuario {UserId}: {Errors}", user.Id, errors);
                    
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = $"Error al establecer nueva contraseña: {errors}"
                    };
                }

                // Marcar token como usado
                resetToken.IsUsed = true;
                resetToken.UsedAt = DateTime.UtcNow;
                await _resetTokenRepo.UpdateAsync(resetToken);

                // Reset intentos fallidos de login
                await _userManager.ResetAccessFailedCountAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, null);

                _logger.LogInformation("Contraseña reseteada exitosamente para usuario {UserId}", user.Id);

                return new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Contraseña actualizada exitosamente. Puede iniciar sesión con su nueva contraseña."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña con token {TokenId}", request.ResetTokenId);
                throw;
            }
        }

        public async Task InvalidateUserTokensAsync(Guid userId)
        {
            await _resetTokenRepo.InvalidateUserTokensAsync(userId);
        }

        private static string GenerateVerificationCode(int length)
        {
            const string digits = "0123456789";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var chars = bytes.Select(b => digits[b % digits.Length]).ToArray();
            return new string(chars);
        }

        private static string HashCode(string code)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }
    }
}
