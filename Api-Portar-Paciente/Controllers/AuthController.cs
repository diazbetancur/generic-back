using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador de autenticación y manejo de sesiones
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IOtpChallengeService _otpService;
        private readonly IAuthVerifyService _verifyService;

        public AuthController(
            IOtpChallengeService otpService,
            IAuthVerifyService verifyService)
        {
            _otpService = otpService;
            _verifyService = verifyService;
        }

        /// <summary>
        /// Valida si un paciente existe, genera y envía OTP, retorna datos enmascarados
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate(ValidateAuthRequest request)
        {
            var result = await _otpService.ValidateAndGenerateOtpAsync(request).ConfigureAwait(false);
            return Ok(result);
        }

        /// <summary>
        /// Reenvía un nuevo OTP al paciente
        /// </summary>
        [HttpPost("resend")]
        [AllowAnonymous]
        public async Task<IActionResult> Resend(ResendOtpRequest request)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var enriched = request with { ClientIp = clientIp };
            var result = await _otpService.ResendOtpAsync(enriched).ConfigureAwait(false);
            return Ok(result);
        }

        /// <summary>
        /// Verifica el OTP y genera un token de sesión
        /// </summary>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var enriched = request with { ClientIp = clientIp };
            var result = await _verifyService.VerifyAsync(enriched).ConfigureAwait(false);
            return Ok(result);
        }

        /// <summary>
        /// Cierra la sesión activa del usuario
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var jti = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) return Ok(new { message = "Sesión cerrada exitosamente" });

            await _verifyService.LogoutAsync(jti).ConfigureAwait(false);
            return Ok(new { message = "Sesión cerrada exitosamente" });
        }
    }
}