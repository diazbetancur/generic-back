using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Aplication.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using CC.Domain.Interfaces.Repositories;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para verificación de OTP y gestión de sesiones
    /// </summary>
    public class AuthVerifyService : IAuthVerifyService
    {
        private readonly IOtpChallengeRepository _otpRepo;
        private readonly IDocTypeRepository _docTypeRepo;
        private readonly ISessionsRepository _sessionRepo;
        private readonly IGeneralSettingsRepository _settingsRepo;
        private readonly IConfiguration _config;
        private readonly ILoginAttemptRepository _loginAttemptRepo;

        public AuthVerifyService(
            IOtpChallengeRepository otpRepo,
            IDocTypeRepository docTypeRepo,
            ISessionsRepository sessionRepo,
            IGeneralSettingsRepository settingsRepo,
            IConfiguration config,
            ILoginAttemptRepository loginAttemptRepo)
        {
            _otpRepo = otpRepo;
            _docTypeRepo = docTypeRepo;
            _sessionRepo = sessionRepo;
            _settingsRepo = settingsRepo;
            _config = config;
            _loginAttemptRepo = loginAttemptRepo;
        }

        public async Task<VerifyOtpResponse> VerifyAsync(VerifyOtpRequest request, CancellationToken ct = default)
        {
            var challenge = await _otpRepo.FindByIdAsync(request.ChallengeId).ConfigureAwait(false);
            if (challenge == null || challenge.UsedAt != null || challenge.ExpiresAt < DateTime.UtcNow)
            {
                await SaveAttempt(request, success: false, reason: "ChallengeInvalidOrExpired");
                throw new UnauthorizedAccessException("Challenge inválido o expirado");
            }

            var docType = await _docTypeRepo.FindByAlternateKeyAsync(d => d.Code == request.DocTypeCode).ConfigureAwait(false);
            if (docType == null || challenge.DocTypeId != docType.Id || challenge.DocNumber != request.DocNumber)
            {
                await SaveAttempt(request, success: false, reason: "IdentityMismatch");
                throw new UnauthorizedAccessException("Identidad no coincide");
            }

            if (!VerifyOtpHash(challenge.CodeHash, request.Otp))
            {
                challenge.FailedAttempts++;
                await _otpRepo.UpdateAsync(challenge).ConfigureAwait(false);

                await SaveAttempt(request, success: false, reason: "InvalidOtp");

                if (challenge.FailedAttempts >= 3)
                {
                    challenge.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
                    await _otpRepo.UpdateAsync(challenge).ConfigureAwait(false);
                    throw new UnauthorizedAccessException("OTP inválido. Máximo de intentos alcanzado, solicite reenvío.");
                }

                throw new UnauthorizedAccessException("OTP inválido");
            }

            challenge.UsedAt = DateTime.UtcNow;
            await _otpRepo.UpdateAsync(challenge).ConfigureAwait(false);

            var userId = $"{request.DocTypeCode}-{request.DocNumber}";
            await InvalidateAllUserSessionsAsync(userId).ConfigureAwait(false);

            var tokenLifetimeMinutes = await SettingsHelper.GetIntSettingAsync(_settingsRepo, "TokenLifetimeMinutes", 30);
            var expires = DateTime.UtcNow.AddMinutes(tokenLifetimeMinutes);
            var jti = Guid.NewGuid().ToString("N");

            var token = BuildJwt(jti, userId, expires);

            var session = new Sessions
            {
                DocTypeId = docType.Id,
                DocNumber = request.DocNumber,
                UserId = userId,
                Jti = jti,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = expires,
                ClientIp = request.ClientIp,
                IsActive = true
            };
            await _sessionRepo.AddAsync(session).ConfigureAwait(false);

            await SaveAttempt(request, success: true, reason: "Success");

            return new VerifyOtpResponse(token, expires);
        }

        public async Task LogoutAsync(string jti, CancellationToken ct = default)
        {
            // Buscar sesión por jti
            var session = await _sessionRepo.FindByAlternateKeyAsync(s => s.Jti == jti).ConfigureAwait(false);
            if (session != null && session.IsActive)
            {
                session.IsActive = false;
                session.RevokedAt = DateTime.UtcNow;
                await _sessionRepo.UpdateAsync(session).ConfigureAwait(false);
            }
        }

        private async Task InvalidateAllUserSessionsAsync(string userId)
        {
            // Obtiene todas las sesiones activas del usuario y las revoca
            var active = await _sessionRepo.GetAllAsync(s => s.UserId == userId && s.IsActive).ConfigureAwait(false);
            foreach (var s in active)
            {
                s.IsActive = false;
                s.RevokedAt = DateTime.UtcNow;
            }
            if (active.Any())
            {
                await _sessionRepo.UpdateRangeAsync(active).ConfigureAwait(false);
            }
        }

        private bool VerifyOtpHash(string storedHash, string otp)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            var candidate = Convert.ToBase64String(hash);
            return candidate.Equals(storedHash, StringComparison.Ordinal);
        }

        private string BuildJwt(string jti, string sub, DateTime expires)
        {
            var secret = _config["Authentication:JwtSecret"] ?? throw new InvalidOperationException("JwtSecret no configurado");
            var issuer = _config["Authentication:Issuer"] ?? "PortalPacientes";
            var audience = _config["Authentication:Audience"] ?? "PortalPacientesUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateJwtSecurityToken(
                issuer: issuer,
                audience: audience,
                subject: new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, sub),
                    new Claim(JwtRegisteredClaimNames.Jti, jti)
                }),
                notBefore: DateTime.UtcNow,
                expires: expires,
                issuedAt: DateTime.UtcNow,
                signingCredentials: creds
            );
            return handler.WriteToken(token);
        }

        private async Task SaveAttempt(VerifyOtpRequest req, bool success, string reason)
        {
            try
            {
                var attempt = new LoginAttempt
                {
                    DocTypeCode = req.DocTypeCode,
                    DocNumber = req.DocNumber,
                    UserId = $"{req.DocTypeCode}-{req.DocNumber}",
                    Success = success,
                    Reason = reason,
                    Ip = req.ClientIp,
                    UserAgent = null,
                    TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
                };
                await _loginAttemptRepo.AddAsync(attempt).ConfigureAwait(false);
            }
            catch { }
        }
    }
}