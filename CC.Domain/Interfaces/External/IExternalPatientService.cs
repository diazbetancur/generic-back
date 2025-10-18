using CC.Domain.Contracts;

namespace CC.Domain.Interfaces.External
{
    public interface IExternalPatientService
    {
        Task<ExternalPatientContact?> GetContactAsync(string docTypeCode, string docNumber, CancellationToken ct = default);
    }
}