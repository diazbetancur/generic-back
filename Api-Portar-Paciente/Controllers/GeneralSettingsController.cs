using CC.Aplication.Services;
using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de configuraciones generales de la aplicación
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralSettingsController : ControllerBase
    {
        private readonly IGeneralSettingsService _generalSettingService;

        public GeneralSettingsController(IGeneralSettingsService generalSettingService)
        {
            _generalSettingService = generalSettingService;
        }

        /// <summary>
        /// Obtiene todas las configuraciones generales
        /// </summary>
        /// <returns>Lista de configuraciones</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var settings = await _generalSettingService.GetAllAsync().ConfigureAwait(false);
            return Ok(settings);
        }

        /// <summary>
        /// Obtiene una configuración por su ID
        /// </summary>
        /// <param name="id">ID de la configuración</param>
        /// <returns>Configuración encontrada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var setting = await _generalSettingService.FindByIdAsync(id).ConfigureAwait(false);
            
            if (setting == null)
                return NotFound(new { message = $"Configuración con ID {id} no encontrada" });

            return Ok(setting);
        }

        /// <summary>
        /// Actualiza una configuración existente
        /// </summary>
        /// <param name="generalSettings">Datos de la configuración a actualizar</param>
        /// <returns>Configuración actualizada</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromBody] GeneralSettingsDto generalSettings)
        {
            if (generalSettings == null)
                return BadRequest(new { message = "Datos de configuración son requeridos" });

            var response = await _generalSettingService.FindByIdAsync(generalSettings.Id).ConfigureAwait(false);

            if (response == null)
                return NotFound(new { message = $"Configuración con ID {generalSettings.Id} no encontrada" });

            generalSettings.DateCreated = response.DateCreated;
            await _generalSettingService.UpdateAsync(generalSettings).ConfigureAwait(false);
            
            return Ok(generalSettings);
        }
    }
}