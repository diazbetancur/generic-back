using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;

namespace CC.Aplication.Services
{
  public class ResponseQuestionService : ServiceBase<ResponseQuestion, ResponseQuestionDto>, IResponseQuestionService
  {
    public ResponseQuestionService(IResponseQuestionRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
  }
}
