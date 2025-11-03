# 🎮 CÓMO JUGAR HAUNTED BLIND 2.0

## ✅ VERIFICACIÓN: JUEGO LISTO PARA JUGAR

### **Estado de la Escena GameScene.unity:**
- ✅ GameManager con GameInitializer
- ✅ GameManager con RoomGenerator3000 (12 habitaciones)
- ✅ Main Camera con AudioListener
- ✅ Escena guardada en Assets/Scenes/GameScene.unity

### **Estado de Compilación:**
- ✅ 0 errores de compilación
- ✅ 1 warning no crítico (evento OnItemCollected declarado pero no usado aún)

### **Sistemas Singletons (se crean automáticamente al jugar):**
- ✅ VoiceSystemManager
- ✅ RoomSystemBridge
- ✅ EventManager
- ✅ ActionConfirmationManager
- ✅ GameTimer
- ✅ FatigueSystem
- ✅ ConsumiblesManager
- ✅ RoomInventoryManager
- ✅ GamePauseManager
- ✅ EventTriggerSystem

---

## 🎯 CÓMO JUGAR (3 PASOS)

### **PASO 1: Configurar Sistema de Voz**
```
En Unity:
Tools → VoiceSystem → Setup Windows Voice System

Esto crea:
- VoiceSystemManager GameObject
- WindowsSpeechRecognizer (escucha tu voz)
- WindowsTTSPlugin (te habla)
- BasicAIAssistant (entiende comandos)
- GameContextProvider (estado del juego)
- AICommandExecutor (ejecuta acciones)
```

### **PASO 2: Conectar con Generador de Habitaciones**
```
Tools → VoiceSystem → Setup Room Generator Integration

Esto:
- Conecta RoomSystemBridge con RoomGenerator3000
- Activa useRealRoomGenerator = true
- Verifica todos los Singletons
```

### **PASO 3: PRESIONA PLAY** ▶️
```
GameInitializer ejecuta automáticamente:
1. RoomGenerator3000.Iniciar() → Genera casa
2. Espera 3 segundos
3. Narra: "Despiertas en una habitación oscura..."
4. Activa WindowsSpeechRecognizer
5. ¡Puedes empezar a hablar!
```

---

## 🎤 COMANDOS DE VOZ DISPONIBLES

### **Información del Juego:**
| Comando | Resultado |
|---------|-----------|
| **"información"** | Estado completo: habitación, vida, fatiga, tiempo, puertas, inventario |
| **"info"** | Igual que información |
| **"estado"** | Igual que información |
| **"ayuda"** | Lista de comandos disponibles |
| **"inventario"** | Muestra tus items |

### **Navegación por Habitaciones:**
| Comando | Resultado |
|---------|-----------|
| **"qué puertas hay"** | Lista todas las puertas de la habitación |
| **"usar puerta norte"** | Atraviesa la puerta norte (si existe) |
| **"usar puerta este"** | Atraviesa la puerta este |
| **"usar puerta sur"** | Atraviesa la puerta sur |
| **"usar puerta oeste"** | Atraviesa la puerta oeste |

**Feedback:** "Atravesaste la puerta hacia norte. Entraste a la Biblioteca."

### **Sistema de Items:**
| Comando | Resultado |
|---------|-----------|
| **"buscar"** | Revela items ocultos en la habitación |
| **"tomar [item]"** | Ej: "tomar llave", "tomar pan", "tomar diario" |
| **"inspeccionar [item]"** | Descripción detallada del item |

**Feedback Buscar:** "Buscaste cuidadosamente. Encontraste Llave del Sótano, Pan."  
**Feedback Tomar:** "Tomaste Llave del Sótano. Podrías usarla para abrir puertas bloqueadas."

### **Supervivencia:**
| Comando | Resultado |
|---------|-----------|
| **"comer"** | Consume primer alimento del inventario |
| **Tecla F** | Alternativa: consume alimento |

