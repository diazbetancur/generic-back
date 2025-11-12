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

            CreateMap<DataPolicyAcceptance, DataPolicyAcceptanceDto>().ReverseMap()
                .ForMember(dest => dest.DocType, opt => opt.Ignore());
            CreateMap<RequestType, RequestTypeDto>().ReverseMap();
            CreateMap<State, StateDto>().ReverseMap();

            CreateMap<Request, RequestDto>()
                .ReverseMap()
                .ForMember(dest => dest.DocType, opt => opt.Ignore())
                .ForMember(dest => dest.RequestType, opt => opt.Ignore())
                .ForMember(dest => dest.State, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore());

            CreateMap<HistoryRequest, HistoryRequestDto>()
                .ReverseMap()
                .ForMember(dest => dest.Request, opt => opt.Ignore())
                .ForMember(dest => dest.OldState, opt => opt.Ignore())
                .ForMember(dest => dest.NewState, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Mapeo de TelemetryLog para telemetría de la aplicación
            CreateMap<TelemetryLog, TelemetryDto>().ReverseMap();

            // Mapeo de OtpChallenge con manejo especial de CodeHash
            CreateMap<OtpChallenge, OtpChallengeDto>()
                .ReverseMap()
                .ForMember(dest => dest.CodeHash, opt => opt.Ignore())
                .ForMember(dest => dest.DocType, opt => opt.Ignore());
        }
    }
}