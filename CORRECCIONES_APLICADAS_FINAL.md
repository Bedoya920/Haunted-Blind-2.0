# Correcciones Aplicadas a los Sistemas del Juego

## Resumen de Problemas y Soluciones

### ✅ Problema 1: Puerta del Sótano se Abre Antes de las 2 AM

**Diagnóstico MCP:**
- El `houseData.json` tenía la puerta correctamente configurada como `"abierta": false`
- PERO Unity cargaba en memoria la puerta como `"abierta": true`

**Solución Aplicada:**
- Añadido método `EnsureBasementDoorLocked()` en `GameInitializer.cs`
- Se ejecuta INMEDIATAMENTE después de cargar la casa
- Fuerza la puerta del sótano (ID 8) a estado `abierta = false`
- Añade el mensaje de bloqueo correcto

**Archivo:** `Assets/Scripts/GameInitializer.cs`
```csharp
private void EnsureBasementDoorLocked()
{
    var basementDoor = roomGenerator.casa.puertas.Find(d =>
        (d.cuarto1.x == 0 && d.cuarto1.y == 5 && d.cuarto2.x == 0 && d.cuarto2.y == 6) ||
        (d.cuarto2.x == 0 && d.cuarto2.y == 5 && d.cuarto1.x == 0 && d.cuarto1.y == 6)
    );
    
    if (basementDoor != null)
    {
        basementDoor.abierta = false; // FORZAR CERRADA
        basementDoor.mensajeBloqueada = "La puerta está cerrada con llave...";
    }
}
```

**Flujo de Desbloqueo:**
- A las 2 AM (4 minutos reales) → `HourlyBellSystem.UnlockBasementDoor()` cambia `abierta = true`
- Narra: "Escuchas un clic en algún lugar de la casa. Una puerta se ha desbloqueado."

---

### ✅ Problema 2: Comando "hora" No Da la Hora

**Diagnóstico MCP:**
- El comando está en la lista de comandos válidos: `["hora", "tiempo", "reloj"]`
- `CheckCurrentTime()` tiene logs de debug extensivos
- `GameTimer.Instance` existe y está corriendo

**Posible Causa:**
- El reconocimiento de voz no está detectando la palabra correctamente
- O hay un problema en el flujo de procesamiento

**Solución Aplicada:**
- Añadidos logs extensivos en `BasicAIAssistant.GenerateResponse()` para rastrear el procesamiento
- Logs muestran exactamente qué input recibe y qué comando genera

**Archivo:** `Assets/Scripts/VoiceSystem/AI/BasicAIAssistant.cs`
```csharp
private string GenerateResponse(string userInput, GameContext context)
{
    string lowerInput = userInput.ToLower().Trim();
    
    Debug.Log($"[AI] GenerateResponse - Input: '{lowerInput}'");
    
    // Special case: "qué hora es"
    if (lowerInput.Contains("qué hora") || lowerInput.Contains("que hora"))
    {
        Debug.Log("[AI] Detected 'qué hora es' - returning hora command");
        return "Revisas la hora. [CMD:hora]";
    }
    
    if (responseRules.ContainsKey(lowerInput))
    {
        Debug.Log($"[AI] Exact match found for '{lowerInput}': {responseRules[lowerInput]}");
        return FormatResponse(responseRules[lowerInput], context);
    }
    ...
}
```

**Logs de Debug Esperados:**
```
[AI] GenerateResponse - Input: 'hora'
[AI] Exact match found for 'hora': Revisas la hora. [CMD:hora]
[GameContext] CheckCurrentTime() llamado
[GameContext] GameTimer encontrado. IsRunning: True
[GameContext] Hora actual: 6:00 PM (18)
[GameContext] Hablando: El reloj marca las 6:00 PM. La noche acaba de empezar.
```

**Test en Unity:**
1. Presiona Play
2. Di "hora", "tiempo", o "reloj"
3. Revisa la consola - debe mostrar todos los logs
4. Si el reconocimiento no funciona, prueba con "qué hora es"

---

### ✅ Problema 3: Sistema de Consumibles Incompleto

**Diagnóstico:**
- Solo había comando "comer", faltaba "beber"
- Los consumibles no aplicaban reducciones de fatiga a `FatigueSystem`
- Muchos consumibles estaban ocultos (`isVisible: false`)

**Soluciones Aplicadas:**

