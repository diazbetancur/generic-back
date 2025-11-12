using AutoMapper;
using CC.Aplication.Services;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio de aplicación para Request con lógica de negocio específica
    /// </summary>
    public class RequestService : ServiceBase<Request, RequestDto>, IRequestService
    {
        private readonly IRequestRepository _requestRepo;
        private readonly IDocTypeRepository _docTypeRepo;
        private readonly IStateRepository _stateRepo;
        private readonly IHistoryRequestRepository _historyRepo;
        private readonly IMapper _mapper;

        public RequestService(
            IRequestRepository requestRepository,
            IDocTypeRepository docTypeRepository,
            IStateRepository stateRepository,
            IHistoryRequestRepository historyRepository,
            IMapper mapper,
            ILogger<RequestService> logger)
            : base(requestRepository, mapper, logger)
        {
            _requestRepo = requestRepository;
            _docTypeRepo = docTypeRepository;
            _stateRepo = stateRepository;
            _historyRepo = historyRepository;
            _mapper = mapper;
        }

        public async Task<RequestDto> CreateRequestAsync(RequestCreateDto dto, CancellationToken ct = default)
        {
            Logger.LogInformation("Creando nueva solicitud: DocType={DocType}, DocNumber={DocNumber}",
                dto.DocTypeCode, dto.DocNumber);

            var docType = await _docTypeRepo.FindByAlternateKeyAsync(
                d => d.Code == dto.DocTypeCode && d.IsActive);

            if (docType == null)
            {
                Logger.LogWarning("DocType no encontrado o inactivo: {DocTypeCode}", dto.DocTypeCode);
                throw new InvalidOperationException($"Tipo de documento '{dto.DocTypeCode}' no válido o inactivo");
            }

            var initialState = await _stateRepo.FindByAlternateKeyAsync(
                s => s.Name == "Recibida" && !s.IsDeleted);

            if (initialState == null)
            {
                Logger.LogError("Estado inicial 'Creado' no encontrado en la base de datos");
                throw new InvalidOperationException("Estado inicial 'Creado' no encontrado. Ejecute el seed de la base de datos.");
            }

            var request = new Request
            {
                Id = Guid.NewGuid(),
                DocTypeId = docType.Id,
                DocNumber = dto.DocNumber,
                RequestTypeId = dto.RequestTypeId,
                Description = dto.Description,
                StateId = initialState.Id,
                AssignedUserId = null,
                DateCreated = DateTime.UtcNow
            };

            var createdRequest = await _requestRepo.AddAsync(request);
            Logger.LogInformation("Solicitud creada con ID: {RequestId}", createdRequest.Id);

            var history = new HistoryRequest
            {
                Id = Guid.NewGuid(),
                RequestId = createdRequest.Id,
                OldStateId = initialState.Id,
                NewStateId = null,
                UserId = null,
                Changes = dto.Description,
                DateCreated = DateTime.UtcNow
            };

            await _historyRepo.AddAsync(history);
            Logger.LogInformation("Historial inicial creado para solicitud: {RequestId}", createdRequest.Id);

            var fullRequest = await _requestRepo.FindByAlternateKeyAsync(
                r => r.Id == createdRequest.Id,
                includeProperties: "DocType,RequestType,State");

            var result = _mapper.Map<RequestDto>(fullRequest);
            return result;
        }

        public async Task<IEnumerable<RequestDto>> GetByPatientAsync(
            string docTypeCode,
            string docNumber,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation("Consultando solicitudes por paciente: {DocType}-{DocNumber}", docTypeCode, docNumber);

            from ??= DateTime.UtcNow.AddYears(-1);
            to ??= DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var results = await _requestRepo.GetAllAsync(
                filter: r => r.DocType.Code == docTypeCode
                          && r.DocNumber == docNumber
                          && r.DateCreated >= from
                          && r.DateCreated <= to,
                orderBy: q => q.OrderByDescending(r => r.DateCreated),
                includeProperties: "DocType,RequestType,State"
            );

            Logger.LogInformation("Encontradas {Count} solicitudes para el paciente", results.Count());
            return _mapper.Map<IEnumerable<RequestDto>>(results);
        }

        public async Task<(IEnumerable<RequestDto> Items, int TotalCount)> GetFilteredAsync(
            RequestListQueryDto query,
            CancellationToken ct = default)
        {
            Logger.LogInformation("Consultando solicitudes con filtros: StateId={StateId}, RequestTypeId={RequestTypeId}",
                query.StateId, query.RequestTypeId);

            Expression<Func<Request, bool>> filter = r => true;

            if (!string.IsNullOrWhiteSpace(query.DocTypeCode))
                filter = CombineExpressions(filter, r => r.DocType.Code == query.DocTypeCode);

            if (!string.IsNullOrWhiteSpace(query.DocNumber))
                filter = CombineExpressions(filter, r => r.DocNumber == query.DocNumber);

            if (query.StateId.HasValue)
                filter = CombineExpressions(filter, r => r.StateId == query.StateId.Value);

            if (query.RequestTypeId.HasValue)
                filter = CombineExpressions(filter, r => r.RequestTypeId == query.RequestTypeId.Value);

            if (query.AssignedUserId.HasValue)
                filter = CombineExpressions(filter, r => r.AssignedUserId == query.AssignedUserId.Value);

            var from = query.DateFrom ?? DateTime.UtcNow.AddYears(-1);
            var to = query.DateTo ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
            filter = CombineExpressions(filter, r => r.DateCreated >= from && r.DateCreated <= to);

            var totalCount = await _requestRepo.AnyAsync(filter) ?
                (await _requestRepo.GetAllAsync(filter)).Count() : 0;

            var items = await _requestRepo.GetAllPagedAsync(
                query.Take,
                query.Skip,
                filter,
                orderBy: q => q.OrderByDescending(r => r.DateCreated),
                includeProperties: "DocType,RequestType,State,AssignedUser"
            );

            Logger.LogInformation("Encontradas {TotalCount} solicitudes totales, retornando {ItemCount} items",
                totalCount, items.Count());

            return (_mapper.Map<IEnumerable<RequestDto>>(items), totalCount);
        }

        public async Task<IEnumerable<HistoryRequestDto>> GetHistoryAsync(Guid requestId, CancellationToken ct = default)
        {
            Logger.LogInformation("Consultando historial de solicitud: {RequestId}", requestId);

            var history = await _historyRepo.GetAllAsync(
                filter: h => h.RequestId == requestId,
                orderBy: q => q.OrderByDescending(h => h.DateCreated),
                includeProperties: "OldState,NewState,User"
            );

            return _mapper.Map<IEnumerable<HistoryRequestDto>>(history);
        }

        /// <summary>
        /// Combina dos expresiones con AND
        /// </summary>
        private Expression<Func<Request, bool>> CombineExpressions(
            Expression<Func<Request, bool>> first,
            Expression<Func<Request, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(Request));
            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );
            return Expression.Lambda<Func<Request, bool>>(combined, parameter);
        }
    }
}