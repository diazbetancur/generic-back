using CC.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CC.Infrastructure.Extensions
{
  /// <summary>
  /// Extension methods for configuring database persistence with multi-provider support
  /// </summary>
  public static class PersistenceExtensions
  {
    /// <summary>
    /// Adds database persistence services with support for multiple providers (SQL Server, PostgreSQL)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unsupported database provider is specified</exception>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
      var provider = configuration["Database:Provider"] ?? "SqlServer";
      var connectionString = configuration.GetConnectionString("DefaultConnection");

      if (string.IsNullOrWhiteSpace(connectionString))
      {
        throw new InvalidOperationException(
            "Database connection string 'DefaultConnection' is not configured. " +
            "Please ensure ConnectionStrings:DefaultConnection is set in appsettings.json");
      }

      // Register AuditingSaveChangesInterceptor
      var auditingEnabledValue = configuration["Auditing:Enabled"];
      var auditingEnabled = !string.IsNullOrEmpty(auditingEnabledValue) &&
                            bool.TryParse(auditingEnabledValue, out var enabled) && enabled;
      services.AddScoped<AuditingSaveChangesInterceptor>();

      // Configure DbContext based on provider
      services.AddDbContext<DBContext>((serviceProvider, options) =>
      {
        switch (provider)
        {
          case "SqlServer":
            options.UseSqlServer(connectionString, sqlOptions =>
                  {
                    sqlOptions.EnableRetryOnFailure(
                              maxRetryCount: 5,
                              maxRetryDelay: TimeSpan.FromSeconds(10),
                              errorNumbersToAdd: null);
                  });
            break;

          case "PostgreSQL":
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
              npgsqlOptions.EnableRetryOnFailure(
                              maxRetryCount: 5,
                              maxRetryDelay: TimeSpan.FromSeconds(10),
                              errorCodesToAdd: null);
            });
            break;
          default:
            throw new InvalidOperationException(
                      $"Unsupported database provider: '{provider}'. " +
                      $"Supported providers are: 'SqlServer', 'PostgreSQL'. " +
                      $"Please update the 'Database:Provider' setting in appsettings.json");
        }

        // Add auditing interceptor if enabled
        if (auditingEnabled)
        {
          var interceptor = serviceProvider.GetRequiredService<AuditingSaveChangesInterceptor>();
          options.AddInterceptors(interceptor);
        }
      });

      return services;
    }
  }
}
