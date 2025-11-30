using Serilog;
using ILogger = Serilog.ILogger;

namespace Api___PROJECT_NAME__.Services
{
    /// <summary>
    /// Servicio en background que limpia logs antiguos autom�ticamente
    /// </summary>
    public class LogCleanupService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly TimeSpan _interval;
        private readonly int _retentionDays;
        private readonly string _logPath;

        public LogCleanupService(IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = Log.ForContext<LogCleanupService>();

            // Configuraci�n desde appsettings
            _retentionDays = _configuration.GetValue<int>("Logging:RetentionDays", 30);
            _logPath = _configuration.GetValue<string>("Logging:LogPath", "logs") ?? "logs";

            // Ejecutar limpieza cada d�a a las 3 AM
            var cleanupHour = _configuration.GetValue<int>("Logging:CleanupHour", 3);
            _interval = TimeSpan.FromHours(24);

            _logger.Information("LogCleanupService configurado: RetentionDays={RetentionDays}, LogPath={LogPath}, CleanupHour={CleanupHour}",
                _retentionDays, _logPath, cleanupHour);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("LogCleanupService iniciado");

            // Esperar hasta la hora de limpieza
            await WaitUntilCleanupTime(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldLogsAsync(stoppingToken);

                    // Esperar 24 horas hasta la pr�xima ejecuci�n
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.Information("LogCleanupService detenido por cancelaci�n");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error en LogCleanupService, reintentando en 1 hora");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.Information("LogCleanupService finalizado");
        }

        private async Task WaitUntilCleanupTime(CancellationToken stoppingToken)
        {
            var cleanupHour = _configuration.GetValue<int>("Logging:CleanupHour", 3);
            var now = DateTime.Now;
            var nextCleanup = new DateTime(now.Year, now.Month, now.Day, cleanupHour, 0, 0);

            if (now >= nextCleanup)
            {
                nextCleanup = nextCleanup.AddDays(1);
            }

            var delay = nextCleanup - now;
            _logger.Information("Pr�xima limpieza de logs programada para: {NextCleanup}", nextCleanup);

            await Task.Delay(delay, stoppingToken);
        }

        private Task CleanupOldLogsAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), _logPath);

                    if (!Directory.Exists(logDirectory))
                    {
                        _logger.Warning("Directorio de logs no encontrado: {LogDirectory}", logDirectory);
                        return;
                    }

                    var cutoffDate = DateTime.Now.AddDays(-_retentionDays);
                    var logFiles = Directory.GetFiles(logDirectory, "log-*.txt");

                    var deletedCount = 0;
                    var totalSize = 0L;

                    foreach (var logFile in logFiles)
                    {
                        var fileInfo = new FileInfo(logFile);

                        if (fileInfo.LastWriteTime < cutoffDate)
                        {
                            var fileSize = fileInfo.Length;

                            try
                            {
                                File.Delete(logFile);
                                deletedCount++;
                                totalSize += fileSize;

                                _logger.Debug("Log eliminado: {FileName}, Tama�o: {Size} bytes, Fecha: {LastWrite}",
                                    fileInfo.Name, fileSize, fileInfo.LastWriteTime);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warning(ex, "No se pudo eliminar log: {FileName}", fileInfo.Name);
                            }
                        }
                    }

                    if (deletedCount > 0)
                    {
                        _logger.Information(
                            "Limpieza de logs completada: {DeletedCount} archivos eliminados, {TotalSize} bytes liberados",
                            deletedCount, totalSize);
                    }
                    else
                    {
                        _logger.Information("Limpieza de logs: No hay archivos antiguos para eliminar");
                    }

                    // Informaci�n adicional
                    var remainingFiles = Directory.GetFiles(logDirectory, "log-*.txt").Length;
                    var directorySize = Directory.GetFiles(logDirectory, "log-*.txt")
                        .Sum(f => new FileInfo(f).Length);

                    _logger.Information(
                        "Estado del directorio de logs: {RemainingFiles} archivos, {DirectorySize} bytes totales",
                        remainingFiles, directorySize);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error al limpiar logs antiguos");
                }
            }, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Information("LogCleanupService deteniendo...");
            await base.StopAsync(stoppingToken);
        }
    }
}
