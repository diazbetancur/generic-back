# ?? LOG CLEANUP AUTOMÁTICO - IMPLEMENTACIÓN COMPLETA

## ? **IMPLEMENTADO EXITOSAMENTE**

### **?? Funcionalidad**
La aplicación ahora limpia automáticamente sus propios logs antiguos sin necesidad de scripts externos o Task Scheduler.

---

## ?? **COMPONENTES AGREGADOS**

### **1. LogCleanupService.cs**
```
Ubicación: Api-Portar-Paciente/Services/LogCleanupService.cs
Tipo: BackgroundService (Hosted Service)
Estado: ? Activo
```

**Características:**
- ? Se ejecuta automáticamente al iniciar la aplicación
- ? Limpieza programada cada 24 horas
- ? Configurable desde appsettings.json
- ? Logging de todas las operaciones
- ? Manejo de errores robusto
- ? No interrumpe la aplicación si falla

---

## ?? **CONFIGURACIÓN**

### **appsettings.json (Producción)**
```json
{
  "Logging": {
    "LogPath": "logs",
    "RetentionDays": 30,
    "CleanupHour": 3
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": null  // ? Serilog no limita, nosotros sí
        }
      }
    ]
  }
}
```

### **appsettings.Development.json**
```json
{
  "Logging": {
    "RetentionDays": 7,  // ? 7 días en dev
    "CleanupHour": 3
  }
}
```

---

## ?? **PARÁMETROS CONFIGURABLES**

| Parámetro | Default | Descripción |
|-----------|---------|-------------|
| `Logging:LogPath` | `"logs"` | Ruta de la carpeta de logs |
| `Logging:RetentionDays` | `30` | Días de retención de logs |
| `Logging:CleanupHour` | `3` | Hora del día para limpiar (24h) |

---

## ?? **CÓMO FUNCIONA**

### **Flujo de Ejecución**

```
1. Aplicación inicia
   ?
2. LogCleanupService se registra como Hosted Service
   ?
3. Calcula próxima hora de limpieza (ej: 3:00 AM)
   ?
4. Espera hasta esa hora
   ?
5. Ejecuta limpieza:
   - Busca archivos log-*.txt
   - Identifica logs más antiguos que RetentionDays
   - Elimina archivos antiguos
   - Loggea resultado
   ?
6. Espera 24 horas
   ?
7. Vuelve al paso 5 (loop infinito)
```

### **Logs Generados**

```
[09:00:00 INF] LogCleanupService configurado: RetentionDays=30, LogPath=logs, CleanupHour=3
[09:00:00 INF] LogCleanupService iniciado
[09:00:00 INF] Próxima limpieza de logs programada para: 2025-01-20 03:00:00

// ... a las 3:00 AM ...

[03:00:00 INF] Limpieza de logs completada: 5 archivos eliminados, 2548765 bytes liberados
[03:00:00 INF] Estado del directorio de logs: 30 archivos, 8734521 bytes totales
```

---

## ?? **EJEMPLO DE USO**

### **Escenario 1: Producción (30 días)**
```json
{
  "Logging": {
    "RetentionDays": 30,
    "CleanupHour": 3
  }
}
```

**Resultado:**
- Logs de hace 31+ días ? ? Eliminados
- Logs de hace 1-30 días ? ? Conservados
- Limpieza a las 3:00 AM cada día

### **Escenario 2: Development (7 días)**
```json
{
  "Logging": {
    "RetentionDays": 7,
    "CleanupHour": 3
  }
}
```

**Resultado:**
- Logs de hace 8+ días ? ? Eliminados
- Logs de hace 1-7 días ? ? Conservados
- Útil para mantener espacio en dev

### **Escenario 3: QA (14 días)**
```json
{
  "Logging": {
    "RetentionDays": 14,
    "CleanupHour": 2
  }
}
```

---

## ?? **CONSIDERACIONES IMPORTANTES**

### **1. Archivos NO Eliminados**
```
? Solo elimina: log-*.txt (archivos de Serilog)
? NO elimina:
   - Otros archivos en la carpeta logs/
   - Subcarpetas
   - Archivos sin patrón log-*.txt
```

### **2. Permisos Necesarios**
```
?? El Application Pool de IIS debe tener permisos de:
   - READ en carpeta logs/
   - WRITE en carpeta logs/
   - DELETE en archivos log-*.txt
```

### **3. Si la Limpieza Falla**
```csharp
// El servicio NO detiene la aplicación
// Solo loggea el error y reintenta en 1 hora

[03:00:00 ERR] Error en LogCleanupService, reintentando en 1 hora
```

### **4. Múltiples Instancias**
```
?? Si tienes múltiples servidores (load balanced):
   - Cada servidor limpia sus propios logs
   - No hay coordinación entre servidores
   - Esto es correcto (logs locales por servidor)
```

---

## ?? **MONITOREO**

### **Logs a Observar**

```bash
# Verificar que el servicio está activo
grep "LogCleanupService configurado" logs/log-*.txt

# Ver cuántos archivos se eliminan
grep "Limpieza de logs completada" logs/log-*.txt

# Detectar errores
grep "Error en LogCleanupService" logs/log-*.txt

# Ver próxima limpieza programada
grep "Próxima limpieza de logs programada" logs/log-*.txt
```

