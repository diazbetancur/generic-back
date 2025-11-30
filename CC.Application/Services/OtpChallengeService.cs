using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Helpers;
using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Aplication.Helpers;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de desafíos OTP (One-Time Password)
    /// </summary>
    public class OtpChallengeService : ServiceBase<OtpChallenge, OtpChallengeDto>, IOtpChallengeService
    {
        private readonly IOtpChallengeRepository _otpRepo;
        private readonly IDocTypeRepository _docTypeRepo;
        private readonly IGeneralSettingsRepository _settingsRepo;
        private readonly IExternalPatientService _externalPatient;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;

        public OtpChallengeService(
            IOtpChallengeRepository repository,
            IMapper mapper,
            ILogger<OtpChallengeService> logger,
            IDocTypeRepository docTypeRepo,
            IGeneralSettingsRepository settingsRepo,
            IExternalPatientService externalPatient,
            ISmsSender smsSender,
            IEmailSender emailSender)
            : base(repository, mapper, logger)
        {
            _otpRepo = repository;
            _docTypeRepo = docTypeRepo;
            _settingsRepo = settingsRepo;
            _externalPatient = externalPatient;
            _smsSender = smsSender;
            _emailSender = emailSender;
        }

        public async Task<ValidateAuthResponse> ValidateAndGenerateOtpAsync(ValidateAuthRequest request, CancellationToken ct = default)
        {
            Logger.LogInformation(
                "Iniciando validación y generación de OTP para {DocType}-{DocNumber}",
                request.DocTypeCode, request.DocNumber);

            try
            {
                var docType = await _docTypeRepo.FindByAlternateKeyAsync(d => d.Code == request.DocTypeCode).ConfigureAwait(false);
                if (docType == null)
                {
                    Logger.LogWarning(
                        "Tipo de documento inválido: {DocType}",
                        request.DocTypeCode);
                    throw new InvalidOperationException("Tipo de documento inválido");
                }

                var contact = await _externalPatient.GetContactAsync(request.DocTypeCode, request.DocNumber, ct).ConfigureAwait(false);
                if (contact == null || (string.IsNullOrWhiteSpace(contact.Mobile) && string.IsNullOrWhiteSpace(contact.Email)))
                {
                    Logger.LogWarning(
                        "No se encontraron medios de contacto para {DocType}-{DocNumber}",
                        request.DocTypeCode, request.DocNumber);

                    var mensaje = await SettingsHelper.GetStringSettingAsync(
                        _settingsRepo,
                        "MensajeSinContacto",
                        "No se encontraron medios de contacto. Por favor comuníquese con el servicio de atención.");

                    return new ValidateAuthResponse(
                        Guid.Empty,
                        null,
                        null,
                        null,
                        null,
                        mensaje);
                }

                var otp = GenerateOtp(4);
                var ttlSeconds = await SettingsHelper.GetIntSettingAsync(_settingsRepo, "OtpTtlSeconds", 300);
                var codeHash = HashOtp(otp);
                var now = DateTime.UtcNow;

                var challenge = new OtpChallenge
                {
                    DocTypeId = docType.Id,
                    DocNumber = request.DocNumber,
                    UserId = $"{request.DocTypeCode}-{request.DocNumber}",
                    CodeHash = codeHash,
                    ExpiresAt = now.AddSeconds(ttlSeconds),
                    ClientIp = null,
                    DeliveredToEmail = false,
                    DeliveredToSms = false
                };

                challenge = await _otpRepo.AddAsync(challenge).ConfigureAwait(false);

                Logger.LogInformation(
                    "OTP challenge creado con ID: {ChallengeId} para usuario {UserId}",
                    challenge.Id, challenge.UserId);

                var maskedPhone = MaskingHelper.MaskPhone(contact.Mobile);
                var maskedEmail = MaskingHelper.MaskEmail(contact.Email);

                // Template para SMS (texto plano)
                var smsTemplate = await SettingsHelper.GetStringSettingAsync(
                    _settingsRepo,
                    key: "OtpMessageTemplate",
                    defaultValue: "Su código de verificación es: {OTP}. Válido por {MINUTES} minutos. Portal Pacientes - Fundación Cardioinfantil.");

                // Template para Email (HTML)
                var emailTemplate = await SettingsHelper.GetStringSettingAsync(
                    _settingsRepo,
                    key: "OtpEmailTemplate",
                    defaultValue: "Su código de verificación es: {OTP}");

                var ttlMinutes = (ttlSeconds / 60).ToString();
                var smsMessage = smsTemplate
                    .Replace("{OTP}", otp, StringComparison.OrdinalIgnoreCase)
                    .Replace("{MINUTES}", ttlMinutes, StringComparison.OrdinalIgnoreCase);

                var emailMessage = emailTemplate
                    .Replace("{OTP}", otp, StringComparison.OrdinalIgnoreCase)
                    .Replace("{MINUTES}", ttlMinutes, StringComparison.OrdinalIgnoreCase);

                // Envío paralelo no bloqueante de SMS y Email
                var sendTasks = new List<Task>();

                if (!string.IsNullOrWhiteSpace(contact.Mobile))
                {
                    sendTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            challenge.DeliveredToSms = true;
                            await _smsSender.SendAsync(contact.Mobile!, smsMessage, ct).ConfigureAwait(false);
                            Logger.LogInformation("SMS enviado exitosamente a {Phone}", maskedPhone);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Error al enviar SMS a {Phone}, continuando...", maskedPhone);
                        }
                    }, ct));
                }

                if (!string.IsNullOrWhiteSpace(contact.Email))
                {
                    sendTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            challenge.DeliveredToEmail = true;
                            await _emailSender.SendAsync(contact.Email, "Código de verificación - Portal Paciente", emailMessage, ct).ConfigureAwait(false);
                            Logger.LogInformation("Email enviado exitosamente a {Email}", maskedEmail);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Error al enviar Email a {Email}, continuando...", maskedEmail);
                        }
                    }, ct));
                }

                // Esperar a que ambos terminen (éxito o fallo)
                if (sendTasks.Any())
                {
                    _ = Task.WhenAll(sendTasks).ConfigureAwait(false);
                }

                await _otpRepo.UpdateAsync(challenge).ConfigureAwait(false);

                return new ValidateAuthResponse(
                    challenge.Id,
                    maskedPhone,
                    maskedEmail,
                    contact.FullName,
                    contact.History,
                    null
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al generar OTP para {DocType}-{DocNumber}",
                    request.DocTypeCode, request.DocNumber);
                throw;
            }
        }

        public async Task<ResendOtpResponse> ResendOtpAsync(ResendOtpRequest request, CancellationToken ct = default)
        {
            Logger.LogInformation(
                "Reenviando OTP para challenge {ChallengeId} desde IP {ClientIp}",
                request.ChallengeId, request.ClientIp);

            try
            {
                var challenge = await _otpRepo.FindByIdAsync(request.ChallengeId).ConfigureAwait(false);
                if (challenge == null)
                {
                    Logger.LogWarning("Challenge {ChallengeId} no encontrado", request.ChallengeId);
                    throw new KeyNotFoundException("Challenge no encontrado");
                }

                var docType = await _docTypeRepo.FindByIdAsync(challenge.DocTypeId).ConfigureAwait(false);
                if (docType == null)
                {
                    Logger.LogWarning("Tipo de documento no encontrado para challenge {ChallengeId}", request.ChallengeId);
                    throw new InvalidOperationException("Tipo de documento inválido");
                }

                var contact = await _externalPatient.GetContactAsync(docType.Code, challenge.DocNumber, ct).ConfigureAwait(false);
                if (contact == null || (string.IsNullOrWhiteSpace(contact.Mobile) && string.IsNullOrWhiteSpace(contact.Email)))
                {
                    Logger.LogWarning(
                        "No se encontraron medios de contacto para reenvío de challenge {ChallengeId}",
                        request.ChallengeId);
                    throw new KeyNotFoundException("No se encontraron medios de contacto");
                }

                var ttlSeconds = await SettingsHelper.GetIntSettingAsync(_settingsRepo, "OtpTtlSeconds", 300);
                var now = DateTime.UtcNow;
                var otp = GenerateOtp(4);
                var codeHash = HashOtp(otp);

                var newChallenge = new OtpChallenge
                {
                    DocTypeId = challenge.DocTypeId,
                    DocNumber = challenge.DocNumber,
                    UserId = challenge.UserId,
                    CodeHash = codeHash,
                    ExpiresAt = now.AddSeconds(ttlSeconds),
                    ClientIp = request.ClientIp,
                    DeliveredToEmail = false,
                    DeliveredToSms = false
                };

                newChallenge = await _otpRepo.AddAsync(newChallenge).ConfigureAwait(false);

                Logger.LogInformation(
                    "Nuevo OTP challenge creado con ID: {NewChallengeId} reemplazando {OldChallengeId}",
                    newChallenge.Id, request.ChallengeId);

                // Template para SMS (texto plano)
                var smsTemplate = await SettingsHelper.GetStringSettingAsync(
                    _settingsRepo,
                    key: "OtpMessageTemplate",
                    defaultValue: "Su código de verificación es: {OTP}. Válido por {MINUTES} minutos. Portal Pacientes - Fundación Cardioinfantil.");

                // Template para Email (HTML)
                var emailTemplate = await SettingsHelper.GetStringSettingAsync(
                    _settingsRepo,
                    key: "OtpEmailTemplate",
                    defaultValue: "Su código de verificación es: {OTP}");

                var ttlMinutes = (ttlSeconds / 60).ToString();
                var smsMessage = smsTemplate
                    .Replace("{OTP}", otp, StringComparison.OrdinalIgnoreCase)
                    .Replace("{MINUTES}", ttlMinutes, StringComparison.OrdinalIgnoreCase);

                var emailMessage = emailTemplate
                    .Replace("{OTP}", otp, StringComparison.OrdinalIgnoreCase)
                    .Replace("{MINUTES}", ttlMinutes, StringComparison.OrdinalIgnoreCase);

                var maskedPhone = MaskingHelper.MaskPhone(contact.Mobile);
                var maskedEmail = MaskingHelper.MaskEmail(contact.Email);

                // Envío paralelo no bloqueante de SMS y Email
                var sendTasks = new List<Task>();

                if (!string.IsNullOrWhiteSpace(contact.Mobile))
                {
                    sendTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            newChallenge.DeliveredToSms = true;
                            await _smsSender.SendAsync(contact.Mobile!, smsMessage, ct).ConfigureAwait(false);
                            Logger.LogInformation("SMS reenviado exitosamente a {Phone}", maskedPhone);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Error al reenviar SMS a {Phone}, continuando...", maskedPhone);
                        }
                    }, ct));
                }

                if (!string.IsNullOrWhiteSpace(contact.Email))
                {
                    sendTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            newChallenge.DeliveredToEmail = true;
                            await _emailSender.SendAsync(contact.Email!, "Código de verificación - Portal Pacientes", emailMessage, ct).ConfigureAwait(false);
                            Logger.LogInformation("Email reenviado exitosamente a {Email}", maskedEmail);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Error al reenviar Email a {Email}, continuando...", maskedEmail);
                        }
                    }, ct));
                }

                if (sendTasks.Any())
                {
                    _ = Task.WhenAll(sendTasks).ConfigureAwait(false);
                }

                await _otpRepo.UpdateAsync(newChallenge).ConfigureAwait(false);

                return new ResendOtpResponse(newChallenge.Id, maskedPhone, maskedEmail);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al reenviar OTP para challenge {ChallengeId}",
                    request.ChallengeId);
                throw;
            }
        }

        private static string GenerateOtp(int length)
        {
            const string digits = "0123456789";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var chars = bytes.Select(b => digits[b % digits.Length]).ToArray();
            return new string(chars);
        }

        private static string HashOtp(string otp)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hash);
        }
    }
}