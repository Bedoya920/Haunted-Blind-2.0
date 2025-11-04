# Corrección: Cuadro Familiar y Primera Entrada a Biblioteca

## ✅ Problema 1: Cuadro Familiar se Podía "Tomar"

### Problema
El cuadro familiar aparecía en la lista de objetos que se podían tomar, pero debería ser solo observable/inspectable (está fijo en la pared).

### Solución Aplicada

**Archivo:** `Assets/Resources/room_inventories.json`

**Cambios:**
1. Removido "tomar" de las acciones disponibles en la Sala (room_2)
2. Cambiado `itemType` de `4` (Objeto) a `3` (Readable/Inspectable)
3. Añadido `failMessage` explicando que no se puede tomar
4. Mejorada la descripción larga para incluir detalles de la familia

**Antes:**
```json
{
    "roomId": "room_2",
    "availableActions": ["inspeccionar", "tomar", "buscar"],  ❌
    "items": [
        {
            "itemId": "item_cuadro",
            "itemType": 4,  ❌ Tipo "Objeto"
            "failMessage": "",  ❌ Sin mensaje
            "longDescription": "Un retrato... Sus ojos parecen seguirte..."
        }
    ]
}
```

**Ahora:**
```json
{
    "roomId": "room_2",
    "availableActions": ["inspeccionar", "buscar"],  ✅ Sin "tomar"
    "items": [
        {
            "itemId": "item_cuadro",
            "itemType": 3,  ✅ Tipo "Readable" (solo inspeccionar)
            "failMessage": "El cuadro está fijo a la pared. No puedes tomarlo.",  ✅
            "longDescription": "Un retrato de una familia que vivió aquí hace décadas. La madre sostiene un ramo de flores de loto blancas, el padre tiene una mano sobre el hombro de un niño pequeño que sostiene un oso de peluche. Sus ojos parecen seguirte mientras te mueves por la habitación. Hay algo inquietante en sus expresiones, como si supieran que estás aquí."  ✅ Descripción mejorada
        }
    ]
}
```

**Resultado:**
- ✅ "inspeccionar" → Muestra la descripción del cuadro
- ✅ "leer cuadro" → Muestra la descripción completa
- ✅ "tomar cuadro" → "El cuadro está fijo a la pared. No puedes tomarlo."

---

## ✅ Problema 2: "Cuando Regresas a la Biblioteca" en Primera Entrada

### Problema CRÍTICO

El evento "Biblioteca - Tras Evento del Niño" (ID 225) que dice:
> "Cuando vuelves a la biblioteca, el aire parece más denso..."

Se estaba disparando en la **PRIMERA entrada** en lugar de disparar el evento de "Primera Entrada" (ID 220).

### Causa

**BUG EN ORDEN DE EJECUCIÓN:**

En `StoryEventTrigger.HandleRoomChanged()`, el código estaba:

```csharp
// 1. Marcar como visitada  ❌ PRIMERO (BUG!)
playerData.MarkRoomVisited(newRoom.roomId);

// 2. Trigger events  ❌ DESPUÉS (mal orden!)
TriggerRoomEntry(newRoom.roomId);
```

**Flujo incorrecto:**
1. Entras a Biblioteca por primera vez
2. `HandleRoomChanged()` se llama
3. **Marca la habitación como visitada** ❌
4. Llama a `TriggerRoomEntry()`
5. `TriggerRoomEntry()` verifica: `!playerData.HasVisitedRoom(roomId)`
6. Como YA se marcó como visitada, retorna `false` ❌
7. Se dispara el evento de **REENTRY** en lugar de **firstEntry** ❌❌❌

### Solución Aplicada

**Archivo:** `Assets/Scripts/VoiceSystem/GameIntegration/StoryEventTrigger.cs`

**Orden corregido:**
```csharp
private void HandleRoomChanged(RoomData newRoom)
{
    // IMPORTANTE: NO marcar como visitada ANTES del trigger
    // El trigger verifica si es primera visita, y LUEGO marca
    
    // 1. Trigger events PRIMERO ✅
    TriggerRoomEntry(newRoom.roomId);
    
    // 2. Marcar como visitada DESPUÉS ✅
    if (playerData != null)
    {
        playerData.MarkRoomVisited(newRoom.roomId);
    }
}
```

