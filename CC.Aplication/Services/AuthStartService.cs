using CC.Aplication.Helpers;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace CC.Aplication.Services
{
    public class AuthStartService : IAuthStartService
    {
        private readonly IDocTypeRepository _docTypeRepo;
        private readonly IOtpChallengeRepository _otpRepo;
        private readonly IGeneralSettingsRepository _settingsRepo;
        private readonly IExternalPatientService _externalPatient;
        private readonly ISmsSender _smsSender;
        private readonly IEmailSender _emailSender;

        public AuthStartService(
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

        public async Task<StartAuthResponse> StartAsync(StartAuthRequest request, CancellationToken ct = default)
        {
            var docType = await _docTypeRepo.FindByAlternateKeyAsync(d => d.Code == request.DocTypeCode).ConfigureAwait(false);
            if (docType == null) throw new InvalidOperationException("Tipo de documento inválido");

            var contact = await _externalPatient.GetContactAsync(request.DocTypeCode, request.DocNumber, ct).ConfigureAwait(false);
            if (contact == null || (string.IsNullOrWhiteSpace(contact.Mobile) && string.IsNullOrWhiteSpace(contact.Email)))
            {
                throw new KeyNotFoundException("No se encontraron medios de contacto");
            }

            var otp = GenerateOtp(4);
            var ttlMinutes = await GetIntSettingAsync("OtpTtlMinutes", 5);
            var codeHash = HashOtp(otp);
            var now = DateTime.UtcNow;
            var challenge = new OtpChallenge
            {
                DocTypeId = docType.Id,
                DocNumber = request.DocNumber,
                UserId = $"{request.DocTypeCode}-{request.DocNumber}",
                CodeHash = codeHash,
                ExpiresAt = now.AddMinutes(ttlMinutes),
                ClientIp = request.ClientIp,
                DeliveredToEmail = false,
                DeliveredToSms = false
            };

            challenge = await _otpRepo.AddAsync(challenge).ConfigureAwait(false);

            // Enmascarar
            var maskedPhone = MaskPhone(contact.Mobile);
            var maskedEmail = MaskEmail(contact.Email);

            // Enviar OTP por ambos canales disponibles (placeholder)
            var message = $"Su código OTP es: {otp}"; // TODO: plantilla por entorno
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

            return new StartAuthResponse(challenge.Id, maskedPhone, maskedEmail);
        }

        public async Task<ResendOtpResponse> ResendAsync(ResendOtpRequest request, CancellationToken ct = default)
        {
            // Buscar challenge
            var challenge = await _otpRepo.FindByIdAsync(request.ChallengeId).ConfigureAwait(false);
            if (challenge == null) throw new KeyNotFoundException("Challenge no encontrado");

            // Buscar docType para reconstruir contactos
            var docType = await _docTypeRepo.FindByIdAsync(challenge.DocTypeId).ConfigureAwait(false);
            if (docType == null) throw new InvalidOperationException("Tipo de documento inválido");

            var contact = await _externalPatient.GetContactAsync(docType.Code, challenge.DocNumber, ct).ConfigureAwait(false);
            if (contact == null || (string.IsNullOrWhiteSpace(contact.Mobile) && string.IsNullOrWhiteSpace(contact.Email)))
            {
                throw new KeyNotFoundException("No se encontraron medios de contacto");
            }

            var userId = challenge.UserId;
            var ttlMinutes = await GetIntSettingAsync("OtpTtlMinutes", 5);
            var now = DateTime.UtcNow;

            // Generar nuevo OTP
            var otp = GenerateOtp(4);
            var codeHash = HashOtp(otp);

            // Crear challenge nuevo
            var newChallenge = new OtpChallenge
            {
                DocTypeId = challenge.DocTypeId,
                DocNumber = challenge.DocNumber,
                UserId = userId,
                CodeHash = codeHash,
                ExpiresAt = now.AddMinutes(ttlMinutes),
                ClientIp = request.ClientIp,
                DeliveredToEmail = false,
                DeliveredToSms = false
            };

            newChallenge = await _otpRepo.AddAsync(newChallenge).ConfigureAwait(false);

            // Enviar OTP (placeholder)
            var message = $"Su código OTP es: {otp}";
            if (!string.IsNullOrWhiteSpace(contact.Mobile))
            {
                await _smsSender.SendAsync(contact.Mobile!, message, ct).ConfigureAwait(false);
                newChallenge.DeliveredToSms = true;
            }
            if (!string.IsNullOrWhiteSpace(contact.Email))
            {
                await _emailSender.SendAsync(contact.Email!, "Código de verificación", message, ct).ConfigureAwait(false);
                newChallenge.DeliveredToEmail = true;
            }

            await _otpRepo.UpdateAsync(newChallenge).ConfigureAwait(false);

            return new ResendOtpResponse(newChallenge.Id, MaskPhone(contact.Mobile), MaskEmail(contact.Email));
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

        private static string? MaskPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var last2 = phone.Length >= 2 ? phone[^2..] : phone;
            return new string('*', Math.Max(0, phone.Length - last2.Length)) + last2;
        }

        private static string? MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var at = email.IndexOf('@');
            if (at <= 0) return "********"; // fallback
            var local = email.Substring(0, at);
            var domain = email.Substring(at);
            var last2 = local.Length >= 2 ? local[^2..] : local;
            return new string('*', Math.Max(0, local.Length - last2.Length)) + last2 + domain;
        }

        private async Task<int> GetIntSettingAsync(string key, int @default)
        {
            return await SettingsHelper.GetIntSettingAsync(_settingsRepo, key, @default).ConfigureAwait(false);
        }
    }
}
