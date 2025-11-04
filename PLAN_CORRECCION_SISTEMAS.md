# Plan de Corrección de Sistemas del Juego

## Diagnóstico MCP Completado

### ✅ Componentes Añadidos a la Escena:
- ScreamerSystem → GameManager
- BasementDoorDialogue → GameManager  
- WinConditionManager → GameManager
- HourlyBellSystem → Ya existía

### ❌ Problemas Detectados:

#### 1. Puerta del Sótano en Memoria
**Estado:** En `houseData.json` está correctamente configurada como `"abierta": false`, PERO Unity está cargando `"abierta": true` en la escena.

**Causa:** RoomGenerator3000 en GameManager tiene la puerta del sótano como abierta en sus datos en memoria.

**Solución:** Forzar recarga del JSON o modificar directamente la puerta en runtime.

#### 2. Comando "hora" No Funciona
**Estado:** El comando está en la lista de comandos válidos pero no se ejecuta.

**Posibles Causas:**
- El recognizer no está detectando la palabra
- CheckCurrentTime() no se está llamando
- GameTimer.Instance es null
- TTS está bloqueado

**Solución:** Añadir logs extensivos y verificar cada paso del flujo.

#### 3. Evento del Niño
**Estado:** BasementDoorDialogue está en la escena pero verifica `child_event_triggered` flag.

**Problema:** El flag se marca al tomar el oso, pero la lógica de BasementDoorDialogue verifica `hasBear && eventTriggered`, lo que significa que el jugador debe TENER el oso en el inventario cuando entra al sótano.

**Solución:** Ajustar la lógica para que funcione correctamente.

#### 4. Sistema de Consumibles
**Estado:** `EatFood()` y `ConsumeFirstConsumable()` existen pero no están completos.

**Problema:** No hay comando "beber" separado, y los consumibles pueden no estar aplicando correctamente la reducción de fatiga a FatigueSystem.

**Solución:** 
- Añadir comando "beber" (alias de "comer")
- Verificar que `FatigueSystem` recibe las reducciones
- Hacer consumibles más accesibles (visibles con "buscar")

---

## Orden de Implementación

### 1. Fix Puerta del Sótano (Crítico - Permite progreso)
- Opción A: Forzar recarga desde JSON
- Opción B: Modificar directamente usando MCP set_component_property

### 2. Fix Comando "hora" (Debug necesario)
- Añadir logs en BasicAIAssistant para ver si procesa el comando
- Verificar que GameTimer.Instance no es null
- Añadir fallback si TTS falla

### 3. Fix Consumibles (Gameplay importante)
- Añadir comando "beber" a BasicAIAssistant
- Verific ar ConsumeFirstConsumable() aplica cambios a FatigueSystem
- Revisar visibilidad de items con "buscar"

### 4. Fix ScreamerSystem (Ya en escena, solo activar)
- Verificar que se activa en TriggerBearEvent()
- Probar que se disparan correctamente

### 5. Fix Evento del Niño (Complejo)
- Ajustar lógica de activación en BasementDoorDialogue
- Añadir detección de "despertar" en GameContextProvider
- Probar flujo completo

---

## Archivos a Modificar

1. `Assets/houseData.json` - Verificar puerta sótano
2. `Assets/Scripts/VoiceSystem/AI/BasicAIAssistant.cs` - Añadir logs y comando "beber"
3. `Assets/Scripts/VoiceSystem/GameIntegration/GameContextProvider.cs` - Fix CheckCurrentTime(), añadir "beber", mejorar consumibles
4. `Assets/Scripts/VoiceSystem/GameIntegration/RoomInventoryManager.cs` - Verificar ConsumeItem()
5. `Assets/Scripts/FatigueSystem.cs` - Verificar que recibe reducciones de fatiga
6. `Assets/Scripts/BasementDoorDialogue.cs` - Ajustar lógica de activación
7. `Assets/Scripts/ScreamerSystem.cs` - Ya está bien, solo verificar activación

---

## Tests Después de Cada Fix

### Test Puerta Sótano:
1. Play
2. Ir a Hab. Niños → "abajo"
3. Debe decir: "La puerta está cerrada..."
4. Esperar 4 min (o acelerar con debug)
5. A las 2 AM debe desbloquearse

### Test Hora:
1. Play
2. "hora" → Debe narrar hora actual
3. Verificar en consola todos los logs

### Test Consumibles:
1. "buscar" en Comedor → Debe encontrar pan
2. "tomar pan"
3. "comer" → Debe restaurar salud/fatiga
4. Verificar en consola que FatigueSystem actualiza

### Test Screamers:
1. Tomar oso
2. Moverse entre habitaciones
3. ~30% chance de screamer
4. Verificar que reduce vida

### Test Evento Niño:
1. Esperar 2 AM
2. Tomar oso
3. Ir al sótano
4. Debe iniciar diálogo
5. Guardar silencio → Flor activa
6. "despertar" → Final malo

