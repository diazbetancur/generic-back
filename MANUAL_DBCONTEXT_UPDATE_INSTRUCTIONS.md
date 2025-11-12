# Instrucciones para Actualizar DBContext.cs

## Paso 1: Agregar DbSets
Buscar en `CC.Infrastructure\Configurations\DBContext.cs` la línea:
```csharp
public DbSet<HistoryRequest> HistoryRequests { get; set; }
```

Inmediatamente DESPUÉS de esa línea, agregar:

```csharp
/// <summary>
/// Permisos del sistema
/// </summary>
public DbSet<Permission> Permissions { get; set; }

/// <summary>
/// Relación Roles-Permisos
/// </summary>
public DbSet<RolePermission> RolePermissions { get; set; }
```

---

## Paso 2: Agregar Configuración de Entidades
Buscar en el método `OnModelCreating` la sección donde está:
```csharp
builder.Entity<HistoryRequest>().HasIndex(h => h.DateCreated);

DisableCascadingDelete(builder);
```

ANTES de la línea `DisableCascadingDelete(builder);`, agregar:

```csharp
// Permission Configuration
builder.Entity<Permission>().HasKey(c => c.Id);
builder.Entity<Permission>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
builder.Entity<Permission>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
builder.Entity<Permission>().HasIndex(p => p.Name).IsUnique();
builder.Entity<Permission>().HasIndex(p => p.Module);
builder.Entity<Permission>().Property(p => p.Name).HasMaxLength(100).IsRequired();
builder.Entity<Permission>().Property(p => p.Module).HasMaxLength(50).IsRequired();
builder.Entity<Permission>().Property(p => p.Description).HasMaxLength(250);

// RolePermission Configuration
builder.Entity<RolePermission>().HasKey(c => c.Id);
builder.Entity<RolePermission>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
builder.Entity<RolePermission>().Property(e => e.DateCreated).HasDefaultValueSql("GETUTCDATE()");
builder.Entity<RolePermission>().HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

// Relaciones RolePermission
builder.Entity<RolePermission>()
    .HasOne(rp => rp.Role)
    .WithMany()
    .HasForeignKey(rp => rp.RoleId)
    .OnDelete(DeleteBehavior.Cascade);

builder.Entity<RolePermission>()
    .HasOne(rp => rp.Permission)
    .WithMany(p => p.RolePermissions)
    .HasForeignKey(rp => rp.PermissionId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

## ? Verificación
Después de hacer los cambios, el DBContext debe tener:
1. ? `public DbSet<Permission> Permissions { get; set; }`
2. ? `public DbSet<RolePermission> RolePermissions { get; set; }`
3. ? Configuración completa de ambas entidades en `OnModelCreating`

## ?? Siguiente Paso
Una vez actualizad el DBContext, ejecutar:
```bash
Add-Migration AddPermissionsAndRolePermissions -Context DBContext
Update-Database -Context DBContext
```
