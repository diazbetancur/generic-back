using CC.Domain.Contracts;

namespace CC.Domain.Interfaces.External
{
    /// <summary>
    /// Interfaz para servicio NilRead (Informes PDF e Imágenes Diagnósticas)
    /// </summary>
    public interface INilReadService
    {
        /// <summary>
        /// Obtiene la lista paginada de exámenes de un paciente
        /// </summary>
        /// <param name="patientId">ID del paciente (ej: CC1014306921)</param>
        /// <param name="limit">Cantidad de registros (default 10, max 50)</param>
        /// <param name="offset">Posición inicial para paginación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de exámenes con metadatos</returns>
        Task<NilReadExamsResult> GetPatientExamsAsync(
            string patientId,
            int limit = 10,
            int offset = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el PDF de un informe
        /// </summary>
        /// <param name="dateFolder">Carpeta de fecha en formato yyyyMMdd</param>
        /// <param name="filename">Nombre del archivo PDF</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Contenido del PDF en bytes</returns>
        Task<NilReadReportResult> GetReportPdfAsync(
            string dateFolder,
            string filename,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un enlace temporal para visualizar imágenes en NilRead Viewer
        /// </summary>
        /// <param name="accession">Número de accesión del examen</param>
        /// <param name="patientId">ID del paciente</param>
        /// <param name="dateTime">Timestamp en formato YYYYMMDDHHmmss (opcional)</param>
        /// <param name="accNumbers">Lista de números de accesión</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>URL del visor con token temporal</returns>
        Task<NilReadViewerLinkResult> GenerateViewerLinkAsync(
            string accession,
            string patientId,
            string? dateTime = null,
            List<string>? accNumbers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica disponibilidad del servicio NilRead
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}
