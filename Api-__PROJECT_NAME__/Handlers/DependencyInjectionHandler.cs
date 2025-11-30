using CC.Domain.Interfaces.External;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using CC.Infrastructure.Configurations;
using CC.Infrastructure.External;
using CC.Infrastructure.External.Email;
using CC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            //3) Servicios de mensajería (SMS / Email)
            RegisterMessagingServices(services, configuration);
        }

        private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
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

        public static void RepositoryRegistration(IServiceCollection services)
        {
            services.AddScoped<IQueryableUnitOfWork, DBContext>();

            // Permission Repositories
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

            // Telemetry Repository
            services.AddScoped<ITelemetryRepository, TelemetryRepository>();

            // Auth-related repositories
            services.AddScoped<IOtpChallengeRepository, OtpChallengeRepository>();
            services.AddScoped<ISessionsRepository, SessionsRepository>();
            services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        }
    }
}