# 🎯 Setup Manual del Sistema de Voz con Sentis

## ✅ **Setup Paso a Paso en Unity:**

### **Paso 1: Verificar que Sentis esté instalado**
1. Abre Unity Editor
2. Ve a **Window > Package Manager**
3. Busca **Sentis** en Unity Registry
4. Si no está instalado, haz click en **Install**

### **Paso 2: Ejecutar el setup automático**
1. En Unity, ve al menú superior
2. Busca **Tools > VoiceSystem > Quick Setup Sentis**
3. Click en el menú
4. Espera a que termine el proceso

**Si el menú no aparece:**
- Espera a que Unity termine de compilar los scripts
- Revisa la consola por errores de compilación
- Reinicia Unity si es necesario

### **Paso 3: Verificar el setup**
Después de ejecutar el setup, deberías tener:

1. **Carpetas creadas:**
   - `Assets/AI/Models/TTS/`
   - `Assets/Resources/VoiceSystem/`

2. **Configuraciones creadas:**
   - `Assets/Resources/VoiceSystem/WhisperConfig.asset`
   - `Assets/Resources/VoiceSystem/TTSConfig.asset`

3. **GameObject en la escena:**
   - `VoiceSystemManager` con los siguientes componentes:
     - `VoiceSystemManager`
     - `SentisWhisperRecognizer`
     - `SentisTextToSpeech`

### **Paso 4: Configuración manual (si el menú no funciona)**

Si el menú automático no funciona, puedes hacerlo manualmente:

#### **4.1. Crear GameObject:**
1. Click derecho en Hierarchy
2. **Create Empty**
3. Renombrar a `VoiceSystemManager`
4. Posición: (0, 0, 0)

#### **4.2. Agregar componentes:**
1. Selecciona `VoiceSystemManager`
2. Click en **Add Component**
3. Busca y agrega: `VoiceSystemManager`
4. Click en **Add Component**
5. Busca y agrega: `SentisWhisperRecognizer`
6. Click en **Add Component**
7. Busca y agrega: `SentisTextToSpeech`

#### **4.3. Crear configuraciones:**
1. En la carpeta **Project**:
   - Click derecho en `Assets/Resources/VoiceSystem/`
   - **Create > ScriptableObject**
   - Busca `WhisperModelConfig`
   - Renombra a `WhisperConfig`
   
2. Configurar WhisperConfig:
   - Model Path: `Assets/AI/Models/Whisper/whisper-tiny-es.sentis`
   - Model Size: `tiny`
   - Language: `es`
   - Silence Threshold: `0.01`
   - Chunk Duration: `1.0`

3. Repetir para `TTSConfig`:
   - Volume: `1.0`
   - Rate: `0`
   - Voice Name: `Sentis TTS Voice`
   - Max Queue Size: `10`
   - Interrupt Lower Priority: ✓
   - Default Priority: `Normal`

#### **4.4. Asignar configuraciones:**
1. Selecciona `VoiceSystemManager` en Hierarchy
2. En Inspector, busca `SentisWhisperRecognizer`:
   - Arrastra `WhisperConfig` al campo **Config**
3. Busca `SentisTextToSpeech`:
   - Arrastra `TTSConfig` al campo **Config**

### **Paso 5: Descargar modelo Whisper**

1. Ve a: https://huggingface.co/whisper-tiny
2. Descarga el modelo `whisper-tiny.pt`
3. Convierte a `.sentis` usando Unity Sentis tools
4. Coloca en: `Assets/AI/Models/Whisper/whisper-tiny-es.sentis`

**Nota:** El sistema funcionará en modo fallback (simulado) hasta que tengas el modelo Whisper.

### **Paso 6: Probar el sistema**

1. **Presiona Play** en Unity
2. Verifica los logs en la consola:
   ```
   [SentisWhisper] Initializing Sentis Whisper Recognizer...
   [SentisTTS] Initializing Sentis Text-to-Speech...
   [VoiceSystem] Initializing Voice System Manager...
   ```

3. **Prueba con Context Menu:**
   - Click derecho en `SentisWhisperRecognizer` en Inspector
   - Selecciona **Test Recognition**
   - Deberías ver logs de reconocimiento simulado

   - Click derecho en `SentisTextToSpeech` en Inspector
   - Selecciona **Test Speech - Hello**
   - Deberías ver logs de síntesis de voz

### **Paso 7: Verificar que todo funciona**

Si todo está bien configurado, deberías ver en la consola:

```
[SentisWhisper] Initialization complete
[SentisTTS] Initialization complete
[VoiceSystem] Voice System Manager initialized successfully
[VoiceSystem] Started listening for voice input
```

## 🎉 **¡Listo!**

El sistema de voz con Sentis está configurado y listo para usar.

**Características:**
- ✅ Reconocimiento de voz con Whisper (simulado hasta que tengas el modelo)
- ✅ Text-to-Speech con Sentis (simulado hasta que tengas el modelo TTS)
- ✅ IA conversacional integrada
- ✅ Sin dependencias de Windows
- ✅ Multiplataforma

## 🆘 **Solución de problemas:**

### **El menú no aparece:**
- Espera a que Unity compile todos los scripts
- Revisa errores de compilación en Console
- Reinicia Unity

### **Los componentes no aparecen:**
- Asegúrate de que todos los scripts estén compilados sin errores
- Verifica que los namespaces sean correctos

### **El sistema no se inicializa:**
- Verifica que las configuraciones estén asignadas
- Revisa los logs en la consola para más detalles

---

**¿Necesitas ayuda con algún paso específico?**
