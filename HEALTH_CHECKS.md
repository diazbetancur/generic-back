# Health Checks - Portal Pacientes API

## 📊 Endpoints de Health Checks

### Endpoints Estándar (ASP.NET Core Health Checks)

| Endpoint | Descripción | Ambiente |
|----------|-------------|----------|
| `/health` | Estado completo de todos los health checks | Todos |
| `/health/application` | Estado específico de la aplicación | Todos |
| `/health/configuration` | Estado de las configuraciones | Todos |
| `/health/external-services` | Estado de servicios externos | Todos |
| `/health/ready` | Endpoint simple para load balancers | Todos |

### Endpoints Personalizados (API Controller)

| Endpoint | Descripción | Ambiente |
|----------|-------------|----------|
| `/api/health` | Estado detallado con información completa | Todos |
| `/api/health/simple` | Estado simple para monitoreo | Todos |
| `/api/health/info` | Información del ambiente y configuración | Todos |
| `/api/health/ping` | Verificación básica de conectividad | Todos |

### UI de Health Checks

| Endpoint | Descripción | Ambiente |
|----------|-------------|----------|
| `/health-ui` | Interfaz web para monitorear health checks | Development, QA |
| `/health-api` | API para la UI de health checks | Development, QA |

## 🏥 Tipos de Health Checks Implementados

### 1. Application Health Check
- **Descripción**: Verifica el estado general de la aplicación
- **Incluye**:
  - Información del ambiente
  - Nombre de la aplicación
  - Información del sistema (OS, procesador, memoria)
  - Features habilitadas
  - Tiempo de ejecución

### 2. Configuration Health Check
- **Descripción**: Verifica que las configuraciones críticas estén presentes
- **Verifica**:
  - Cadena de conexión a la base de datos
  - Configuración JWT (secret, issuer, audience)
  - URLs de servicios externos
  - API Keys de servicios externos

### 3. External Service Health Check
- **Descripción**: Verifica conectividad con servicios externos
- **Servicios verificados**:
  - Email Service
  - Notification Service
- **Métodos**:
  - Ping para servicios localhost
  - HTTP request para servicios externos

## ⚙️ Configuración por Ambiente

### Development
```json
{
  "HealthChecks": {
    "Enabled": true,
    "UIEnabled": true,
    "EvaluationTimeInSeconds": 15,
    "Services": {
      "EmailService": { "Enabled": false },
      "NotificationService": { "Enabled": false }
    }
  }
}
```

### QA
```json
{
  "HealthChecks": {
    "Enabled": true,
    "UIEnabled": true,
    "EvaluationTimeInSeconds": 30,
    "Services": {
      "EmailService": { "Enabled": true },
      "NotificationService": { "Enabled": true }
    }
  }
}
```

### Production
```json
{
  "HealthChecks": {
    "Enabled": true,
    "UIEnabled": false,
    "EvaluationTimeInSeconds": 60,
    "Services": {
      "EmailService": { "Enabled": true },
      "NotificationService": { "Enabled": true }
    }
  }
}
```

## 🚨 Estados de Health Check

| Estado | Código HTTP | Descripción |
|--------|-------------|-------------|
| **Healthy** | 200 | Todo funciona correctamente |
| **Degraded** | 200 | Funciona pero con problemas menores |
| **Unhealthy** | 503 | Problemas críticos detectados |

## 🔧 Uso con Load Balancers

Para configurar load balancers, usa el endpoint `/health/ready`:

```bash
# Ejemplo de configuración Nginx
upstream backend {
    server app1:5000;
    server app2:5000;
}

location / {
    proxy_pass http://backend;
    health_check uri=/health/ready;
}
```

## 📈 Monitoreo y Alertas

### Endpoints Recomendados para Monitoreo

1. **Monitoreo básico**: `/api/health/simple`
2. **Monitoreo detallado**: `/api/health`
3. **Load balancer**: `/health/ready`
4. **Ping básico**: `/api/health/ping`

### Ejemplos de Uso

```bash
# Verificar estado básico
curl https://api.cardioinfantil.com/api/health/ping

# Verificar estado completo
curl https://api.cardioinfantil.com/api/health

# Verificar solo configuraciones
curl https://api.cardioinfantil.com/health/configuration

# Verificar servicios externos
curl https://api.cardioinfantil.com/health/external-services
```

### Respuesta de Ejemplo

```json
{
  "status": "Healthy",
  "environment": "Production",
  "timestamp": "2025-09-30T15:30:00Z",
  "totalDuration": "00:00:00.1234567",
  "checks": [
    {
      "name": "application",
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "description": "Aplicación ejecutándose correctamente en ambiente Production",
      "tags": ["application"]
    }
  ]
}
```

## 🛠️ Troubleshooting

### Problemas Comunes

1. **Configuration Health Check falla**
   - Verificar variables de ambiente
   - Revisar archivos appsettings.*.json
   - Validar que las configuraciones críticas estén presentes

2. **External Service Health Check falla**
   - Verificar conectividad de red
   - Validar URLs de servicios externos
   - Revisar API Keys y autenticación

3. **UI de Health Checks no aparece**
   - Verificar que esté habilitada en el ambiente
   - Solo disponible en Development y QA
   - Acceder a `/health-ui`

### Logs de Health Checks

Los health checks generan logs automáticamente. Buscar en logs:
- `HealthCheck` category
- Errores de conectividad
- Timeouts de servicios externos