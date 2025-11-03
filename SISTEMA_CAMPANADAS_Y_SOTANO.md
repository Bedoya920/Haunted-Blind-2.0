# 🕐 Sistema de Campanadas y Puerta del Sótano

## ✅ Implementación Completada

### 🔔 Sistema de Campanadas Horarias

**Funcionalidad:**
- Cada hora del juego (cada 1/12 del tiempo total) suena una campanada
- El juego va de **6:00 PM** a **6:00 AM** (12 horas)
- El sistema anuncia con voz: "X campanadas resuenan en la casa. Son las X:00 AM/PM."

**Archivos modificados:**
- `Assets/Scripts/GameTimer.cs` - Añadido:
  - `OnHourPassed` event
  - `GetCurrentHour()` - Devuelve hora actual (18-23, 0-5)
  - `GetCurrentTimeString()` - Devuelve "7:00 PM", "2:00 AM", etc.
  - `IsItTwoAM()` - Verifica si es las 2 AM (hora del evento del niño)

- `Assets/Scripts/HourlyBellSystem.cs` - **NUEVO**
  - Escucha el evento `OnHourPassed` del GameTimer
  - Anuncia las campanadas con TTS
  - Desbloquea la puerta del sótano a las 2 AM

---

### 🚪 Puerta del Sótano Bloqueada

**Funcionalidad:**
- La puerta al sótano (entre Hab. Niños y Sótano) está **bloqueada** al inicio
- Mensaje de bloqueo: *"La puerta está cerrada con llave. Algo te dice que debes esperar a que el reloj marque las 2 AM."*
- **A las 2 AM**, el sistema desbloquea automáticamente la puerta
- Se narra: *"Escuchas un clic en algún lugar de la casa. Una puerta se ha desbloqueado."*

**Archivos modificados:**
- `Assets/_Game/Scripts/RoomGenerator3000.cs`:
  - `CrearPuerta()` ahora acepta parámetros `bloqueada` y `mensajeBloqueada`
  - Puerta del sótano (ID:8) creada con `bloqueada=true`

---

### ⏰ Comando de Voz "Hora"

**Comandos disponibles:**
- `"hora"`
- `"tiempo"`
- `"reloj"`
- `"qué hora es"`

**Respuesta:**
```
"El reloj marca las 8:00 PM. La noche acaba de empezar."
```

**Mensajes especiales:**
- **2:00 AM**: "Una sensación extraña te recorre. Es la hora en que los muertos hablan."
- **0:00 AM - 6:00 AM**: "La madrugada avanza. El amanecer se acerca."
- **6:00 PM - 11:59 PM**: "La noche acaba de empezar."

**Archivos modificados:**
- `Assets/Scripts/VoiceSystem/GameIntegration/GameContextProvider.cs`:
  - Añadidos casos `"hora"`, `"tiempo"`, `"reloj"`
  - Método `CheckCurrentTime()` que narra la hora actual
  
- `Assets/Scripts/VoiceSystem/GameIntegration/CommandValidator.cs`:
  - Comandos de tiempo añadidos a lista de comandos siempre válidos
  
- `Assets/Scripts/VoiceSystem/AI/BasicAIAssistant.cs`:
  - Patrones de reconocimiento para comandos de tiempo

---

## 📋 Flujo del Juego Completo (GDD)

### 🎮 Secuencia de Eventos

```
1. [6:00 PM] - Inicio del juego
   └─ Campanada: "6 campanadas resuenan en la casa. Son las 6:00 PM."

2. [7:00 PM] - Primera hora
   └─ Campanada: "7 campanadas resuenan en la casa. Son las 7:00 PM."

3. ... (cada hora suena campanada)

4. [2:00 AM] - ¡HORA CLAVE!
   ├─ Campanada: "Dos campanadas resuenan en la casa. Son las 2:00 AM. 
   │              El reloj del hall marca eternamente esta hora."
   ├─ Desbloqueo automático de la puerta del sótano
   └─ Narración: "Escuchas un clic en algún lugar de la casa. 
                  Una puerta se ha desbloqueado."

5. [Sótano] - Evento del Niño
   ├─ Diálogo del niño: "¿Has traído mi oso?"
   │  
   ├─ SI TIENE EL OSO:
   │  ├─ Niño: "Lo encontraste... Ahora puedes irte."
   │  ├─ Se activa la flor de loto (marchita → viva)
   │  └─ Continúa con misión de victoria
   │  
   └─ SI NO TIENE EL OSO:
      └─ Niño: "Vuelve cuando lo tengas..."

6. [Victoria] - Condición de victoria
   ├─ Tomar la flor de loto viva (Habitación Principal)
   ├─ Ir a la Sala Principal (retrato familiar)
   ├─ Comando: "dar flor"
   ├─ Comando: "renacer"
   └─ Final bueno: "La flor brilla... Has logrado renacer"

7. [6:00 AM] - Fin del tiempo
   └─ Game Over: "El amanecer llega. No lograste escapar."
```

---

## 🔧 IMPORTANTE: Regenerar houseData.json

**ANTES de ejecutar el juego**, debes regenerar el mapa para que incluya la puerta del sótano bloqueada:

