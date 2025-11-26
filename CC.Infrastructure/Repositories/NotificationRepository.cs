using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para gestión de preferencias de notificación
    /// </summary>
    public class NotificationRepository : ERepositoryBase<Notification>, INotificationRepository
    {
        private readonly IQueryableUnitOfWork _unitOfWork;

        public NotificationRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Notification?> GetByPatientDocumentAsync(Guid docTypeId, string docNumber)
        {
            return await _unitOfWork.GetSet<Notification>()
                .AsNoTracking()
                .Include(n => n.DocType)
                .FirstOrDefaultAsync(n => n.DocTypeId == docTypeId && n.DocNumber == docNumber);
        }

        public async Task<bool> ExistsAsync(Guid docTypeId, string docNumber)
        {
            return await _unitOfWork.GetSet<Notification>()
                .AsNoTracking()
                .AnyAsync(n => n.DocTypeId == docTypeId && n.DocNumber == docNumber);
        }
    }
}
