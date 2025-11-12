using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz de servicio extendida para Request con lógica de negocio específica
    /// </summary>
    public interface IRequestService : IServiceBase<Request, RequestDto>
    {
        /// <summary>
        /// Crea una nueva solicitud con toda la lógica de negocio
        /// (buscar DocType por código, asignar estado inicial, crear historial)
        /// </summary>
        Task<RequestDto> CreateRequestAsync(RequestCreateDto dto, CancellationToken ct = default);

        /// <summary>
        /// Obtiene solicitudes de un paciente específico por documento
        /// </summary>
        /// <param name="docTypeCode">Código del tipo de documento</param>
        /// <param name="docNumber">Número de documento</param>
        /// <param name="from">Fecha desde (default: último año)</param>
        /// <param name="to">Fecha hasta (default: hoy)</param>
        Task<IEnumerable<RequestDto>> GetByPatientAsync(
            string docTypeCode,
            string docNumber,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene solicitudes con filtros generales (para admin)
        /// </summary>
        Task<(IEnumerable<RequestDto> Items, int TotalCount)> GetFilteredAsync(
            RequestListQueryDto query,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene el historial completo de una solicitud
        /// </summary>
        Task<IEnumerable<HistoryRequestDto>> GetHistoryAsync(Guid requestId, CancellationToken ct = default);
    }
}
