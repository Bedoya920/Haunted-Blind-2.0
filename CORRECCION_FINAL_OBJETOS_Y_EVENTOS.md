# Corrección Final: Objetos No-Tomables y Eventos de Primera Entrada

## ✅ Problema 1: Cuadro y Libros se Podían "Tomar"

### Objetos Corregidos

| Objeto | Ubicación | Tipo | Acción Correcta | Mensaje si intentas tomar |
|--------|-----------|------|-----------------|---------------------------|
| **Cuadro Familiar** | Sala (room_2) | Readable | `leer cuadro` o `inspeccionar` | "El cuadro está fijo a la pared. No puedes tomarlo." |
| **Libro Fénix** | Biblioteca (room_3) | Readable | `leer` | "El libro está muy deteriorado. Es mejor leerlo aquí sin moverlo." |
| **Libro Quimera** | Biblioteca (room_3) | Readable | `leer` | "El libro está muy deteriorado. Es mejor leerlo aquí sin moverlo." |
| **Libro Loto** | Biblioteca (room_3) | Readable | `leer` | "El libro está muy deteriorado. Es mejor leerlo aquí sin moverlo." |
| **Carta del Padre** | Hab. Principal (room_7) | Readable | `leer` | "Es mejor leer la carta aquí. El papel es muy frágil." |
| **Diario** | Hall (room_1) | Readable | `leer` | "El diario está muy deteriorado. Es mejor leerlo aquí sin moverlo." |

### Cambios Aplicados

**Archivo:** `Assets/Resources/room_inventories.json`

**Sala (room_2):**
```json
"availableActions": ["inspeccionar", "buscar"],  // Removido "tomar"
```

**Biblioteca (room_3):**
```json
"availableActions": ["inspeccionar", "leer", "buscar"],  // Removido "tomar"
```

**Todos los objetos readable:**
- ✅ `itemType: 3` (Readable)
- ✅ `failMessage` añadido con explicación
- ✅ Solo accesibles vía "leer" o "inspeccionar"

---

## ✅ Problema 2: Descripciones de Primera Entrada NO Saltaban

### BUG CRÍTICO Encontrado

Los eventos de `firstEntry` en `story_events.json` usaban nombres en minúsculas:
- `"roomId": "hall"` ❌
- `"roomId": "sala"` ❌
- `"roomId": "biblioteca"` ❌

Pero `RoomSystemBridge` genera roomIds con formato:
- `"room_1"` ✅
- `"room_2"` ✅
- `"room_3"` ✅

**Resultado:** Los eventos NUNCA coincidían con las habitaciones reales.

### Solución Aplicada

**Archivo:** `Assets/Resources/story_events.json`

**Corregidos TODOS los roomIds:**
```json
ANTES                          AHORA
"roomId": "hall"          →    "roomId": "room_1"
"roomId": "sala"          →    "roomId": "room_2"
"roomId": "biblioteca"    →    "roomId": "room_3"
"roomId": "comedor"       →    "roomId": "room_4"
"roomId": "cocina"        →    "roomId": "room_5"
"roomId": "baño"          →    "roomId": "room_6"
"roomId": "habitacion_principal" → "roomId": "room_7"
"roomId": "sotano"        →    "roomId": "room_8"
"roomId": "habitacion_niños"   → "roomId": "room_9"
```

**Total de eventos actualizados:** ~40 eventos

---

## ✅ Problema 3: Orden de Ejecución de Eventos

### Bug Secundario

`StoryEventTrigger.HandleRoomChanged()` marcaba la habitación como visitada ANTES de verificar si era primera entrada.

**Solución:** Invertido el orden.

**Archivo:** `Assets/Scripts/VoiceSystem/GameIntegration/StoryEventTrigger.cs`

```csharp
ANTES (Incorrecto):
1. playerData.MarkRoomVisited(newRoom.roomId);  ❌
2. TriggerRoomEntry(newRoom.roomId);            ❌

AHORA (Correcto):
1. TriggerRoomEntry(newRoom.roomId);            ✅
2. playerData.MarkRoomVisited(newRoom.roomId);  ✅
```

---

## 🎮 Experiencia del Jugador Ahora

### Primera Entrada a Cualquier Habitación

**Ejemplo - Sala Principal:**
1. Entras por primera vez
2. Se dispara evento ID 210 "Sala - Primera Entrada"
3. Narra (25 segundos):
   > "Entras a la sala principal. Es amplia pero no lo suficiente, el polvo cubre la tela de viejos sofás y la atmósfera se vuelve densa. El piano al fondo tiene las teclas amarillentas, y cada tanto una nota vibra por sí sola. Frente a ti, un retrato familiar: el padre, rígido y serio, con un reloj de bolsillo abierto; la madre, serena, sostiene un ramo de flores de loto; y entre ellos, los gemelos, uno sonríe, el otro mira al suelo, abrazando un oso rojo de peluche."

