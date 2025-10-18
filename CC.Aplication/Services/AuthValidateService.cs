using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Helpers;
using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Aplication.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace CC.Aplication.Services
{
    public class AuthValidateService : IAuthValidateService
    {
        private readonly IDocTypeRepository _docTypeRepo;
        private readonly IOtpChallengeRepository _otpRepo;
        private readonly IGeneralSettingsRepository _settingsRepo;
        private readonly IExternalPatientService _externalPatient;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;

        public AuthValidateService(
            IDocTypeRepository docTypeRepo,
            IOtpChallengeRepository otpRepo,
            IGeneralSettingsRepository settingsRepo,
            IExternalPatientService externalPatient,
            ISmsSender smsSender,
            IEmailSender emailSender)
        {
            _docTypeRepo = docTypeRepo;
            _otpRepo = otpRepo;
            _settingsRepo = settingsRepo;
            _externalPatient = externalPatient;
            _smsSender = smsSender;
            _emailSender = emailSender;
        }

        public async Task<ValidateAuthResponse> ValidateAsync(ValidateAuthRequest request, CancellationToken ct = default)
        {
            var docType = await _docTypeRepo.FindByAlternateKeyAsync(d => d.Code == request.DocTypeCode).ConfigureAwait(false);
            if (docType == null) throw new InvalidOperationException("Tipo de documento inválido");

            var contact = await _externalPatient.GetContactAsync(request.DocTypeCode, request.DocNumber, ct).ConfigureAwait(false);

            if (contact == null || (string.IsNullOrWhiteSpace(contact.Mobile) && string.IsNullOrWhiteSpace(contact.Email)))
            {
                var mensaje = await SettingsHelper.GetStringSettingAsync(
                    _settingsRepo,
                    "MensajeSinContacto",
                    "No se encontraron medios de contacto. Por favor comuníquese con el servicio de atención.");
                throw new KeyNotFoundException(mensaje);
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

            var maskedPhone = MaskingHelper.MaskPhone(contact.Mobile);
            var maskedEmail = MaskingHelper.MaskEmail(contact.Email);

            //TODO: Mensaje para el envio del correo o el SMS desde configuración
            var message = $"Su código OTP es: {otp}";
            if (!string.IsNullOrWhiteSpace(contact.Mobile))
            {
                await _smsSender.SendAsync(contact.Mobile!, message, ct).ConfigureAwait(false);
                challenge.DeliveredToSms = true;
            }
            if (!string.IsNullOrWhiteSpace(contact.Email))
            {
                await _emailSender.SendAsync(contact.Email!, "Código de verificación", message, ct).ConfigureAwait(false);
                challenge.DeliveredToEmail = true;
            }

            await _otpRepo.UpdateAsync(challenge).ConfigureAwait(false);

            return new ValidateAuthResponse(
                challenge.Id,
                maskedPhone,
                maskedEmail,
                contact.FullName,
                contact.History
            );
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