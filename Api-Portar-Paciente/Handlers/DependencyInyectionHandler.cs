using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Infrastructure.Configurations;
using CC.Infrastructure.External;
using CC.Infrastructure.External.Email;
using CC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Reflection;
using ILogger = Serilog.ILogger;
using CC.Aplication.Services;
using CC.Infrastructure.External.Mocks;

namespace Api_Portar_Paciente.Handlers
{
    public static class DependencyInyectionHandler
    {
        public static void DepencyInyectionConfig(IServiceCollection services, IConfiguration configuration, string environment)
        {
            try
            {
                Console.WriteLine($"Environment detected in DI: {environment}");

                #region Database Configuration

                bool auditingEnabled = configuration.GetValue<bool>("Auditing:Enabled");

                services.AddScoped<AuditingSaveChangesInterceptor>();

                services.AddDbContext<DBContext>((serviceProvider, opt) =>
                {
                    var conn = configuration.GetConnectionString("DefaultConnection");
                    opt.UseSqlServer(conn, sql =>
                    {
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
                    if (auditingEnabled)
                    {
                        var interceptor = serviceProvider.GetRequiredService<AuditingSaveChangesInterceptor>();
                        opt.AddInterceptors(interceptor);
                    }
                });

                #endregion Database Configuration

                #region AutoMapper

                services.AddAutoMapper(typeof(CC.Domain.AutoMapperProfile));

                #endregion AutoMapper

                #region Services and Repositories

                ServicesRegistration(services, configuration);
                RepositoryRegistration(services);

                #endregion Services and Repositories

                services.AddSingleton<ExceptionControl>();

                #region Logging

                Logger logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        retainedFileCountLimit: 7)
                    .CreateLogger();

                logger.Information("DI configured for {Environment}. AuditingEnabled={AuditingEnabled}", environment, auditingEnabled);
                services.AddSingleton<ILogger>(logger);

                #endregion Logging

                services.AddTransient<SeedDB>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DI failure: {ex.Message}");
                throw new InvalidOperationException("Dependency injection configuration failed", ex);
            }
        }

        public static void ServicesRegistration(IServiceCollection services, IConfiguration configuration)
        {
            //1) Options centralizadas
            ConfigureOptions(services, configuration);

            //2) Servicios core de aplicación
            RegisterCoreServices(services);

            //3) Integraciones externas (reales por defecto)
            RegisterExternalIntegrations(services, configuration);

            //4) Servicios de mensajería (SMS / Email)
            RegisterMessagingServices(services, configuration);

            //5) Overrides de mocks (al final, para que ganen sobre los reales)
            RegisterMockOverrides(services, configuration);
        }

        private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
            // External Patient options
            services.Configure<CC.Infrastructure.External.Patients.ExternalPatientOptions>(options =>
            {
                options.ServiceName = "ExternalPatientService";
                options.BaseUrl = configuration["ExternalsAPI:PatienteBaseUrl"] ?? string.Empty;
                options.TimeoutSeconds = configuration.GetValue<int>("ExternalsAPI:PatienteTimeoutSeconds", 60);
                options.ApiKey = configuration["ExternalsAPI:ApiKey"];
                options.PatientEndpoint = "/api/Paciente";
                options.AllowInvalidCerts = configuration.GetValue<bool>("ExternalsAPI:AllowInvalidCerts", false);
            });

            // Xero Viewer options
            services.Configure<CC.Infrastructure.External.Xero.XeroViewerOptions>(options =>
            {
                options.ServiceName = "XeroViewerService";
                options.BaseUrl = configuration["ExternalServices:XeroViewer:BaseUrl"] ?? "http://10.3.0.79:6663";
                options.TimeoutSeconds = configuration.GetValue<int>("ExternalServices:XeroViewer:TimeoutSeconds", 30);
                options.ApiKey = configuration["ExternalServices:XeroViewer:ApiKey"] ?? "C4rdio1nfanf1l";
                options.HealthEndpoint = "/health";
                options.StudiesEndpoint = "/patients/{patient_id}/studies";
                options.ViewerLinkEndpoint = "/studies/{study_uid}/viewer-link";
                options.DefaultLimit = 10;
                options.MaxLimit = 50;
                options.AllowInvalidCerts = configuration.GetValue<bool>("ExternalServices:XeroViewer:AllowInvalidCerts", false);
            });

            // NilRead options
            services.Configure<CC.Infrastructure.External.NilRead.NilReadOptions>(options =>
            {
                options.ServiceName = "NilReadService";
                options.BaseUrl = configuration["ExternalServices:NilRead:BaseUrl"] ?? "http://10.3.0.79:8086";
                options.TimeoutSeconds = configuration.GetValue<int>("ExternalServices:NilRead:TimeoutSeconds", 30);
                options.ApiKey = configuration["ExternalServices:NilRead:ApiKey"] ?? "FundacionC4rd1o";
                options.HealthEndpoint = "/health";
                options.ExamsEndpoint = "/patients/{patient_id}/exams";
                options.ReportEndpoint = "/reports/{dateFolder}/{filename}";
                options.ViewerLinkEndpoint = "/exams/{accession}/viewer-link";
                options.DefaultLimit = configuration.GetValue<int>("ExternalServices:NilRead:DefaultLimit", 10);
                options.MaxLimit = configuration.GetValue<int>("ExternalServices:NilRead:MaxLimit", 50);
                options.AllowInvalidCerts = configuration.GetValue<bool>("ExternalServices:NilRead:AllowInvalidCerts", false);
            });

            // Mock Xero options (opcional)
            services.Configure<MockXeroOptions>(configuration.GetSection("Mocks:Xero"));

            // SMS options
            services.Configure<CC.Infrastructure.External.Sms.SmsServiceOptions>(options =>
            {
                options.ServiceName = "LiwaSmsService";
                options.BaseUrl = configuration["ExternalServices:Sms:BaseUrl"] ?? "https://api.liwa.co";
                options.TimeoutSeconds = configuration.GetValue<int>("ExternalServices:Sms:TimeoutSeconds", 30);
                options.AuthEndpoint = "/v2/auth/login";
                options.SendSingleEndpoint = "/v2/sms/single";
                options.SendMultipleEndpoint = "/v2/sms/multiple";
                options.Account = configuration["ExternalServices:Sms:Account"] ?? string.Empty;
                options.Password = configuration["ExternalServices:Sms:Password"] ?? string.Empty;
                options.LiwaApiKey = configuration["ExternalServices:Sms:LiwaApiKey"] ?? string.Empty;
                options.DefaultCountryCode = "57";
                options.DefaultMessageType = 1;
                options.TokenExpirationMinutes = 55;
            });

            // Email Graph options
            services.Configure<CC.Infrastructure.External.Email.EmailServiceOptions>(options =>
            {
                options.ServiceName = "GraphEmailService";
                options.BaseUrl = "https://graph.microsoft.com/v1.0";
                options.TimeoutSeconds = configuration.GetValue<int>("ExternalServices:Email:TimeoutSeconds", 30);
                options.TenantId = configuration["ExternalServices:Email:TenantId"] ?? string.Empty;
                options.ClientId = configuration["ExternalServices:Email:ClientId"] ?? string.Empty;
                options.ClientSecret = configuration["ExternalServices:Email:ClientSecret"] ?? string.Empty;
                options.SendFromUserId = configuration["ExternalServices:Email:SendFromUserId"] ?? string.Empty;
                options.DefaultFromEmail = configuration["ExternalServices:Email:DefaultFromEmail"] ?? string.Empty;
                options.DefaultFromName = configuration["ExternalServices:Email:DefaultFromName"] ?? "Portal Pacientes";
                options.GraphBaseUrl = "https://graph.microsoft.com/v1.0";
                options.Scopes = new[] { "https://graph.microsoft.com/.default" };
            });

            // Email SMTP options
            services.Configure<SmtpEmailOptions>(options =>
            {
                options.Host = configuration["Email:Host"] ?? string.Empty;
                options.Port = configuration.GetValue<int>("Email:Port", 587);
                options.UseSsl = configuration.GetValue<bool>("Email:UseSsl", true);
                options.TimeoutSeconds = configuration.GetValue<int>("Email:TimeoutSeconds", 30);
                options.Username = configuration["Email:Username"] ?? string.Empty;
                options.Password = configuration["Email:Password"] ?? string.Empty;
                options.FromEmail = configuration["Email:FromEmail"] ?? options.Username;
                options.FromName = configuration["Email:FromName"] ?? "Portal Pacientes";
            });
        }