**Ejemplo - Biblioteca:**
1. Entras por primera vez
2. Se dispara evento ID 220 "Biblioteca - Primera Entrada"
3. Narra (22 segundos):
   > "La puerta de la Biblioteca se abre con un lamento seco. Un olor a humedad, cuero viejo y polvo te envuelve. Las ventanas están cubiertas con cortinas gruesas que no dejan pasar la luz, apenas un hilo pálido se filtra desde la parte superior, iluminando partículas flotantes en el aire. El silencio aquí no es vacío; parece lleno de murmullos apagados, como si las páginas aún conservaran la voz de quien las escribió."

### Interacción con Objetos

**Cuadro Familiar (Sala):**
- ✅ "inspeccionar" → Lista objetos, incluyendo "Cuadro Familiar"
- ✅ "leer cuadro" → Descripción completa de la familia
- ❌ "tomar cuadro" → "El cuadro está fijo a la pared. No puedes tomarlo."

**Libros (Biblioteca):**
- ✅ "inspeccionar" → Lista 3 libros + pan
- ✅ "leer" → Lee el primer libro (Fénix)
- ✅ "leer" (segunda vez) → Lee el segundo libro (Quimera)
- ✅ "leer" (tercera vez) → Lee el tercer libro (Loto)
- ❌ "tomar libro" → "El libro está muy deteriorado. Es mejor leerlo aquí sin moverlo."

---

## 🧪 Test de Verificación

### Test 1: Primera Entrada con Narraciones
```
1. Play
2. Espera a que termine "Despiertas en una casa oscura..."
3. Estás en Hall → Debe narrar automáticamente:
   "En el hall principal el aire es espeso, pesado..." ✅
4. "derecha" → Ir a Sala
5. Debe narrar automáticamente:
   "Entras a la sala principal. Es amplia..." ✅
6. "derecha" → Ir a Biblioteca  
7. Debe narrar automáticamente:
   "La puerta de la Biblioteca se abre con un lamento seco..." ✅
```

### Test 2: Objetos No-Tomables
```
En Sala:
- "inspeccionar" → Lista "Cuadro Familiar" ✅
- "leer cuadro" → Descripción de la familia ✅
- "tomar cuadro" → Mensaje de error ✅

En Biblioteca:
- "inspeccionar" → Lista 3 libros ✅
- "leer" → Lee Libro Fénix ✅
- "tomar libro" → Mensaje de error ✅
```

### Test 3: Re-entrada (Antes del Oso)
```
1. Biblioteca → Hall → Sala → Biblioteca
2. Al volver a Biblioteca:
   - NO debe narrar "Cuando regresas..." ✅
   - NO debe narrar "Narración Corta" ✅
   - Solo silencio o descripción básica ✅
```

---

## 📊 Archivos Modificados

| Archivo | Cambios |
|---------|---------|
| `story_events.json` | • Todos los roomIds corregidos (hall → room_1, etc.)<br>• ~40 eventos actualizados |
| `room_inventories.json` | • Removido "tomar" de Sala y Biblioteca<br>• Añadido failMessage a 6 objetos readable<br>• Descripción del cuadro mejorada |
| `StoryEventTrigger.cs` | • MarkRoomVisited() movido DESPUÉS de TriggerRoomEntry() |

---

## Logs de Debug Esperados

### Primera Entrada a Sala:
```
[StoryEventTrigger] Cambio de habitación: room_2 (Sala)
[StoryEventTrigger] Trigger room entry: room_2 (firstEntry)
[StoryEventTrigger] ✅ Ejecutando evento: Sala - Primera Entrada
[EventManager] Narrando evento: Sala - Primera Entrada
```

### Intentar Tomar Cuadro:
```
[RoomInventory] Intentando tomar: item_cuadro
[RoomInventory] ❌ No se puede tomar: El cuadro está fijo a la pared. No puedes tomarlo.
```

### Intentar Tomar Libro:
```
[RoomInventory] Intentando tomar: book_fenix
[RoomInventory] ❌ No se puede tomar: El libro está muy deteriorado. Es mejor leerlo aquí sin moverlo.
```

---

## 🎯 Resumen de Correcciones

✅ **Eventos de primera entrada AHORA funcionan** (roomIds corregidos)  
✅ **Cuadro familiar NO se puede tomar** (solo leer/inspeccionar)  
✅ **Libros NO se pueden tomar** (solo leer)  
✅ **Diario y carta NO se pueden tomar** (solo leer)  
✅ **Mensajes de error apropiados** para cada objeto  
✅ **Orden de eventos corregido** (MarkRoomVisited después de trigger)

---

**🎉 El juego ahora funciona correctamente:**

- 📖 Narraciones de primera entrada se disparan automáticamente
- 🖼️ Objetos decorativos/legibles NO se pueden tomar
- 📚 Libros solo se pueden leer
- 🎭 Progresión narrativa funciona según el GDD

**Prueba ahora y deberías escuchar las descripciones largas al entrar a cada habitación por primera vez.**

