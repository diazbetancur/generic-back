using CC.Domain.Dtos;

namespace CC.Domain.Interfaces.Services
{
    public interface IPatientService
    {
        Task<PatientContactDto> GetPatientInformationAsync(string docTypeCode, string docNumber, CancellationToken ct = default);
    }
}