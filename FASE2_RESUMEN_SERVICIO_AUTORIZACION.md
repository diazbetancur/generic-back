# ?? Fase 2: Servicio de Autorización - Resumen

## ? Archivos Creados (5 archivos)

### Domain Layer
1. ? **CC.Domain/Interfaces/Services/IAuthorizationService.cs** - Interface del servicio de autorización
   - `UserHasPermissionAsync()` - Verificar permiso de usuario
   - `GetUserPermissionsAsync()` - Obtener todos los permisos de un usuario
   - `RoleHasPermissionAsync()` - Verificar permiso de rol
   - `InvalidateUserCache()` - Invalidar cache de usuario
   - `InvalidateRoleCacheAsync()` - Invalidar cache de rol

### Application Layer
2. ? **CC.Aplication/Services/AuthorizationService.cs** - Implementación con cache
   - ? Cache en memoria (10 minutos de expiración)
   - ? Logging completo de operaciones
   - ? Manejo de errores robusto
   - ? Verificación de permisos por usuario
   - ? Invalidación de cache por usuario y rol

### Infrastructure Layer
3. ? **CC.Infrastructure/Authorization/PermissionRequirement.cs** - Requisito de autorización custom
4. ? **CC.Infrastructure/Authorization/PermissionHandler.cs** - Handler de autorización
   - ? Verifica claim "UserType" (bloquea pacientes de acceso admin)
   - ? Obtiene userId del token
   - ? Usa `IAuthorizationService` para verificar permisos
   - ? Logging detallado de autorizaciones

### Configuration
5. ? **Api-Portar-Paciente/Handlers/DependencyInyectionHandler.cs** - Servicio registrado en DI

---

## ?? Funcionalidades Implementadas

### Cache de Permisos
- **Clave de cache:** `UserPermissions_{userId}`
- **Tiempo de expiración:** 10 minutos
- **Invalidación:**
  - Manual por usuario: `InvalidateUserCache(userId)`
  - Por rol: `InvalidateRoleCacheAsync(roleId)` - invalida todos los usuarios del rol

### Verificación de Permisos
```csharp
// Verificar si usuario tiene permiso
var hasPermission = await _authService.UserHasPermissionAsync(userId, "Requests.Create");

// Obtener todos los permisos de un usuario
var permissions = await _authService.GetUserPermissionsAsync(userId);
```

### Flujo de Autorización
1. Usuario hace request a endpoint protegido
2. `PermissionHandler` intercepta
3. Verifica que usuario esté autenticado
4. Verifica que UserType no sea "Patient" (para permisos admin)
5. Obtiene userId del claim
6. Consulta `IAuthorizationService.UserHasPermissionAsync()`
7. AuthorizationService:
   - Busca en cache
   - Si no existe, consulta BD (roles ? permisos)
   - Guarda en cache
   - Retorna resultado
8. Handler permite o deniega acceso

---

## ?? Políticas Configuradas (27 policies)

### Basadas en UserType (2)
- **PatientOnly**: Solo usuarios con claim `UserType = "Patient"`
- **AdminOnly**: Solo usuarios con claim `UserType = "Admin"`

### Basadas en Permisos (25)

#### Módulo Requests (6)
- `CanViewRequests` ? Requests.View
- `CanCreateRequests` ? Requests.Create
- `CanUpdateRequests` ? Requests.Update
- `CanDeleteRequests` ? Requests.Delete
- `CanAssignRequests` ? Requests.Assign
- `CanChangeRequestState` ? Requests.ChangeState

#### Módulo Users (3)
- `CanViewUsers` ? Users.View
- `CanManageUsers` ? Users.Create
- `CanAssignRoles` ? Users.AssignRoles

#### Módulo Roles (3)
- `CanViewRoles` ? Roles.View
- `CanManageRoles` ? Roles.Create
- `CanManagePermissions` ? Roles.ManagePermissions

#### Módulo Reports (3)
- `CanViewReports` ? Reports.View
- `CanExportReports` ? Reports.Export
- `CanViewAllReports` ? Reports.ViewAll

#### Módulo NilRead (3)
- `CanViewExams` ? NilRead.ViewExams
- `CanViewMedicalReports` ? NilRead.ViewReports
- `CanViewMedicalImages` ? NilRead.ViewImages

#### Módulo Configuration (3)
- `CanViewConfig` ? Config.View
- `CanUpdateConfig` ? Config.Update
- `CanViewAuditLog` ? Config.ViewAuditLog

---

## ?? PASOS MANUALES REQUERIDOS

### Paso 1: Configurar Políticas en Program.cs
Seguir instrucciones en: `PROGRAM_CS_AUTHORIZATION_CONFIG.md`

**Ubicación:** Después de la configuración de JWT, antes de `Custom DI registrations`

**Agregar:**
- `AddHttpContextAccessor()`
- Registro de `PermissionHandler`
- 27 políticas con `AddAuthorization()`

---

## ?? Uso en Controllers

### Ejemplo 1: Solo Pacientes
```csharp
[ApiController]
[Route("api/patient/[controller]")]
[Authorize(Policy = "PatientOnly")]
public class MyDataController : ControllerBase
{
    [HttpGet]
    public IActionResult GetMyData()
    {
        // Solo pacientes autenticados pueden acceder
        return Ok("Datos del paciente");
    }
}
```

