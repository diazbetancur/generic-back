using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace CC.Infrastructure.Configurations
{
    public class DBContext : IdentityDbContext<User, Role, Guid>, IQueryableUnitOfWork
    {
        public DBContext(DbContextOptions<DBContext> options)
: base(options)
        {
        }

        /// <summary>
        /// FrecuentQuestions
        /// </summary>
        public DbSet<FrecuentQuestions> FrecuentQuestions { get; set; }

        /// <summary>
        /// CardioTV
        /// </summary>
        public DbSet<CardioTV> CardioTVs { get; set; }


        /// <summary>
        /// AuditLog
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Question
        /// </summary>
        public DbSet<Question> Questions { get; set; }

        /// <summary>
        /// ResponseQuestion
        /// </summary>
        public DbSet<ResponseQuestion> ResponseQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<FrecuentQuestions>().HasKey(c => c.Id);
            builder.Entity<FrecuentQuestions>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<FrecuentQuestions>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<CardioTV>().HasKey(c => c.Id);
            builder.Entity<CardioTV>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<CardioTV>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<AuditLog>().HasKey(c => c.Id);
            builder.Entity<AuditLog>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<AuditLog>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Question>().HasKey(c => c.Id);
            builder.Entity<Question>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<Question>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<ResponseQuestion>().HasKey(c => c.Id);
            builder.Entity<ResponseQuestion>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<ResponseQuestion>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

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