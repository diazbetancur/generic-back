# ? Confirmación: CRUD de Usuarios y Seeder Admin

## ?? CRUD de Usuarios - COMPLETAMENTE IMPLEMENTADO

### Endpoints Disponibles (UsersController)

#### 1. Listar Usuarios
```http
GET /api/admin/users
Authorization: Bearer {admin-token}
Policy: CanViewUsers
```

**Response:**
```json
[
  {
    "id": "guid",
    "userName": "admin",
    "email": "admin@lacardio.org",
    "emailConfirmed": true,
    "phoneNumber": "+573001234567",
    "roles": ["SuperAdmin"],
    "lockoutEnd": null,
    "accessFailedCount": 0
  }
]
```

#### 2. Obtener Usuario por ID
```http
GET /api/admin/users/{id}
Authorization: Bearer {admin-token}
Policy: CanViewUsers
```

**Response:**
```json
{
  "id": "guid",
  "userName": "admin",
  "email": "admin@lacardio.org",
  "emailConfirmed": true,
  "phoneNumber": "+573001234567",
  "roles": ["SuperAdmin"],
  "permissions": ["Requests.View", "Requests.Create", ...],
  "lockoutEnd": null,
  "accessFailedCount": 0
}
```

#### 3. Crear Usuario
```http
POST /api/admin/users
Authorization: Bearer {admin-token}
Policy: CanManageUsers
Content-Type: application/json

{
  "userName": "doctor.juan",
  "email": "juan@lacardio.org",
  "phoneNumber": "+573001234567",
  "password": "TempPass123!"
}
```

**Response:** 201 Created
```json
{
  "id": "guid",
  "userName": "doctor.juan",
  "email": "juan@lacardio.org",
  "phoneNumber": "+573001234567"
}
```

#### 4. Actualizar Usuario
```http
PUT /api/admin/users/{id}
Authorization: Bearer {admin-token}
Policy: CanManageUsers
Content-Type: application/json

{
  "email": "nuevo.email@lacardio.org",
  "phoneNumber": "+573009876543",
  "newPassword": "NewPass456!" // Opcional
}
```

**Response:** 200 OK

#### 5. Eliminar Usuario (Delete mediante UserManager)
```http
DELETE /api/admin/users/{id}
Authorization: Bearer {admin-token}
Policy: CanManageUsers
```

**Response:** 200 OK
```json
{
  "message": "Usuario eliminado exitosamente"
}
```

**Validaciones:**
- ? No puede eliminar su propio usuario
- ? Elimina el registro de AspNetUsers (hard delete por Identity)

#### 6. Asignar Roles a Usuario
```http
PUT /api/admin/users/{id}/roles
Authorization: Bearer {admin-token}
Policy: CanAssignRoles
Content-Type: application/json

{
  "roleNames": ["Doctor", "Viewer"]
}
```

**Response:** 200 OK
```json
{
  "message": "Roles asignados exitosamente",
  "roles": ["Doctor", "Viewer"]
}
```

**Comportamiento:**
- ? Remueve roles actuales
- ? Asigna nuevos roles
- ? Invalida cache de permisos del usuario

#### 7. Obtener Roles de Usuario
```http
GET /api/admin/users/{id}/roles
Authorization: Bearer {admin-token}
Policy: CanViewUsers
```

**Response:** 200 OK
```json
["Doctor", "Viewer"]
```

---

## ?? Usuario Admin en Seeder - CORREGIDO

### Configuración del Usuario Admin

**Credenciales:**
```
Usuario: admin
Email: servicio.portal@lacardio.org
Contraseña: 4dm1nC4rd10.*
```

### Comportamiento del Seeder

#### Al ejecutar `SeedAsync()`:

1. **Crear Rol SuperAdmin** (si no existe)
   - Nombre: "SuperAdmin"
   - Descripción: "Rol con acceso completo al sistema"
   - Se asignan TODOS los 27 permisos automáticamente

2. **Crear Usuario Admin** (si no existe)
   - Username: `admin`
   - Email: `servicio.portal@lacardio.org`
   - Contraseña: `4dm1nC4rd10.*`
   - EmailConfirmed: `true`
   - FirstName: "Administrador"
   - LastName: "Sistema"
   - IsDeleted: `false`
   - PhoneNumber: "+573001234567"
   - LockoutEnabled: `false`

