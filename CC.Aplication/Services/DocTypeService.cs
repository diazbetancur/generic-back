using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de tipos de documento
    /// </summary>
    public class DocTypeService : ServiceBase<DocType, DocTypeDto>, IDocTypeService
    {
        public DocTypeService(
            IDocTypeRepository repository, 
            IMapper mapper,
            ILogger<DocTypeService> logger) 
            : base(repository, mapper, logger)
        {
        }
    }
}