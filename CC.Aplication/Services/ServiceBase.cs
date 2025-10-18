using AutoMapper;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio base genérico que implementa operaciones CRUD estándar
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad del dominio</typeparam>
    /// <typeparam name="TEntityDto">Tipo de DTO de la entidad</typeparam>
    public abstract class ServiceBase<TEntity, TEntityDto> : IServiceBase<TEntity, TEntityDto>
        where TEntity : class
        where TEntityDto : class
    {
        private readonly IERepositoryBase<TEntity> repository;
        private readonly IMapper mapper;
        protected readonly ILogger Logger;

        protected ServiceBase(
            IERepositoryBase<TEntity> repository, 
            IMapper mapper,
            ILogger logger)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.Logger = logger;
        }

        public virtual async Task<TEntityDto> AddAsync(TEntityDto entityDto)
        {
            Logger.LogInformation(
                "Creando nueva entidad {EntityType}",
                typeof(TEntity).Name);

            try
            {
                var result = mapper.Map<TEntityDto>(
                    await repository.AddAsync(mapper.Map<TEntity>(entityDto))
                        .ConfigureAwait(false));

                Logger.LogInformation(
                    "Entidad {EntityType} creada exitosamente con ID: {EntityId}",
                    typeof(TEntity).Name,
                    result?.GetType().GetProperty("Id")?.GetValue(result));

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al crear entidad {EntityType}",
                    typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntityDto> entities)
        {
            var count = entities?.Count() ?? 0;
            Logger.LogInformation(
                "Creando {Count} entidades {EntityType}",
                count, typeof(TEntity).Name);

            try
            {
                await repository.AddRangeAsync(mapper.Map<IEnumerable<TEntity>>(entities))
                    .ConfigureAwait(false);

                Logger.LogInformation(
                    "{Count} entidades {EntityType} creadas exitosamente",
                    count, typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al crear {Count} entidades {EntityType}",
                    count, typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return await repository.AnyAsync(filter).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(TEntityDto entity)
        {
            var entityId = entity?.GetType().GetProperty("Id")?.GetValue(entity);
            Logger.LogInformation(
                "Eliminando entidad {EntityType} con ID: {EntityId}",
                typeof(TEntity).Name, entityId);

            try
            {
                await repository.DeleteAsync(mapper.Map<TEntity>(entity)).ConfigureAwait(false);

                Logger.LogInformation(
                    "Entidad {EntityType} con ID: {EntityId} eliminada exitosamente",
                    typeof(TEntity).Name, entityId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al eliminar entidad {EntityType} con ID: {EntityId}",
                    typeof(TEntity).Name, entityId);
                throw;
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TEntityDto> entities)
        {
            var count = entities?.Count() ?? 0;
            Logger.LogInformation(
                "Eliminando {Count} entidades {EntityType}",
                count, typeof(TEntity).Name);

            try
            {
                await repository.DeleteRangeAsync(mapper.Map<IEnumerable<TEntity>>(entities))
                    .ConfigureAwait(false);

                Logger.LogInformation(
                    "{Count} entidades {EntityType} eliminadas exitosamente",
                    count, typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al eliminar {Count} entidades {EntityType}",
                    count, typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task ExecuteSqlCommandAsync(string query, params object[] parameters)
        {
            Logger.LogInformation(
                "Ejecutando comando SQL: {Query}",
                query);

            await repository.ExecuteSqlCommandAsync(query, parameters).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntityDto>> ExecuteStoredProcedureAsync(string query, params object[] parameters)
        {
            Logger.LogInformation(
                "Ejecutando procedimiento almacenado: {Query}",
                query);

            return mapper.Map<IEnumerable<TEntityDto>>(
                await repository.ExecuteStoredProcedureAsync(query, parameters)
                    .ConfigureAwait(false));
        }

        public virtual async Task<TEntityDto> FindByIdAsync(dynamic id)
        {
            var entity = await repository.FindByIdAsync(id).ConfigureAwait(false);
            
            if (entity == null)
            {
                Logger.LogWarning(
                    "Entidad {EntityType} con ID: {EntityId} no encontrada",
                    typeof(TEntity).Name, (object)id);
            }
            
            return entity != null ? mapper.Map<TEntityDto>(entity) : null;
        }

        public virtual async Task<TEntityDto> FindByAlternateKeyAsync(Expression<Func<TEntity, bool>> alternateKey, string includeProperties = "")
        {
            var entity = await repository.FindByAlternateKeyAsync(alternateKey, includeProperties)
                .ConfigureAwait(false);
            return entity != null ? mapper.Map<TEntityDto>(entity) : null;
        }

        public virtual async Task<IEnumerable<TEntityDto>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, 
            string includeProperties = "")
        {
            Logger.LogInformation(
                "Obteniendo todas las entidades {EntityType}",
                typeof(TEntity).Name);

            var results = mapper.Map<IEnumerable<TEntityDto>>(
                await repository.GetAllAsync(filter, orderBy, includeProperties)
                    .ConfigureAwait(false));

            Logger.LogInformation(
                "Se obtuvieron {Count} entidades {EntityType}",
                results?.Count() ?? 0, typeof(TEntity).Name);

            return results;
        }

        public virtual async Task<IEnumerable<TEntityDto>> GetAllPagedAsync(
            int take, 
            int skip, 
            Expression<Func<TEntity, bool>> filter = null, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, 
            string includeProperties = "")
        {
            Logger.LogInformation(
                "Obteniendo entidades {EntityType} paginadas. Skip: {Skip}, Take: {Take}",
                typeof(TEntity).Name, skip, take);

            return mapper.Map<IEnumerable<TEntityDto>>(
                await repository.GetAllPagedAsync(take, skip, filter, orderBy, includeProperties)
                    .ConfigureAwait(false));
        }

        public virtual async Task UpdateAsync(TEntityDto entity)
        {
            var entityId = entity?.GetType().GetProperty("Id")?.GetValue(entity);
            Logger.LogInformation(
                "Actualizando entidad {EntityType} con ID: {EntityId}",
                typeof(TEntity).Name, entityId);

            try
            {
                await repository.UpdateAsync(mapper.Map<TEntity>(entity)).ConfigureAwait(false);

                Logger.LogInformation(
                    "Entidad {EntityType} con ID: {EntityId} actualizada exitosamente",
                    typeof(TEntity).Name, entityId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogWarning(ex,
                    "Conflicto de concurrencia al actualizar entidad {EntityType} con ID: {EntityId}",
                    typeof(TEntity).Name, entityId);
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al actualizar entidad {EntityType} con ID: {EntityId}",
                    typeof(TEntity).Name, entityId);
                throw;
            }
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntityDto> entities)
        {
            var count = entities?.Count() ?? 0;
            Logger.LogInformation(
                "Actualizando {Count} entidades {EntityType}",
                count, typeof(TEntity).Name);

            try
            {
                await repository.UpdateRangeAsync(mapper.Map<IEnumerable<TEntity>>(entities))
                    .ConfigureAwait(false);

                Logger.LogInformation(
                    "{Count} entidades {EntityType} actualizadas exitosamente",
                    count, typeof(TEntity).Name);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogWarning(ex,
                    "Conflicto de concurrencia al actualizar {Count} entidades {EntityType}",
                    count, typeof(TEntity).Name);
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error al actualizar {Count} entidades {EntityType}",
                    count, typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task TruncateTableAsync()
        {
            Logger.LogWarning(
                "Truncando tabla {EntityType}",
                typeof(TEntity).Name);

            await repository.ExecuteSqlCommandAsync($"TRUNCATE TABLE [used].[{typeof(TEntityDto).Name}]")
                .ConfigureAwait(false);
        }
    }
}