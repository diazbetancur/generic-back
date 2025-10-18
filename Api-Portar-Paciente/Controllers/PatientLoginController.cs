using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientLoginController : ControllerBase
    {
        private readonly IAuthValidateService _validateService;
        private readonly IAuthStartService _startService;

        public PatientLoginController(IAuthValidateService validateService, IAuthStartService startService)
        {
            _validateService = validateService;
            _startService = startService;
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate(ValidateAuthRequest request)
        {
            var result = await _validateService.ValidateAsync(request).ConfigureAwait(false);
            return Ok(result);
        }

        // Paso 2: Orquestación para iniciar OTP si el front decide continuar
        [HttpPost("start")]
        [AllowAnonymous]
        public async Task<IActionResult> Start(StartAuthRequest request)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var enriched = request with { ClientIp = clientIp };
            var result = await _startService.StartAsync(enriched).ConfigureAwait(false);
            return Ok(result);
        }
    }
}