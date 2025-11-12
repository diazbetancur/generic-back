# ?? Fase 3: Controllers de Administración - Resumen

## ? Archivos Creados (3 controllers)

### Controllers de Administración
1. ? **Api-Portar-Paciente/Controllers/Admin/PermissionsController.cs**
   - GET /api/admin/permissions - Listar todos los permisos agrupados por módulo
   - GET /api/admin/permissions/module/{module} - Permisos por módulo
   - GET /api/admin/permissions/{id} - Obtener permiso por ID
   - GET /api/admin/permissions/modules - Listar módulos disponibles

2. ? **Api-Portar-Paciente/Controllers/Admin/RolesController.cs**
   - GET /api/admin/roles - Listar todos los roles
   - GET /api/admin/roles/{id} - Obtener rol por ID
   - POST /api/admin/roles - Crear nuevo rol
   - PUT /api/admin/roles/{id} - Actualizar rol
   - DELETE /api/admin/roles/{id} - Eliminar rol
   - GET /api/admin/roles/{id}/permissions - Obtener permisos de un rol
   - PUT /api/admin/roles/{id}/permissions - Actualizar permisos de un rol

3. ? **Api-Portar-Paciente/Controllers/Admin/UsersController.cs**
   - GET /api/admin/users - Listar todos los usuarios
   - GET /api/admin/users/{id} - Obtener usuario por ID
   - POST /api/admin/users - Crear nuevo usuario
   - PUT /api/admin/users/{id} - Actualizar usuario
   - DELETE /api/admin/users/{id} - Eliminar usuario
   - GET /api/admin/users/{id}/roles - Obtener roles de un usuario
   - PUT /api/admin/users/{id}/roles - Asignar roles a un usuario

---

## ?? ERRORES A CORREGIR (4 issues)

### Issue 1: Paquetes NuGet Faltantes en CC.Infrastructure
**Error:** `Microsoft.AspNetCore.Authorization` not found

**Solución:** Instalar paquetes (ya documentado en `NUGET_PACKAGES_INFRASTRUCTURE.md`)
```bash
Install-Package Microsoft.AspNetCore.Authorization -ProjectName CC.Infrastructure
Install-Package Microsoft.AspNetCore.Http.Abstractions -ProjectName CC.Infrastructure
```

---

### Issue 2: Entidad Role sin propiedad Description
**Error:** `Role does not contain a definition for 'Description'`

**Solución:** Agregar propiedad Description a la entidad Role

**Archivo:** `CC.Domain/Entities/Role.cs`

**Agregar:**
```csharp
/// <summary>
/// Descripción del rol
/// </summary>
public string? Description { get; set; }
```

**Ubicación:** Después de la propiedad `Name` o cualquier propiedad existente.

---

### Issue 3: Conflicto de Namespace IAuthorizationService
**Error:** `'IAuthorizationService' is an ambiguous reference`

**Solución:** Usar alias en RolesController y UsersController

**En RolesController.cs, agregar al inicio:**
```csharp
using AuthService = CC.Domain.Interfaces.Services.IAuthorizationService;
```

**Y cambiar:**
```csharp
// DE:
private readonly IAuthorizationService _authService;

// A:
private readonly AuthService _authService;
```

**Lo mismo en el constructor:**
```csharp
// DE:
IAuthorizationService authService,

// A:
AuthService authService,
```

**Aplicar el mismo cambio en UsersController.cs**

---

### Issue 4: GetUsersInRoleAsync no existe en RoleManager<Role>
**Error:** `RoleManager<Role>' does not contain a definition for 'GetUsersInRoleAsync'`

**Solución:** Usar UserManager en su lugar

**En RolesController.cs, agregar inyección de UserManager:**
```csharp
private readonly UserManager<User> _userManager;

public RolesController(
    RoleManager<Role> roleManager,
    UserManager<User> userManager,  // <-- AGREGAR
    IPermissionRepository permissionRepo,
    IRolePermissionRepository rolePermissionRepo,
    AuthService authService,
    ILogger<RolesController> logger)
{
    _roleManager = roleManager;
    _userManager = userManager;  // <-- AGREGAR
    _permissionRepo = permissionRepo;
    _rolePermissionRepo = rolePermissionRepo;
    _authService = authService;
    _logger = logger;
}
```

**Y en el método DeleteRole, cambiar:**
```csharp
// DE:
var usersInRole = await _roleManager.GetUsersInRoleAsync(role.Name!);

// A:
var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
```

---

## ?? Correcciones Resumidas

### 1. Instalar Paquetes NuGet
```bash
Install-Package Microsoft.AspNetCore.Authorization -ProjectName CC.Infrastructure
Install-Package Microsoft.AspNetCore.Http.Abstractions -ProjectName CC.Infrastructure
```

### 2. Actualizar Role.cs
Agregar propiedad `Description`:
```csharp
public string? Description { get; set; }
```

### 3. RolesController.cs - Cambios
```csharp
// Línea 1: Agregar alias
using AuthService = CC.Domain.Interfaces.Services.IAuthorizationService;

// Línea 22: Agregar UserManager
private readonly UserManager<User> _userManager;

// Línea 24: Cambiar tipo
private readonly AuthService _authService;

// Constructor: Agregar UserManager
public RolesController(
    RoleManager<Role> roleManager,
    UserManager<User> userManager,  // <-- NUEVO
    IPermissionRepository permissionRepo,
    IRolePermissionRepository rolePermissionRepo,
    AuthService authService,  // <-- CAMBIO DE TIPO
    ILogger<RolesController> logger)
{
    _roleManager = roleManager;
    _userManager = userManager;  // <-- NUEVO
    // ...resto del código
}

// Método DeleteRole (línea ~310): Cambiar
var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
```

