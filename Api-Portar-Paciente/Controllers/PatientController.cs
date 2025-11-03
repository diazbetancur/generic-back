using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>
        /// Obtiene los datos de contacto del paciente en claro
        /// </summary>
        /// <param name="docTypeCode">Código de tipo de documento</param>
        /// <param name="docNumber">Número de documento</param>
        [HttpGet("{docTypeCode}/{docNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetContact(string docTypeCode, string docNumber, CancellationToken ct)
        {
            var data = await _patientService.GetPatientInformationAsync(docTypeCode, docNumber, ct).ConfigureAwait(false);
            return Ok(data);
        }
    }
}