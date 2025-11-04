# 🔊 Sistema de Audio - Guía Completa

## 📋 Resumen

Sistema completo de audio implementado con:
- ✅ Música ambiental dinámica (pre/post 2 AM)
- ✅ Efectos de sonido (puertas, pasos, ambiente)
- ✅ Screamers con fade transitions
- ✅ Campanadas horarias
- ✅ Control de volumen por voz
- ✅ Sistema de fade profesional

---

## 📁 Estructura de Carpetas

```
Assets/Resources/Audio/
├── Music/
│   ├── ambient_pre2am.mp3        (Loop antes de las 2 AM)
│   ├── ambient_post2am.mp3       (Loop después de las 2 AM)
│   └── child_event.mp3           (Durante diálogo del niño)
│
├── Effects/
│   ├── footsteps.mp3             (Pasos del jugador)
│   ├── door_open.mp3             (Puerta abriéndose)
│   ├── door_close.mp3            (Puerta cerrándose - opcional)
│   ├── creak_01.mp3              (Crujido ambiental 1)
│   ├── creak_02.mp3              (Crujido ambiental 2)
│   ├── creak_03.mp3              (Crujido ambiental 3)
│   ├── whisper_01.mp3            (Susurro post-2 AM)
│   ├── whisper_02.mp3            (Susurro post-2 AM)
│   └── whisper_03.mp3            (Susurro post-2 AM)
│
├── Screamers/
│   ├── screamer_kitchen.mp3      (Screamer de cocina)
│   ├── screamer_bathroom.mp3     (Screamer de baño)
│   ├── screamer_bedroom.mp3      (Screamer de habitaciones)
│   ├── screamer_dining.mp3       (Screamer de comedor)
│   ├── screamer_library.mp3      (Screamer de biblioteca)
│   └── screamer_default.mp3      (Screamer genérico)
│
└── Bells/
    └── bell_chime.mp3            (Campanada del reloj)
```

---

## ⚙️ Configuración de Unity para MP3

### Importar Archivos de Audio

1. **Arrastra tus archivos MP3** a las carpetas correspondientes en `Assets/Resources/Audio/`

2. **Selecciona cada archivo** y configura en el Inspector:

#### Para MÚSICA (ambient_*.mp3, child_event.mp3):
```
Format Settings:
  - Load Type: Streaming
  - Compression Format: Vorbis
  - Quality: 100
  - Sample Rate Setting: Preserve Sample Rate
```

#### Para EFECTOS (footsteps, doors, creaks, whispers):
```
Format Settings:
  - Load Type: Decompress On Load
  - Compression Format: Vorbis
  - Quality: 70
  - Sample Rate Setting: Optimize Sample Rate
```

#### Para SCREAMERS:
```
Format Settings:
  - Load Type: Decompress On Load
  - Compression Format: Vorbis
  - Quality: 100
  - Sample Rate Setting: Preserve Sample Rate
```

#### Para CAMPANADAS:
```
Format Settings:
  - Load Type: Decompress On Load
  - Compression Format: Vorbis
  - Quality: 80
  - Sample Rate Setting: Optimize Sample Rate
```

---

## 🎛️ Configurar SoundLibrary.asset

1. **Crear el asset**:
   - Click derecho en `Assets/Resources/Audio/`
   - Create → Audio → Sound Library
   - Renombrar a `SoundLibrary`

2. **Poblar la biblioteca**:
   - En el Inspector, click en `Create Sample Entries` (Context Menu)
   - Esto creará todas las entradas necesarias

3. **Asignar AudioClips manualmente**:
   - Para cada entrada en la lista `sounds`:
     - Expande la entrada (ej: "ambient_pre2am")
     - Arrastra el archivo MP3 correspondiente al campo `Clip`
     - Verifica que `Sound Id` coincida con el nombre del archivo
     - Ajusta `Default Volume` si es necesario

**Ejemplo de entrada configurada**:
```
Sound Id: ambient_pre2am
Clip: ambient_pre2am (AudioClip)
Category: Music
Default Volume: 0.6
Description: Ambiente pre-2 AM
```

---

## 🎮 Comandos de Voz para Control de Audio

El jugador puede controlar el volumen usando voz:

| Comando | Acción |
|---------|--------|
| "subir volumen" | Aumenta volumen maestro +10% |
| "bajar volumen" | Disminuye volumen maestro -10% |
| "aumentar volumen" | (alias de subir) |
| "disminuir volumen" | (alias de bajar) |
| "silenciar" | Silencia todo el audio |
| "silenciar audio" | (alias de silenciar) |
| "activar sonido" | Reactiva el audio |
| "activar audio" | (alias de activar sonido) |

---

## 🔧 Componentes del Sistema

### 1. SoundManager (Singleton)
**Archivo**: `Assets/Scripts/Audio/SoundManager.cs`

