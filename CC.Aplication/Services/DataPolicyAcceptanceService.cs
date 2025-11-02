using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    public class DataPolicyAcceptanceService : ServiceBase<DataPolicyAcceptance, DataPolicyAcceptanceDto>, IDataPolicyAcceptanceService
    {
        private readonly IDocTypeRepository _docTypeRepo;
        private readonly IDataPolicyAcceptanceRepository _dataPolicyAcceptanceRepo;

        public DataPolicyAcceptanceService(
            IDataPolicyAcceptanceRepository repository,
            IDocTypeRepository docTypeRepository,
            IMapper mapper,
            ILogger<DataPolicyAcceptanceService> logger)
            : base(repository, mapper, logger)
        {
            _dataPolicyAcceptanceRepo = repository;
            _docTypeRepo = docTypeRepository;
        }

        public override async Task<DataPolicyAcceptanceDto> AddAsync(DataPolicyAcceptanceDto entityDto)
        {
            try
            {
                var code = entityDto.DocTypeCode.Trim();
                var docNumber = entityDto.DocNumber.Trim();

                var docType = await _docTypeRepo.FindByAlternateKeyAsync(x => x.Code == code && x.IsActive)
                    .ConfigureAwait(false);
                if (docType == null)
                {
                    Logger.LogWarning("Tipo de documento no encontrado o inactivo para código: {DocTypeCode}", code);
                    return null;
                }

                entityDto.DoctTypeId = docType.Id;

                var exists = await _dataPolicyAcceptanceRepo.AnyAsync(x => x.DoctTypeId == entityDto.DoctTypeId && x.DocNumber == docNumber)
                    .ConfigureAwait(false);
                if (exists)
                {
                    return null;
                }
                return await base.AddAsync(entityDto).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error registrando aceptación de política para DocTypeCode={DocTypeCode}, DocNumber={DocNumber}, Error: {message}", entityDto?.DocTypeCode, entityDto?.DocNumber, ex.Message);
                return null;
            }
        }
    }
}