        private static void RegisterCoreServices(IServiceCollection services)
        {
            // CRUD Services (siguen patrón ServiceBase)
            services.AddScoped<IFrequentQuestionsService, FrequentQuestionsService>();
            services.AddScoped<ICardioTVService, CardioTVService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IResponseQuestionService, ResponseQuestionService>();
            services.AddScoped<IGeneralSettingsService, GeneralSettingService>();
            services.AddScoped<IDocTypeService, DocTypeService>();
            services.AddScoped<IDataPolicyAcceptanceService, DataPolicyAcceptanceService>();
            services.AddScoped<IPatientService, PatientService>();

            // RequestType and State Services
            services.AddScoped<IRequestTypeService, RequestTypeService>();
            services.AddScoped<IStateService, StateService>();

            // Request and HistoryRequest Services
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IHistoryRequestService, HistoryRequestService>();

            // Authorization Service
            services.AddScoped<CC.Domain.Interfaces.Services.IAuthorizationService, CC.Aplication.Services.AuthorizationService>();

            // JWT Token Generator
            services.AddSingleton<CC.Aplication.Utils.JwtTokenGenerator>();

            // Telemetry Service
            services.AddScoped<ITelemetryService, TelemetryService>();

            // Auth Services
            services.AddScoped<IOtpChallengeService, OtpChallengeService>();
            services.AddScoped<IAuthVerifyService, AuthVerifyService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();

            // Reports
            services.AddScoped<IReportsService, ReportsService>();
        }

