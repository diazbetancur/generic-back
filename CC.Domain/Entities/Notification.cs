namespace CC.Domain.Entities
{
    /// <summary>
    /// Preferencias de notificación del paciente
    /// </summary>
    public class Notification : EntityBase<Guid>
    {
        /// <summary>
        /// Tipo de documento del paciente
        /// </summary>
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Navegación al tipo de documento
        /// </summary>
        public virtual DocType DocType { get; set; }

        /// <summary>
        /// Número de documento del paciente
        /// </summary>
        public required string DocNumber { get; set; }

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
