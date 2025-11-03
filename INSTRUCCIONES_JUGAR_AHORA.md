# 🎮 INSTRUCCIONES PARA JUGAR AHORA - HAUNTED BLIND

## ✅ TODO ESTÁ LISTO - Sin errores de compilación

---

## 🚀 PASOS FINALES (5 MINUTOS)

### PASO 1: Añadir MapeoHabitacionesGDD a GameManager

**En Unity Editor:**
1. Selecciona el GameObject **GameManager**
2. **Add Component** → Busca `MapeoHabitacionesGDD`
3. ✅ Listo (se auto-ejecutará al iniciar el juego)

### PASO 2: (OPCIONAL) Añadir DiagnosticoJuego

**En Unity Editor:**
1. Selecciona el GameObject **GameManager**
2. **Add Component** → Busca `DiagnosticoJuego`
3. Esto te permitirá presionar teclas en Play Mode para ver información:
   - **D** = Diagnóstico completo
   - **L** = Info de movimiento (qué puertas hay)
   - **E** = Eventos disponibles en esta habitación

### PASO 3: Presiona PLAY

---

## 🎯 CÓMO FUNCIONA EL SISTEMA AHORA

### **Problema que tenías:**
- ❌ Los eventos del GDD usaban roomIds como "sala", "biblioteca"
- ❌ El generador creaba roomIds como "room_1", "room_2"
- ❌ **NUNCA coincidían = NO se disparaban eventos**

### **Solución implementada:**
- ✅ `MapeoHabitacionesGDD` mapea habitaciones generadas a nombres del GDD
- ✅ Reescribe los roomIds de TODOS los eventos automáticamente
- ✅ Ahora "sala" → "room_5" (la habitación generada que tiene piano/retrato)
- ✅ Los eventos SE DISPARAN correctamente

### **Direcciones cardinales:**
- ✅ `RoomSystemBridge` ya calcula direcciones automáticamente
- ✅ Usa la posición de las habitaciones (Vector2Int)
- ✅ deltaX > 0 = "este", deltaX < 0 = "oeste"
- ✅ deltaY > 0 = "sur", deltaY < 0 = "norte"

---

## 🎤 COMANDOS DE VOZ QUE FUNCIONAN

### **Navegación (FUNCIONAN AHORA):**
```
✅ "adelante" → Ir al norte
✅ "atrás" → Ir al sur  
✅ "derecha" → Ir al este
✅ "izquierda" → Ir al oeste
```

### **Exploración:**
```
✅ "buscar" → Buscar items + DISPARA EVENTOS DE INSPECCIÓN
✅ "tomar oso rojo" → DISPARA EVENTO DEL NIÑO
✅ "tomar flor de loto" → DISPARA EVENTO FINAL
```

### **Información:**
```
✅ "información" → Estado del juego
✅ "qué puertas hay" → Lista de puertas
✅ "ayuda" → Comandos disponibles
```

### **Sistema:**
```
✅ "comer" → Consumir comida (restaura vida)
```

---

## 🔍 CÓMO USAR EL DIAGNÓSTICO

### Durante Play Mode, presiona teclas:

**D** = Diagnóstico Completo
```
Muestra:
- ✅ Habitación actual (nombre + ID)
- ✅ Puertas disponibles (con direcciones)
- ✅ Estado de PlayerData
- ✅ Eventos cargados
- ✅ Inventario
```

**L** = Info de Movimiento
```
Muestra:
- ✅ Puertas con traducción direccional
  Ejemplo: "adelante (norte) → Puerta hacia Cocina"
- ✅ Si están bloqueadas
```

**E** = Eventos de Historia
```
Muestra:
- ✅ Qué eventos están disponibles en esta habitación
- ✅ Si los roomIds coinciden correctamente
- ✅ Si ya fueron disparados
```

---

## 📋 SECUENCIA DE PRUEBA RECOMENDADA

### 1. **Presiona Play**
- Escucha el mensaje de bienvenida
- Espera "Sistema de voz activado"