**Flujo correcto AHORA:**
1. Entras a Biblioteca por primera vez
2. `HandleRoomChanged()` se llama
3. Llama a `TriggerRoomEntry()` PRIMERO ✅
4. `TriggerRoomEntry()` verifica: `!playerData.HasVisitedRoom("room_3")` → `true` ✅
5. Se dispara el evento de **firstEntry** (ID 220) ✅
6. LUEGO marca la habitación como visitada ✅
7. En próximas entradas, se dispararán eventos de **reentry**

---

## Resultado

### Primera Entrada a Biblioteca (Ahora Correcto)

**Narra:**
> "La puerta de la Biblioteca se abre con un lamento seco. Un olor a humedad, cuero viejo y polvo te envuelve. Las ventanas están cubiertas con cortinas gruesas que no dejan pasar la luz, apenas un hilo pálido se filtra desde la parte superior, iluminando partículas flotantes en el aire. El silencio aquí no es vacío; parece lleno de murmullos apagados, como si las páginas aún conservaran la voz de quien las escribió."

**✅ Atmosférica pero NO agresiva**  
**✅ Descripción inicial apropiada**

### Re-entrada a Biblioteca (Antes del Oso)

**NO narra nada** (bloqueado por el sistema que añadimos)

### Re-entrada a Biblioteca (Después del Oso)

**Narra:**
> "El aire está espeso. La biblioteca te recibe con su silencio pesado..." (Narración Corta)

### Re-entrada a Biblioteca (Después del Diálogo del Niño)

**Narra:**
> "Cuando vuelves a la biblioteca, el aire parece más denso, casi irrespirable. Las páginas de los libros tiemblan..." (Tras Evento del Niño)

---

## Test de Verificación

### Test Cuadro Familiar:
```
1. Play
2. Ir a Sala
3. "inspeccionar" → Debe listar "Cuadro Familiar"
4. "leer cuadro" → Debe narrar descripción completa
5. "tomar cuadro" → "El cuadro está fijo a la pared. No puedes tomarlo." ✅
```

### Test Biblioteca Primera Entrada:
```
1. Play (juego nuevo)
2. Hall → Sala → Biblioteca
3. DEBE narrar: "La puerta de la Biblioteca se abre con un lamento seco..." ✅
4. NO debe decir: "Cuando regresas..." ✅
5. "leer" → Debe poder leer libros normalmente ✅
```

### Test Biblioteca Re-entrada:
```
1. Biblioteca → Hall → Biblioteca (segunda vez)
2. ANTES del oso: NO debe narrar nada ✅
3. Tomar el oso
4. Volver a Biblioteca
5. AHORA sí: "El aire está espeso..." ✅
```

---

## Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `room_inventories.json` | • Cuadro: removido de "tomar", tipo 3, failMessage añadido<br>• Descripción mejorada con detalles de la familia |
| `StoryEventTrigger.cs` | • `MarkRoomVisited()` movido DESPUÉS de `TriggerRoomEntry()`<br>• Orden de ejecución corregido |

---

## Logs de Debug Esperados

### Primera Entrada a Biblioteca:
```
[StoryEventTrigger] Cambio de habitación: room_3 (Biblioteca)
[StoryEventTrigger] Trigger room entry: room_3 (firstEntry)  ← Correcto
[StoryEventTrigger] ✅ Ejecutando evento: Biblioteca - Primera Entrada
[EventManager] Narrando evento: Biblioteca - Primera Entrada
```

### Segunda Entrada (Antes del Oso):
```
[StoryEventTrigger] Cambio de habitación: room_3 (Biblioteca)
[StoryEventTrigger] Trigger room entry: room_3 (reentry - cooldown OK)
[StoryEventTrigger] Evento 'Biblioteca - Narración Corta' bloqueado - Requiere evento del niño  ← Bloqueado
```

### Segunda Entrada (Después del Oso):
```
[StoryEventTrigger] Cambio de habitación: room_3 (Biblioteca)
[StoryEventTrigger] Trigger room entry: room_3 (reentry - cooldown OK)
[StoryEventTrigger] ✅ Ejecutando evento: Biblioteca - Narración Corta
[EventManager] Narrando evento: Biblioteca - Narración Corta
```

---

**✅ Correcciones aplicadas!**

**El cuadro ahora es solo inspectable y la Biblioteca narra correctamente la primera entrada.**

**La progresión narrativa ahora sigue el flujo del GDD.**

