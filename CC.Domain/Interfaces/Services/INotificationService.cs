using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Services
{
    /// <summary>
    /// Servicio para gestión de preferencias de notificación
    /// </summary>
    public interface INotificationService : IServiceBase<Notification, NotificationDto>
    {
    }
}