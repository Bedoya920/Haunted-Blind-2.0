# Corrección: Narraciones que se Activaban Antes de las 2 AM

## Problema Reportado

Las descripciones atmosféricas/post-corrupción de las habitaciones se estaban activando ANTES del evento del niño (antes de las 2 AM / antes de tomar el oso).

## Diagnóstico

### Sistema de Eventos de Habitación

El sistema tiene 3 tipos de eventos de entrada a habitaciones:

1. **`firstEntry`** - Primera vez que entras a una habitación
   - Narraciones largas e inmersivas
   - Solo se disparan UNA vez
   - Ejemplo: "La puerta de la Biblioteca se abre con un lamento seco..."

2. **`reentry`** - Cada vez que RE-entras a una habitación visitada
   - **Subtipo A:** "Narraciones Cortas" (sin requisitos)
   - **Subtipo B:** "Tras Evento del Niño" (requiere flag `child_dialogue_complete`)

3. **Action-based** - Eventos de inspección, lectura, etc.

### Problema Identificado

Las **"Narraciones Cortas"** (`reentry`, sin `requiredFlag`) incluyen descripciones que suenan como post-corrupción:

**Ejemplos:**
- **Sala:** "Las flores se manchan de rojo, el oso desapareció, el niño apunta en tu dirección" 🔴
- **Comedor:** "Las sillas se giran hacia ti, te observan; el mantel tiembla" 🔴
- **Cocina:** "Ollas chirrían con comida que destila putrefacción" 🔴
- **Baño:** "El espejo no refleja tu figura; solo una sombra que parpadea" 🔴
- **Hab. Principal:** "El aire conserva aroma del loto. Sombras donde no hay nadie" 🔴

Estas narraciones son **TOO INTENSE** para la fase inicial del juego y solo deberían aparecer DESPUÉS del evento del niño.

---

## Solución Aplicada

### Opción 1: Bloqueo en `StoryEventTrigger.cs` (Implementada)

Modificado el método `HandleRoomChanged()` para que **NO dispare "Narraciones Cortas" de reentry** hasta que el jugador haya tomado el oso (`child_event_triggered` flag).

**Archivo:** `Assets/Scripts/VoiceSystem/GameIntegration/StoryEventTrigger.cs`

**Código:**
```csharp
// En el bloque de reentry (líneas 125-146):
var storyEvent = eventManager.GetStoryEventByTrigger(roomId, "reentry");
if (storyEvent != null && CheckConditions(storyEvent))
{
    // NUEVO: Solo disparar eventos de reentry DESPUÉS de tomar el oso
    var playerState = PlayerStateManager.Instance;
    bool shouldNarrate = true;
    
    // Bloquear "Narraciones Cortas" antes del evento del niño
    if (playerState != null && !playerState.HasSeenEvent("child_event_triggered"))
    {
        if (storyEvent.eventName.Contains("Narración Corta"))
        {
            shouldNarrate = false;
            LogDebug($"[StoryEventTrigger] Evento '{storyEvent.eventName}' bloqueado - Requiere evento del niño");
        }
    }
    
    if (shouldNarrate)
    {
        ExecuteEvent(storyEvent);
        eventCooldowns[cooldownKey] = Time.time;
    }
}
```

---

## Flujo del Juego Ahora

### Fase 1: ANTES de Tomar el Oso (6 PM - ~2 AM)

**Narraciones activas:**
- ✅ `firstEntry` - Primera entrada a cada habitación (narración larga)
- ❌ `Narraciones Cortas` - BLOQUEADAS
- ❌ `Tras Evento del Niño` - BLOQUEADAS (requieren `child_dialogue_complete`)

**Experiencia:**
- La casa es misteriosa pero no agresivamente aterradora
- Solo escuchas las descripciones iniciales de cada habitación
- Al re-entrar a habitaciones, NO hay narraciones extra

---

### Fase 2: DESPUÉS de Tomar el Oso (Post 2 AM)

**Trigger:** Tomar el oso → Setea flag `child_event_triggered`

**Narraciones activas:**
- ✅ `firstEntry` - Si aún no visitaste alguna habitación
- ✅ `Narraciones Cortas` - **AHORA ACTIVAS** (descripciones atmosféricas)
- ❌ `Tras Evento del Niño` - BLOQUEADAS (aún requieren `child_dialogue_complete`)
- ✅ `Screamers` - **ACTIVADOS** (30% chance por habitación)

