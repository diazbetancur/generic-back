using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain
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
            // Mapeos de Permission
            CreateMap<Permission, PermissionDto>().ReverseMap();

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