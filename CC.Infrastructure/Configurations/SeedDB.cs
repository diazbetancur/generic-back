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
                        NormalizedName = adminRoleName.ToUpperInvariant(),
                        isSystem = true
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
                    PhoneNumber = "3001234567",
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
                    Key = "MessageDataPolicyAcceptances",
                    Value = "Mensaje para mostrar ",
                    Description = "Pendiente el mensaje que va ir aqui",
                }
);
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

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "OtpMessageTemplate",
                    Value = "Su código de verificación es: {OTP}. Válido por {MINUTES} minutos. Portal Paciente. Fundación Cardioinfantil - LaCardio",
                    Description = "Template del mensaje OTP para SMS (texto plano)",
                }
                );

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "OtpEmailTemplate",
                    Value = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Código de Verificación</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                    <!-- Header con logo -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #0066cc 0%, #004c99 100%); padding: 30px 40px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: bold;'>Portal Paciente</h1>
                            <p style='color: #e6f2ff; margin: 5px 0 0 0; font-size: 14px;'>Fundación Cardioinfantil - LaCardio</p>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px;'>
                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                Cordial saludo,
                            </p>

                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                Se ha generado un código de verificación para acceder al Portal de Paciente de la Fundación Cardioinfantil - LaCardio.
                            </p>

                            <!-- Código OTP destacado -->
                            <table width='100%' cellpadding='0' cellspacing='0' style='margin: 0 0 30px 0;'>
                                <tr>
                                    <td align='center' style='background-color: #f8f9fa; border: 2px dashed #0066cc; border-radius: 8px; padding: 30px;'>
                                        <p style='color: #666666; font-size: 14px; margin: 0 0 10px 0; text-transform: uppercase; letter-spacing: 1px;'>
                                            Su código de verificación es:
                                        </p>
                                        <p style='color: #0066cc; font-size: 36px; font-weight: bold; margin: 0; letter-spacing: 8px; font-family: Courier New, monospace;'>
                                            {OTP}
                                        </p>
                                        <p style='color: #999999; font-size: 12px; margin: 15px 0 0 0;'>
                                            Válido por {MINUTES} minutos
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 10px 0;'>
                                Muchas gracias.
                            </p>

                            <p style='color: #0066cc; font-size: 16px; line-height: 1.6; margin: 0; font-weight: 600;'>
                                Fundación Cardioinfantil - LaCardio
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 20px 40px; border-top: 1px solid #e0e0e0;'>
                            <p style='color: #999999; font-size: 12px; line-height: 1.6; margin: 0 0 10px 0; text-align: center;'>
                                Este es un mensaje automático, por favor no responda a este correo.
                            </p>
                            <p style='color: #999999; font-size: 12px; line-height: 1.6; margin: 0; text-align: center;'>
                                Si no solicitó este código, por favor ignore este mensaje.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                    Description = "Template HTML del mensaje OTP para Email",
                }
                );

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "ConfirmLoginEmailTemplate",
                    Value = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Ingreso Exitoso</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>

                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #0066cc 0%, #004c99 100%); padding: 30px 40px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: bold;'>Portal Paciente</h1>
                            <p style='color: #e6f2ff; margin: 5px 0 0 0; font-size: 14px;'>Fundación Cardioinfantil - LaCardio</p>
                        </td>
                    </tr>

                    <!-- Contenido -->
                    <tr>
                        <td style='padding: 40px;'>

                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                Cordial saludo,
                            </p>

                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                Le informamos que su ingreso al <strong>Portal de Paciente</strong> de la Fundación Cardioinfantil - LaCardio se realizó de manera <strong>exitosa</strong>.
                            </p>

                            <table width='100%' cellpadding='0' cellspacing='0' style='margin: 0 0 30px 0;'>
                                <tr>
                                    <td style='background-color: #f8f9fa; border-radius: 8px; padding: 25px; border: 1px solid #e0e0e0;'>
                                        <p style='color: #666666; font-size: 14px; margin: 0 0 5px 0; text-transform: uppercase; letter-spacing: 1px;'>
                                            Fecha y hora del acceso:
                                        </p>

                                        <p style='color: #0066cc; font-size: 20px; font-weight: bold; margin: 10px 0 0 0;'>
                                            {DATETIME}
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 10px 0;'>
                                Si no reconoce este inicio de sesión, por favor comuníquese con nosotros de inmediato.
                            </p>

                            <p style='color: #0066cc; font-size: 16px; line-height: 1.6; margin: 0; font-weight: 600;'>
                                Fundación Cardioinfantil - LaCardio
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 20px 40px; border-top: 1px solid #e0e0e0;'>
                            <p style='color: #999999; font-size: 12px; line-height: 1.6; margin: 0 0 10px 0; text-align: center;'>
                                Este es un mensaje automático, por favor no responda a este correo.
                            </p>
                            <p style='color: #999999; font-size: 12px; line-height: 1.6; margin: 0; text-align: center;'>
                                Si no realizó este ingreso, por favor repórtelo inmediatamente.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                    Description = "Template HTML del mensaje de confirmación de ingreso exitoso al Portal Paciente",
                });

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
                    DateCreated = DateTime.UtcNow,
                    IsSystem = true
                });

                _dbContext.States.Add(new Domain.Entities.State
                {
                    Id = Guid.NewGuid(),
                    Name = "Anulada por tiempo",
                    HexColor = "#F44336",
                    IsDeleted = false,
                    DateCreated = DateTime.UtcNow,
                    IsSystem = true
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
                    // Módulo FrequentQuestions (Preguntas Frecuentes)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "FrequentQuestions.View", Module = "FrequentQuestions", Description = "Ver preguntas frecuentes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "FrequentQuestions.Create", Module = "FrequentQuestions", Description = "Crear preguntas frecuentes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "FrequentQuestions.Update", Module = "FrequentQuestions", Description = "Actualizar preguntas frecuentes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "FrequentQuestions.Delete", Module = "FrequentQuestions", Description = "Eliminar preguntas frecuentes", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Survey (Encuestas)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Survey.View", Module = "Survey", Description = "Ver encuestas", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Survey.Create", Module = "Survey", Description = "Crear encuestas", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Survey.Update", Module = "Survey", Description = "Actualizar encuestas", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Survey.Delete", Module = "Survey", Description = "Eliminar encuestas", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Survey.ViewResults", Module = "Survey", Description = "Ver resultados de encuestas", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo States (Estados)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "States.View", Module = "States", Description = "Ver estados de solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "States.Create", Module = "States", Description = "Crear estados", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "States.Update", Module = "States", Description = "Actualizar estados", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "States.Delete", Module = "States", Description = "Eliminar estados", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo RequestType (Tipos de Solicitud)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "RequestType.View", Module = "RequestType", Description = "Ver tipos de solicitud", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "RequestType.Create", Module = "RequestType", Description = "Crear tipos de solicitud", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "RequestType.Update", Module = "RequestType", Description = "Actualizar tipos de solicitud", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "RequestType.Delete", Module = "RequestType", Description = "Eliminar tipos de solicitud", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Requests (Solicitudes)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.View", Module = "Requests", Description = "Ver solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Create", Module = "Requests", Description = "Crear solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Update", Module = "Requests", Description = "Actualizar solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Delete", Module = "Requests", Description = "Eliminar solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.Assign", Module = "Requests", Description = "Asignar solicitudes a usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Requests.ChangeState", Module = "Requests", Description = "Cambiar estado de solicitudes", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Users (Usuarios)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.View", Module = "Users", Description = "Ver usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Create", Module = "Users", Description = "Crear usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Update", Module = "Users", Description = "Actualizar usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.Delete", Module = "Users", Description = "Eliminar usuarios", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Users.AssignRoles", Module = "Users", Description = "Asignar roles a usuarios", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Roles (Roles)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.View", Module = "Roles", Description = "Ver roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Create", Module = "Roles", Description = "Crear roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Update", Module = "Roles", Description = "Actualizar roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.Delete", Module = "Roles", Description = "Eliminar roles", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Roles.ManagePermissions", Module = "Roles", Description = "Gestionar permisos de roles", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Reports (Reportes del Sistema)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.View", Module = "Reports", Description = "Ver reportes del sistema", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.Create", Module = "Reports", Description = "Crear reportes personalizados", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.Export", Module = "Reports", Description = "Exportar reportes", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Reports.ViewAll", Module = "Reports", Description = "Ver todos los reportes del sistema", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Configuration (Configuración del Sistema)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.View", Module = "Configuration", Description = "Ver configuraciones del sistema", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.Update", Module = "Configuration", Description = "Actualizar configuraciones del sistema", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.ViewAuditLog", Module = "Configuration", Description = "Ver logs de auditoría", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Configuration.ManageTemplates", Module = "Configuration", Description = "Gestionar plantillas de email/SMS", IsActive = true, DateCreated = DateTime.UtcNow },

                    // Módulo Telemetry (Telemetría y Analytics)
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Telemetry.View", Module = "Telemetry", Description = "Ver telemetría propia", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Telemetry.ViewAll", Module = "Telemetry", Description = "Ver toda la telemetría del sistema", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Telemetry.Export", Module = "Telemetry", Description = "Exportar datos de telemetría", IsActive = true, DateCreated = DateTime.UtcNow },
                    new Domain.Entities.Permission { Id = Guid.NewGuid(), Name = "Telemetry.ViewMetrics", Module = "Telemetry", Description = "Ver métricas del sistema", IsActive = true, DateCreated = DateTime.UtcNow },
                };

                await _dbContext.Permissions.AddRangeAsync(permissions);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}