**Experiencia:**
- La casa se vuelve más agresiva
- Cada vez que re-entras a una habitación, escuchas descripciones inquietantes
- Screamers te atacan aleatoriamente
- La corrupción es evidente

---

### Fase 3: DESPUÉS del Diálogo del Niño (Post-Sótano)

**Trigger:** Completar diálogo en sótano → Setea flag `child_dialogue_complete`

**Narraciones activas:**
- ✅ TODAS las narraciones
- ✅ `Tras Evento del Niño` - **AHORA ACTIVAS** (descripciones de la casa transformada)

**Experiencia:**
- La casa ha cambiado completamente
- El retrato familiar está alterado
- La flor de loto viva aparece
- Puedes proceder a la victoria

---

## Logs de Debug

### Antes de Tomar el Oso:
```
[StoryEventTrigger] Trigger room entry: room_2 (reentry - cooldown OK)
[StoryEventTrigger] Evento 'Sala - Narración Corta' bloqueado - Requiere evento del niño
```

### Después de Tomar el Oso:
```
[StoryEventTrigger] Trigger room entry: room_2 (reentry - cooldown OK)
[StoryEventTrigger] ✅ Ejecutando evento: Sala - Narración Corta
[EventManager] Narrando evento: Sala - Narración Corta
```

### Después del Diálogo del Niño:
```
[StoryEventTrigger] Trigger room entry: room_2 (reentry - cooldown OK)
[StoryEventTrigger] ✅ Ejecutando evento: Sala - Tras Evento del Niño
[EventManager] Narrando evento: Sala - Tras Evento del Niño
```

---

## Cambios Adicionales

### BasementDoorDialogue.cs

Añadido el flag `child_dialogue_complete` cuando el jugador guarda silencio:

```csharp
public void OnPlayerSilence()
{
    waitingForResponse = false;
    
    if (playerState != null)
    {
        playerState.SetEventFlag("lotus_flower_activated");
        playerState.SetEventFlag("child_dialogue_complete"); // ← NUEVO
    }
    ...
}
```

Este flag desbloquea las narraciones "Tras Evento del Niño".

---

## Test de Verificación

### Test 1: Fase Inicial (Antes del Oso)
```
1. Play
2. Hall → Sala → Biblioteca → Hall → Sala
3. Al re-entrar a Sala, NO debe narrar descripciones atmosféricas
4. Solo silencio o descripciones básicas
```

### Test 2: Después del Oso (Fase de Corrupción)
```
1. Esperar 2 AM
2. Ir a Hab. Niños → "tomar oso"
3. Volver a Sala
4. Ahora SÍ debe narrar: "Las flores se manchan de rojo..."
5. Screamers también activos
```

### Test 3: Después del Sótano (Fase de Transformación)
```
1. Completar diálogo del niño (guardar silencio)
2. Volver a Sala
3. Debe narrar: "El retrato familiar es distinto. La madre ya no sostiene las flores..."
4. Volver a Hab. Principal
5. Debe narrar: "La flor de loto ahora irradia un brillo pálido..."
```

---

## Resumen de Correcciones

| Archivo | Cambio |
|---------|--------|
| `StoryEventTrigger.cs` | Bloquear "Narraciones Cortas" antes de `child_event_triggered` |
| `BasementDoorDialogue.cs` | Setear `child_dialogue_complete` al completar diálogo |
| `story_events.json` | Añadido `requiredFlag` a "Sala - Narración Corta" |

---

## Flags de Progresión

| Flag | Cuándo se Activa | Qué Desbloquea |
|------|------------------|----------------|
| `child_event_triggered` | Al tomar el oso | • Narraciones Cortas de reentry<br>• Screamers<br>• Diálogo del niño (si vas al sótano) |
| `child_dialogue_complete` | Al completar diálogo (silencio) | • Eventos "Tras Evento del Niño"<br>• Flor de loto viva |
| `lotus_flower_activated` | Al completar diálogo (silencio) | • Flor de loto viva se vuelve visible |
| `gave_flower_to_portrait` | Al dar la flor en la Sala | • Condición para victoria |

---

**✅ Corrección aplicada!**

**Las narraciones atmosféricas intensas ahora SOLO se activan después de tomar el oso (evento del niño).**

**Prueba el juego y verifica que al inicio la casa es misteriosa pero NO agresivamente aterradora.**

