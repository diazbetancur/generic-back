using CC.Domain.Dtos;

namespace CC.Domain.Interfaces.Services
{
    public interface IAuthValidateService
    {
        Task<ValidateAuthResponse> ValidateAsync(ValidateAuthRequest request, CancellationToken ct = default);
    }
}
