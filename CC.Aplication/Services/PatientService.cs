using CC.Domain.Dtos;
using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CC.Aplication.Services
{
    public class PatientService : IPatientService
    {
        private readonly IExternalPatientService _externalPatient;
        private readonly ILogger<PatientService> _logger;

        public PatientService(IExternalPatientService externalPatient, ILogger<PatientService> logger)
        {
            _externalPatient = externalPatient;
            _logger = logger;
        }

        public async Task<PatientContactDto> GetPatientInformationAsync(string docTypeCode, string docNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(docTypeCode) || string.IsNullOrWhiteSpace(docNumber))
            {
                _logger.LogWarning("GetPatientInformationAsync: parámetros inválidos");
                throw new ValidationException("docTypeCode y docNumber son requeridos");
            }

            try
            {
                var contact = await _externalPatient.GetContactAsync(docTypeCode, docNumber, ct).ConfigureAwait(false);
                if (contact == null)
                {
                    _logger.LogInformation("Paciente no encontrado: {DocType}-{DocNumber}", docTypeCode, docNumber);
                    throw new KeyNotFoundException("Paciente no encontrado");
                }

                return new PatientContactDto(contact.Mobile, contact.Email, contact.FullName, contact.History, contact.Address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando paciente: {DocType}-{DocNumber}", docTypeCode, docNumber);
                throw;
            }
        }
    }
}