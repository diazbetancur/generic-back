# Portal Pacientes API

API en .NET8 organizada en arquitectura por capas y preparada para múltiples ambientes (Development, QA, Producción) con auditoría, health checks y soft delete.

## 🧱 Arquitectura por Capas

| Capa | Descripción |
|------|-------------|
| Api-Portar-Paciente | Endpoints, middleware, configuración de pipeline, feature flags y health checks. |
| CC.Domain | Entidades, contratos (interfaces), DTOs, perfiles AutoMapper. |
| CC.Aplication | Servicios de aplicación / casos de uso (orquestan repositorios y mapping). |
| CC.Infrastructure | EF Core (DbContext, migraciones, repositorios), interceptor de auditoría, acceso a datos. |

### Principios
- Separación de responsabilidades
- Inversión de dependencias vía DI
- Configuración por ambiente (appsettings.* + variables)
- Observabilidad (logs + health endpoints)

## ✅ Prerrequisitos
- .NET SDK8.0+
- SQL Server2019+ o Azure SQL
- PowerShell7+ (recomendado) o CMD
- (Opcional) Herramientas EF Core si usarás CLI: `dotnet tool install --global dotnet-ef`

## 📑 Auditoría (Resumen)
Auditoría habilitable vía `Auditing:Enabled` captura operaciones CREATE / UPDATE / DELETE antes de persistir.
Campos registrados: Usuario (placeholder), Entidad, Acción, Valores Anteriores, Valores Nuevos, Campos Cambiados, Fecha UTC, TraceId.
Formato y limitaciones detallados en `AUDIT_LOGS.md`.


## ❤️ Health Checks
Endpoints expuestos:
- `/health` (liveness)
- `/health/ready` (readiness agregada)
- `/health/application`
- `/health/configuration`
- `/health/external-services`
- (si UI habilitado) `/health-ui`

## 🌐 Configuración de Ambientes

La aplicación está configurada para funcionar en tres ambientes:

### 🔧 Ambientes Disponibles
1. **Development (dev)** - Desarrollo local
2. **QA (qa)** - Pruebas funcionales
3. **Production (pdn)** - Producción

### 📁 Archivos de Configuración
- `appsettings.json` (base)
- `appsettings.Development.json`
- `appsettings.qa.json`
- `appsettings.pdn.json`

### 🚀 Cómo Ejecutar en Diferentes Ambientes

#### Visual Studio / VS Code
Selecciona el perfil apropiado desde el menú de debug:
- Development (https)
- QA
- Production

#### Línea de Comandos (PowerShell)
Desarrollo:
```powershell
dotnet run --project .\Api-Portar-Paciente\Api-Portar-Paciente.csproj --environment Development
```
QA:
```powershell
dotnet run --project .\Api-Portar-Paciente\Api-Portar-Paciente.csproj --environment qa
```
Producción:
```powershell
dotnet run --project .\Api-Portar-Paciente\Api-Portar-Paciente.csproj --environment pdn
```

