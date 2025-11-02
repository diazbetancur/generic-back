using CC.Domain.Contracts;
using CC.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External.Mocks
{
    /// <summary>
    /// Mock de IXeroViewerService para entornos de desarrollo/QA sin dependencia del servicio Xero real.
    /// </summary>
    public sealed class MockXeroViewerService : IXeroViewerService
    {
        private readonly ILogger<MockXeroViewerService> _logger;
        private readonly MockXeroOptions _options;

        // Dataset estático basado en el ejemplo proporcionado
        private static readonly List<XeroStudy> StudiesDataset = new()
        {
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.1726504.1726478", Description = "RADIOGRAFÍA DE TORAX", Accession = "1726504", DateTime = "2021-05-03T15:36:00-05:00" },
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.1726483.1726457", Description = "PERFUSION MIOCARDICA EN REPOSO", Accession = "1726483", DateTime = "2021-05-03T15:27:00-05:00" },
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.1045195.1045169", Description = "RADIOGRAFÍA DE COLUMNA TORACICA", Accession = "1045195", DateTime = "2018-05-11T10:46:00-05:00" },
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.970.944", Description = "TOMOGRAFIA COMPUTADA DE TORAX", Accession = "970", DateTime = "2013-04-30T10:26:00-05:00" },
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.966.940", Description = "RADIOGRAFÍA DE MAXILAR INFERIOR", Accession = "966", DateTime = "2013-04-26T17:51:00-05:00" },
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.958.932", Description = "DOPPLER DE VASOS RENALES", Accession = "958", DateTime = "2013-04-24T13:43:59-05:00" },
            new XeroStudy { Uid = "1.3.51.0.1.1.192.168.153.65.938.912", Description = "TOMOGRAFIA COMPUTADA DE CRANEO CON CONTRASTE", Accession = "938", DateTime = "2013-04-23T11:28:59-05:00" }
        };

        public MockXeroViewerService(ILogger<MockXeroViewerService> logger, IOptions<MockXeroOptions> options)
        {
            _logger = logger;
            _options = options.Value ?? new MockXeroOptions();
            _logger.LogWarning("USANDO MOCK SERVICE - MockXeroViewerService (SOLO DESARROLLO/QA)");
        }

        public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MockXeroViewerService.IsAvailableAsync => true");
            return Task.FromResult(true);
        }

        public Task<StudiesResult> GetPatientStudiesAsync(string patientId, int limit = 10, int offset = 0, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient ID es requerido", nameof(patientId));

            _logger.LogInformation("MockXeroViewerService.GetPatientStudiesAsync patientId={PatientId}, limit={Limit}, offset={Offset}", patientId, limit, offset);

            // Normalizar paginación
            if (limit < 1) limit = 1;
            if (limit > 50) limit = 50;
            if (offset < 0) offset = 0;

            var total = StudiesDataset.Count;
            var page = StudiesDataset.Skip(offset).Take(limit).ToList();
            var count = page.Count;
            int? nextOffset = (offset + count) < total ? offset + count : null;

            var result = new StudiesResult(
                Success: true,
                PatientId: patientId,
                Offset: offset,
                Limit: limit,
                Count: count,
                Total: total,
                NextOffset: nextOffset,
                Studies: page,
                ErrorMessage: null
            );

            return Task.FromResult(result);
        }

        public Task<ViewerLinkResult> GenerateViewerLinkAsync(string studyUid, string? patientId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(studyUid))
                throw new ArgumentException("Study UID es requerido", nameof(studyUid));

            _logger.LogInformation("MockXeroViewerService.GenerateViewerLinkAsync studyUid={StudyUid}, patientId={PatientId}", studyUid, patientId);

            var token = string.IsNullOrWhiteSpace(_options.StaticToken)
                ? $"mock-token-{Guid.NewGuid():N}"
                : _options.StaticToken!;

            var viewerUrlBase = _options.ViewerUrlBase ?? "https://xero-mock-viewer.local/view?study=";
            var viewerUrl = $"{viewerUrlBase}{Uri.EscapeDataString(studyUid)}";
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes > 0 ? _options.ExpiresMinutes : 15);

            var result = new ViewerLinkResult(
                Success: true,
                Token: token,
                ViewerUrl: viewerUrl,
                ExpiresAt: expiresAt,
                ErrorMessage: null
            );

            return Task.FromResult(result);
        }
    }
}