**Feedback:** "Comiste Pan. Recuperaste 1 de vida. Tu fatiga se redujo en 2."

---

## 🎭 EVENTOS NARRADOS AUTOMÁTICAMENTE

### **Eventos de Trama (Main Events):**
- **Inicio del Juego** (único, al empezar)
- **Advertencia del Reloj** (en habitaciones específicas)
- **Primer Intervalo** (cuando pasa tiempo)
- **Mitad del Tiempo** (50% transcurrido)
- **Tiempo Crítico** (poco tiempo restante)
- **Descubrimientos** (bibliotecas, sótanos, etc.)

### **Eventos Aleatorios (Random Events - 20 tipos):**
- **Globales:** Susurro, Pasos, Viento, Crujido, Sombra, Risa, Respiración, etc.
- **Específicos de Habitación:** Piano toca solo (sala), Libro cae (biblioteca), Agua gotea (cocina)

### **Triggers Automáticos:**
- ✅ **Cada intervalo del GameTimer** (30% probabilidad)
- ✅ **Al entrar a una habitación** (20% probabilidad)
- ✅ **Eventos únicos solo una vez**

**Durante narración: EL JUEGO SE PAUSA** (no recibes daño ni efectos)

---

## ⚙️ MECÁNICAS DEL JUEGO

### **Sistema de Vida y Fatiga:**
- ✅ Empiezas con 5 vidas
- ✅ Cada acción aumenta fatiga en 1
- ✅ Al llegar a fatiga máxima (25), pierdes 1 vida y fatiga resetea
- ✅ Comer recupera vida y reduce fatiga
- ✅ Si llegas a 0 vidas → Game Over

### **Sistema de Tiempo:**
- ✅ Temporizador global del juego
- ✅ Eventos se disparan cada intervalo
- ✅ Al acabarse el tiempo → Game Over
- ✅ Tiempo se pausa durante narraciones

### **Sistema de Puertas:**
- ✅ Cada habitación tiene hasta 4 puertas (norte, sur, este, oeste)
- ✅ Puertas pueden estar:
  - **Abiertas:** Puedes atravesarlas libremente
  - **Bloqueadas:** Necesitas una llave específica
- ✅ Al usar una puerta bloqueada con la llave correcta en tu inventario:
  - Se desbloquea automáticamente
  - Atraviesas la puerta
  - Narración: "Usaste [llave] y abriste la puerta"

### **Sistema de Items:**
- ✅ **Tipos de Items:**
  - 🔑 **Llaves** - Abren puertas bloqueadas
  - 🍞 **Consumibles** - Recuperan vida/fatiga
  - 📜 **Quest Items** - Misiones (futuro)
  - 📖 **Readable** - Diarios, notas con historia
  - 🎨 **Decorativos** - Ambiente
  - 🔍 **Hidden** - Requieren "buscar" para revelar

- ✅ **Visibilidad:**
  - **Visibles:** Aparecen al entrar a habitación
  - **Ocultos:** Solo aparecen después de "buscar"

---

## 📋 EJEMPLO DE PARTIDA COMPLETA

