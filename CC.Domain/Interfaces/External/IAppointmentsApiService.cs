using CC.Domain.Dtos.ExternalApis;

namespace CC.Domain.Interfaces.External
{
    /// <summary>
    /// Interfaz para el servicio de API de Citas Médicas
    /// </summary>
    public interface IAppointmentsApiService
    {
        /// <summary>
        /// Verifica el estado de salud de la API
        /// </summary>
        Task<ApiHealthDto?> CheckHealthAsync(CancellationToken ct = default);

        /// <summary>
        /// Obtiene todas las citas futuras de un paciente
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="ct">Token de cancelación</param>
        Task<AppointmentsResponseDto?> GetPatientAppointmentsAsync(
            string patientId,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene las citas de un paciente con paginación
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="limit">Cantidad de registros por página (máximo 200)</param>
        /// <param name="offset">Número de registros a saltar</param>
        /// <param name="ct">Token de cancelación</param>
        Task<AppointmentsPaginatedResponseDto?> GetPatientAppointmentsPaginatedAsync(
            string patientId,
            int limit = 50,
            int offset = 0,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene información de la versión de la API
        /// </summary>
        Task<ApiVersionDto?> GetVersionAsync(CancellationToken ct = default);
    }
}
