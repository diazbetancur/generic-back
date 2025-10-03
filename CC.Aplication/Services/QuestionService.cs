using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CC.Aplication.Services
{
    public class QuestionService : ServiceBase<Question, QuestionDto>, IQuestionService
    {

        public QuestionService(IQuestionRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}