### 4. UsersController.cs - Cambios
```csharp
// Línea 1: Agregar alias
using AuthService = CC.Domain.Interfaces.Services.IAuthorizationService;

// Línea 21: Cambiar tipo
private readonly AuthService _authService;

// Constructor: Cambiar tipo
public UsersController(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    AuthService authService,  // <-- CAMBIO DE TIPO
    ILogger<UsersController> logger)
```

---

## ? Después de Correcciones

### Build
```bash
dotnet build
```

Debe compilar sin errores.

### Testing de Endpoints

#### 1. Listar Permisos
```bash
GET /api/admin/permissions
Authorization: Bearer {admin-token}
```

**Response esperado:**
```json
[
  {
    "module": "Requests",
    "permissions": [
      {
        "id": "guid",
        "name": "Requests.View",
        "module": "Requests",
        "description": "Ver solicitudes",
        "isActive": true
      }
    ]
  }
]
```

#### 2. Crear Rol
```bash
POST /api/admin/roles
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "Doctor",
  "description": "Médico tratante"
}
```

#### 3. Asignar Permisos a Rol
```bash
PUT /api/admin/roles/{roleId}/permissions
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "permissionIds": [
    "guid-permission-1",
    "guid-permission-2"
  ]
}
```

#### 4. Crear Usuario
```bash
POST /api/admin/users
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "userName": "doctor.juan",
  "email": "juan@cardioinfantil.org",
  "phoneNumber": "+573001234567",
  "password": "TempPass123!"
}
```

#### 5. Asignar Roles a Usuario
```bash
PUT /api/admin/users/{userId}/roles
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "roleNames": ["Doctor", "Viewer"]
}
```

---

## ?? Flujo Completo de Configuración

### 1. Crear Rol
```
POST /api/admin/roles
Body: { "name": "Doctor", "description": "Médico" }
? Retorna roleId
```

### 2. Asignar Permisos al Rol
```
PUT /api/admin/roles/{roleId}/permissions
Body: { "permissionIds": [...] }
? Rol tiene permisos
```

### 3. Crear Usuario
```
POST /api/admin/users
Body: { "userName": "...", "email": "...", "password": "..." }
? Retorna userId
```

### 4. Asignar Rol a Usuario
```
PUT /api/admin/users/{userId}/roles
Body: { "roleNames": ["Doctor"] }
? Usuario tiene rol ? Usuario tiene permisos
```

### 5. Usuario Hace Login
```
POST /api/auth/admin/login (endpoint a implementar en Fase 4)
? Retorna JWT con claims:
   - UserType: "Admin"
   - Role: "Doctor"
   - NameIdentifier: userId
```

### 6. Usuario Accede a Endpoint Protegido
```
GET /api/admin/requests
Authorization: Bearer {token}
[Authorize(Policy = "CanViewRequests")]
? PermissionHandler verifica permiso
? AuthorizationService consulta cache o BD
? Encuentra "Requests.View" en permisos del usuario
? Acceso permitido
```

---

## ?? Endpoints por Policy

### PermissionsController
- `CanViewRoles` - Todos los endpoints (GET)

### RolesController
- `CanViewRoles` - GET (listar, obtener por ID, permisos de rol)
- `CanManageRoles` - POST, PUT, DELETE (crear, actualizar, eliminar)
- `CanManagePermissions` - PUT /roles/{id}/permissions

### UsersController
- `CanViewUsers` - GET (listar, obtener por ID, roles de usuario)
- `CanManageUsers` - POST, PUT, DELETE (crear, actualizar, eliminar)
- `CanAssignRoles` - PUT /users/{id}/roles

---

## ?? Seguridad Implementada

### 1. Todos requieren AdminOnly
```csharp
[Authorize(Policy = "AdminOnly")]
public class RolesController
```
? Pacientes bloqueados automáticamente

### 2. Permisos Granulares
Cada endpoint verifica permiso específico adicional

### 3. Prevención de Auto-Eliminación
```csharp
// En UsersController.DeleteUser
if (currentUserId == id.ToString())
    return BadRequest("No puede eliminarse a sí mismo");
```

### 4. Validación de Roles
```csharp
// Antes de eliminar rol
var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
if (usersInRole.Any())
    return BadRequest("Rol tiene usuarios asignados");
```

### 5. Invalidación de Cache
```csharp
// Al actualizar permisos de rol
await _authService.InvalidateRoleCacheAsync(roleId);

// Al asignar roles a usuario
_authService.InvalidateUserCache(userId);
```

---

## ?? Próxima Fase (Fase 4)

Una vez corregidos los errores y verificado el build:

### Fase 4: Endpoints de Autenticación Admin
- [ ] POST /api/auth/admin/login - Login para usuarios admin
- [ ] Generar JWT con claims UserType="Admin" y roles
- [ ] Actualizar RequestController con policies
- [ ] Testing completo end-to-end

---

## ? Checklist Fase 3

- [x] PermissionsController creado
- [x] RolesController creado
- [x] UsersController creado
- [ ] **Instalar paquetes NuGet** ?? **PENDIENTE**
- [ ] **Agregar Description a Role** ?? **PENDIENTE**
- [ ] **Corregir conflicto IAuthorizationService** ?? **PENDIENTE**
- [ ] **Agregar UserManager a RolesController** ?? **PENDIENTE**
- [ ] Build exitoso
- [ ] Testing de endpoints

---

**Estado:** ? Código completo, 4 correcciones menores pendientes para compilar.
