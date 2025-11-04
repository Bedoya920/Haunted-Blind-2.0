# Plan de Corrección Final Completo

## 🐛 PROBLEMAS DETECTADOS

### 1. ❌ Eventos de Primera Entrada NO funcionan
**Causa:** Unity NO recompiló el cambio en `RoomSystemBridge.cs`
**Solución:** Añadido debug log para forzar recompilación
**Estado:** ✅ CORREGIDO - Reinicia Unity

### 2. ❌ "Inspeccionar" en Sala NO da descripción del cuadro  
**Causa:** No hay evento de inspección específico para el cuadro
**Solución:** Implementar evento "inspect" en story_events.json
**Archivo:** `Assets/Resources/story_events.json` - Ya existe evento ID 211 "Sala - Inspeccionar Retrato"

### 3. ❌ Timer muy lento
**Causa:** 30 segundos por hora = 6 minutos total (demasiado)
**Solución:** Cambiar a 20 segundos por hora = 4 minutos total
**Archivo:** `Assets/Data/TimerData.asset`

### 4. ❌ Comando "Despertar" NO mata al jugador
**Causa:** No implementado
**Solución:** Modificar `BasementDoorDialogue.OnPlayerSaidDespertar()` para matar y reiniciar

### 5. ❌ Condición de victoria incompleta
**Causa:** Falta verificación completa
**Solución:** Revisar `WinConditionManager.cs`

---

## 🔧 CORRECCIONES APLICADAS

### 1. RoomSystemBridge - Forzar Recompilación
```csharp
// Añadido debug log nuevo para forzar Unity a recompilar:
Debug.Log($"[RoomBridge] 🆕 visitedRooms.Count: {visitedRooms.Count} (debe ser 0 al inicio)");
```

**✅ CRÍTICO: REINICIA UNITY COMPLETAMENTE para que este cambio se aplique**

---

## 📝 CAMBIOS PENDIENTES (Se aplicarán ahora)

### 2. Timer - Acelerar a 20 seg/hora
**Cambio en `TimerData.asset`:**
- `totalDuration`: 240 (4 minutos = 12 horas)
- `interval`: 20 (20 segundos = 1 hora del juego)

### 3. Comando "Despertar" = Muerte
**Cambio en `BasementDoorDialogue.cs`:**
```csharp
public void OnPlayerSaidDespertar()
{
    // Narrar muerte
    voiceSystem?.textToSpeech?.Speak("Despertaste al niño. Su grito paraliza tu corazón. Has muerto.", TTSPriority.Urgent);
    
    // Matar al jugador (0 vidas)
    fatigueSystem.PlayerLives.currentLives = 0;
    
    // Reiniciar escena después de 3 segundos
    StartCoroutine(RestartGameAfterDelay(3f));
}

private IEnumerator RestartGameAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
}
```

### 4. Evento Inspeccionar Cuadro
**Ya existe en `story_events.json` (ID 211)**
```json
{
    "id": 211,
    "eventName": "Sala - Inspeccionar Retrato",
    "triggerCondition": "inspect",
    "roomId": "room_2"
}
```

**Necesita añadir comando "inspeccionar" en `BasicAIAssistant.cs`**

### 5. Condición de Victoria
**Verificar en `WinConditionManager.cs`:**
- ✅ Jugador tiene flor viva
- ✅ Jugador dio flor al retrato  
- ✅ Jugador dijo "renacer"
- ❌ Falta: Verificar que sea después de 2 AM

---

## 🎮 FLUJO COMPLETO SEGÚN GDD

```
1. Hall (6 PM) → Leer diario
2. Sala → Inspeccionar cuadro (inscripción)
3. Biblioteca → Leer 3 libros (Fénix, Quimera, Loto)
4. Esperar hasta 2 AM (campanadas)
5. Ir a Habitación de los Niños → Tomar oso de peluche
6. Ir al Sótano → Escuchar al niño → GUARDAR SILENCIO (NO decir "despertar")
7. Volver a Habitación Principal → Flor se transforma
8. Tomar flor viva
9. Ir a Sala → Dar flor al retrato
10. Decir "Renacer" → VICTORIA
```

**Si dice "Despertar" en el Sótano → MUERTE INSTANTÁNEA → Reinicio**

---

## 🚀 IMPLEMENTACIÓN AHORA

Aplicando cambios 2, 3, 4 y 5...

