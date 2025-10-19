using CC.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Api_Portar_Paciente.Services
{
    /// <summary>
    /// Servicio en background que limpia automáticamente registros expirados de autenticación
    /// </summary>
    public class AuthCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly TimeSpan _interval;
        private readonly int _otpRetentionHours;
        private readonly int _sessionRetentionDays;

        public AuthCleanupService(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = Log.ForContext<AuthCleanupService>();

            // Configuración desde appsettings
            _otpRetentionHours = _configuration.GetValue<int>("Authentication:OtpRetentionHours", 24);
            _sessionRetentionDays = _configuration.GetValue<int>("Authentication:SessionRetentionDays", 30);

            // Ejecutar limpieza cada día a las 3 AM
            _interval = TimeSpan.FromHours(24);

            _logger.Information(
                "AuthCleanupService configurado: OtpRetentionHours={OtpRetention}, SessionRetentionDays={SessionRetention}",
                _otpRetentionHours, _sessionRetentionDays);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("AuthCleanupService iniciado");

            // Esperar hasta la hora de limpieza
            await WaitUntilCleanupTime(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupAuthDataAsync(stoppingToken);

                    // Esperar 24 horas hasta la próxima ejecución
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.Information("AuthCleanupService detenido por cancelación");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error en AuthCleanupService, reintentando en 1 hora");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.Information("AuthCleanupService finalizado");
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
            _logger.Information("Próxima limpieza de datos de autenticación programada para: {NextCleanup}", nextCleanup);

            await Task.Delay(delay, stoppingToken);
        }

        private async Task CleanupAuthDataAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

            try
            {
                _logger.Information("Iniciando limpieza de datos de autenticación");

                // Limpiar OTP Challenges expirados
                var otpDeletedCount = await CleanupOtpChallengesAsync(dbContext, stoppingToken);

                // Limpiar Sessions inactivas/expiradas
                var sessionsDeletedCount = await CleanupSessionsAsync(dbContext, stoppingToken);

                _logger.Information(
                    "Limpieza de autenticación completada: {OtpDeleted} OTP challenges eliminados, {SessionsDeleted} sesiones eliminadas",
                    otpDeletedCount, sessionsDeletedCount);

                // Estadísticas finales
                await LogDatabaseStatsAsync(dbContext, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al limpiar datos de autenticación");
            }
        }

        private async Task<int> CleanupOtpChallengesAsync(DBContext dbContext, CancellationToken stoppingToken)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddHours(-_otpRetentionHours);

                _logger.Debug(
                    "Eliminando OTP challenges expirados antes de: {CutoffDate}",
                    cutoffDate);

                // Eliminar challenges expirados
                var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                    @"DELETE FROM OtpChallenges 
                      WHERE ExpiresAt < {0}",
                    cutoffDate,
                    stoppingToken);

                if (deletedCount > 0)
                {
                    _logger.Information(
                        "OTP Challenges eliminados: {Count} registros (expirados antes de {CutoffDate})",
                        deletedCount, cutoffDate);
                }
                else
                {
                    _logger.Information("No hay OTP challenges expirados para eliminar");
                }

                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al limpiar OTP challenges");
                return 0;
            }
        }

        private async Task<int> CleanupSessionsAsync(DBContext dbContext, CancellationToken stoppingToken)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_sessionRetentionDays);

                _logger.Debug(
                    "Eliminando sesiones inactivas/expiradas antes de: {CutoffDate}",
                    cutoffDate);

                // Eliminar sesiones inactivas o revocadas que ya expiraron
                var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                    @"DELETE FROM Sessions 
                      WHERE (IsActive = 0 OR RevokedAt IS NOT NULL)
                        AND (ExpiresAt < {0} OR RevokedAt < {0})",
                    cutoffDate,
                    stoppingToken);

                if (deletedCount > 0)
                {
                    _logger.Information(
                        "Sesiones eliminadas: {Count} registros (inactivas antes de {CutoffDate})",
                        deletedCount, cutoffDate);
                }
                else
                {
                    _logger.Information("No hay sesiones inactivas para eliminar");
                }

                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error al limpiar sesiones");
                return 0;
            }
        }

        private async Task LogDatabaseStatsAsync(DBContext dbContext, CancellationToken stoppingToken)
        {
            try
            {
                // Contar registros restantes
                var otpCount = await dbContext.OtpChallenges.CountAsync(stoppingToken);
                var sessionsCount = await dbContext.Sessions.CountAsync(stoppingToken);
                var activeSessionsCount = await dbContext.Sessions
                    .CountAsync(s => s.IsActive, stoppingToken);

                _logger.Information(
                    "Estado de tablas de autenticación: OtpChallenges={OtpCount}, Sessions={SessionsCount} (Activas={ActiveCount})",
                    otpCount, sessionsCount, activeSessionsCount);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "No se pudieron obtener estadísticas de la BD");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Information("AuthCleanupService deteniendo...");
            await base.StopAsync(stoppingToken);
        }
    }
}
