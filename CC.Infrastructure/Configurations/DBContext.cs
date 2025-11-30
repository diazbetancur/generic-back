using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Configurations
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación
    /// </summary>
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