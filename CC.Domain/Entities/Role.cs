using Microsoft.AspNetCore.Identity;

namespace CC.Domain.Entities
{
    public class Role : IdentityRole<Guid>
    {
        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string? Description { get; set; }

        public bool IsDeleted { get; set; }
    }
}