**Responsabilidades**:
- Gestiona pools de AudioSource (music, ambient, effects, screamers)
- Sistema de fade in/out profesional
- Control de volumen por categoría
- Carga automática de SoundLibrary

**Métodos principales**:
```csharp
// Música de fondo
soundManager.PlayBackgroundMusic("ambient_pre2am", loop: true, fadeTime: 2f);
soundManager.StopBackgroundMusic(fadeTime: 2f);

// Efectos ambientales aleatorios
string[] effects = { "creak_01", "creak_02", "creak_03" };
soundManager.StartAmbientEffects(effects, delayMin: 5f, delayMax: 15f);
soundManager.StopAmbientEffects();

// Screamer con fade
soundManager.PlayScreamer("screamer_kitchen", onComplete: () => {
    // Callback después del sonido
});

// Efecto de una vez
soundManager.PlayOneShot("door_open", volumeScale: 0.7f);

// Control de volumen
soundManager.AdjustMasterVolume(0.1f); // +10%
soundManager.SetMute(true);
```

### 2. AmbientMusicController (Singleton)
**Archivo**: `Assets/Scripts/Audio/AmbientMusicController.cs`

**Responsabilidades**:
- Cambia música según fase del juego
- Gestiona efectos ambientales aleatorios
- Se suscribe a eventos del GameTimer y PlayerStateManager

**Fases musicales**:
1. **Pre2AM**: Música tensa, efectos de crujidos
2. **Post2AM**: Música más intensa, crujidos + susurros
3. **ChildEvent**: Música especial durante diálogo del niño

**Métodos públicos**:
```csharp
// Cambiar música manualmente
ambientController.StartChildEventMusic();
ambientController.EndChildEventMusic();

// Queries
bool isPost2AM = ambientController.IsPost2AM();
string phase = ambientController.GetCurrentPhase();
```

### 3. SoundLibrary (ScriptableObject)
**Archivo**: `Assets/Scripts/Audio/SoundLibrary.cs`
**Asset**: `Assets/Resources/Audio/SoundLibrary.asset`

**Estructura**:
```csharp
[System.Serializable]
public class SoundEntry
{
    public string soundId;           // ID único
    public AudioClip clip;           // Archivo de audio
    public AudioCategory category;   // Music/Effects/Screamers
    public float defaultVolume;      // 0-1
    public string description;       // Opcional
}
```

---

## 🔗 Puntos de Integración

### 1. Movimiento entre Habitaciones
**Archivo**: `RoomSystemBridge.cs` (líneas 674-708)

Al moverse exitosamente:
1. Sonido de puerta abre (0.7 volumen)
2. Delay 0.4 segundos
3. Sonido de pasos (0.5 volumen)

### 2. Screamers Aleatorios
**Archivo**: `ScreamerSystem.cs` (líneas 109-171)

Secuencia:
1. Fade out música (1 segundo)
2. Sonido de screamer
3. Callback → narración TTS
4. Pausa 0.5 segundos
5. Fade in música (1.5 segundos)

**Mapeo de habitaciones**:
- room_3 (Biblioteca) → screamer_library
- room_4 (Comedor) → screamer_dining
- room_5 (Cocina) → screamer_kitchen
- room_6 (Baño) → screamer_bathroom
- room_7 (Hab. Principal) → screamer_bedroom
- room_9 (Hab. Niños) → screamer_bedroom

### 3. Screamers de Eventos
**Archivo**: `StoryEventTrigger.cs` (líneas 297-346)

**Mapeo de eventos**:
- Evento 300 (Cocina Risa) → screamer_kitchen
- Evento 301 (Baño Espejo) → screamer_bathroom
- Evento 302 (Hab. Niños Caja) → screamer_bedroom
- Evento 303 (Comedor Sombra) → screamer_dining
- Evento 304 (Biblioteca Libros) → screamer_library

### 4. Campanadas Horarias
**Archivo**: `HourlyBellSystem.cs` (líneas 79-134)

Secuencia:
1. Reproducir N campanadas (según hora)
2. Pausa 1.5 segundos entre campanadas
3. Pausa 0.5 segundos final
4. Narración con TTS

**Ejemplo**: A las 2 AM → 2 campanadas + "Dos campanadas resuenan en la casa. Son las 2 de la madrugada."

### 5. Cambios de Música Automáticos
**Archivo**: `AmbientMusicController.cs` (líneas 85-145)

**Triggers**:
- Inicio del juego (delay 3s) → ambient_pre2am
- Al llegar a las 2 AM → ambient_post2am (si child_event_triggered está activo)
- Al tomar el oso → ambient_post2am (inmediato)
- Durante diálogo del niño → child_event (manual desde BasementDoorDialogue)

---

## 🎵 Comportamiento Esperado

