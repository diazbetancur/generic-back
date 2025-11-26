namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para preferencias de notificación del paciente
    /// </summary>
    public class NotificationDto : BaseDto<Guid>
    {
        /// <summary>
        /// Tipo de documento del paciente
        /// </summary>
        public Guid? DocTypeId { get; set; }

        /// <summary>
        /// Código del tipo de documento (ej: CC, TI)
        /// </summary>
        public string DocTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Número de documento del paciente
        /// </summary>
        public string DocNumber { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el paciente acepta recibir notificaciones por Email
        /// </summary>
        public bool Email { get; set; }

        /// <summary>
        /// Indica si el paciente acepta recibir notificaciones por SMS
        /// </summary>
        public bool SMS { get; set; }

        /// <summary>
        /// Indica si el paciente NO desea recibir ninguna notificación
        /// </summary>
        public bool NoReceiveNotifications { get; set; }
    }
}