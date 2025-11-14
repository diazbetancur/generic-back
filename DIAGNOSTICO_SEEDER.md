# ?? Diagnóstico: ¿Por qué no se ejecuta el Seeder?

## ? El Seeder SÍ está configurado

### Ubicación en Program.cs (líneas 310-325):

```csharp
// ===== APLICAR MIGRACIONES Y SEED =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<DBContext>();
        await db.Database.MigrateAsync();  // ? Aplica migraciones

        var seeder = services.GetRequiredService<SeedDB>();
        await seeder.SeedAsync();  // ? Ejecuta seed
        
        Log.Information("? Migraciones aplicadas e inicialización de datos completada");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "? Error al aplicar migraciones/seed al iniciar la aplicación");
        // ?? NO lanza excepción - La app sigue funcionando
    }
}
```

---

## ?? Posibles Causas

### 1. ?? Falta la Migración (MÁS PROBABLE)

**Problema:** Las tablas `Permissions`, `RolePermissions`, `PasswordResetTokens` no existen aún.

**Síntoma:**
```
? Error al aplicar migraciones/seed al iniciar la aplicación
System.Data.SqlClient.SqlException: Invalid object name 'Permissions'.
```

**Solución:**
```bash
# Crear migración
Add-Migration AddPermissionsRolesAndAdmin -Context DBContext

# Aplicar migración
Update-Database -Context DBContext

# Ejecutar aplicación
dotnet run
```

### 2. ? Ya se ejecutó exitosamente

**Problema:** Los datos ya existen en BD y las validaciones previenen duplicados.

**Verificar en BD:**
```sql
-- Ver tipos de documento (debe tener 5)
SELECT * FROM DocTypes;

-- Ver estados (debe tener 5)
SELECT * FROM States;

-- Ver permisos (debe tener 27)
SELECT * FROM Permissions;

-- Ver usuario admin
SELECT * FROM AspNetUsers WHERE UserName = 'admin';

-- Ver rol SuperAdmin
SELECT * FROM AspNetRoles WHERE Name = 'SuperAdmin';

-- Ver permisos del rol SuperAdmin (debe tener 27)
SELECT r.Name, COUNT(*) as PermisosAsignados
FROM AspNetRoles r
JOIN RolePermissions rp ON r.Id = rp.RoleId
WHERE r.Name = 'SuperAdmin'
GROUP BY r.Name;
```

### 3. ?? Error en Dependency Injection

**Problema:** `SeedDB` no está registrado en DI.

**Verificar en DependencyInyectionHandler.cs:**
```csharp
// Debe existir esta línea (al final del método DepencyInyectionConfig)
services.AddTransient<SeedDB>();
```

**Si no existe, agregar:**
```csharp
public static void DepencyInyectionConfig(...)
{
    // ... otras configuraciones ...
    
    services.AddTransient<SeedDB>();  // ? Debe estar aquí
}
```

### 4. ?? Problema con UserManager/RoleManager

**Problema:** Identity no está configurado antes del seeder.

**Verificar orden en Program.cs:**
```csharp
// ? CORRECTO - Este orden debe existir:
1. builder.Services.AddIdentity<User, Role>(...) 
2. builder.Services.AddEntityFrameworkStores<DBContext>()
3. Api_Portar_Paciente.Handlers.DependencyInyectionHandler.DepencyInyectionConfig(...)
4. var app = builder.Build();
5. using (var scope = app.Services.CreateScope()) { ... seeder ... }
```

---

## ?? Verificación Rápida

### Paso 1: Ver logs en consola al ejecutar

```bash
dotnet run

# Buscar en la salida:
[INFO] Ejecutándose en ambiente: Development
[INFO] Identity configurado correctamente
[INFO] Políticas de autorización configuradas...
# ? ? ? Buscar esta línea específica ? ? ?
[INFO] ? Migraciones aplicadas e inicialización de datos completada

# O buscar errores:
[ERROR] ? Error al aplicar migraciones/seed al iniciar la aplicación
```

### Paso 2: Si ves error, ver detalles

El error se logea pero la app sigue corriendo. Buscar en logs:

```
[ERROR] ? Error al aplicar migraciones/seed al iniciar la aplicación
System.InvalidOperationException: No service for type 'SeedDB' has been registered
```

O

```
[ERROR] ? Error al aplicar migraciones/seed al iniciar la aplicación
Microsoft.Data.SqlClient.SqlException: Invalid object name 'Permissions'
```

### Paso 3: Verificar que SeedDB está en DI

```csharp
// Buscar en DependencyInyectionHandler.cs (al final)
services.AddTransient<SeedDB>();
```

### Paso 4: Verificar migración

```bash
# Listar migraciones
dotnet ef migrations list --project CC.Infrastructure

# Debe aparecer algo como:
20250115_AddPermissionsRolesAndAdmin (Pending)  ? Si dice Pending, falta aplicar

# Aplicar todas las pendientes
dotnet ef database update --project CC.Infrastructure
```

---

## ?? Solución Completa

### Si el problema es FALTA LA MIGRACIÓN:

```bash
# 1. Crear migración
Add-Migration AddAdminSystemComplete -Context DBContext

# 2. Revisar migración generada
# Verificar que incluye:
# - Tabla Permissions
# - Tabla RolePermissions  
# - Tabla PasswordResetTokens
# - Columna Description en AspNetRoles

# 3. Aplicar migración
Update-Database -Context DBContext

# 4. Ejecutar aplicación
dotnet run

# 5. Buscar en logs:
? Migraciones aplicadas e inicialización de datos completada
? Usuario admin creado exitosamente
   Usuario: admin
   Email: servicio.portal@lacardio.org
   Rol: SuperAdmin
```

