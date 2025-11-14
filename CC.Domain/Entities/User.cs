using Microsoft.AspNetCore.Identity;

namespace CC.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public Guid DocTypeId { get; set; }
        public virtual DocType DocType { get; set; }
        public string DocumentNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}