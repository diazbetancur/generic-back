using CC.Domain.Dtos.ExternalApis;

namespace CC.Domain.Interfaces.External
{
    /// <summary>
    /// Interfaz para el servicio de API de Historia Clínica
    /// </summary>
    public interface IClinicalHistoryApiService
    {
        /// <summary>
        /// Verifica el estado de salud de la API
        /// </summary>
        Task<ApiHealthDto?> CheckHealthAsync(CancellationToken ct = default);

        /// <summary>
        /// Obtiene los episodios de un paciente
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="onlyPdf">Retornar solo episodios con PDF disponible</param>
        /// <param name="limit">Límite de resultados</param>
        /// <param name="offset">Offset para paginación</param>
        /// <param name="ct">Token de cancelación</param>
        Task<EpisodesResponseDto?> GetPatientEpisodesAsync(
            string patientId,
            bool onlyPdf = false,
            int? limit = null,
            int? offset = null,
            CancellationToken ct = default);

        /// <summary>
        /// Descarga el PDF de historia clínica de un episodio
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="episode">Número de episodio</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Stream del archivo PDF o null si no existe</returns>
        Task<Stream?> DownloadClinicalHistoryPdfAsync(
            string patientId,
            string episode,
            CancellationToken ct = default);

        /// <summary>
        /// Descarga el documento PDF de incapacidad de un episodio
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="episode">Número de episodio</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Stream del archivo PDF o null si no existe</returns>
        Task<Stream?> DownloadIncapacityPdfAsync(
            string patientId,
            string episode,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene información de la versión de la API
        /// </summary>
        Task<ApiVersionDto?> GetVersionAsync(CancellationToken ct = default);
    }
}
