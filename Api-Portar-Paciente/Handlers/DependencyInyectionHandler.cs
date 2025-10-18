using CC.Aplication.Services;
using AutoMapper;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Infrastructure.Configurations;
using CC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Reflection;
using ILogger = Serilog.ILogger;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External;

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DI failure: {ex.Message}");
                throw new InvalidOperationException("Dependency injection configuration failed", ex);
            }
        }

        public static void ServicesRegistration(IServiceCollection services, IConfiguration configuration)
        {
            // CRUD Services (siguen patrón ServiceBase)
            services.AddScoped<IFrequentQuestionsService, FrequentQuestionsService>();
            services.AddScoped<ICardioTVService, CardioTVService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IResponseQuestionService, ResponseQuestionService>();
            services.AddScoped<IGeneralSettingsService, GeneralSettingService>();
            services.AddScoped<IDocTypeService, DocTypeService>();

            // Auth Services (ahora siguen patrón ServiceBase)
            services.AddScoped<IOtpChallengeService, OtpChallengeService>();
            services.AddScoped<IAuthVerifyService, AuthVerifyService>();

            // External integrations - Patient Service con Options pattern
            services.Configure<CC.Infrastructure.External.Patients.ExternalPatientOptions>(options =>
            {
                options.ServiceName = "ExternalPatientService";
                options.BaseUrl = configuration["ExternalsAPI:PatienteBaseUrl"] ?? string.Empty;
                options.TimeoutSeconds = configuration.GetValue<int>("ExternalsAPI:PatienteTimeoutSeconds", 60);
                options.ApiKey = configuration["ExternalsAPI:ApiKey"];
                options.PatientEndpoint = "/api/Paciente";
                options.AllowInvalidCerts = configuration.GetValue<bool>("ExternalsAPI:AllowInvalidCerts", false);
            });

            var allowInvalidCerts = configuration.GetValue<bool>("ExternalsAPI:AllowInvalidCerts") || 
                                   configuration.GetValue<bool>("ApiSettings:AllowInvalidCerts");
            
            services.AddHttpClient<IExternalPatientService, ExternalPatientService>()
                    .ConfigurePrimaryHttpMessageHandler(() => HttpClientConfiguration.CreateHandler(allowInvalidCerts));

            // Liwa SMS Service
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

            services.AddHttpClient<ILiwaSmsService, LiwaSmsService>();

            // SMS and Email senders (legacy - mantener compatibilidad)
            services.AddScoped<ISmsSender, SmsSender>();
            services.AddScoped<IEmailSender, EmailSender>();
        }

        public static void RepositoryRegistration(IServiceCollection services)
        {
            services.AddScoped<IQueryableUnitOfWork, DBContext>();
            services.AddScoped<IFrequentQuestionsRepository, FrequentQuestionsRepository>();
            services.AddScoped<ICardioTVRepository, CardioTVRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IResponseQuestionRepository, ResponseQuestionRepository>();

            // Auth-related repositories
            services.AddScoped<IGeneralSettingsRepository, GeneralSettingsRepository>();
            services.AddScoped<IDocTypeRepository, DocTypeRepository>();
            services.AddScoped<IOtpChallengeRepository, OtpChallengeRepository>();
            services.AddScoped<ISessionsRepository, SessionsRepository>();
        }
    }
}