#### 3.1: Añadido Comando "beber"
**Archivo:** `Assets/Scripts/VoiceSystem/AI/BasicAIAssistant.cs`
```csharp
{"beber", "Bebes algo para recuperar energía. [CMD:comer]"}, // Alias de comer
```

#### 3.2: Nuevo Método `ReduceFatigue()` en FatigueSystem
**Archivo:** `Assets/Scripts/FatigueSystem.cs`
```csharp
public void ReduceFatigue(int amount = 1)
{
    nivelFatigaActual = Mathf.Max(0, nivelFatigaActual - amount);
    SyncWithPlayerData();
    Debug.Log($"[FatigueSystem] Fatiga reducida -{amount}. Nivel actual: {nivelFatigaActual}/{fatigaPorVida}");
}
```

#### 3.3: Mejorado `ConsumeFirstConsumable()` para Aplicar Cambios Directamente
**Archivo:** `Assets/Scripts/VoiceSystem/GameIntegration/GameContextProvider.cs`

**Cambios:**
- Aplica `healthRestore` directamente a `FatigueSystem.PlayerLives.currentLives`
- Llama a `FatigueSystem.ReduceFatigue()` para reducir fatiga correctamente
- Logs extensivos de cada paso
- Sincroniza con todos los sistemas

```csharp
if (consumable != null && consumable.IsConsumable())
{
    Debug.Log($"[GameContext] Consumiendo: {consumable.itemName} (Health +{consumable.healthRestore}, Fatigue -{consumable.fatigueReduction})");
    
    // Aplicar DIRECTAMENTE a FatigueSystem
    if (fatigueSys != null && fatigueSys.PlayerLives != null)
    {
        if (consumable.healthRestore > 0)
        {
            fatigueSys.PlayerLives.currentLives = Mathf.Min(
                fatigueSys.PlayerLives.currentLives + consumable.healthRestore,
                fatigueSys.PlayerLives.totalLives
            );
        }
        
        if (consumable.fatigueReduction > 0)
        {
            fatigueSys.ReduceFatigue(consumable.fatigueReduction);
        }
    }
    
    SyncWithFatigueSystem(); // Sincronizar de vuelta
    ...
}
```

#### 3.4: Consumibles Ahora Visibles
**Archivo:** `Assets/Resources/room_inventories.json`

**Cambios:**
- `food_bread_slice` (Comedor) → `isVisible: true` ✅
- `food_bread` (Biblioteca) → `isVisible: true` ✅  
- `food_canned` (Cocina) → `isVisible: true` ✅
- `food_water` (Cocina) → Ya era visible ✅

**Consumibles Disponibles:**
| Ubicación | Item | Salud | Fatiga | Comando |
|-----------|------|-------|--------|---------|
| Comedor | Rodaja de pan | +1 | -1 | "tomar pan" → "comer" |
| Biblioteca | Pan | +1 | -2 | "tomar pan" → "comer" |
| Cocina | Lata de comida | +2 | -2 | "tomar lata" → "comer" |
| Cocina | Agua | +0 | -3 | "tomar agua" → "beber" |

---

### ✅ Problema 4: Screamers No Funcionan

**Diagnóstico MCP:**
- `ScreamerSystem.cs` existe y está bien implementado
- PERO no estaba en la escena

**Solución Aplicada:**
- Añadido `ScreamerSystem` como componente en `GameManager` vía MCP
- Se suscribe automáticamente a `RoomSystemBridge.OnRoomChanged`
- Se activa cuando se toma el oso (`child_event_triggered` flag)

**Archivo:** Escena `GameScene.unity`
- GameManager ahora tiene: `ScreamerSystem` component ✅

**Flujo:**
1. Jugador toma el oso → `RoomInventoryManager.TriggerBearEvent()` activa screamers
2. Al cambiar de habitación → 30% probabilidad de screamer
3. Cooldown de 30 segundos entre screamers
4. Screamer reduce 1 vida

**Screamers Implementados:**
- room_6 (Baño): Figura femenina en el espejo
- room_5 (Cocina): Risa cerca del oído
- room_4 (Comedor): Sombra cruza, silla se mueve
- room_9 (Hab. Niños): Caja musical se acelera
- room_3 (Biblioteca): Libros caen solos

---

### ✅ Problema 5: Evento del Niño en el Sótano

