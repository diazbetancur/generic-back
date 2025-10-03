# Auditoría de Cambios (AuditLogs)

La aplicación registra de forma automática operaciones CREATE, UPDATE y DELETE sobre las entidades persistidas vía Entity Framework Core utilizando un `SaveChangesInterceptor`.

## Activación / Desactivación

Configuración en `appsettings.{Environment}.json`:
```json
"Auditing": {
  "Enabled": true,
  "PlaceholderUserId": "DEV-USER"
}
```
Si `Enabled` es `false`, el interceptor no se agrega al `DbContext`.

## Columnas de la tabla `AuditLogs`
| Columna          | Descripción |
|------------------|-------------|
| Id               | Guid generado por SQL (`NEWID()`). |
| EntityName       | Nombre CLR de la entidad (ej. `FrecuentQuestions`). |
| EntityId         | Valor de la clave primaria (puede estar vacío en CREATE si el Id lo genera la BD y aún no se conoce). |
| Action           | `Create`, `Update` o `Delete`. |
| UserId / UserName| Usuario que provocó el cambio (placeholder hasta integrar autenticación). |
| OldValues        | JSON con valores antes del cambio (solo Update/Delete). |
| NewValues        | JSON con valores después del cambio (solo Create/Update). |
| ChangedColumns   | JSON (array) con nombres de columnas modificadas (solo Update). |
| TraceId          | Correlación distribuida si existe `Activity.Current`. |
| Environment      | Ambiente ASPNETCORE_ENVIRONMENT. |
| DateCreated      | Fecha UTC (`GETUTCDATE()`). |

## Ejemplo de registro (UPDATE)
```json
{
  "Id": "2f3ae9c5-...",
  "EntityName": "FrecuentQuestions",
  "EntityId": "4a7c...",
  "Action": "Update",
  "UserId": "DEV-USER",
  "OldValues": "{\"Question\":\"¿Antigua?\"}",
  "NewValues": "{\"Question\":\"¿Nueva?\"}",
  "ChangedColumns": "[\"Question\"]",
  "Environment": "Development",
  "DateCreated": "2025-10-03T21:15:00Z"
}
```

## Limitaciones actuales
- En `Create` el `EntityId` puede venir vacío si el Guid se genera en SQL. Opciones futuras:
  - Generar Guid en código antes de agregar.
  - Post-procesar después de `SaveChanges` (requiere rediseño).
- No se capturan IP, ruta HTTP, ni Claims reales (simplificación solicitada).

## Próximas mejoras sugeridas
1. Resolver usuario real desde `HttpContext` (`IHttpContextAccessor`).
2. Exponer endpoint de consulta filtrada por entidad / fecha.
3. Exportación programada a almacenamiento de largo plazo.
4. Encriptar campos sensibles (si hubiera PII futura).

## Cómo probar rápidamente
1. Crear una pregunta frecuente (POST /api/FrecuentQuestions).
2. Modificarla (PUT...).
3. Consultar la tabla `AuditLogs` en la base.

## Estructura del Interceptor (resumen)
- Recorre entradas del ChangeTracker.
- Ignora entidades `AuditLog`.
- Serializa diferencias solo cuando existen cambios reales.

Fin.