        private static void RegisterExternalIntegrations(IServiceCollection services, IConfiguration configuration)
        {
            var allowInvalidCerts = configuration.GetValue<bool>("ExternalsAPI:AllowInvalidCerts") ||
                                    configuration.GetValue<bool>("ApiSettings:AllowInvalidCerts");

            services.AddHttpClient<IExternalPatientService, ExternalPatientService>()
                .ConfigurePrimaryHttpMessageHandler(() => HttpClientConfiguration.CreateHandler(allowInvalidCerts));

            var xeroAllowInvalidCerts = configuration.GetValue<bool>("ExternalServices:XeroViewer:AllowInvalidCerts");
            services.AddHttpClient<IXeroViewerService, XeroViewerService>()
                .ConfigurePrimaryHttpMessageHandler(() => HttpClientConfiguration.CreateHandler(xeroAllowInvalidCerts));

            var nilReadAllowInvalidCerts = configuration.GetValue<bool>("ExternalServices:NilRead:AllowInvalidCerts");
            services.AddHttpClient<INilReadService, NilReadService>()
                .ConfigurePrimaryHttpMessageHandler(() => HttpClientConfiguration.CreateHandler(nilReadAllowInvalidCerts));
        }

        private static void RegisterMessagingServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ILiwaSmsService, LiwaSmsService>();
            services.AddHttpClient<IGraphEmailService, GraphEmailService>();

            // Selección dinámica de implementación de IEmailSender
            var emailMode = configuration["Email:Mode"]?.Trim().ToLowerInvariant();
            if (emailMode == "smtp")
            {
                services.AddScoped<IEmailSender, SmtpEmailSender>();
            }
            else
            {
                services.AddScoped<IEmailSender, EmailSender>(); // Graph por defecto
            }

            // Envoltorios simples (legacy) para envío SMS
            services.AddScoped<ISmsSender, SmsSender>();
        }

        private static void RegisterMockOverrides(IServiceCollection services, IConfiguration configuration)
        {
            var useMockPatient = configuration.GetValue<bool>("Features:UseMockPatientService", false);
            var useMockXero = configuration.GetValue<bool>("Features:UseMockXeroService", false);

            if (useMockPatient)
            {
                services.AddScoped<IExternalPatientService, MockExternalPatientService>();
            }

            if (useMockXero)
            {
                services.AddScoped<IXeroViewerService, MockXeroViewerService>();
            }
        }

        public static void RepositoryRegistration(IServiceCollection services)
        {
            services.AddScoped<IQueryableUnitOfWork, DBContext>();
            services.AddScoped<IFrequentQuestionsRepository, FrequentQuestionsRepository>();
            services.AddScoped<ICardioTVRepository, CardioTVRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IResponseQuestionRepository, ResponseQuestionRepository>();

            // RequestType and State Repositories
            services.AddScoped<IRequestTypeRepository, RequestTypeRepository>();
            services.AddScoped<IStateRepository, StateRepository>();

            // Request and HistoryRequest Repositories
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IHistoryRequestRepository, HistoryRequestRepository>();

            // Permission Repositories
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

            // Telemetry Repository
            services.AddScoped<ITelemetryRepository, TelemetryRepository>();

            // Auth-related repositories
            services.AddScoped<IGeneralSettingsRepository, GeneralSettingsRepository>();
            services.AddScoped<IDocTypeRepository, DocTypeRepository>();
            services.AddScoped<IDataPolicyAcceptanceRepository, DataPolicyAcceptanceRepository>();
            services.AddScoped<IOtpChallengeRepository, OtpChallengeRepository>();
            services.AddScoped<ISessionsRepository, SessionsRepository>();
            services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        }
    }
}