namespace CC.Domain.Entities
{
    /// <summary>
    /// Solicitud creada por un paciente
    /// </summary>
    public class Request : EntityBase<Guid>
    {
        /// <summary>
        /// Tipo de documento del paciente
        /// </summary>
        public Guid DocTypeId { get; set; }
        public virtual DocType DocType { get; set; }

        /// <summary>
        /// Número de documento del paciente
        /// </summary>
        public string DocNumber { get; set; }

        /// <summary>
        /// Estado actual de la solicitud (siempre tiene valor, inicial: "Creado")
        /// </summary>
        public Guid StateId { get; set; }
        public virtual State State { get; set; }

        /// <summary>
        /// Tipo de solicitud
        /// </summary>
        public Guid RequestTypeId { get; set; }
        public virtual RequestType RequestType { get; set; }

        /// <summary>
        /// Descripción de la solicitud proporcionada por el paciente
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Usuario asignado para atender la solicitud (null hasta que se asigne)
        /// </summary>
        public Guid? AssignedUserId { get; set; }
        public virtual User AssignedUser { get; set; }
    }
}