using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Services
{
    public interface IOtpChallengeService : IServiceBase<OtpChallenge, OtpChallengeDto>
    {
        Task<ValidateAuthResponse> ValidateAndGenerateOtpAsync(ValidateAuthRequest request, CancellationToken ct = default);

        Task<ResendOtpResponse> ResendOtpAsync(ResendOtpRequest request, CancellationToken ct = default);
    }
}