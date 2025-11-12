using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz de repositorio para Permission
    /// </summary>
    public interface IPermissionRepository : IERepositoryBase<Permission>
    {
        /// <summary>
        /// Obtiene permisos por módulo
        /// </summary>
        Task<IEnumerable<Permission>> GetByModuleAsync(string module);

        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        Task<Permission?> GetByNameAsync(string name);
    }
}