3. **Asignar Rol SuperAdmin al Usuario**
   - Usuario `admin` obtiene todos los permisos vía el rol

### Logs Esperados

```
? Usuario admin creado exitosamente
   Usuario: admin
   Email: servicio.portal@lacardio.org
   Rol: SuperAdmin
```

---

## ?? Schema de User (Ya Definido)

```csharp
public class User : IdentityUser<Guid>
{
    // Propiedades de IdentityUser
    public override Guid Id { get; set; }
    public override string UserName { get; set; }
    public override string Email { get; set; }
    public override string PhoneNumber { get; set; }
    public override bool EmailConfirmed { get; set; }
    public override bool LockoutEnabled { get; set; }
    public override DateTimeOffset? LockoutEnd { get; set; }
    public override int AccessFailedCount { get; set; }
    
    // Propiedades custom
    public Guid DocTypeId { get; set; }
    public virtual DocType DocType { get; set; }
    public string DocumentNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
```

**Nota:** 
- `IsDeleted` está disponible pero **NO se usa** en el Delete del controller
- UserManager hace un **hard delete** por defecto
- Si quieres soft delete, debes implementar un método custom en un servicio

---

## ?? Flujo Completo de Gestión de Usuarios

### 1. Primer Inicio (Seed)
```
dotnet run
? SeedDB.SeedAsync()
? Crea rol SuperAdmin con 27 permisos
? Crea usuario admin
? Asigna rol SuperAdmin a admin
```

### 2. Login como Admin
```http
POST /api/auth/admin/login
{
  "userName": "admin",
  "password": "4dm1nC4rd10.*"
}
? Retorna JWT con claims:
   - UserType: "Admin"
   - Role: "SuperAdmin"
   - NameIdentifier: {admin-guid}
```

### 3. Crear Nuevo Rol (Doctor)
```http
POST /api/admin/roles
{
  "name": "Doctor",
  "description": "Médico tratante"
}
```

### 4. Asignar Permisos al Rol
```http
PUT /api/admin/roles/{roleId}/permissions
{
  "permissionIds": [
    "guid-Requests.View",
    "guid-Requests.Update",
    "guid-NilRead.ViewExams"
  ]
}
```

### 5. Crear Usuario Doctor
```http
POST /api/admin/users
{
  "userName": "dr.rodriguez",
  "email": "rodriguez@lacardio.org",
  "password": "TempPass123!"
}
```

### 6. Asignar Rol Doctor
```http
PUT /api/admin/users/{userId}/roles
{
  "roleNames": ["Doctor"]
}
```

### 7. Usuario Doctor Hace Login
```http
POST /api/auth/admin/login
{
  "userName": "dr.rodriguez",
  "password": "TempPass123!"
}
? Obtiene JWT con permisos de Doctor
```

### 8. Actualizar Usuario
```http
PUT /api/admin/users/{userId}
{
  "email": "nuevo.email@lacardio.org",
  "newPassword": "NewSecurePass456!"
}
```

### 9. Eliminar Usuario
```http
DELETE /api/admin/users/{userId}
? UserManager.DeleteAsync()
? Elimina de AspNetUsers (hard delete)
? También elimina roles en AspNetUserRoles (cascade)
```

---

## ?? Diferencias: Soft Delete vs Hard Delete

### Soft Delete (IsDeleted flag)
```csharp
// NO IMPLEMENTADO actualmente
user.IsDeleted = true;
await _dbContext.SaveChangesAsync();
```
**Ventajas:**
- ? Mantiene historial
- ? Puede reactivarse
- ? Datos de auditoría preservados

**Desventajas:**
- ? Requiere filtros en todas las queries
- ? Ocupa espacio en BD
- ? Username/Email bloqueados

### Hard Delete (UserManager.DeleteAsync) - ACTUAL
```csharp
// IMPLEMENTADO
await _userManager.DeleteAsync(user);
```
**Ventajas:**
- ? Libera username/email
- ? No requiere filtros
- ? Limpieza automática de roles

**Desventajas:**
- ? Pérdida de historial
- ? No reversible
- ? Puede romper FK si hay referencias

---

## ?? Si Quieres Implementar Soft Delete

