using CC.Domain.Contracts;
using CC.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;

namespace CC.Infrastructure.External.Mocks
{
    /// <summary>
    /// Mock del servicio de pacientes para desarrollo local sin conexión a servicios externos
    /// </summary>
    public class MockExternalPatientService : IExternalPatientService
    {
        private readonly ILogger<MockExternalPatientService> _logger;

        // Datos de prueba simulados
        private readonly List<ExternalPatientContact> _mockPatients = new()
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
                Mobile: "3205551234",
                Email: "carlos.gomez@email.com",
                FullName: "Carlos Gómez Hernández",
                History: "HC345678"
            )
        };

        public MockExternalPatientService(ILogger<MockExternalPatientService> logger)
        {
            _logger = logger;
            _logger.LogWarning("?? USANDO MOCK SERVICE - MockExternalPatientService (SOLO DESARROLLO)");
        }

        public Task<ExternalPatientContact?> GetContactAsync(
            string docTypeCode,
            string docNumber,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "?? MockExternalPatientService: Simulando consulta de paciente - DocType: {DocType}, DocNumber: {DocNumber}",
                docTypeCode, docNumber);

            // Simular delay de red (opcional, para realismo)
            Task.Delay(200, ct).Wait(ct);

            // Lógica de mock: devolver paciente basado en últimos dígitos del documento
            var lastDigit = docNumber.Length > 0 ? int.Parse(docNumber[^1].ToString()) : 0;
            var index = lastDigit % _mockPatients.Count;

            var patient = _mockPatients[index];

            _logger.LogInformation(
                "?? MockExternalPatientService: Paciente simulado encontrado - Historia: {Historia}, Nombre: {Nombre}",
                patient.History, patient.FullName);

            return Task.FromResult<ExternalPatientContact?>(patient);
        }
    }
}