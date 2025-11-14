using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Aplication.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly IDataPolicyAcceptanceService _dataPolicyService;
        private readonly UserManager<User> _userManager;
        private readonly ILoginAttemptRepository _loginAttemptRepo;
        private readonly IPasswordResetService _passwordResetService;
        private readonly JwtTokenGenerator _jwtGenerator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IOtpChallengeService otpService,
            IAuthVerifyService verifyService,
            IDataPolicyAcceptanceService dataPolicyService,
            UserManager<User> userManager,
            ILoginAttemptRepository loginAttemptRepo,
            IPasswordResetService passwordResetService,
            JwtTokenGenerator jwtGenerator,
            ILogger<AuthController> logger)
        {
            _otpService = otpService;
            _verifyService = verifyService;
            _dataPolicyService = dataPolicyService;
            _userManager = userManager;
            _loginAttemptRepo = loginAttemptRepo;
            _passwordResetService = passwordResetService;
            _jwtGenerator = jwtGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Login para usuarios administrativos (usuario/contraseña)
        /// </summary>
        [HttpPost("admin/login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { error = "Usuario y contraseña son requeridos" });
                }

                _logger.LogInformation("Intento de login admin para usuario: {UserName}", request.UserName);

                var user = await _userManager.FindByNameAsync(request.UserName);

                if (user == null || user.IsDeleted)
                {
                    await LogLoginAttempt(request.UserName, false, "Usuario no encontrado o eliminado");
                    _logger.LogWarning("Intento de login fallido: Usuario {UserName} no existe o está eliminado", request.UserName);
                    return Unauthorized(new { error = "Usuario o contraseña incorrectos" });
                }

                // Verificar si el usuario está bloqueado
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await LogLoginAttempt(request.UserName, false, "Usuario bloqueado");
                    _logger.LogWarning("Intento de login: Usuario {UserName} está bloqueado hasta {LockoutEnd}",
                        request.UserName, user.LockoutEnd);

                    return Unauthorized(new
                    {
                        error = "Usuario bloqueado por múltiples intentos fallidos",
                        lockoutEnd = user.LockoutEnd
                    });
                }

                // Verificar contraseña
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

                if (!passwordValid)
                {
                    await _userManager.AccessFailedAsync(user);
                    await LogLoginAttempt(request.UserName, false, "Contraseña incorrecta");

                    var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                    _logger.LogWarning("Contraseña incorrecta para {UserName}. Intentos fallidos: {FailedCount}",
                        request.UserName, failedCount);

                    return Unauthorized(new
                    {
                        error = "Usuario o contraseña incorrectos",
                        remainingAttempts = 5 - failedCount
                    });
                }

                // Reset intentos fallidos en login exitoso
                await _userManager.ResetAccessFailedCountAsync(user);

                // Obtener roles del usuario
                var roles = await _userManager.GetRolesAsync(user);

                // Generar JWT usando el generador centralizado
                var token = _jwtGenerator.GenerateAdminToken(user, roles);

                await LogLoginAttempt(request.UserName, true, "Login exitoso");

                _logger.LogInformation("Login exitoso para usuario admin: {UserName} con roles: {Roles}",
                    request.UserName, string.Join(", ", roles));

                return Ok(new AdminLoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtGenerator.GetTokenLifetimeMinutes()),
                    User = new AdminUserInfo
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = roles.ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login admin para usuario: {UserName}", request.UserName);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        private async Task LogLoginAttempt(string userName, bool success, string? reason = null)
        {
            try
            {
                var loginAttempt = new LoginAttempt
                {
                    DocTypeCode = "ADMIN",
                    DocNumber = userName,
                    Success = success,
                    Reason = reason,
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    TraceId = HttpContext.TraceIdentifier,
                    UserId = userName
                };

                await _loginAttemptRepo.AddAsync(loginAttempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar intento de login para {UserName}", userName);
            }
        }

        /// <summary>
        /// Valida si un paciente existe, genera y envía OTP, retorna datos enmascarados
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate(ValidateAuthRequest request)
        {
            await _dataPolicyService.AddAsync(new DataPolicyAcceptanceDto
            {
                DocTypeCode = request.DocTypeCode,
                DocNumber = request.DocNumber,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                PolicyVersion = "1.0"
            });
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

        /// <summary>
        /// Solicita recuperación de contraseña - Envía código por SMS y Email
        /// </summary>
        [HttpPost("admin/forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
            {
                return BadRequest(new { error = "Usuario o email es requerido" });
            }

            var result = await _passwordResetService.RequestPasswordResetAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Verifica código y establece nueva contraseña
        /// </summary>
        [HttpPost("admin/reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request.ResetTokenId == Guid.Empty)
            {
                return BadRequest(new { error = "Token de recuperación inválido" });
            }

            if (string.IsNullOrWhiteSpace(request.VerificationCode))
            {
                return BadRequest(new { error = "Código de verificación es requerido" });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { error = "Nueva contraseña es requerida" });
            }

            var result = await _passwordResetService.ResetPasswordAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}