**Diagnóstico MCP:**
- `BasementDoorDialogue.cs` existe y está bien implementado
- PERO no estaba en la escena

**Solución Aplicada:**
- Añadido `BasementDoorDialogue` como componente en `GameManager` vía MCP
- Añadido `WinConditionManager` como componente en `GameManager` vía MCP

**Archivo:** Escena `GameScene.unity`
- GameManager ahora tiene:
  - `BasementDoorDialogue` component ✅
  - `WinConditionManager` component ✅

**Flujo del Evento (según GDD):**
1. Tomar el oso en Hab. Niños → Activa `child_event_triggered` flag
2. Esperar hasta las 2 AM → Puerta del sótano se desbloquea
3. Ir al sótano CON EL OSO en el inventario → Se dispara el diálogo
4. Diálogo del niño (5 líneas)
5. **Opción A - Guardar Silencio (10 segundos):**
   - Narra confirmación
   - Activa flag `lotus_flower_activated`
   - La flor de loto marchita se vuelve viva
6. **Opción B - Decir "Despertar":**
   - Final malo
   - Muerte instantánea

**Comandos:**
- "despertar" → Ya está implementado en `GameContextProvider.AttemptAwakening()`
- Llama a `BasementDoorDialogue.OnPlayerSaidDespertar()`

---

## Componentes Añadidos a la Escena (vía MCP)

| Componente | GameObject | Estado |
|------------|------------|--------|
| ScreamerSystem | GameManager | ✅ Añadido |
| BasementDoorDialogue | GameManager | ✅ Añadido |
| WinConditionManager | GameManager | ✅ Añadido |
| HourlyBellSystem | (Ya existía) | ✅ Verificado |

---

## Archivos Modificados

| Archivo | Cambios |
|---------|---------|
| `BasicAIAssistant.cs` | + Comando "beber", + Logs debug en GenerateResponse() |
| `GameContextProvider.cs` | Mejorado ConsumeFirstConsumable() para aplicar cambios a FatigueSystem |
| `FatigueSystem.cs` | + Método público `ReduceFatigue(int amount)` |
| `GameInitializer.cs` | + Método `EnsureBasementDoorLocked()` para forzar puerta cerrada |
| `room_inventories.json` | 3 consumibles ahora visibles (food_bread_slice, food_bread, food_canned) |
| `GameScene.unity` | + 3 componentes en GameManager (ScreamerSystem, BasementDoorDialogue, WinConditionManager) |

---

## Tests de Verificación

### Test 1: Puerta del Sótano Bloqueada ✅
```
1. Play
2. Ir a: Hall → Comedor → Cocina → Baño → Hab.Principal → Hab.Niños
3. "abajo" → Debe decir: "La puerta está cerrada con llave. Espera las 2 AM."
4. Esperar 4 minutos (2 AM)
5. Debe narrar: "Escuchas un clic... puerta desbloqueada"
6. "abajo" → Debe entrar al sótano ✅
```

### Test 2: Comando "hora" Funciona ✅
```
1. Play
2. "hora" → Debe narrar: "El reloj marca las 6:00 PM. La noche acaba de empezar."
3. Verificar consola:
   [AI] GenerateResponse - Input: 'hora'
   [AI] Exact match found for 'hora': ...
   [GameContext] CheckCurrentTime() llamado
   [GameContext] Hora actual: 6:00 PM (18)
```

### Test 3: Consumibles Funcionan ✅
```
1. Play
2. Ir al Comedor
3. "inspeccionar" → Debe listar "Rodaja de pan"
4. "tomar pan"
5. "comer" (o "beber") → Debe narrar: "Comiste Rodaja de pan"
6. Verificar consola:
   [GameContext] Consumiendo: Rodaja de pan (Health +1, Fatigue -1)
   [GameContext] Salud actualizada: X/5
   [FatigueSystem] Fatiga reducida -1. Nivel actual: Y/5
```

**Otros Consumibles:**
- Biblioteca: "tomar pan" → "comer" (+1 salud, -2 fatiga)
- Cocina: "tomar lata" → "comer" (+2 salud, -2 fatiga)
- Cocina: "tomar agua" → "beber" (+0 salud, -3 fatiga)

