using Serilog;
using Serilog.Events;

namespace Api_Portar_Paciente.Handlers
{
    /// <summary>
    /// Handler para configuración de Serilog
    /// </summary>
    public static class LoggingHandler
    {
        /// <summary>
        /// Configura Serilog antes de construir el host
        /// </summary>
        public static void ConfigureBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Iniciando aplicación Portal Pacientes API");
        }

        /// <summary>
        /// Configura Serilog completo desde appsettings
        /// </summary>
        public static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithProperty("Application", "PortalPacientesAPI");
            });
        }

        /// <summary>
        /// Configura el middleware de request logging
        /// </summary>
        public static void UseRequestLogging(WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                    diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                };
            });
        }
    }
}
