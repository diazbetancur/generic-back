using CC.Domain.Dtos;

namespace CC.Domain.Interfaces.Services
{
    public interface IAuthStartService
    {
        Task<StartAuthResponse> StartAsync(StartAuthRequest request, CancellationToken ct = default);
        Task<ResendOtpResponse> ResendAsync(ResendOtpRequest request, CancellationToken ct = default);
    }

    public interface IAuthVerifyService
    {
        Task<VerifyOtpResponse> VerifyAsync(VerifyOtpRequest request, CancellationToken ct = default);
        Task LogoutAsync(string jti, CancellationToken ct = default);
    }
}
