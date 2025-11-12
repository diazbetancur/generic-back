# Configuración de Autorización - Program.cs

## AGREGAR DESPUÉS DE LA CONFIGURACIÓN DE AUTENTICACIÓN JWT

Buscar la sección donde termina la configuración de autenticación:
```csharp
Log.Information("Autenticación JWT configurada correctamente");
```

Inmediatamente DESPUÉS de esa línea, AGREGAR:

```csharp
// ===== AUTHORIZATION POLICIES CONFIGURATION =====
builder.Services.AddHttpContextAccessor();

// Registrar Authorization Handler
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, 
    CC.Infrastructure.Authorization.PermissionHandler>();

builder.Services.AddAuthorization(options =>
{
    // ===== Políticas basadas en UserType =====
    
    // Policy para solo pacientes
    options.AddPolicy("PatientOnly", policy =>
        policy.RequireClaim("UserType", "Patient"));
    
    // Policy para solo administradores (cualquier rol de admin)
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("UserType", "Admin"));
    
    // ===== Políticas basadas en permisos específicos =====
    
    // Módulo Requests
    options.AddPolicy("CanViewRequests", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Requests.View")));
    
    options.AddPolicy("CanCreateRequests", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Requests.Create")));
    
    options.AddPolicy("CanUpdateRequests", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Requests.Update")));
    
    options.AddPolicy("CanDeleteRequests", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Requests.Delete")));
    
    options.AddPolicy("CanAssignRequests", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Requests.Assign")));
    
    options.AddPolicy("CanChangeRequestState", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Requests.ChangeState")));
    
    // Módulo Users
    options.AddPolicy("CanViewUsers", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Users.View")));
    
    options.AddPolicy("CanManageUsers", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Users.Create")));
    
    options.AddPolicy("CanAssignRoles", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Users.AssignRoles")));
    
    // Módulo Roles
    options.AddPolicy("CanViewRoles", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Roles.View")));
    
    options.AddPolicy("CanManageRoles", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Roles.Create")));
    
    options.AddPolicy("CanManagePermissions", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Roles.ManagePermissions")));
    
    // Módulo Reports
    options.AddPolicy("CanViewReports", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Reports.View")));
    
    options.AddPolicy("CanExportReports", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Reports.Export")));
    
    options.AddPolicy("CanViewAllReports", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Reports.ViewAll")));
    
    // Módulo NilRead
    options.AddPolicy("CanViewExams", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("NilRead.ViewExams")));
    
    options.AddPolicy("CanViewMedicalReports", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("NilRead.ViewReports")));
    
    options.AddPolicy("CanViewMedicalImages", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("NilRead.ViewImages")));
    
    // Módulo Configuration
    options.AddPolicy("CanViewConfig", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Config.View")));
    
    options.AddPolicy("CanUpdateConfig", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Config.Update")));
    
    options.AddPolicy("CanViewAuditLog", policy =>
        policy.AddRequirements(new CC.Infrastructure.Authorization.PermissionRequirement("Config.ViewAuditLog")));
});

Log.Information("Políticas de autorización configuradas: PatientOnly, AdminOnly y permisos granulares");
```

## ? Verificación

Después de agregar el código, verificar que:
1. No hay errores de compilación
2. La aplicación inicia correctamente
3. Los logs muestran: "Políticas de autorización configuradas..."

## ?? Testing

Para probar que las políticas funcionan:

1. **Endpoint sin autorización:**
```csharp
[HttpGet("public")]
public IActionResult Public() => Ok("Acceso público");
```

2. **Endpoint solo para pacientes:**
```csharp
[Authorize(Policy = "PatientOnly")]
[HttpGet("patient-only")]
public IActionResult PatientOnly() => Ok("Solo pacientes");
```

3. **Endpoint con permiso específico:**
```csharp
[Authorize(Policy = "CanViewRequests")]
[HttpGet("admin/requests")]
public IActionResult AdminRequests() => Ok("Requiere permiso Requests.View");
```

## ?? Políticas Configuradas (27 policies)

### Políticas de UserType (2)
- PatientOnly
- AdminOnly

### Políticas de Permisos (25)
- Requests: 6 políticas
- Users: 3 políticas
- Roles: 3 políticas
- Reports: 3 políticas
- NilRead: 3 políticas
- Configuration: 3 políticas

---

**Nota:** Estas políticas se usarán en los controllers mediante el atributo `[Authorize(Policy = "NombrePolicy")]`