### Fase 1: Inicio del Juego
```
[3 segundos después del inicio]
→ Música: ambient_pre2am (loop)
→ Efectos aleatorios: creak_01, creak_02, creak_03 (cada 5-15 segundos)
```

### Fase 2: Primera Hora
```
[Jugador se mueve]
→ Sonido: door_open → pasos
→ Música: continúa ambient_pre2am
```

### Fase 3: Campanada (ej: 7 PM)
```
[GameTimer dispara OnHourPassed]
→ Sonido: bell_chime (1 vez)
→ [Pausa 1.5s]
→ TTS: "Una campanada resuena en la casa. Es la 1 de la tarde."
```

### Fase 4: Jugador Toma el Oso
```
[child_event_triggered flag activado]
→ Música: crossfade a ambient_post2am (3 segundos)
→ Efectos: ahora incluyen whisper_01, whisper_02, whisper_03
→ ScreamerSystem: ACTIVADO
```

### Fase 5: Screamer Aleatorio
```
[Jugador entra a Cocina, roll 30% → éxito]
→ Música: fade out (1s)
→ Sonido: screamer_kitchen
→ [Espera duración del clip]
→ TTS: "Abres los cajones, y el sonido de la madera al ceder..."
→ Música: fade in (1.5s)
→ Vida: -1
```

### Fase 6: Diálogo del Niño
```
[BasementDoorDialogue.DialogueSequence()]
→ Música: crossfade a child_event (2s)
→ Efectos aleatorios: DETENIDOS
→ [Diálogo completo]
→ Música: crossfade a ambient_post2am (2s)
→ Efectos aleatorios: REINICIADOS
```

---

## 🎚️ Control de Volumen

### Categorías de Audio

| Categoría | Volumen Inicial | Uso |
|-----------|----------------|-----|
| **Music** | 60% | Música de fondo loop |
| **Effects** | 80% | Puertas, pasos, crujidos, susurros |
| **Screamers** | 100% | Sonidos de screamers |
| **Master** | 100% | Multiplica todos los volúmenes |

### Fórmula de Volumen Final
```
Volumen Final = defaultVolume × categoryVolume × masterVolume
```

**Ejemplo**:
- Screamer con `defaultVolume = 1.0`
- `screamersVolume = 1.0`
- `masterVolume = 0.8` (80%)
- **Volumen Final** = 1.0 × 1.0 × 0.8 = 0.8 (80%)

### Ajustes Dinámicos (por voz)

```csharp
// El jugador dice "subir volumen"
masterVolume += 0.1f;  // Aumenta 10%
UpdateAllVolumes();
TTS: "Volumen ajustado al 90 por ciento"

// El jugador dice "silenciar"
musicVolume = 0, effectsVolume = 0, screamersVolume = 0
TTS: "Audio silenciado"
```

---

## 🎼 Configuración Avanzada

### SoundManager (Inspector)

```
[Header: Referencias]
Sound Library: SoundLibrary.asset  (auto-cargado desde Resources)

[Header: Audio Sources]
Music Source: (auto-creado)
Ambient Source: (auto-creado)
Effects Source: (auto-creado)
Screamer Source: (auto-creado)

[Header: Volumen por Categoría]
Master Volume: 1.0
Music Volume: 0.6
Effects Volume: 0.8
Screamers Volume: 1.0

[Header: Configuración de Fade]
Default Fade Time: 2.0
Screamer Fade Out Time: 1.0
Screamer Fade In Time: 1.5

[Header: Estado]
Is Muted: false
Enable Debug Logs: true
```

### AmbientMusicController (Inspector)

```
[Header: IDs de Música]
Pre2AM Music Id: ambient_pre2am
Post2AM Music Id: ambient_post2am
Child Event Music Id: child_event

[Header: Efectos Ambientales Pre-2 AM]
Pre Event Ambient Effects:
  - creak_01
  - creak_02
  - creak_03

[Header: Efectos Ambientales Post-2 AM]
Post Event Ambient Effects:
  - creak_01
  - creak_02
  - creak_03
  - whisper_01
  - whisper_02
  - whisper_03

[Header: Configuración de Efectos Aleatorios]
Effect Delay Min: 5.0
Effect Delay Max: 15.0

[Header: Estado]
Current Phase: Pre2AM
Enable Debug Logs: true
```

---

## 🔊 Cómo Agregar Nuevos Sonidos

### Paso 1: Preparar el Archivo
1. Nombre el archivo descriptivamente (ej: `door_creak.mp3`)
2. Formato recomendado: MP3, 44.1 kHz, mono (efectos) o stereo (música)
3. Normaliza el audio a -3dB para evitar clipping

### Paso 2: Importar a Unity
1. Arrastra el archivo a la carpeta apropiada en `Assets/Resources/Audio/`
2. Configura Import Settings según el tipo (ver arriba)
3. Click **Apply**

