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

                ServicesRegistration(services);
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

                // SeedDB removed (not implemented)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DI failure: {ex.Message}");
                throw new InvalidOperationException("Dependency injection configuration failed", ex);
            }
        }

        public static void ServicesRegistration(IServiceCollection services)
        {
            services.AddScoped<IFrecuentQuestionsService, FrecuentQuestionsService>();
            services.AddScoped<ICardioTVService, CardioTVService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IResponseQuestionService, ResponseQuestionService>();
        }

        public static void RepositoryRegistration(IServiceCollection services)
        {
            services.AddScoped<IQueryableUnitOfWork, DBContext>();
            services.AddScoped<IFrecuentQuestionsRepository, FrecuentQuestionsRepository>();
            services.AddScoped<ICardioTVRepository, CardioTVRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IResponseQuestionRepository, ResponseQuestionRepository>();
        }
    }
}