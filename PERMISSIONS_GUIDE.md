# ?? Guía: Cómo Agregar Nuevos Permisos al Sistema

## ?? Proceso Completo en 5 Pasos

### ? Paso 1: Agregar Constantes de Permisos

**Archivo:** `CC.Domain/Constants/PermissionConstants.cs`

```csharp
/// <summary>
/// Módulo de [NombreModulo]
/// </summary>
public static class [NombreModulo]
{
    public const string Module = "[NombreModulo]";
    public const string View = "[NombreModulo].View";
    public const string Create = "[NombreModulo].Create";
    public const string Update = "[NombreModulo].Update";
    public const string Delete = "[NombreModulo].Delete";
    // ... otros permisos específicos
}
```

**Ejemplo real agregado:**
```csharp
public static class Content
{
    public const string Module = "Content";
    public const string View = "Content.View";
    public const string Create = "Content.Create";
    public const string Update = "Content.Update";
    public const string Delete = "Content.Delete";
    public const string Publish = "Content.Publish";
}
```

---

### ? Paso 2: Agregar Políticas de Autorización

**En el mismo archivo**, dentro de la clase `Policies`:

```csharp
public static class Policies
{
    // ...existing policies...
    
    // Políticas de [NombreModulo]
    public const string CanView[NombreModulo] = "CanView[NombreModulo]";
    public const string CanManage[NombreModulo] = "CanManage[NombreModulo]";
}
```

**Ejemplo real agregado:**
```csharp
// Políticas de Content
public const string CanViewContent = "CanViewContent";
public const string CanManageContent = "CanManageContent";
public const string CanPublishContent = "CanPublishContent";
```

---

### ? Paso 3: Actualizar Métodos Auxiliares

**En `PermissionConstants.cs`**, actualizar:

1. **GetAllPermissions():**
```csharp
public static string[] GetAllPermissions()
{
    return new[]
    {
        // ...existing permissions...
        // [NombreModulo]
        [NombreModulo].View, [NombreModulo].Create, [NombreModulo].Update, [NombreModulo].Delete,
    };
}
```

2. **GetPermissionsByModule():**
```csharp
public static string[] GetPermissionsByModule(string module)
{
    return module switch
    {
        // ...existing cases...
        "[NombreModulo]" => new[] { 
            [NombreModulo].View, 
            [NombreModulo].Create, 
            [NombreModulo].Update, 
            [NombreModulo].Delete 
        },
        _ => Array.Empty<string>()
    };
}
```

---

### ? Paso 4: Agregar Permisos al Seeder

**Archivo:** `CC.Infrastructure/Configurations/SeedDB.cs`

**En el método `EnsurePermissions()`, agregar:**

```csharp
// Permisos de [NombreModulo]
new Permission 
{ 
    Name = PermissionConstants.[NombreModulo].View, 
    Module = PermissionConstants.[NombreModulo].Module, 
    Description = "Ver [descripción]",
    IsActive = true
},
new Permission 
{ 
    Name = PermissionConstants.[NombreModulo].Create, 
    Module = PermissionConstants.[NombreModulo].Module, 
    Description = "Crear [descripción]",
    IsActive = true
},
// ... etc
```

**Ejemplo real:**
```csharp
// Permisos de Content (Gestión de Contenido)
new Permission 
{ 
    Name = PermissionConstants.Content.View, 
    Module = PermissionConstants.Content.Module, 
    Description = "Ver contenido (CardioTV, FAQs, etc.)",
    IsActive = true
},
new Permission 
{ 
    Name = PermissionConstants.Content.Create, 
    Module = PermissionConstants.Content.Module, 
    Description = "Crear nuevo contenido",
    IsActive = true
},
new Permission 
{ 
    Name = PermissionConstants.Content.Update, 
    Module = PermissionConstants.Content.Module, 
    Description = "Editar contenido existente",
    IsActive = true
},
new Permission 
{ 
    Name = PermissionConstants.Content.Delete, 
    Module = PermissionConstants.Content.Module, 
    Description = "Eliminar contenido",
    IsActive = true
},
new Permission 
{ 
    Name = PermissionConstants.Content.Publish, 
    Module = PermissionConstants.Content.Module, 
    Description = "Publicar/despublicar contenido",
    IsActive = true
},
```

---

### ? Paso 5: Configurar Políticas en AuthorizationPoliciesConfiguration

**Archivo:** `Api-Portar-Paciente/Configuration/AuthorizationPoliciesConfiguration.cs`

**En el método `ConfigurePolicies()`, agregar:**

```csharp
// ===== MÓDULO [NOMBREMODULO] =====

options.AddPolicy(PermissionConstants.Policies.CanView[NombreModulo], policy =>
    policy.AddRequirements(new PermissionRequirement(PermissionConstants.[NombreModulo].View)));

options.AddPolicy(PermissionConstants.Policies.CanManage[NombreModulo], policy =>
    policy.AddRequirements(new PermissionRequirement(PermissionConstants.[NombreModulo].Create)));
```

**Ejemplo real:**
```csharp
// ===== MÓDULO CONTENT =====

options.AddPolicy(PermissionConstants.Policies.CanViewContent, policy =>
    policy.AddRequirements(new PermissionRequirement(PermissionConstants.Content.View)));

options.AddPolicy(PermissionConstants.Policies.CanManageContent, policy =>
    policy.AddRequirements(new PermissionRequirement(PermissionConstants.Content.Create)));

options.AddPolicy(PermissionConstants.Policies.CanPublishContent, policy =>
    policy.AddRequirements(new PermissionRequirement(PermissionConstants.Content.Publish)));
```

