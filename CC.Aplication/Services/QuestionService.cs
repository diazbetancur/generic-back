using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de preguntas y encuestas
    /// </summary>
    public class QuestionService : ServiceBase<Question, QuestionDto>, IQuestionService
    {
        public QuestionService(
            IQuestionRepository repository, 
            IMapper mapper,
            ILogger<QuestionService> logger) 
            : base(repository, mapper, logger)
        {
        }
    }
}

