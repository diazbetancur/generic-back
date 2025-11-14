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
            await EnsureDocType();
            await EnsureGeneralSettings();
            await EnsureStates();
            await EnsurePermissions();
            await EnsureRolesWithPermissions();
            await EnsureAdmin();
        }

        /// <summary>
        /// Asegura que exista el usuario admin con contraseña predeterminada
        /// </summary>
        private async Task EnsureAdmin()
        {
            const string adminUserName = "admin";
            const string adminEmail = "servicio.portal@lacardio.org";
            const string adminPassword = "4dm1nC4rd10.*";
            const string adminRoleName = "SuperAdmin";

            try
            {
                var existingAdmin = await _userManager.FindByNameAsync(adminUserName);
                if (existingAdmin != null)
                {
                    return; // Usuario admin ya existe
                }

                if (!await _roleManager.RoleExistsAsync(adminRoleName))
                {
                    var superAdminRole = new Domain.Entities.Role
                    {
                        Name = adminRoleName,
                        Description = "Rol con acceso completo al sistema",
                        NormalizedName = adminRoleName.ToUpperInvariant()
                    };

                    var roleResult = await _roleManager.CreateAsync(superAdminRole);
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Error al crear rol SuperAdmin: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }

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

                // Crear usuario admin
                var adminUser = new Domain.Entities.User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Administrador",
                    LastName = "Sistema",
                    IsDeleted = false,
                    PhoneNumber = "+573001234567",
                    LockoutEnabled = false,
                    DocumentNumber = "ADMIN001",
                    DocTypeId = _dbContext.DocTypes.First(d => d.Code == "CC").Id
                };

                var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Error al crear usuario admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                if (!addToRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Error al asignar rol SuperAdmin: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }

                Console.WriteLine($"✅ Usuario admin creado exitosamente");
                Console.WriteLine($"   Usuario: {adminUserName}");
                Console.WriteLine($"   Email: {adminEmail}");
                Console.WriteLine($"   Rol: {adminRoleName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear usuario admin: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureDocType()
        {
            if (!_dbContext.DocTypes.Any())
            {
                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "CC",
                    Description = "Cédula de Ciudadanía",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "CE",
                    Description = "Cédula de Extranjería",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "TI",
                    Description = "Tarjeta de Identidad",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "RC",
                    Description = "Registro Civil",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "PP",
                    Description = "Pasaporte",
                    IsActive = true,
                }
);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task EnsureGeneralSettings()
        {
            if (!_dbContext.GeneralSettings.Any())
            {
                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "TokenLifetimeMinutes",
                    Value = "30",
                    Description = "Tiempo de vida del token en minutos",
                }
                );

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "OtpTtlSeconds",
                    Value = "300",
                    Description = "Tiempo de vida del OTP en segundos",
                }
                );

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "MensajeSinContacto",
                    Value = "No se encontraron medios de contacto registrados. Por favor comuníquese al WhatsApp +57 300 123 45 67 para actualizar sus datos.",
                    Description = "Mensaje mostrado cuando el usuario no tiene email ni celular registrado",
                }
                );

                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task EnsureStates()
        {
            if (!_dbContext.States.Any())
            {
                _dbContext.States.Add(new Domain.Entities.State
                {
                    Id = Guid.NewGuid(),
                    Name = "Recibida",
                    HexColor = "#2196F3",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                });

                _dbContext.States.Add(new Domain.Entities.State
                {
                    Id = Guid.NewGuid(),
                    Name = "En Proceso",
                    HexColor = "#FF9800",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                });

                _dbContext.States.Add(new Domain.Entities.State
                {
                    Id = Guid.NewGuid(),
                    Name = "Completada",
                    HexColor = "#4CAF50",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                });

                _dbContext.States.Add(new Domain.Entities.State
                {
                    Id = Guid.NewGuid(),
                    Name = "Requiere Informacion",
                    HexColor = "#9E9E9E",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                });

                _dbContext.States.Add(new Domain.Entities.State
                {
                    Id = Guid.NewGuid(),
                    Name = "Cancelada",
                    HexColor = "#F44336",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task EnsurePermissions()
        {
            if (!_dbContext.Permissions.Any())
            {
                var permissions = new[]
                {
                    // Módulo Requests
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.View", Module = "Requests", Description = "Ver solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Create", Module = "Requests", Description = "Crear solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Update", Module = "Requests", Description = "Actualizar solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Delete", Module = "Requests", Description = "Eliminar solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Assign", Module = "Requests", Description = "Asignar solicitudes a usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.ChangeState", Module = "Requests", Description = "Cambiar estado de solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Users
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.View", Module = "Users", Description = "Ver usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Create", Module = "Users", Description = "Crear usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Update", Module = "Users", Description = "Actualizar usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Delete", Module = "Users", Description = "Eliminar usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.AssignRoles", Module = "Users", Description = "Asignar roles a usuarios", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Roles
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.View", Module = "Roles", Description = "Ver roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Create", Module = "Roles", Description = "Crear roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Update", Module = "Roles", Description = "Actualizar roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Delete", Module = "Roles", Description = "Eliminar roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.ManagePermissions", Module = "Roles", Description = "Gestionar permisos de roles", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Reports
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.View", Module = "Reports", Description = "Ver reportes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.Export", Module = "Reports", Description = "Exportar reportes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.ViewAll", Module = "Reports", Description = "Ver todos los reportes del sistema", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo NilRead
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "NilRead.ViewExams", Module = "NilRead", Description = "Ver exámenes diagnósticos", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "NilRead.ViewReports", Module = "NilRead", Description = "Ver informes PDF", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "NilRead.ViewImages", Module = "NilRead", Description = "Ver imágenes diagnósticas", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Configuration
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Config.View", Module = "Configuration", Description = "Ver configuraciones", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Config.Update", Module = "Configuration", Description = "Actualizar configuraciones", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Config.ViewAuditLog", Module = "Configuration", Description = "Ver logs de auditoría", IsActive = true, DateCreated = DateTime.UtcNow },
                };

                await _dbContext.Permissions.AddRangeAsync(permissions);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task EnsureRolesWithPermissions()
        {
            // Este método se completará en la Fase 2 cuando implementemos el AuthorizationService
            // Por ahora es un placeholder para que el seed funcione
            await Task.CompletedTask;
        }
    }
}