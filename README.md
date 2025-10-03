# Portal Pacientes API

API en .NET 8 organizada en arquitectura por capas y preparada para múltiples ambientes (Development, QA, Producción) con auditoría, health checks y soft delete.

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

## 📑 Auditoría (Resumen)
Auditoría habilitable vía `Auditing:Enabled` captura operaciones CREATE / UPDATE / DELETE antes de persistir.
Campos registrados: Usuario (placeholder), Entidad, Acción, Valores Anteriores, Valores Nuevos, Campos Cambiados, Fecha UTC, TraceId.
Formato y limitaciones detallados en `AUDIT_LOGS.md`.

## 🗑️ Soft Delete
Aplicado a `FrecuentQuestions` mediante propiedad `IsDeleted` + filtro global en `DbContext`.
`DeleteAsync` del repositorio genérico detecta la propiedad y marca `IsDeleted = true` (no elimina físicamente).

## ❤️ Health Checks
Endpoints expuestos:
- `/health` (liveness)
- `/health/ready` (readiness agregada)
- `/health/application`
- `/health/configuration`
- `/health/external-services`
HealthChecks UI temporalmente comentada hasta definir almacenamiento.

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
| Development | On | Debug | 8h | On |
| QA | On | Information | 2h | On |
| Production | Off | Warning/Error | 1h | Off |

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
```powershell
# Crear migración
dotnet ef migrations add <Nombre> -p .\CC.Infrastructure\CC.Infrastructure.csproj -s .\Api-Portar-Paciente\Api-Portar-Paciente.csproj

# Aplicar migraciones
dotnet ef database update -p .\CC.Infrastructure\CC.Infrastructure.csproj -s .\Api-Portar-Paciente\Api-Portar-Paciente.csproj
```

## 🔮 Backlog (Extracto Próximo)
1. FluentValidation para DTOs
2. Integrar UserId real (Claims) en auditoría
3. Versionado de API
4. Rate limiting
5. Cache + Polly (resiliencia HTTP / DB intermitente)
6. Observabilidad avanzada (OpenTelemetry métricas + tracing distribuido)
7. CI/CD + análisis estático (Sonar / SAST)
8. Seed inicial (roles, usuario admin, FAQs demo)

## 🛡️ Notas de Seguridad Futuras
- Centralizar configuración sensible en Azure Key Vault
- Revisar headers de seguridad (CSP, HSTS, X-Content-Type-Options)
- Implementar bloqueo de cuenta por intentos fallidos
- Sanitizar payloads en logs (evitar PII)

---
Fin.