### **Métricas Recomendadas**
```
- Archivos eliminados por ejecución
- Bytes liberados por ejecución
- Errores en limpieza
- Archivos restantes después de limpieza
- Tamaño total del directorio logs/
```

---

## ?? **TESTING**

### **Prueba Manual 1: Simular Limpieza Inmediata**
```json
// Temporalmente en appsettings.Development.json
{
  "Logging": {
    "RetentionDays": 0,  // ? Elimina todos los logs
    "CleanupHour": 14    // ? Hora actual (ej: 2 PM)
  }
}
```

**Pasos:**
1. Detener aplicación
2. Cambiar configuración arriba
3. Iniciar aplicación
4. Esperar 1 minuto hasta la hora configurada
5. Verificar logs: "Limpieza de logs completada"

### **Prueba Manual 2: Crear Logs Antiguos**
```powershell
# PowerShell - Crear logs antiguos para testing
cd logs/
New-Item -Name "log-20240101.txt" -ItemType File -Value "Test old log"
(Get-Item "log-20240101.txt").LastWriteTime = "2024-01-01"

# Reiniciar app y verificar que se elimina en la próxima limpieza
```

---

## ?? **VENTAJAS vs SCRIPT EXTERNO**

| Característica | Script PowerShell | LogCleanupService |
|----------------|-------------------|-------------------|
| **Portabilidad** | ? Solo Windows | ? Cross-platform |
| **Configuración** | ? Task Scheduler manual | ? appsettings.json |
| **Logging** | ?? Manual (Event Viewer) | ? Integrado en Serilog |
| **Mantenimiento** | ?? Archivo separado | ? Parte de la app |
| **Deployment** | ?? Deploy separado | ? Deploy automático |
| **Monitoreo** | ?? Separado | ? Con la app |
| **Errores** | ? Silent fail | ? Logged & retry |
| **Por Ambiente** | ? Mismo script | ? Config por env |

---

## ?? **CASOS DE USO ADICIONALES**

### **Caso 1: Cambiar Hora de Limpieza por Ambiente**

```json
// appsettings.Production.json
{
  "Logging": {
    "CleanupHour": 3  // 3 AM (bajo tráfico)
  }
}

// appsettings.Development.json
{
  "Logging": {
    "CleanupHour": 23  // 11 PM (fuera de horas)
  }
}
```

### **Caso 2: Retención Más Larga para Auditorías**

```json
// Si necesitas guardar logs por compliance
{
  "Logging": {
    "RetentionDays": 90  // 3 meses
  }
}
```

### **Caso 3: Desactivar Limpieza Temporalmente**

```json
// Retención muy alta = prácticamente no elimina nada
{
  "Logging": {
    "RetentionDays": 9999
  }
}
```

---

## ?? **MANTENIMIENTO**

### **Actualizar Retención**
```json
// 1. Editar appsettings.Production.json
{
  "Logging": {
    "RetentionDays": 45  // Nueva retención
  }
}

// 2. Reiniciar aplicación (o esperar reload automático)
// 3. Verificar en logs próxima limpieza
```

### **Forzar Limpieza Inmediata**
```csharp
// Opción 1: Cambiar CleanupHour a hora actual + 1 minuto
// Opción 2: Reiniciar app (recalcula próxima limpieza)
// Opción 3: Agregar endpoint admin (futuro):

[HttpPost("admin/cleanup-logs")]
[Authorize(Roles = "Admin")]
public IActionResult ForceCleanup()
{
    // Trigger manual cleanup
}
```

---

## ? **RESUMEN**

### **Estado Final**
```
? LogCleanupService implementado
? Configuración en appsettings.json
? Registro en Program.cs
? Build exitoso
? Logs informativos
? Manejo de errores
? Cross-platform
? Production ready
```

### **Beneficios**
- ?? **Limpieza automática** - Sin intervención manual
- ?? **Monitoreable** - Todo en los logs
- ?? **Configurable** - Por ambiente
- ?? **Portable** - Parte de la aplicación
- ?? **Seguro** - No detiene la app si falla
- ?? **Cross-platform** - Funciona en Windows/Linux

### **Recomendaciones**
1. ? Configurar `RetentionDays` según compliance
2. ? Monitorear logs de limpieza semanalmente
3. ? Ajustar `CleanupHour` para horario de bajo tráfico
4. ? En on-premise, validar permisos de carpeta
5. ? Considerar archiving antes de eliminar (opcional)

---

## ?? **CONCLUSIÓN**

**La aplicación ahora gestiona sus propios logs de forma autónoma y eficiente.**

No necesitas:
- ? Scripts externos
- ? Task Scheduler
- ? Cron jobs
- ? Mantenimiento manual

**Todo está integrado y funcionando automáticamente.** ??

---

**Fecha de Implementación:** $(Get-Date -Format "yyyy-MM-dd")  
**Estado:** ? **PRODUCTION READY**  
**Build:** ? **EXITOSO**
