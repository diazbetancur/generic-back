using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Configurations
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación
    /// </summary>
    public class DBContext : IdentityDbContext<User, Role, Guid>, IQueryableUnitOfWork
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        /// <summary>
        /// Preguntas frecuentes (FAQs)
        /// </summary>
        public DbSet<FrequentQuestions> FrequentQuestions { get; set; }

        /// <summary>
        /// Contenido CardioTV
        /// </summary>
        public DbSet<CardioTV> CardioTVs { get; set; }

        /// <summary>
        /// Logs de auditoría
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Preguntas de encuestas
        /// </summary>
        public DbSet<Question> Questions { get; set; }

        /// <summary>
        /// Respuestas a preguntas de encuestas
        /// </summary>
        public DbSet<ResponseQuestion> ResponseQuestions { get; set; }

        /// <summary>
        /// Tipos de documento
        /// </summary>
        public DbSet<DocType> DocTypes { get; set; }

        /// <summary>
        /// Desafíos OTP (One-Time Password)
        /// </summary>
        public DbSet<OtpChallenge> OtpChallenges { get; set; }

        /// <summary>
        /// Sesiones activas de usuarios
        /// </summary>
        public DbSet<Sessions> Sessions { get; set; }

        /// <summary>
        /// Configuraciones generales de la aplicación
        /// </summary>
        public DbSet<GeneralSettings> GeneralSettings { get; set; }

        /// <summary>
        /// Intentos de inicio de sesión
        /// </summary>
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        /// <summary>
        /// Registro de telemetría de la aplicación (consultas, descargas y métricas)
        /// </summary>
        public DbSet<TelemetryLog> TelemetryLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // FrequentQuestions Configuration
            builder.Entity<FrequentQuestions>().HasKey(c => c.Id);
            builder.Entity<FrequentQuestions>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<FrequentQuestions>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            // CardioTV Configuration
            builder.Entity<CardioTV>().HasKey(c => c.Id);
            builder.Entity<CardioTV>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<CardioTV>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            // AuditLog Configuration
            builder.Entity<AuditLog>().HasKey(c => c.Id);
            builder.Entity<AuditLog>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<AuditLog>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            // Question Configuration
            builder.Entity<Question>().HasKey(c => c.Id);
            builder.Entity<Question>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<Question>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            // ResponseQuestion Configuration
            builder.Entity<ResponseQuestion>().HasKey(c => c.Id);
            builder.Entity<ResponseQuestion>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<ResponseQuestion>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            // DocType Configuration
            builder.Entity<DocType>().HasKey(c => c.Id);
            builder.Entity<DocType>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<DocType>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<DocType>().HasIndex(d => d.Code).IsUnique();

            // OtpChallenge Configuration
            builder.Entity<OtpChallenge>().HasKey(c => c.Id);
            builder.Entity<OtpChallenge>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<OtpChallenge>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<OtpChallenge>().HasIndex(o => new { o.UserId, o.ExpiresAt });

            // Sessions Configuration
            builder.Entity<Sessions>().HasKey(c => c.Id);
            builder.Entity<Sessions>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<Sessions>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Sessions>().HasIndex(s => s.Jti).IsUnique();
            builder.Entity<Sessions>().HasIndex(s => s.IssuedAt);
            builder.Entity<Sessions>().HasIndex(s => s.LastSeenAt);
            builder.Entity<Sessions>().HasIndex(s => s.IsActive);

            // GeneralSettings Configuration
            builder.Entity<GeneralSettings>().HasKey(c => c.Id);
            builder.Entity<GeneralSettings>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<GeneralSettings>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<GeneralSettings>().HasIndex(g => g.Key).IsUnique();

            // LoginAttempt Configuration
            builder.Entity<LoginAttempt>().HasKey(c => c.Id);
            builder.Entity<LoginAttempt>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<LoginAttempt>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<LoginAttempt>().HasIndex(l => l.DateCreated);
            builder.Entity<LoginAttempt>().HasIndex(l => new { l.Success, l.DateCreated });
            builder.Entity<LoginAttempt>().HasIndex(l => new { l.DocNumber, l.DateCreated });

            // TelemetryLog Configuration
            builder.Entity<TelemetryLog>().HasKey(c => c.Id);
            builder.Entity<TelemetryLog>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<TelemetryLog>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<TelemetryLog>().HasIndex(t => t.ActivityDate);
            builder.Entity<TelemetryLog>().HasIndex(t => new { t.UserDocType, t.UserDocNumber });
            builder.Entity<TelemetryLog>().HasIndex(t => new { t.DocumentType, t.ActivityType });
            builder.Entity<TelemetryLog>().HasIndex(t => new { t.ActivityType, t.ActivityDate });
            builder.Entity<TelemetryLog>().HasIndex(t => new { t.TelemetryType, t.ActivityDate });

            DisableCascadingDelete(builder);
        }

        private static void DisableCascadingDelete(ModelBuilder modelBuilder)
        {
            var relationship = modelBuilder.Model.GetEntityTypes()
                .Where(e => e.ClrType.Namespace == null || !e.ClrType.Namespace.StartsWith("Microsoft.AspNetCore.Identity"))
                .SelectMany(e => e.GetForeignKeys());

            foreach (var r in relationship)
            {
                r.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public void Commit()
        {
            try
            {
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
        }

        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await ex.Entries.Single().ReloadAsync().ConfigureAwait(false);
            }
        }

        public void DetachLocal<TEntity>(TEntity entity, EntityState state) where TEntity : class
        {
            if (entity is null)
            {
                return;
            }

            var local = Set<TEntity>().Local.ToList();

            if (local?.Any() ?? false)
            {
                local.ForEach(item =>
                {
                    Entry(item).State = EntityState.Detached;
                });
            }

            Entry(entity).State = state;
        }

        public DbContext GetContext()
        {
            return this;
        }

        public DbSet<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }
    }
}