### 🌍 Variables de Ambiente
Puedes sobrescribir cualquier configuración usando variables de ambiente (en PowerShell):
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=mi-servidor;Database=mi-bd;..."; dotnet run --project .\Api-Portar-Paciente\Api-Portar-Paciente.csproj
```

### 📝 Carga de Variables
1. Copiar `.env.example` a `.env`
2. Ajustar valores
3. Verificar que no se versionen secrets

### 🔐 Configuraciones Recomendadas
| Ambiente | Swagger | Logging Nivel | JWT Expiration | Error Details |
|----------|---------|---------------|----------------|---------------|
| Development | On | Debug |8h | On |
| QA | On | Information |2h | On |
| Production | Off | Warning/Error |1h | Off |

### 🔒 Seguridad
⚠️ Importante:
- No commitear secrets
- Usar variables de ambiente / Key Vault
- Revisar expiración de tokens
- Rotar claves JWT antes de cada release mayor

### 📋 Checklist de Despliegue
- [ ] Verificar cadenas de conexión
- [ ] Actualizar claves JWT
- [ ] Configurar URLs de servicios externos
- [ ] Validar configuración de logging
- [ ] Probar health checks
- [ ] Confirmar Swagger deshabilitado en producción

## 🧩 Feature Flags y Mocks
Ejemplo de configuración (appsettings.*):
```json
{
 "Features": {
 "EnableSwagger": true,
 "EnableDetailedLogging": true,
 "EnableMetrics": false,
 "UseMockPatientService": true,
 "UseMockXeroService": true
 },
 "Mocks": {
 "Xero": {
 "ViewerUrlBase": "https://xero-mock-viewer.local/view?study=",
 "ExpiresMinutes":15,
 "StaticToken": null
 }
 }
}
```
- UseMockPatientService: usa `MockExternalPatientService` en lugar del servicio real.
- UseMockXeroService: usa `MockXeroViewerService` que devuelve datos mock y link de visor configurable.

## 🔗 Configuración de Servicios Externos
- Servicio de Pacientes (on-prem):
```json
{
 "ExternalsAPI": {
 "PatienteBaseUrl": "https://10.3.0.66:8596",
 "PatienteTimeoutSeconds":60,
 "AllowInvalidCerts": true,
 "ApiKey": null
 }
}
```
- Xero Viewer:
```json
{
 "ExternalServices": {
 "XeroViewer": {
 "BaseUrl": "http://10.3.0.79:6663",
 "TimeoutSeconds":30,
 "ApiKey": "<requerido en producción>",
 "AllowInvalidCerts": false
 }
 }
}
```
- SMS y Email (resumen): definir credenciales/tenant/secret según ambiente para `ExternalServices:Sms` y `ExternalServices:Email`.

## 🔍 Endpoints de prueba
- Swagger: `/swagger`
- Health: `/health`, `/health/ready`, `/health/application`, `/health/configuration`, `/health/external-services`, `/health-ui`
- Xero (testing):
 - `GET api/Xero/health`
 - `GET api/Xero/patients/{patientId}/studies`
 - `POST api/Xero/studies/{studyUid}/viewer-link`

## 🌱 Seed de Base de Datos
- Al iniciar la app, se ejecuta `SeedDB.SeedAsync()` automáticamente.
- Si ocurre un error de seed, se registra pero no detiene el arranque de la aplicación.

## 🛠️ Comandos Útiles
```powershell
# Construir Release
dotnet build -c Release

# Publicar (output en ./publish)
dotnet publish .\Api-Portar-Paciente\Api-Portar-Paciente.csproj -c Release -o .\publish

# Ver logs filtrando 'Portal'
dotnet run --project .\Api-Portar-Paciente\Api-Portar-Paciente.csproj --environment qa | Select-String "Portal"
```

## 🗃️ Migraciones EF Core

Notas:
- El `DbContext` se llama `DBContext` y vive en el proyecto `CC.Infrastructure`.
- El proyecto de inicio (startup) es `Api-Portar-Paciente`.
- Ejecuta los comandos desde la carpeta raíz de la solución.

### Opción1: .NET CLI (dotnet-ef)
Instalar herramientas EF (una vez):
```powershell
dotnet tool install --global dotnet-ef
```

Agregar migración (incluyendo el contexto explícito):
```powershell
dotnet ef migrations add <NombreMigracion> \
 -p .\CC.Infrastructure\CC.Infrastructure.csproj \
 -s .\Api-Portar-Paciente\Api-Portar-Paciente.csproj \
 --context DBContext
```

Actualizar base de datos (incluyendo el contexto explícito):
```powershell
dotnet ef database update \
 -p .\CC.Infrastructure\CC.Infrastructure.csproj \
 -s .\Api-Portar-Paciente\Api-Portar-Paciente.csproj \
 --context DBContext
```

Revertir última migración (sin aplicar a la base):
```powershell
dotnet ef migrations remove \
 -p .\CC.Infrastructure\CC.Infrastructure.csproj \
 -s .\Api-Portar-Paciente\Api-Portar-Paciente.csproj \
 --context DBContext
```

Apuntar a una migración específica:
```powershell
dotnet ef database update <NombreMigracion> \
 -p .\CC.Infrastructure\CC.Infrastructure.csproj \
 -s .\Api-Portar-Paciente\Api-Portar-Paciente.csproj \
 --context DBContext
```

### Opción2: Package Manager Console (Visual Studio)
Recomendado si `dotnet ef` presenta fallos de arranque del host.

Pasos previos:
- En Visual Studio, abrir `Tools > NuGet Package Manager > Package Manager Console`.
- Seleccionar `Default project`: `CC.Infrastructure`.
- Asegurar que el proyecto de inicio (Startup Project) sea `Api-Portar-Paciente`.

Comandos (use el nombre exacto del contexto: `DBContext`):
```powershell
# Agregar migración
Add-Migration <NombreMigracion> -Context DBContext 

# Actualizar base de datos
Update-Database -Context DBContext 

# Revertir última migración (no aplica cambios a la DB)
Remove-Migration -Context DBContext 

# Actualizar a una migración específica
Update-Database <NombreMigracion> -Context DBContext
```

Tips:
- Si usas PMC, no necesitas instalar `dotnet-ef`.
- Si recibes errores de inicio del host, valida `ConnectionStrings:DefaultConnection` en appsettings y el proyecto de inicio.
