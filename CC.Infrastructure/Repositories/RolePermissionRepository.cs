using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para RolePermission
    /// </summary>
    public class RolePermissionRepository : ERepositoryBase<RolePermission>, IRolePermissionRepository
    {
        private readonly IQueryableUnitOfWork _unitOfWork;

        public RolePermissionRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            return await _unitOfWork.GetSet<RolePermission>()
                .AsNoTracking()
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task DeleteByRoleIdAsync(Guid roleId)
        {
            var rolePermissions = await _unitOfWork.GetSet<RolePermission>()
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            if (rolePermissions.Any())
            {
                _unitOfWork.GetSet<RolePermission>().RemoveRange(rolePermissions);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName)
        {
            return await _unitOfWork.GetSet<RolePermission>()
                .AsNoTracking()
                .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permissionName && rp.Permission.IsActive);
        }
    }
}
