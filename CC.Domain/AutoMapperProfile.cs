using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain
{
    /// <summary>
    /// Perfil de AutoMapper para configuración de mapeos entre entidades y DTOs
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapeos de entidades CRUD
            CreateMap<FrequentQuestions, FrequentQuestionsDto>().ReverseMap();
            CreateMap<CardioTV, CardioTVDto>().ReverseMap();
            CreateMap<Question, QuestionDto>().ReverseMap();
            CreateMap<ResponseQuestion, ResponseQuestionDto>().ReverseMap()
                .ForMember(dest => dest.Question, opt => opt.Ignore());
            CreateMap<GeneralSettings, GeneralSettingsDto>().ReverseMap();
            CreateMap<DocType, DocTypeDto>().ReverseMap();
            CreateMap<DataPolicyAcceptance, DataPolicyAcceptanceDto>().ReverseMap();

            // Mapeo de TelemetryLog para telemetría de la aplicación
            CreateMap<TelemetryLog, TelemetryDto>().ReverseMap();

            // Mapeo de OtpChallenge con manejo especial de CodeHash
            CreateMap<OtpChallenge, OtpChallengeDto>()
                .ReverseMap()
                .ForMember(dest => dest.CodeHash, opt => opt.Ignore());
        }
    }
}