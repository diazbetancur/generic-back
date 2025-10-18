using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de configuraciones generales de la aplicación
    /// </summary>
    public class GeneralSettingService : ServiceBase<GeneralSettings, GeneralSettingsDto>, IGeneralSettingsService
    {
        public GeneralSettingService(
            IGeneralSettingsRepository repository, 
            IMapper mapper,
            ILogger<GeneralSettingService> logger) 
            : base(repository, mapper, logger)
        {
        }
    }
}