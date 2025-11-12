using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para Permission
    /// </summary>
    public class PermissionRepository : ERepositoryBase<Permission>, IPermissionRepository
    {
        private readonly IQueryableUnitOfWork _unitOfWork;

        public PermissionRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
        {
            return await _unitOfWork.GetSet<Permission>()
                .AsNoTracking()
                .Where(p => p.Module == module && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await _unitOfWork.GetSet<Permission>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == name);
        }
    }
}
