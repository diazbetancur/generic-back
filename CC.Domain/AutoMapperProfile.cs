using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<FrecuentQuestions, FrecuentQuestionsDto>().ReverseMap();
            CreateMap<CardioTV, CardioTVDto>().ReverseMap();
            CreateMap<Question, QuestionDto>().ReverseMap();
            CreateMap<ResponseQuestion, ResponseQuestionDto>().ReverseMap()
                .ForMember(dest => dest.Question, opt => opt.Ignore());
        }
    }
}