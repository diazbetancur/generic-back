using CC.Domain.Contracts;

namespace CC.Domain.Interfaces.External
{
    /// <summary>
    /// Resultado de lista de estudios con paginación
    /// </summary>
    public record StudiesResult(
        bool Success,
        string? PatientId,
        int Offset,
        int Limit,
        int Count,
        int Total,
        int? NextOffset,
        List<XeroStudy>? Studies,
        string? ErrorMessage = null
    );

    /// <summary>
    /// Resultado de generación de enlace del visor
    /// </summary>
    public record ViewerLinkResult(
        bool Success,
        string? Token,
        string? ViewerUrl,
        DateTime? ExpiresAt,
        string? ErrorMessage = null
    );

    /// <summary>
    /// Servicio para integración con Xero Viewer (Visor de Imágenes Diagnósticas)
    /// </summary>
    public interface IXeroViewerService
    {
        /// <summary>
        /// Obtiene la lista de estudios de un paciente con paginación
        /// </summary>
        /// <param name="patientId">ID del paciente en Xero</param>
        /// <param name="limit">Número máximo de estudios por página (1-50)</param>
        /// <param name="offset">Posición inicial para paginación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de estudios con información de paginación</returns>
        Task<StudiesResult> GetPatientStudiesAsync(
            string patientId,
            int limit = 10,
            int offset = 0,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un enlace temporal para visualizar un estudio en el visor
        /// </summary>
        /// <param name="studyUid">UID del estudio</param>
        /// <param name="patientId">ID del paciente (opcional, para auditoría)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token y URL del visor con fecha de expiración</returns>
        Task<ViewerLinkResult> GenerateViewerLinkAsync(
            string studyUid,
            string? patientId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica que el servicio esté disponible
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}