---

## ?? Cómo Aplicar los Cambios

### 1. **Crear Migración**
```bash
Add-Migration AddNewPermissions -Context DBContext
```

### 2. **Aplicar Migración**
```bash
Update-Database -Context DBContext
```

### 3. **Ejecutar Seeder**
El seeder se ejecuta automáticamente al iniciar la aplicación si hay permisos nuevos.

### 4. **Verificar en Base de Datos**
```sql
SELECT * FROM Permissions WHERE Module = 'Content';
```

---

## ?? Cómo Usar los Nuevos Permisos en Controllers

### Opción 1: A Nivel de Controller (Toda la clase)
```csharp
[Authorize(Policy = "CanManageContent")]
public class CardioTVController : ControllerBase
{
    // Todos los métodos requieren "Content.Create"
}
```

### Opción 2: A Nivel de Método (Granular)
```csharp
[Authorize]
public class CardioTVController : ControllerBase
{
    [HttpGet] // Solo requiere autenticación
    public async Task<IActionResult> GetAll() { }
    
    [HttpPost]
    [Authorize(Policy = "CanManageContent")] // Requiere permiso específico
    public async Task<IActionResult> Create([FromBody] CardioTVDto dto) { }
    
    [HttpPut]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update([FromBody] CardioTVDto dto) { }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Delete(Guid id) { }
}
```

---

## ?? Permisos Actualmente Implementados

| Módulo | Permisos | Estado |
|--------|----------|--------|
| **Requests** | View, Create, Update, Delete, Assign, ChangeState | ? Implementado |
| **Users** | View, Create, Update, Delete, AssignRoles | ? Implementado |
| **Roles** | View, Create, Update, Delete, ManagePermissions | ? Implementado |
| **Reports** | View, Export, ViewAll | ? Implementado |
| **NilRead** | ViewExams, ViewReports, ViewImages | ? Implementado |
| **Configuration** | View, Update, ViewAuditLog | ? Implementado |
| **Content** | View, Create, Update, Delete, Publish | ? **NUEVO** |
| **States** | View, Manage | ? **NUEVO** |
| **Telemetry** | View, ViewAll, Export | ? **NUEVO** |

---

## ??? Herramientas de Gestión

### Endpoint para Listar Todos los Permisos
```http
GET /api/admin/permissions
Authorization: Bearer {admin-token}
```

### Endpoint para Asignar Permisos a un Rol
```http
PUT /api/admin/roles/{roleId}/permissions
Content-Type: application/json
Authorization: Bearer {admin-token}

{
  "permissionIds": [
    "guid-del-permiso-1",
    "guid-del-permiso-2"
  ]
}
```

### Endpoint para Ver Permisos de un Usuario
```http
GET /api/authinfo/me
Authorization: Bearer {user-token}
```

Respuesta:
```json
{
  "userId": "...",
  "permissions": [
    "Content.View",
    "Content.Create",
    "Content.Update"
  ]
}
```

---

## ?? Notas Importantes

1. **SuperAdmin siempre tiene acceso total** sin necesidad de permisos explícitos
2. **Los permisos se cachean** - usa `InvalidateUserCache()` después de cambios
3. **Naming Convention:** `{Module}.{Action}` (ej: `Content.Create`)
4. **Policy Naming:** `Can{Action}{Module}` (ej: `CanManageContent`)
5. **Los cambios en PermissionConstants requieren recompilación**

---

## ?? Flujo Completo de Ejemplo

```
1. Developer agrega constantes ? PermissionConstants.cs
2. Developer agrega al seeder ? SeedDB.cs
3. Developer configura políticas ? AuthorizationPoliciesConfiguration.cs
4. Developer crea migración ? Add-Migration
5. Developer aplica migración ? Update-Database
6. Seeder crea permisos en BD (auto al iniciar app)
7. Admin asigna permisos a roles ? /api/admin/roles/{id}/permissions
8. Frontend usa permisos para UI ? GET /api/authinfo/me
9. Backend valida en controllers ? [Authorize(Policy = "...")]
```

---

## ?? Checklist de Implementación

- [ ] Agregar constantes en `PermissionConstants.cs`
- [ ] Agregar políticas en `PermissionConstants.Policies`
- [ ] Actualizar `GetAllPermissions()`
- [ ] Actualizar `GetPermissionsByModule()`
- [ ] Agregar permisos en `SeedDB.EnsurePermissions()`
- [ ] Configurar políticas en `AuthorizationPoliciesConfiguration`
- [ ] Crear migración
- [ ] Aplicar migración
- [ ] Verificar en base de datos
- [ ] Asignar permisos a roles
- [ ] Usar en controllers
- [ ] Probar con diferentes roles

---

## ?? Resultado Final

Después de completar todos los pasos, tendrás:

? Nuevos permisos en la base de datos  
? Políticas de autorización configuradas  
? Controllers protegidos con permisos granulares  
? Frontend puede consultar permisos del usuario  
? SuperAdmin mantiene acceso total automático  

---

**Estado:** ? Content, States y Telemetry ya agregados  
**Próximos módulos sugeridos:** DocTypes, RequestTypes, GeneralSettings