### Paso 3: Agregar a SoundLibrary
1. Abre `Assets/Resources/Audio/SoundLibrary.asset`
2. Expande la lista `sounds`
3. Incrementa `Size` en +1
4. Configura la nueva entrada:
   - **Sound Id**: "door_creak"
   - **Clip**: Arrastra el AudioClip
   - **Category**: Effects
   - **Default Volume**: 0.7
   - **Description**: "Puerta crujiendo"

### Paso 4: Usar en Código
```csharp
var soundManager = Audio.SoundManager.Instance;
soundManager.PlayOneShot("door_creak", 0.8f);
```

---

## 🐛 Troubleshooting

### "SoundLibrary.asset no encontrado"
- Verifica que el asset esté en `Assets/Resources/Audio/SoundLibrary.asset`
- El sistema funcionará sin él, pero sin sonidos

### "Música no cambia al llegar a las 2 AM"
- Verifica que `child_event_triggered` flag esté activo
- Revisa logs: `[AmbientMusic] Fase: Post-2 AM`

### "Screamers no tienen sonido"
- Verifica que los clips estén asignados en SoundLibrary
- Revisa logs: `[SoundManager] ⚠️ Screamer 'X' no encontrado`
- El sistema hará fallback a solo narración

### "Volumen demasiado bajo/alto"
- Ajusta `masterVolume` en SoundManager Inspector
- O usa comandos de voz: "subir volumen" / "bajar volumen"

### "Efectos ambientales no suenan"
- Verifica que `StartAmbientEffects()` se llamó
- Revisa logs: `[SoundManager] 🌊 Efectos ambientales iniciados`
- Verifica que los IDs de sonido coincidan con SoundLibrary

---

## 📊 Logs de Debug

Activa `Enable Debug Logs` en cada componente para ver:

```
[SoundManager] 🎵 Reproduciendo música: ambient_pre2am (fade: 2s)
[SoundManager] 🌫️ Efecto ambiental: creak_02
[SoundManager] 👻 SCREAMER: screamer_bathroom
[SoundManager] 🔊 OneShot: footsteps (vol: 0.50)
[SoundManager] 🔊 Volumen maestro: 0.90 (90%)

[AmbientMusic] ✅ Suscrito a GameTimer.OnHourPassed
[AmbientMusic] 🎵 Iniciando música ambiente pre-2 AM
[AmbientMusic] 🕐 Hora cambiada a: 7
[AmbientMusic] 👶 Evento del niño detectado - Cambiando a música post-2 AM
[AmbientMusic] 🎵 Fase: Post-2 AM (más intenso)

[HourlyBell] 🔔 Campanadas anunciadas: 2 (2 de la madrugada)
```

---

## 🎯 Checklist de Testing

- [ ] Música pre-2 AM inicia 3 segundos después del juego
- [ ] Efectos aleatorios (crujidos) suenan cada 5-15 segundos
- [ ] Al moverse, suena puerta + pasos
- [ ] Campanadas suenan correctamente (N veces según hora)
- [ ] Al tomar el oso, música cambia a post-2 AM
- [ ] Efectos ahora incluyen susurros
- [ ] Screamers tienen sonido + fade + narración
- [ ] Comandos de volumen funcionan ("subir volumen", "silenciar")
- [ ] Durante diálogo del niño, música cambia temporalmente
- [ ] Después del diálogo, música vuelve a post-2 AM

---

## 📝 Notas Importantes

1. **Fallback Seguro**: Si no hay sonidos asignados, el sistema funciona solo con TTS
2. **Performance**: Música usa Streaming, efectos usan Decompress On Load
3. **Sincronización**: Todos los sonidos respetan el sistema de fade para evitar cortes abruptos
4. **Prioridad**: Screamers tienen máxima prioridad, silencian música temporalmente
5. **Efectos Ambientales**: Se reproducen en loop automático con delays aleatorios

---

## 🚀 Inicialización Automática

El sistema se inicializa automáticamente en `GameInitializer.cs`:

```csharp
// 6.10. SoundManager
GameObject soundManagerObj = new GameObject("SoundManager");
soundManagerObj.AddComponent<Audio.SoundManager>();

// 6.11. AmbientMusicController
GameObject ambientMusicObj = new GameObject("AmbientMusicController");
ambientMusicObj.AddComponent<Audio.AmbientMusicController>();
```

**No requiere configuración manual en la escena.**

---

## ✅ Sistema Completo

El sistema de audio está **100% integrado** con:
- ✅ Música dinámica por fase
- ✅ Efectos de movimiento
- ✅ Screamers con fade profesional
- ✅ Campanadas horarias
- ✅ Control de volumen por voz
- ✅ Fallback seguro si faltan clips
- ✅ Inicialización automática

**Solo falta agregar los archivos MP3 a las carpetas.**

