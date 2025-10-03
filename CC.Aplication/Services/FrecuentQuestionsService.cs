using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;

namespace CC.Aplication.Services
{
    public class FrecuentQuestionsService : ServiceBase<FrecuentQuestions, FrecuentQuestionsDto>, IFrecuentQuestionsService
    {
        public FrecuentQuestionsService(IFrecuentQuestionsRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}