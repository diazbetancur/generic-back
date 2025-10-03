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
    }
}