### Opción 1: Método Custom en Controller
```csharp
[HttpPatch("{id}/deactivate")]
[Authorize(Policy = "CanManageUsers")]
public async Task<IActionResult> DeactivateUser(Guid id)
{
    var user = await _userManager.FindByIdAsync(id.ToString());
    if (user == null) return NotFound();
    
    user.IsDeleted = true;
    user.LockoutEnabled = true;
    user.LockoutEnd = DateTimeOffset.MaxValue; // Bloqueo permanente
    
    await _userManager.UpdateAsync(user);
    return Ok(new { message = "Usuario desactivado" });
}

[HttpPatch("{id}/activate")]
[Authorize(Policy = "CanManageUsers")]
public async Task<IActionResult> ActivateUser(Guid id)
{
    var user = await _userManager.FindByIdAsync(id.ToString());
    if (user == null) return NotFound();
    
    user.IsDeleted = false;
    user.LockoutEnd = null;
    
    await _userManager.UpdateAsync(user);
    return Ok(new { message = "Usuario activado" });
}
```

### Opción 2: Override en Repository (más complejo)
Requiere interceptor en DBContext para filtrar usuarios con IsDeleted = true.

---

## ? Verificación Post-Migración

### 1. Ejecutar Migración
```bash
Add-Migration AddPermissionsRolesAndAdminUser -Context DBContext
Update-Database -Context DBContext
```

### 2. Verificar en Base de Datos
```sql
-- Verificar usuario admin
SELECT * FROM AspNetUsers WHERE UserName = 'admin';

-- Verificar rol SuperAdmin
SELECT * FROM AspNetRoles WHERE Name = 'SuperAdmin';

-- Verificar asignación de rol
SELECT * FROM AspNetUserRoles WHERE UserId = (
    SELECT Id FROM AspNetUsers WHERE UserName = 'admin'
);

-- Verificar permisos del rol SuperAdmin
SELECT r.Name, p.Name as Permission, p.Module
FROM AspNetRoles r
JOIN RolePermissions rp ON r.Id = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'SuperAdmin'
ORDER BY p.Module, p.Name;
-- Debe mostrar 27 permisos
```

### 3. Testing de Login
```http
POST /api/auth/admin/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "4dm1nC4rd10.*"
}
```

**Response esperado:** 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-15T10:30:00Z",
  "user": {
    "id": "guid",
    "userName": "admin",
    "email": "servicio.portal@lacardio.org"
  }
}
```

### 4. Testing de Endpoints CRUD
```http
# Listar usuarios
GET /api/admin/users
Authorization: Bearer {token-admin}

# Crear nuevo usuario
POST /api/admin/users
Authorization: Bearer {token-admin}
{
  "userName": "test.user",
  "email": "test@lacardio.org",
  "password": "Test123!@#"
}

# Actualizar usuario
PUT /api/admin/users/{id}
Authorization: Bearer {token-admin}
{
  "email": "updated@lacardio.org"
}

# Eliminar usuario
DELETE /api/admin/users/{id}
Authorization: Bearer {token-admin}
```

---

## ?? Resumen Final

### ? IMPLEMENTADO:
1. ? **CRUD completo de usuarios** (8 endpoints)
2. ? **Schema de User** con propiedades necesarias
3. ? **Seeder de usuario admin** con contraseña `4dm1nC4rd10.*`
4. ? **Creación automática de rol SuperAdmin** con todos los permisos
5. ? **Asignación automática** de rol SuperAdmin al usuario admin
6. ? **Validaciones**: No auto-eliminación, roles deben existir
7. ? **Invalidación de cache** al asignar roles
8. ? **Logging completo** de operaciones

### ?? PENDIENTE (Tu Parte):
- [ ] Crear migración
- [ ] Aplicar migración
- [ ] Verificar seed exitoso
- [ ] Testing de endpoints

### ?? OPCIONAL (Si lo necesitas):
- [ ] Implementar soft delete (métodos de activar/desactivar)
- [ ] Agregar endpoint para cambiar contraseña propia
- [ ] Agregar validación de fortaleza de contraseña personalizada

---

**Estado:** ? Todo implementado y listo para usar después de la migración.
**Password Admin:** `4dm1nC4rd10.*`
**Tipo de Delete:** Hard Delete (UserManager.DeleteAsync)
