using Microsoft.AspNetCore.Identity;

namespace CC.Domain.Entities
{
    /// <summary>
    /// Application user extending ASP.NET Core Identity
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Full name (computed property)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}