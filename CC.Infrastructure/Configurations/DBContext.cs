using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Configurations
{
    /// <summary>
    /// Main database context for the application
    /// </summary>
    public class DBContext : IdentityDbContext<User, Role, Guid>, IQueryableUnitOfWork
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        // ===== CORE ENTITIES =====
        /// <summary>
        /// Audit logs for tracking system changes
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// General application settings (key-value configuration)
        /// </summary>
        public DbSet<GeneralSettings> GeneralSettings { get; set; }

        // ===== AUTHORIZATION =====
        /// <summary>
        /// System permissions
        /// </summary>
        public DbSet<Permission> Permissions { get; set; }

        /// <summary>
        /// Role-Permission relationships
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // AuditLog Configuration
            builder.Entity<AuditLog>().HasKey(c => c.Id);
            builder.Entity<AuditLog>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<AuditLog>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");

            // GeneralSettings Configuration
            builder.Entity<GeneralSettings>().HasKey(c => c.Id);
            builder.Entity<GeneralSettings>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<GeneralSettings>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<GeneralSettings>().HasIndex(g => g.Key).IsUnique();
            builder.Entity<GeneralSettings>().Property(g => g.Key).HasMaxLength(100).IsRequired();

            // Permission Configuration
            builder.Entity<Permission>().HasKey(c => c.Id);
            builder.Entity<Permission>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<Permission>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Permission>().HasIndex(p => p.Name).IsUnique();
            builder.Entity<Permission>().HasIndex(p => p.Module);
            builder.Entity<Permission>().Property(p => p.Name).HasMaxLength(100).IsRequired();
            builder.Entity<Permission>().Property(p => p.Module).HasMaxLength(50).IsRequired();
            builder.Entity<Permission>().Property(p => p.Description).HasMaxLength(250);

            // RolePermission Configuration
            builder.Entity<RolePermission>().HasKey(c => c.Id);
            builder.Entity<RolePermission>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<RolePermission>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<RolePermission>().HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

            // RolePermission Relationships
            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

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