using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de respuestas a preguntas de encuestas
    /// </summary>
    public class ResponseQuestionService : ServiceBase<ResponseQuestion, ResponseQuestionDto>, IResponseQuestionService
    {
        public ResponseQuestionService(
            IResponseQuestionRepository repository, 
            IMapper mapper,
            ILogger<ResponseQuestionService> logger) 
            : base(repository, mapper, logger)
        {
        }
    }
}