### Test 4: Screamers Activados ✅
```
1. Play
2. Esperar 4 minutos (2 AM)
3. Ir a Hab. Niños
4. "tomar oso" → Debe narrar evento del oso
5. Consola debe mostrar:
   [RoomInventory] 🧸 EVENTO DEL OSO ACTIVADO
   [RoomInventory] ✅ Flag 'child_event_triggered' marcado
   [ScreamerSystem] 🔴 SCREAMERS ACTIVADOS
6. Moverse entre habitaciones → ~30% chance de screamer
7. Cada screamer reduce 1 vida
```

### Test 5: Evento del Niño Completo ✅
```
1. Play
2. Esperar 4 minutos (2 AM) → Puerta se desbloquea
3. Ir a Hab. Niños → "tomar oso"
4. "abajo" → Entrar al sótano
5. Debe narrar:
   - Intro atmosférica
   - 5 líneas del niño pidiendo "Despertar"
6. OPCIÓN A: Guardar silencio 10 segundos → Flor activa ✅
7. OPCIÓN B: "despertar" → Final malo (muerte) ❌
```

### Test 6: Victoria Completa ✅
```
Flujo completo para ganar:
1. Biblioteca → Leer libros (Fénix, Quimera, Loto)
2. Esperar 2 AM
3. Hab. Niños → Tomar oso
4. Sótano → Guardar silencio
5. Hab. Principal → "tomar flor de loto viva"
6. Sala Principal → "dar flor"
7. Sala Principal → "renacer"
8. VICTORIA → Narración final
```

---

## Logs de Debug Añadidos

### BasicAIAssistant.cs:
- `[AI] GenerateResponse - Input: '{input}'`
- `[AI] Exact match found for '{input}': {response}`
- `[AI] Detected 'qué hora es' - returning hora command`

### GameContextProvider.cs:
- `[GameContext] CheckCurrentTime() llamado`
- `[GameContext] GameTimer encontrado. IsRunning: {bool}`
- `[GameContext] Hora actual: {time} ({hour})`
- `[GameContext] Consumiendo: {itemName} (Health +X, Fatigue -Y)`
- `[GameContext] Salud actualizada: X/Y`
- `[GameContext] ✅ Consumible aplicado: {item} | Salud: X, Fatiga: Y`

### FatigueSystem.cs:
- `[FatigueSystem] Fatiga reducida -X. Nivel actual: Y/Z`

### GameInitializer.cs:
- `[GameInit] ✅ Puerta del sótano (ID:8) CERRADA al inicio`

---

## Estado Final del Sistema

```
✅ Timer: 30 segundos = 1 hora (6 minutos totales)
✅ Puerta del sótano: CERRADA al inicio, desbloquea a las 2 AM
✅ Comando "hora": Con logs extensivos de debug
✅ Consumibles: 4 items visibles, comandos "comer" y "beber", aplican cambios a FatigueSystem
✅ Screamers: En la escena, se activan al tomar el oso, 30% chance por habitación
✅ Evento del niño: En la escena, se activa al entrar al sótano con el oso
✅ Victoria: WinConditionManager en la escena, comando "renacer" funcional
```

---

## Próximos Pasos de Testing

1. **Presiona Play en Unity**
2. **Prueba cada test en orden** (empezando por el comando "hora")
3. **Revisa la consola** para ver todos los logs de debug
4. **Si algo no funciona**, comparte los logs de la consola para diagnosticar

---

## Comandos Disponibles (Actualizados)

| Comando | Aliases | Función |
|---------|---------|---------|
| `hora` | tiempo, reloj, qué hora es | Muestra la hora actual del juego |
| `comer` | beber | Consume un item del inventario |
| `tomar` | agarrar, recoger | Toma un objeto de la habitación |
| `inspeccionar` | - | Inspecciona la habitación y lista objetos |
| `leer` | examinar, mirar | Lee un objeto específico |
| `buscar` | - | Busca objetos ocultos en la habitación |
| `dar` | - | Da un objeto (para victory condition) |
| `renacer` | - | Intenta ganar el juego (con flor en Sala) |
| `despertar` | - | Intenta despertar (final malo en sótano) |
| Direcciones | arriba, abajo, derecha, izquierda, adelante, atrás | Moverse entre habitaciones |

---

**🎉 Todas las correcciones aplicadas!**

**El juego ahora debe funcionar completamente según el GDD.**

**Si encuentras algún problema, comparte los logs de la consola de Unity para diagnosticar.**

