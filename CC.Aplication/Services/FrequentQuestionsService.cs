using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    public class FrequentQuestionsService : ServiceBase<FrequentQuestions, FrequentQuestionsDto>, IFrequentQuestionsService
    {
        public FrequentQuestionsService(
            IFrequentQuestionsRepository repository,
            IMapper mapper,
            ILogger<FrequentQuestionsService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}