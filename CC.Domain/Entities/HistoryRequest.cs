namespace CC.Domain.Entities
{
    /// <summary>
    /// Historial de cambios de una solicitud (tracking de estados y modificaciones)
    /// </summary>
    public class HistoryRequest : EntityBase<Guid>
    {
        /// <summary>
        /// ID de la solicitud relacionada
        /// </summary>
        public Guid RequestId { get; set; }

        public virtual Request Request { get; set; }

        /// <summary>
        /// Estado anterior (en creación: estado inicial "Creado")
        /// </summary>
        public Guid? OldStateId { get; set; }

        public virtual State OldState { get; set; }

        /// <summary>
        /// Nuevo estado (en creación: null, ya que no hay cambio de estado)
        /// </summary>
        public Guid? NewStateId { get; set; }

        public virtual State NewState { get; set; }

        /// <summary>
        /// Usuario que realizó el cambio (null en creación automática desde frontend)
        /// </summary>
        public Guid? UserId { get; set; }

        public virtual User User { get; set; }

        /// <summary>
        /// Descripción de los cambios realizados
        /// </summary>
        public string Changes { get; set; }
    }
}