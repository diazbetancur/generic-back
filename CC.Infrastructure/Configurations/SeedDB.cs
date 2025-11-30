using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Configurations
{
    public class SeedDB
    {
        private readonly DBContext _dbContext;
        private readonly UserManager<Domain.Entities.User> _userManager;
        private readonly RoleManager<Domain.Entities.Role> _roleManager;

        public SeedDB(
            DBContext dBContext,
            UserManager<Domain.Entities.User> userManager,
            RoleManager<Domain.Entities.Role> roleManager)
        {
            _dbContext = dBContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await EnsureBasicPermissions();
            await EnsureAdmin();
        }

        /// <summary>
        /// Ensures basic permissions exist for a generic system
        /// </summary>
        private async Task EnsureBasicPermissions()
        {
            if (!_dbContext.Permissions.Any())
            {
                var permissions = new[]
                {
                    // User Management Module
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.View", Module = "Users", Description = "View users", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Create", Module = "Users", Description = "Create users", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Update", Module = "Users", Description = "Update users", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Delete", Module = "Users", Description = "Delete users", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.AssignRoles", Module = "Users", Description = "Assign roles to users", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Role Management Module
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.View", Module = "Roles", Description = "View roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Create", Module = "Roles", Description = "Create roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Update", Module = "Roles", Description = "Update roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Delete", Module = "Roles", Description = "Delete roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.ManagePermissions", Module = "Roles", Description = "Manage role permissions", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Configuration Module
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.View", Module = "Configuration", Description = "View system configurations", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.Update", Module = "Configuration", Description = "Update system configurations", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.ViewAuditLog", Module = "Configuration", Description = "View audit logs", IsActive = true, DateCreated = DateTime.UtcNow },
                };

                await _dbContext.Permissions.AddRangeAsync(permissions);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Ensures default admin user exists
        /// </summary>
        private async Task EnsureAdmin()
        {
            const string adminUserName = "admin";
            const string adminEmail = "admin@yourcompany.com";
            const string adminPassword = "Admin123!*";
            const string adminRoleName = "SuperAdmin";

            try
            {
                var existingAdmin = await _userManager.FindByNameAsync(adminUserName);
                if (existingAdmin != null)
                {
                    return; // Admin user already exists
                }

                // Create SuperAdmin role if it doesn't exist
                if (!await _roleManager.RoleExistsAsync(adminRoleName))
                {
                    var superAdminRole = new Domain.Entities.Role
                    {
                        Name = adminRoleName,
                        Description = "Full system access role",
                        NormalizedName = adminRoleName.ToUpperInvariant(),
                        isSystem = true
                    };

                    var roleResult = await _roleManager.CreateAsync(superAdminRole);
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Error creating SuperAdmin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }

                    // Assign all permissions to SuperAdmin role
                    var allPermissions = await _dbContext.Permissions.ToListAsync();
                    foreach (var permission in allPermissions)
                    {
                        var rolePermission = new Domain.Entities.RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = superAdminRole.Id,
                            PermissionId = permission.Id,
                            DateCreated = DateTime.UtcNow
                        };
                        await _dbContext.RolePermissions.AddAsync(rolePermission);
                    }
                    await _dbContext.SaveChangesAsync();
                }

                // Create admin user
                var adminUser = new Domain.Entities.User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    IsDeleted = false,
                    PhoneNumber = "+1234567890",
                    LockoutEnabled = false
                };

                var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Error creating admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                if (!addToRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Error assigning SuperAdmin role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }

                Console.WriteLine($"✅ Admin user created successfully");
                Console.WriteLine($"   Username: {adminUserName}");
                Console.WriteLine($"   Email: {adminEmail}");
                Console.WriteLine($"   Role: {adminRoleName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating admin user: {ex.Message}");
                throw;
            }
        }
    }
}