### Si el problema es FALTA REGISTRO EN DI:

```csharp
// En DependencyInyectionHandler.cs, al final del método:
public static void DepencyInyectionConfig(IServiceCollection services, IConfiguration configuration, string environment)
{
    // ... todo el código existente ...
    
    services.AddTransient<SeedDB>();  // ? Agregar esta línea si no existe
}
```

### Si el problema es YA SE EJECUTÓ:

```sql
-- Verificar datos en BD
SELECT 'DocTypes' as Tabla, COUNT(*) as Registros FROM DocTypes
UNION ALL
SELECT 'States', COUNT(*) FROM States
UNION ALL
SELECT 'Permissions', COUNT(*) FROM Permissions
UNION ALL
SELECT 'Usuarios', COUNT(*) FROM AspNetUsers
UNION ALL
SELECT 'Roles', COUNT(*) FROM AspNetRoles;

-- Resultado esperado:
-- DocTypes     5
-- States       5
-- Permissions  27
-- Usuarios     1 (admin)
-- Roles        1 (SuperAdmin)
```

---

## ?? Validación Final

### 1. Verificar Estructura de BD

```sql
-- Verificar tablas creadas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Permissions', 'RolePermissions', 'PasswordResetTokens')
ORDER BY TABLE_NAME;

-- Resultado esperado:
-- PasswordResetTokens
-- Permissions
-- RolePermissions
```

### 2. Verificar Usuario Admin

```sql
-- Ver usuario admin
SELECT Id, UserName, Email, FirstName, LastName, EmailConfirmed, LockoutEnabled
FROM AspNetUsers 
WHERE UserName = 'admin';

-- Verificar rol
SELECT r.Name, r.Description
FROM AspNetRoles r
WHERE r.Name = 'SuperAdmin';

-- Verificar asignación de rol
SELECT u.UserName, r.Name as Rol
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'admin';
```

### 3. Verificar Permisos

```sql
-- Ver permisos por módulo
SELECT Module, COUNT(*) as CantidadPermisos
FROM Permissions
WHERE IsActive = 1
GROUP BY Module
ORDER BY Module;

-- Resultado esperado:
-- Configuration    3
-- NilRead          3
-- Reports          3
-- Requests         6
-- Roles            5
-- Users            5
-- TOTAL: 27 permisos
```

### 4. Verificar Permisos del SuperAdmin

```sql
-- Ver permisos asignados al rol SuperAdmin
SELECT r.Name as Rol, p.Module, p.Name as Permiso, p.Description
FROM AspNetRoles r
JOIN RolePermissions rp ON r.Id = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.Id
WHERE r.Name = 'SuperAdmin'
ORDER BY p.Module, p.Name;

-- Debe mostrar 27 permisos
```

---

## ?? Checklist de Diagnóstico

```
[ ] 1. Verificar que la migración existe y está aplicada
      dotnet ef migrations list

[ ] 2. Verificar que SeedDB está registrado en DI
      Buscar: services.AddTransient<SeedDB>();

[ ] 3. Ejecutar aplicación y buscar log de éxito
      Buscar: "? Migraciones aplicadas e inicialización de datos completada"

[ ] 4. Si hay error, ver detalles en logs
      Buscar: "? Error al aplicar migraciones/seed"

[ ] 5. Verificar tablas en BD
      SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%Perm%'

[ ] 6. Verificar usuario admin en BD
      SELECT * FROM AspNetUsers WHERE UserName = 'admin'

[ ] 7. Verificar permisos en BD
      SELECT COUNT(*) FROM Permissions -- Debe ser 27

[ ] 8. Probar login admin
      POST /api/auth/admin/login
      { "userName": "admin", "password": "4dm1nC4rd10.*" }
```

---

## ?? Solución Rápida si NO funciona nada:

```csharp
// Agregar logging adicional en SeedDB.cs para debugging:

public async Task SeedAsync()
{
    Console.WriteLine("?? [SEED] Iniciando SeedAsync()");
    
    try
    {
        Console.WriteLine("?? [SEED] Ejecutando EnsureDocType()...");
        await EnsureDocType();
        
        Console.WriteLine("?? [SEED] Ejecutando EnsureGeneralSettings()...");
        await EnsureGeneralSettings();
        
        Console.WriteLine("?? [SEED] Ejecutando EnsureStates()...");
        await EnsureStates();
        
        Console.WriteLine("?? [SEED] Ejecutando EnsurePermissions()...");
        await EnsurePermissions();
        
        Console.WriteLine("?? [SEED] Ejecutando EnsureRolesWithPermissions()...");
        await EnsureRolesWithPermissions();
        
        Console.WriteLine("?? [SEED] Ejecutando EnsureAdmin()...");
        await EnsureAdmin();
        
        Console.WriteLine("? [SEED] SeedAsync() completado exitosamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? [SEED] Error en SeedAsync(): {ex.Message}");
        Console.WriteLine($"? [SEED] StackTrace: {ex.StackTrace}");
        throw;
    }
}
```

Con estos logs adicionales podrás ver exactamente dónde falla el seed.

---

**Estado:** El seeder está configurado correctamente, solo falta aplicar la migración.
**Causa más probable:** Falta ejecutar `Update-Database` para crear las tablas.
