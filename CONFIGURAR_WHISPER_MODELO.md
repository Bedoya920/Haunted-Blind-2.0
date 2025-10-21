# 🎯 Configurar Modelo Whisper Descargado

## ✅ **Modelo Detectado:**
- **Archivo**: `Assets/AI/Models/Whisper/pytorch_model.bin`
- **Estado**: ✅ Descargado correctamente

## 🔧 **Paso 1: Convertir a Formato Sentis**

### **Opción A: Unity Sentis Tools (Recomendado)**
1. Abre Unity Editor
2. Ve a **Window > Sentis > Model Import**
3. Si no aparece el menú, verifica que Sentis esté instalado:
   - **Window > Package Manager**
   - Busca **Sentis** en Unity Registry
   - Instala si no está instalado

### **Opción B: Renombrar Manualmente (Temporal)**
1. En Unity, ve a la carpeta `Assets/AI/Models/Whisper/`
2. Click derecho en `pytorch_model.bin`
3. **Rename** a `whisper-tiny-es.sentis`
4. Unity intentará importarlo como modelo Sentis

### **Opción C: Usar ONNX (Alternativo)**
1. Descarga el modelo ONNX desde: https://github.com/onnx/models
2. Colócalo en `Assets/AI/Models/Whisper/`
3. Unity puede importar ONNX directamente

## 🎮 **Paso 2: Crear VoiceSystemManager**

### **Crear GameObject:**
1. Click derecho en **Hierarchy**
2. **Create Empty**
3. Renombrar a `VoiceSystemManager`
4. Posición: (0, 0, 0)

### **Agregar Componentes:**
1. Selecciona `VoiceSystemManager`
2. **Add Component** → Busca `VoiceSystemManager`
3. **Add Component** → Busca `SentisWhisperRecognizer`
4. **Add Component** → Busca `SentisTextToSpeech`

## ⚙️ **Paso 3: Crear Configuraciones**

### **Crear WhisperConfig:**
1. Click derecho en `Assets/Resources/VoiceSystem/`
2. **Create > ScriptableObject**
3. Busca `WhisperModelConfig`
4. Renombrar a `WhisperConfig`

### **Configurar WhisperConfig:**
- **Model Path**: `Assets/AI/Models/Whisper/whisper-tiny-es.sentis`
- **Model Size**: `Tiny`
- **Language**: `es`
- **Silence Threshold**: `0.01`
- **Chunk Duration**: `1.0`

### **Crear TTSConfig:**
1. Click derecho en `Assets/Resources/VoiceSystem/`
2. **Create > ScriptableObject**
3. Busca `TTSConfig`
4. Renombrar a `TTSConfig`

### **Configurar TTSConfig:**
- **Volume**: `1.0`
- **Rate**: `0`
- **Voice Name**: `Sentis TTS Voice`
- **Max Queue Size**: `10`
- **Interrupt Lower Priority**: ✓
- **Default Priority**: `Normal`

## 🔗 **Paso 4: Asignar Configuraciones**

### **En SentisWhisperRecognizer:**
1. Selecciona `VoiceSystemManager`
2. En Inspector, busca `SentisWhisperRecognizer`
3. Arrastra `WhisperConfig` al campo **Config**

### **En SentisTextToSpeech:**
1. En Inspector, busca `SentisTextToSpeech`
2. Arrastra `TTSConfig` al campo **Config**

## 🧪 **Paso 5: Probar el Sistema**

### **Verificar Setup:**
1. **Presiona Play** en Unity
2. Revisa la consola para logs:
   ```
   [SentisWhisper] Initializing Sentis Whisper Recognizer...
   [SentisTTS] Initializing Sentis Text-to-Speech...
   [VoiceSystem] Voice System Manager initialized successfully
   ```

### **Pruebas con Context Menu:**
1. Click derecho en `SentisWhisperRecognizer` en Inspector
2. **Test Recognition** - Debería mostrar reconocimiento simulado
3. Click derecho en `SentisTextToSpeech` en Inspector
4. **Test Speech - Hello** - Debería mostrar síntesis simulado

## 🎯 **Resultado Esperado:**

Si todo está configurado correctamente:
- ✅ **Sistema se inicializa** sin errores
- ✅ **Reconocimiento simulado** funciona
- ✅ **TTS simulado** funciona
- ✅ **IA conversacional** responde
- ✅ **Comandos del juego** se ejecutan

## 🆘 **Solución de Problemas:**

### **Error: "Model not found"**
- Verifica que el archivo esté en `Assets/AI/Models/Whisper/`
- Asegúrate de que tenga extensión `.sentis`

### **Error: "Config not assigned"**
- Verifica que las configuraciones estén creadas
- Asegúrate de que estén asignadas en los componentes

### **Error: "Sentis not available"**
- El sistema funcionará en modo fallback (simulado)
- Instala Sentis desde Package Manager para funcionalidad completa

## 🎉 **¡Listo!**

Una vez completado este setup:
- El sistema funcionará en **modo simulado** (sin modelo real)
- Podrás probar toda la funcionalidad
- Cuando tengas el modelo `.sentis`, el reconocimiento será real

**¿Necesitas ayuda con algún paso específico?**