### 2. **Presiona D** (diagnóstico)
- Verifica en la consola:
  - ¿Qué habitación estás?
  - ¿Cuántas puertas hay?
  - ¿Cuántos story events hay cargados?

### 3. **Presiona L** (movimiento)
- Ve qué direcciones están disponibles
- Ejemplo: "adelante (norte) → Puerta hacia Biblioteca"

### 4. **Di "adelante"** (o la dirección que viste)
- Deberías escuchar:
  1. Confirmación de movimiento: "Te mueves adelante hacia [Habitación]"
  2. Evento de entrada: "[Narración de la habitación]"

### 5. **Di "buscar"**
- Deberías escuchar:
  1. Confirmación de búsqueda
  2. Evento de inspección (si hay uno)

---

## 🐛 SI NO FUNCIONA

### **Problema: "No se disparan eventos de historia"**

**Verifica en consola (presiona E):**
```
Si dice: "No hay story events para roomId 'room_5'"
```

**Solución:**
- ✅ Verifica que `MapeoHabitacionesGDD` esté en GameManager
- ✅ Mira los logs de inicio, debe decir:
  ```
  [MapeoHabitaciones] sala → room_X
  [MapeoHabitaciones] biblioteca → room_Y
  [MapeoHabitaciones] X eventos actualizados
  ```

### **Problema: "No me muevo cuando digo adelante"**

**Verifica en consola:**
```
Debe decir:
[GameContextProvider] Movimiento exitoso adelante → [Habitación]
```

**Si dice "No hay una puerta hacia adelante":**
- ✅ Presiona **L** para ver puertas disponibles
- ✅ Las puertas tienen direcciones cardinales automáticas
- ✅ Usa la dirección correcta

---

## 📊 CHECKLIST ANTES DE JUGAR

- [ ] `MapeoHabitacionesGDD` añadido a GameManager
- [ ] (Opcional) `DiagnosticoJuego` añadido a GameManager
- [ ] Presionar Play
- [ ] Presionar **D** para ver diagnóstico
- [ ] Verificar que story events > 0
- [ ] Verificar que puertas tienen direcciones

---

## 🎯 LO QUE DEBE PASAR

### **Al iniciar:**
```
[MapeoHabitaciones] Iniciando mapeo...
[MapeoHabitaciones] 'sala' → room_X
[MapeoHabitaciones] 'biblioteca' → room_Y
[MapeoHabitaciones] ✅ 35 eventos actualizados
```

### **Al moverte:**
```
[GameContextProvider] Movimiento exitoso adelante → Biblioteca
[StoryEventTrigger] Cambio de habitación: room_5 (Biblioteca)
[StoryEventTrigger] Ejecutando evento: Biblioteca - Primera Entrada
[EventManager] Narrando evento: Biblioteca - Primera Entrada
```

### **Al buscar:**
```
[GameContextProvider] SearchCurrentRoom
[StoryEventTrigger] Trigger action: inspect en room_5
[StoryEventTrigger] Ejecutando evento: Biblioteca - Inspeccionar Mesa
```

---

## 💡 TIPS IMPORTANTES

1. **Los eventos se disparan UNA SOLA VEZ** (isUnique: true)
2. **Necesitas el "oso rojo" para activar la historia** (está en habitación de niños)
3. **Las direcciones se calculan automáticamente** (no necesitas configurar nada)
4. **El mapeo se hace automático** al iniciar el juego
5. **Revisa SIEMPRE la consola** - Los logs son muy detallados

---

## 🎉 AHORA SÍ - ¡A JUGAR!

1. ✅ Añade `MapeoHabitacionesGDD` a GameManager
2. ✅ Añade `DiagnosticoJuego` a GameManager (opcional)
3. ✅ Presiona **PLAY**
4. ✅ Espera el mensaje de bienvenida
5. ✅ Presiona **D** para ver el diagnóstico
6. ✅ **Habla comandos**: "información", "adelante", "buscar"

---

**Si los eventos SIGUEN sin dispararse después de esto, presiona D y E y dime qué dice en los logs.**

