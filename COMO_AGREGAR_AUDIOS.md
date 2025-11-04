# 🔊 CÓMO AGREGAR AUDIOS AL JUEGO

## 📁 1. ESTRUCTURA DE CARPETAS

Coloca los archivos de audio en esta estructura dentro de Unity:

```
Assets/
└── Resources/
    └── Audio/
        ├── Music/          ← Música de fondo (loop)
        ├── Effects/        ← Efectos de sonido (door, footsteps)
        ├── Screamers/      ← Sonidos de screamers
        └── Bells/          ← Campanadas del reloj
```

**IMPORTANTE**: La carpeta debe ser `Assets/Resources/Audio/` (no `Assets/Audio/`)

---

## 📋 2. ARCHIVOS DE AUDIO NECESARIOS

### 🎵 Música (`Assets/Resources/Audio/Music/`)

| Nombre del archivo | ID del sistema | Descripción | Duración recomendada |
|-------------------|----------------|-------------|----------------------|
| `ambient_pre2am.mp3` | `ambient_pre2am` | Música antes de las 2 AM | 2-5 minutos (loop) |
| `ambient_post2am.mp3` | `ambient_post2am` | Música después de las 2 AM | 2-5 minutos (loop) |
| `child_event.mp3` | `child_event` | Música durante evento del niño | 1-3 minutos (loop) |

### 🔊 Efectos (`Assets/Resources/Audio/Effects/`)

| Nombre del archivo | ID del sistema | Descripción | Duración |
|-------------------|----------------|-------------|----------|
| `door_open.mp3` | `door_open` | Sonido de puerta abriéndose | 1-2 segundos |
| `footsteps.mp3` | `footsteps` | Sonido de pasos | 1-2 segundos |
| `creak_01.mp3` | `creak_01` | Crujido ambiental 1 | 2-3 segundos |
| `creak_02.mp3` | `creak_02` | Crujido ambiental 2 | 2-3 segundos |
| `creak_03.mp3` | `creak_03` | Crujido ambiental 3 | 2-3 segundos |
| `whisper_01.mp3` | `whisper_01` | Susurro ambiental 1 | 2-4 segundos |
| `whisper_02.mp3` | `whisper_02` | Susurro ambiental 2 | 2-4 segundos |
| `whisper_03.mp3` | `whisper_03` | Susurro ambiental 3 | 2-4 segundos |

### 👻 Screamers (`Assets/Resources/Audio/Screamers/`)

| Nombre del archivo | ID del sistema | Descripción | Duración |
|-------------------|----------------|-------------|----------|
| `screamer_kitchen.mp3` | `screamer_kitchen` | Screamer de la cocina | 2-4 segundos |
| `screamer_bathroom.mp3` | `screamer_bathroom` | Screamer del baño | 2-4 segundos |
| `screamer_bedroom.mp3` | `screamer_bedroom` | Screamer de habitación | 2-4 segundos |
| `screamer_dining.mp3` | `screamer_dining` | Screamer del comedor | 2-4 segundos |
| `screamer_library.mp3` | `screamer_library` | Screamer de la biblioteca | 2-4 segundos |
| `screamer_default.mp3` | `screamer_default` | Screamer genérico | 2-4 segundos |

### 🔔 Campanadas (`Assets/Resources/Audio/Bells/`)

| Nombre del archivo | ID del sistema | Descripción | Duración |
|-------------------|----------------|-------------|----------|
| `bell_chime.mp3` | `bell_chime` | Una campanada del reloj | 1-2 segundos |

---

## ⚙️ 3. CONFIGURACIÓN EN UNITY

### Paso 1: Importar los archivos

1. Copia los archivos `.mp3` a las carpetas correspondientes
2. Unity los importará automáticamente

### Paso 2: Ajustar configuración de importación

Para CADA archivo de audio:

1. Selecciona el archivo en Unity
2. En el Inspector, configura:

```
✅ Load Type: Compressed In Memory
✅ Compression Format: Vorbis
✅ Quality: 70-100 (ajustar según tamaño)
✅ Preload Audio Data: ✓ (activado)
✅ Load In Background: ✗ (desactivado para efectos cortos)
```

**Excepciones**:
- **Música (loops largos)**: 
  - `Load Type: Streaming`
  - `Load In Background: ✓`
  
- **Screamers**: 
  - `Compression Format: PCM` (mejor calidad)
  - `Quality: 100`

### Paso 3: Crear el SoundLibrary

1. En Unity, click derecho en `Assets/Resources/Audio/`
2. `Create → ScriptableObjects → SoundLibrary`
3. Nombrar: `SoundLibrary`

### Paso 4: Asignar AudioClips al SoundLibrary

1. Selecciona `SoundLibrary.asset`
2. En el Inspector, click `+` para cada sonido
3. Asignar:

```
Sound Entry [0]:
├── Sound ID: "ambient_pre2am"
├── Clip: [Arrastrar ambient_pre2am.mp3]
├── Category: Music
└── Default Volume: 0.6

Sound Entry [1]:
├── Sound ID: "door_open"
├── Clip: [Arrastrar door_open.mp3]
├── Category: Effects
└── Default Volume: 0.7

... (repetir para todos los sonidos)
```

