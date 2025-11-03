using CC.Domain.Contracts;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using CC.Infrastructure.External.Patients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Servicio para integración con API externa de pacientes (solo implementación real)
    /// </summary>
    public class ExternalPatientService : ExternalServiceBase<ExternalPatientOptions>, IExternalPatientService
    {
        public ExternalPatientService(
            HttpClient httpClient,
            IOptions<ExternalPatientOptions> options,
            ILogger<ExternalPatientService> logger)
            : base(httpClient, options.Value, logger)
        {
            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para ExternalPatientService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("ExternalPatientService inicializado correctamente");
        }

        /// <summary>
        /// Obtiene información de contacto de un paciente desde el servicio externo
        /// </summary>
        /// <param name="docTypeCode">Código del tipo de documento</param>
        /// <param name="docNumber">Número de documento</param>
        /// <param name="ct">Token de cancelación</param>
        /// <returns>Información de contacto del paciente o null si no se encuentra</returns>
        public async Task<ExternalPatientContact?> GetContactAsync(
            string docTypeCode,
            string docNumber,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "Consultando paciente externo: DocType={DocType}, DocNumber={DocNumber}",
                docTypeCode, docNumber);

            // Construir URL con query parameters
            var queryParams = new Dictionary<string, string?>
            {
                ["identificacion"] = docNumber,
                ["tipoId"] = docTypeCode
            };

            var url = BuildUrl(Options.PatientEndpoint, queryParams);

            // Realizar petición GET usando la clase base
            var dto = await GetAsync<ExternalPatientDto>(url, ct);

            if (dto == null)
            {
                Logger.LogWarning(
                    "Paciente no encontrado en servicio externo: DocType={DocType}, DocNumber={DocNumber}",
                    docTypeCode, docNumber);
                return null;
            }

            // Mapear DTO a contract del dominio
            var contact = MapToContact(dto);

            Logger.LogInformation(
                "Paciente encontrado: DocType={DocType}, DocNumber={DocNumber}, Historia={Historia}",
                docTypeCode, docNumber, contact.History);

            return contact;
        }

        /// <summary>
        /// Mapea el DTO externo al contract del dominio
        /// </summary>
        private static ExternalPatientContact MapToContact(ExternalPatientDto dto)
        {
            // Determinar teléfono móvil (priorizar celular sobre teléfono fijo)
            var mobile = string.IsNullOrWhiteSpace(dto.Celular)
                ? dto.Telefono
                : dto.Celular;

            // Construir nombre completo
            var nameParts = new[] { dto.Nombre, dto.Apellido, dto.Apellido2 }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            var fullName = string.Join(" ", nameParts);

            // historia puede venir como número; convertir a string
            var history = dto.Historia?.ToString();

            return new ExternalPatientContact(
                Mobile: mobile,
                Email: dto.Correo,
                FullName: string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                History: history,
                Address: dto.Address
            );
        }
    }
}