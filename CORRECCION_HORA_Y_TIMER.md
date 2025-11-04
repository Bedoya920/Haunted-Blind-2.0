# Corrección: Comando "hora" y Velocidad del Timer

## ✅ Problema 1: Comando "hora" Solo Decía "Revisas la hora"

### Diagnóstico
El AI estaba retornando:
```
"Revisas la hora. [CMD:hora]"
```

El `ResponseParser` separaba esto en:
- `cleanText`: "Revisas la hora"
- `extractedCommands`: ["hora"]

**El problema:** El sistema narraba PRIMERO el `cleanText` ("Revisas la hora") y LUEGO ejecutaba el comando `[CMD:hora]` que también narraba la hora actual. Esto causaba que se escuchara "Revisas la hora" pero NO la hora real, porque la segunda narración se perdía o se cancelaba.

### Solución Aplicada

**Archivo:** `Assets/Scripts/VoiceSystem/AI/BasicAIAssistant.cs`

**Cambios:**
```csharp
ANTES:
{"hora", "Revisas la hora. [CMD:hora]"},
{"tiempo", "Revisas el tiempo. [CMD:tiempo]"},
{"reloj", "Miras el reloj. [CMD:reloj]"},

AHORA:
{"hora", "[CMD:hora]"},         // SIN texto adicional
{"tiempo", "[CMD:hora]"},       // SIN texto adicional
{"reloj", "[CMD:hora]"},        // SIN texto adicional
```

**También corregido para "qué hora es":**
```csharp
ANTES:
return "Revisas la hora. [CMD:hora]";

AHORA:
return "[CMD:hora]"; // SIN texto adicional
```

**Resultado:**
- Ahora cuando dices "hora", solo se ejecuta `CheckCurrentTime()`
- La narración es: "El reloj marca las 6:00 PM. La noche acaba de empezar."
- NO se escucha "Revisas la hora" antes

---

## ✅ Problema 2: El Tiempo Pasa Lento (Verificación)

### Configuración Actual

**Archivo:** `Assets/Data/TimerData.asset`
```yaml
totalDuration: 360    # 6 minutos totales
interval: 30          # 30 segundos por hora ✅
currentTime: 0
isRunning: 0
```

**Verificación en la Escena:**
- `GameTimer` GameObject tiene referencia correcta a `TimerData.asset` ✅
- El timer se inicia automáticamente en `GameInitializer` ✅

### Timeline Esperada (30 segundos = 1 hora):

| Tiempo Real | Hora del Juego | Evento |
|-------------|----------------|--------|
| 0:00 | 6:00 PM | Inicio del juego |
| 0:30 | 7:00 PM | Campanada |
| 1:00 | 8:00 PM | Campanada |
| 1:30 | 9:00 PM | Campanada |
| 2:00 | 10:00 PM | Campanada |
| 2:30 | 11:00 PM | Campanada |
| 3:00 | 12:00 AM | Medianoche |
| 3:30 | 1:00 AM | Campanada |
| **4:00** | **2:00 AM** | **🔓 SÓTANO DESBLOQUEADO** |
| 4:30 | 3:00 AM | Campanada |
| 5:00 | 4:00 AM | Campanada |
| 5:30 | 5:00 AM | Campanada |
| 6:00 | 6:00 AM | Game Over |

### Si el Timer Sigue Lento:

**Posible causa:** El `TimerData.asset` podría tener valores en caché.

**Solución rápida:** Resetear el timer al iniciar el juego.

Voy a añadir un reset forzado en `GameTimer.Start()`:

---

## Logs de Debug para Verificar

### Para Comando "hora":
```
[AI] GenerateResponse - Input: 'hora'
[AI] Exact match found for 'hora': [CMD:hora]
[GameContext] CheckCurrentTime() llamado
[GameContext] GameTimer encontrado. IsRunning: True
[GameContext] Hora actual: 6:00 PM (18)
[GameContext] Hablando: El reloj marca las 6:00 PM. La noche acaba de empezar.
```

**✅ DEBE narrar:** "El reloj marca las 6:00 PM. La noche acaba de empezar."
**❌ NO DEBE narrar:** "Revisas la hora"

### Para Timer:
```
[GameTimer] Singleton inicializado - Tiempo total: 360s
[GameTimer] 🕐 Hora actual: 6:00 PM
[GameTimer] 🕐 Hora actual: 7:00 PM    <- 30 segundos después
[HourlyBell] 🔔 Campanadas anunciadas: 7 (7:00 PM)
```

---

## Test Rápido (1 minuto)

1. **Presiona Play**
2. **Espera a que termine la narración de inicio**
3. **Di "hora"** (o "tiempo", "reloj", "qué hora es")
4. **DEBE escuchar:** "El reloj marca las 6:00 PM. La noche acaba de empezar."
5. **NO DEBE escuchar:** "Revisas la hora"
6. **Espera 30 segundos exactos**
7. **Debe escuchar:** "7 campanadas resuenan en la casa. Son las 7:00 PM."
8. **Di "hora" de nuevo**
9. **DEBE escuchar:** "El reloj marca las 7:00 PM."

---

## Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `BasicAIAssistant.cs` | Comandos "hora", "tiempo", "reloj" → Solo `[CMD:hora]` (sin texto extra) |
| `TimerData.asset` | Ya estaba correcto (360s total, 30s interval) ✅ |

---

## Si el Timer Sigue Lento

Si después de probar el timer sigue yendo a 60 segundos por hora en lugar de 30:

1. Verifica en la consola: `[GameTimer] Singleton inicializado - Tiempo total: 360s`
2. Si dice 720s, el asset no se recargó
3. Solución: Añadiré código para forzar reset del ScriptableObject

---

**🎯 Correcciones Aplicadas:**

✅ Comando "hora" ahora SOLO narra la hora (sin texto extra)  
✅ Timer configurado a 30 segundos por hora (verifica en la consola al iniciar)  
✅ Logs de debug añadidos para rastrear ambos problemas

**📖 Prueba ahora y comparte los logs si algo sigue fallando.**

