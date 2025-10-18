using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers.Base
{
    /// <summary>
    /// Controlador base genérico para operaciones CRUD estándar
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad del dominio</typeparam>
    /// <typeparam name="TDto">DTO de la entidad</typeparam>
    [ApiController]
    public abstract class ControllerBase<TEntity, TDto> : ControllerBase
        where TEntity : class
        where TDto : BaseDto<Guid>
    {
        protected readonly IServiceBase<TEntity, TDto> Service;

        protected ControllerBase(IServiceBase<TEntity, TDto> service)
        {
            Service = service;
        }

        /// <summary>
        /// Obtiene todos los registros
        /// </summary>
        [HttpGet]
        public virtual async Task<IActionResult> GetAllAsync()
        {
            var result = await Service.GetAllAsync().ConfigureAwait(false);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un registro por ID
        /// </summary>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await Service.FindByIdAsync(id).ConfigureAwait(false);
            if (result == null)
                return NotFound(new { message = "Registro no encontrado" });

            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo registro
        /// </summary>
        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] TDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Service.AddAsync(dto).ConfigureAwait(false);
            return Ok(result);
        }

        /// <summary>
        /// Actualiza un registro existente
        /// </summary>
        [HttpPut]
        public virtual async Task<IActionResult> Put([FromBody] TDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await Service.FindByIdAsync(dto.Id).ConfigureAwait(false);
            if (existing == null)
                return NotFound(new { message = "Registro no encontrado" });

            // Preservar DateCreated del registro existente
            dto.DateCreated = existing.DateCreated;

            await Service.UpdateAsync(dto).ConfigureAwait(false);
            return Ok(dto);
        }

        /// <summary>
        /// Elimina un registro por ID
        /// </summary>
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            var existing = await Service.FindByIdAsync(id).ConfigureAwait(false);
            if (existing == null)
                return NotFound(new { message = "Registro no encontrado" });

            await Service.DeleteAsync(existing).ConfigureAwait(false);
            return Ok(new { message = "Registro eliminado exitosamente", data = existing });
        }
    }
}