### Ejemplo 2: Permiso Específico
```csharp
[ApiController]
[Route("api/admin/[controller]")]
public class RequestsController : ControllerBase
{
    // Solo usuarios con permiso "Requests.View"
    [Authorize(Policy = "CanViewRequests")]
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok("Todas las solicitudes");
    }
    
    // Solo usuarios con permiso "Requests.Update"
    [Authorize(Policy = "CanUpdateRequests")]
    [HttpPut("{id}")]
    public IActionResult Update(Guid id)
    {
        return Ok("Solicitud actualizada");
    }
}
```

### Ejemplo 3: Múltiples Políticas (OR)
```csharp
// Pacientes O usuarios con permiso
[Authorize(Policy = "PatientOnly,CanViewRequests")]
[HttpGet]
public IActionResult GetRequests() { }
```

### Ejemplo 4: Autorización Programática
```csharp
public class MyService
{
    private readonly IAuthorizationService _authService;
    
    public async Task<bool> CanUserEdit(Guid userId)
    {
        return await _authService.UserHasPermissionAsync(userId, "Requests.Update");
    }
}
```

---

## ?? Testing

### 1. Verificar que el servicio funciona
```csharp
// En un controller de prueba
[HttpGet("test-permission")]
public async Task<IActionResult> TestPermission(
    [FromServices] IAuthorizationService authService)
{
    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var hasPermission = await authService.UserHasPermissionAsync(
        userId, "Requests.View");
    
    return Ok(new { userId, hasPermission });
}
```

### 2. Verificar cache
```csharp
// Primera llamada: Consulta BD
var perms1 = await authService.GetUserPermissionsAsync(userId);

// Segunda llamada (dentro de 10 min): Obtiene del cache
var perms2 = await authService.GetUserPermissionsAsync(userId);
```

### 3. Verificar invalidación
```csharp
// Asignar nuevo permiso a rol del usuario
await roleService.AssignPermission(roleId, permissionId);

// Invalidar cache del rol
await authService.InvalidateRoleCacheAsync(roleId);

// Próxima llamada consultará BD actualizada
var perms = await authService.GetUserPermissionsAsync(userId);
```

---

## ?? Logs Esperados

### Autorización Exitosa
```
[Debug] Verificación de permiso Requests.View para usuario {guid}: True
[Debug] Usuario {guid} tiene permiso Requests.View
```

### Autorización Denegada
```
[Warning] Usuario {guid} NO tiene permiso Requests.Delete
```

### Cache Hit
```
[Debug] Permisos obtenidos del cache para usuario {guid}
```

### Cache Miss
```
[Debug] Obteniendo permisos de BD para usuario {guid}
[Information] Permisos cargados para usuario {guid}: 12 permisos únicos
```

### Invalidación
```
[Information] Cache de permisos invalidado para usuario {guid}
[Information] Cache de permisos invalidado para 5 usuarios del rol {guid}
```

---

## ?? Próximos Pasos (Fase 3)

Una vez completada la configuración manual de Program.cs, continuar con:
- [ ] Crear `RolesController` (gestión de roles)
- [ ] Crear `PermissionsController` (listar permisos disponibles)
- [ ] Endpoint para asignar permisos a roles
- [ ] Endpoint para obtener permisos de un rol
- [ ] Actualizar `RequestController` con políticas

---

## ? Checklist de Completación Fase 2

- [x] Crear IAuthorizationService
- [x] Implementar AuthorizationService con cache
- [x] Crear PermissionRequirement
- [x] Crear PermissionHandler
- [x] Registrar en DI
- [ ] **Configurar políticas en Program.cs** ?? **SIGUIENTE PASO**
- [ ] Testing de autorización básica
- [ ] Verificar logs de autorización

---

## ?? Notas Técnicas

### Performance
- Cache en memoria reduce consultas a BD
- Tiempo de cache: 10 minutos (ajustable)
- Cache se invalida automáticamente al actualizar permisos de rol

### Seguridad
- Pacientes bloqueados de permisos admin a nivel de Handler
- Verificación de autenticación previa a verificación de permisos
- Logging completo de intentos de acceso

### Escalabilidad
- Cache distribuido: Cambiar `IMemoryCache` por Redis en producción
- Considerar cache de nivel 2 (L2) para roles con muchos usuarios
- Políticas creadas una vez al startup (no hay overhead en runtime)

---

## ??? Troubleshooting

### Error: "No authorization policy found"
**Causa:** Program.cs no tiene configuración de políticas
**Solución:** Seguir `PROGRAM_CS_AUTHORIZATION_CONFIG.md`

### Usuario con permiso es rechazado
**Causa:** Cache desactualizado o rol no tiene permiso
**Solución:**
```csharp
// Invalidar cache y verificar en BD
authService.InvalidateUserCache(userId);
var perms = await authService.GetUserPermissionsAsync(userId);
// Verificar que el permiso está en la lista
```

### Paciente accede a endpoint admin
**Causa:** Policy no configurada correctamente
**Solución:** Verificar que Policy usa `PermissionRequirement` (no solo `RequireRole`)

---

**Estado actual:** ? Fase 2 código completo, esperando configuración manual de Program.cs.

Una vez completado Program.cs, ejecutar aplicación y verificar logs muestren:
```
"Políticas de autorización configuradas: PatientOnly, AdminOnly y permisos granulares"
```
