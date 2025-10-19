using CC.Domain.Contracts;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using CC.Infrastructure.External.Patients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Servicio para integración con API externa de pacientes
    /// </summary>
    public class ExternalPatientService : ExternalServiceBase<ExternalPatientOptions>, IExternalPatientService
    {
        private readonly bool _useMockData;

        // Datos mock para desarrollo local
        private static readonly List<ExternalPatientContact> _mockPatients = new()
        {
            new ExternalPatientContact(
                Mobile: "3122305410",
                Email: "juan.perez@email.com",
                FullName: "Juan Pérez García",
                History: "HC123456"
            ),
            new ExternalPatientContact(
                Mobile: "3122305410",
                Email: "maria.rodriguez@email.com",
                FullName: "María Rodríguez López",
                History: "HC789012"
            ),
            new ExternalPatientContact(
                Mobile: "3122305410",
                Email: "carlos.gomez@email.com",
                FullName: "Carlos Gómez Hernández",
                History: "HC345678"
            )
        };

        public ExternalPatientService(
            HttpClient httpClient,
            IOptions<ExternalPatientOptions> options,
            ILogger<ExternalPatientService> logger,
            IConfiguration configuration)
            : base(httpClient, options.Value, logger)
        {
            // Leer configuración como boolean (maneja tanto "true" string como true boolean)
            var mockConfig = configuration["Features:UseMockPatientService"];
            _useMockData = !string.IsNullOrEmpty(mockConfig) && 
                          (mockConfig.Equals("true", StringComparison.OrdinalIgnoreCase) || mockConfig == "True");

            // Validar configuración solo si NO estamos en modo mock
            if (!_useMockData && !Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para ExternalPatientService: {Options.GetValidationErrors()}");
            }

            if (_useMockData)
            {
                Logger.LogWarning("?? USANDO DATOS MOCK - ExternalPatientService (SOLO DESARROLLO)");
            }
            else
            {
                Logger.LogInformation("ExternalPatientService inicializado correctamente");
            }
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
            // ===== MODO MOCK: Retornar datos simulados =====
            if (_useMockData)
            {
                Logger.LogInformation(
                    "?? MOCK: Simulando consulta de paciente - DocType: {DocType}, DocNumber: {DocNumber}",
                    docTypeCode, docNumber);

                // Simular delay de red (realismo)
                await Task.Delay(200, ct);

                // Seleccionar paciente mock basado en último dígito del documento
                var lastDigit = docNumber.Length > 0 ? int.Parse(docNumber[^1].ToString()) : 0;
                var index = lastDigit % _mockPatients.Count;
                var mockPatient = _mockPatients[index];

                Logger.LogInformation(
                    "?? MOCK: Paciente simulado encontrado - Historia: {Historia}, Nombre: {Nombre}",
                    mockPatient.History, mockPatient.FullName);

                return mockPatient;
            }

            // ===== MODO REAL: Consultar API externa =====
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

            return new ExternalPatientContact(
                Mobile: mobile,
                Email: dto.Correo,
                FullName: string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                History: dto.Historia
            );
        }
    }
}