### Opción 1: Menú de Unity (Editor)
1. Abre Unity Editor
2. Ve a **Tools → Regenerar Mapa Completo**
3. Verás el mensaje: "Mapa regenerado con narraciones del GDD y guardado en houseData.json"

### Opción 2: Manual (Context Menu)
1. Selecciona el GameObject con `RoomGenerator3000` en la escena
2. Click derecho en el componente
3. Elige **Generar Mapa de Campaña**
4. Luego elige **Guardar Casa JSON**

---

## 🎯 Nuevos Archivos Creados

| Archivo | Propósito |
|---------|-----------|
| `HourlyBellSystem.cs` | Sistema de campanadas y desbloqueo del sótano |
| `BasementDoorDialogue.cs` | Diálogo del niño en el sótano |
| `ScreamerSystem.cs` | Sistema de jump scares tras tomar el oso |
| `LotusFlowerTransformation.cs` | Transforma flor marchita → viva |
| `WinConditionManager.cs` | Gestiona condición de victoria |
| `ForceResumeRecognition.cs` | Previene bloqueos del reconocimiento de voz |
| `RegenerarMapaEditor.cs` | Editor script para regenerar mapa |

---

## 🧪 Pruebas Recomendadas

### Test 1: Campanadas
```
1. Inicia el juego
2. Di "hora" → Debe responder "6:00 PM"
3. Espera ~1/12 del tiempo total del juego
4. Debe sonar: "7 campanadas resuenan en la casa. Son las 7:00 PM."
```

### Test 2: Puerta del Sótano Bloqueada
```
1. Ve a Habitación de los Niños
2. Intenta ir "abajo" al sótano
3. Debe decir: "La puerta está cerrada con llave. 
                Algo te dice que debes esperar a que el reloj marque las 2 AM."
4. Espera hasta las 2 AM (escucha campanadas)
5. Debe narrar: "Escuchas un clic en algún lugar de la casa. 
                 Una puerta se ha desbloqueado."
6. Ahora "abajo" debe funcionar
```

### Test 3: Evento del Niño (2 AM)
```
REQUISITO: Tener el oso de peluche en inventario

1. Espera a las 2 AM
2. Ve al sótano (ahora desbloqueado)
3. Debe activarse el diálogo del niño
4. Si tienes el oso: activa la flor de loto
5. Si no: te pide que vuelvas
```

### Test 4: Flujo Completo de Victoria
```
1. Toma el oso de peluche (Hab. Niños)
2. Espera a las 2 AM (campanadas + desbloqueo)
3. Ve al sótano
4. Completa diálogo del niño
5. Ve a Habitación Principal
6. Toma la flor de loto (ahora viva)
7. Ve a la Sala Principal (retrato)
8. Di "dar flor"
9. Di "renacer"
10. ¡VICTORIA!
```

---

## 🐛 Solución de Problemas

### Problema: La puerta del sótano no se desbloquea
**Solución:**
- Verifica que `HourlyBellSystem` esté activo en la escena
- Revisa los logs: `[HourlyBell] 🔓 Son las 2 AM - Desbloqueando puerta del sótano`

### Problema: No suenan las campanadas
**Solución:**
- Verifica que `GameTimer.OnHourPassed` esté suscrito
- Revisa los logs: `[GameTimer] 🕐 Hora actual: X:00 AM/PM`

### Problema: El comando "hora" no funciona
**Solución:**
- Verifica que `BasicAIAssistant` tenga los patrones de tiempo
- Revisa que `CommandValidator` incluya "hora"/"tiempo"/"reloj"

---

## 📊 Resumen de Cambios

| Sistema | Estado | Archivos |
|---------|--------|----------|
| ✅ Campanadas horarias | Implementado | GameTimer.cs, HourlyBellSystem.cs |
| ✅ Puerta bloqueada | Implementado | RoomGenerator3000.cs, HourlyBellSystem.cs |
| ✅ Comando "hora" | Implementado | GameContextProvider.cs, CommandValidator.cs, BasicAIAssistant.cs |
| ✅ Desbloqueo 2 AM | Implementado | HourlyBellSystem.cs |
| ✅ Evento del niño | Implementado | BasementDoorDialogue.cs |
| ✅ Flor de loto | Implementado | LotusFlowerTransformation.cs |
| ✅ Condición de victoria | Implementado | WinConditionManager.cs |

---

## 🎮 Comandos de Voz Nuevos

| Comando | Acción |
|---------|--------|
| `hora` | Revisar la hora del juego |
| `tiempo` | Revisar la hora del juego |
| `reloj` | Revisar la hora del juego |
| `qué hora es` | Revisar la hora del juego |
| `renacer` | Intentar final bueno (con flor) |
| `despertar` | Activar final malo (diálogo del niño) |

---

## ✅ TODO List

- [x] Sistema de campanadas implementado
- [x] Comando "hora" funcional
- [x] Puerta del sótano bloqueada hasta 2 AM
- [x] Desbloqueo automático a las 2 AM
- [ ] **Regenerar houseData.json** (HAZLO AHORA)
- [ ] Probar flujo completo en Unity

---

**¡El sistema está completo! Solo falta regenerar el JSON y probar el juego.**

