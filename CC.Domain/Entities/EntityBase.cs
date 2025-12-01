using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.Domain.Entities
{
    public class EntityBase<T>
    {
        /// <summary>
        /// Id model
        /// </summary>
        [Key]
        public T Id { get; set; }

        /// <summary>
        /// Date created
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Constructor to initialize default values
        /// </summary>
        protected EntityBase()
        {
            // Generate Guid for Id if T is Guid
            if (typeof(T) == typeof(Guid))
            {
                Id = (T)(object)Guid.NewGuid();
            }

            // Set DateCreated to current UTC time
            DateCreated = DateTime.UtcNow;
        }
    }
}