using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.Domain.Dtos
{
    /// <summary>
    /// Clase base para todos los DTOs del dominio
    /// </summary>
    /// <typeparam name="T">Tipo del identificador</typeparam>
    public class BaseDto<T>
    {
        /// <summary>
        /// Identificador único
        /// </summary>
        public T? Id { get; set; }

        /// <summary>
        /// Fecha y hora de creación (UTC)
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// Token de versión para control de concurrencia optimista.
        /// Debe ser enviado en actualizaciones para detectar cambios concurrentes.
        /// </summary>
        public byte[]? RowVersion { get; set; }
    }
}