**IMPORTANTE**: El `Sound ID` debe coincidir **exactamente** con los nombres de la tabla anterior.

---

## 🎨 4. FORMATO Y CALIDAD RECOMENDADOS

### Para Música:
```
Formato: MP3 (preferido) o WAV
Bitrate: 128-192 kbps
Sample Rate: 44100 Hz
Canales: Stereo
```

### Para Efectos y Screamers:
```
Formato: MP3 (preferido) o WAV
Bitrate: 96-128 kbps (efectos), 192 kbps (screamers)
Sample Rate: 44100 Hz
Canales: Mono (efectos) o Stereo (screamers)
```

---

## 📦 5. PARA COMMITEAR

### Archivos a incluir en Git:

```
✅ Assets/Resources/Audio/Music/*.mp3
✅ Assets/Resources/Audio/Effects/*.mp3
✅ Assets/Resources/Audio/Screamers/*.mp3
✅ Assets/Resources/Audio/Bells/*.mp3
✅ Assets/Resources/Audio/SoundLibrary.asset
✅ Assets/Resources/Audio/*.meta (archivos de metadatos de Unity)
```

### Estructura final del commit:

```
Assets/
└── Resources/
    └── Audio/
        ├── Music/
        │   ├── ambient_pre2am.mp3
        │   ├── ambient_pre2am.mp3.meta
        │   ├── ambient_post2am.mp3
        │   ├── ambient_post2am.mp3.meta
        │   ├── child_event.mp3
        │   └── child_event.mp3.meta
        ├── Effects/
        │   ├── door_open.mp3
        │   ├── door_open.mp3.meta
        │   ├── footsteps.mp3
        │   ├── footsteps.mp3.meta
        │   ├── creak_01.mp3
        │   ├── creak_01.mp3.meta
        │   ... (etc.)
        ├── Screamers/
        │   ├── screamer_kitchen.mp3
        │   ├── screamer_kitchen.mp3.meta
        │   ... (etc.)
        ├── Bells/
        │   ├── bell_chime.mp3
        │   └── bell_chime.mp3.meta
        ├── SoundLibrary.asset
        └── SoundLibrary.asset.meta
```

### Comando Git:

```bash
# Añadir todos los archivos de audio
git add Assets/Resources/Audio/

# Commit
git commit -m "feat: Agregar archivos de audio del juego

- Música ambiental (pre/post 2AM, evento niño)
- Efectos de sonido (puertas, pasos, ambientales)
- Screamers (6 habitaciones)
- Campanadas del reloj
- SoundLibrary configurado con todos los clips"
```

---

## 🔍 6. VERIFICACIÓN

### Checklist antes de commitear:

- [ ] Todos los archivos están en las carpetas correctas
- [ ] Los nombres de archivo coinciden con los IDs del sistema
- [ ] `SoundLibrary.asset` existe en `Assets/Resources/Audio/`
- [ ] Todos los AudioClips están asignados en `SoundLibrary`
- [ ] Los `Sound ID` coinciden exactamente con los nombres esperados
- [ ] Los archivos `.meta` están incluidos en Git
- [ ] Probaste el juego y los sonidos funcionan

### Probar en Unity:

1. Iniciar el juego
2. Verificar en consola:
   ```
   ✅ "[SoundManager] ✅ SoundLibrary cargado con XX sonidos"
   ```
3. Moverte entre habitaciones → Escuchar puerta + pasos
4. Esperar 1 minuto → Escuchar campanada
5. Tomar el oso → Escuchar cambio de música

---

## 🚨 ERRORES COMUNES

### Error: "SoundLibrary.asset no encontrado"
**Solución**: El archivo debe estar en `Assets/Resources/Audio/SoundLibrary.asset` (exactamente esa ruta)

### Error: "Música 'ambient_pre2am' no encontrada"
**Solución**: Verifica que:
1. El archivo existe en `Assets/Resources/Audio/Music/`
2. El `Sound ID` en `SoundLibrary` es exactamente `"ambient_pre2am"` (sin espacios, case-sensitive)
3. El AudioClip está asignado en el Inspector

### Error: Los sonidos no se reproducen
**Solución**: Verifica en el Inspector de `SoundLibrary`:
1. Cada entrada tiene un AudioClip asignado (no está vacío)
2. El `Default Volume` no está en 0
3. El `Category` es correcto (Music/Effects/Screamers/Bells)

---

## 📊 RESUMEN RÁPIDO

1. **Coloca archivos** → `Assets/Resources/Audio/[Music|Effects|Screamers|Bells]/`
2. **Crea SoundLibrary** → `Assets/Resources/Audio/SoundLibrary.asset`
3. **Asigna clips** → Arrastra cada audio al SoundLibrary con su ID correcto
4. **Commitea** → `git add Assets/Resources/Audio/` + `git commit`
5. **Verifica** → Inicia el juego y comprueba que funcionan

---

## ✅ LISTO

Una vez completados estos pasos, el sistema de audio estará completamente funcional y listo para producción.

**Recuerda**: Si faltan algunos audios, el juego seguirá funcionando (mostrará warnings pero no crasheará).