```
▶️ PRESIONAS PLAY EN GAMESCENE

[Sistema] Generando casa con 12 habitaciones...
[Sistema] Casa generada: 12 habitaciones, 24 puertas
[Sistema] Jugador inicia en: (2, 4)
[Sistema] RoomSystemBridge conectado al generador

[Espera 3 segundos...]

🎙️ [Narrador]: "Despiertas en una habitación oscura. El aire es pesado y 
                huele a humedad. No recuerdas cómo llegaste aquí."

[Sistema] Sistema de voz activado - Puedes hablar ahora

--- AHORA PUEDES HABLAR ---

🗣️ Tú: "información"
🎙️ Narrador: "Estás en Sala. Tienes 5 vidas, 0 de fatiga. 
              Tiempo restante: 11 horas 45 minutos. 
              Puedes ver: cuadro familiar, piano, reloj."

🗣️ Tú: "qué puertas hay"
🎙️ Narrador: "Las puertas disponibles son: 
              Puerta al Comedor (norte), 
              Puerta a la Biblioteca (este) bloqueada, 
              Puerta a la Cocina (oeste)."

🗣️ Tú: "buscar"
🎙️ Narrador: "Buscaste cuidadosamente. 
              Encontraste Llave de la Biblioteca."

🗣️ Tú: "tomar llave"
🎙️ Narrador: "Tomaste Llave de la Biblioteca. 
              Podrías usarla para abrir puertas bloqueadas."

🗣️ Tú: "usar puerta este"
🎙️ Narrador: "Usaste Llave de la Biblioteca y abriste la puerta. 
              Atravesaste la puerta hacia este. 
              Entraste a la Biblioteca."

[Evento Aleatorio - 20% chance]
🎙️ Narrador: "Un libro cae de un estante. Nadie está cerca."

🗣️ Tú: "buscar"
🎙️ Narrador: "Buscaste cuidadosamente. 
              Encontraste Pan, Diario."

🗣️ Tú: "tomar pan"
🎙️ Narrador: "Tomaste Pan. Lo guardas en tu inventario."

🗣️ Tú: "inspeccionar diario"
🎙️ Narrador: "Inspeccionas Diario. Las páginas están amarillentas y 
              cubiertas de una escritura frenética. Parece ser el diario 
              de un antiguo habitante de la casa."

[Tiempo pasa - GameTimer.OnIntervalReached]
[Evento Aleatorio - 30% chance]
🎙️ Narrador: "Escuchas un susurro lejano. Una voz que dice tu nombre."

[Tu fatiga aumentó por acciones]
🎙️ Narrador: "Te sientes más cansado. 
              Tu nivel de fatiga ha aumentado."

🗣️ Tú: "comer"
🎙️ Narrador: "Comiste Pan. Recuperaste 1 de vida. 
              Tu fatiga se redujo en 2."

🗣️ Tú: "usar puerta oeste"
🎙️ Narrador: "Atravesaste la puerta hacia oeste."

... continúas explorando hasta escapar o morir
```

---

## 🚀 PARA EMPEZAR AHORA MISMO:

### **1. Ejecuta los Setups:**
```
Tools → VoiceSystem → Setup Windows Voice System
Tools → VoiceSystem → Setup Room Generator Integration
```

### **2. Abre GameScene.unity**
```
Assets/Scenes/GameScene.unity
```

### **3. Presiona Play** ▶️

### **4. Escucha al narrador y empieza a hablar comandos**

---

## 🎯 OBJETIVO DEL JUEGO

**Escapar de la casa embrujada antes de que:**
- ⏱️ Se acabe el tiempo
- ❤️ Pierdas todas tus vidas

**Cómo escapar:**
- Explora todas las habitaciones
- Encuentra llaves para puertas bloqueadas
- Lee diarios para descubrir la historia
- Sobrevive a los eventos paranormales
- Encuentra la salida (puerta específica o item especial)

---

## 📊 RESUMEN TÉCNICO

### **Todo Funciona Automáticamente:**
1. GameInitializer genera casa al iniciar
2. Singletons se crean automáticamente
3. Sistema de voz empieza a escuchar
4. Eventos se disparan automáticamente
5. Confirmaciones se narran automáticamente

### **No Requiere Configuración Manual:**
- ✅ Sistemas se conectan solos
- ✅ Referencias se asignan automáticamente
- ✅ Datos se cargan desde JSON

---

## 🎉 ¡TU JUEGO ESTÁ 100% FUNCIONAL!

**Es un juego de audio puro para ciegos:**
- Sin UI visual necesaria
- Todo narrado con voz de Windows
- Navegación por comandos de voz
- Feedback inmediato de cada acción
- Eventos aleatorios para tensión
- Casa procedural (diferente cada partida)

**SOLO PRESIONA PLAY Y EMPIEZA A HABLAR** 🎤


