using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de preferencias de notificación
    /// </summary>
    public class NotificationService : ServiceBase<Notification, NotificationDto>, INotificationService
    {
        public NotificationService(
            INotificationRepository repository,
            IMapper mapper,
            ILogger<NotificationService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}