using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para gestión de preferencias de notificación
    /// </summary>
    public interface INotificationRepository : IERepositoryBase<Notification>
    {
        /// <summary>
        /// Obtiene las preferencias de notificación de un paciente por su documento
        /// </summary>
        /// <param name="docTypeId">ID del tipo de documento</param>
        /// <param name="docNumber">Número de documento</param>
        /// <returns>Preferencias de notificación o null si no existen</returns>
        Task<Notification?> GetByPatientDocumentAsync(Guid docTypeId, string docNumber);

        /// <summary>
        /// Verifica si un paciente tiene preferencias de notificación configuradas
        /// </summary>
        /// <param name="docTypeId">ID del tipo de documento</param>
        /// <param name="docNumber">Número de documento</param>
        /// <returns>True si existen preferencias configuradas</returns>
        Task<bool> ExistsAsync(Guid docTypeId, string docNumber);
    }
}
