# Sistema de Voz con IA Conversacional - Haunted Blind

## ✅ Sistema Implementado Exitosamente

He implementado un **sistema modular de voz con IA conversacional** para tu juego "Haunted Blind". El sistema incluye:

### 🎯 Componentes Principales

#### 1. **Reconocimiento de Voz (Sentis + Whisper)**
- `SentisWhisperRecognizer.cs` - Reconocimiento de voz usando Whisper en Sentis
- `AudioProcessor.cs` - Preprocesamiento de audio y detección de silencio
- `WhisperModelConfig.cs` - Configuración del modelo Whisper

#### 2. **Síntesis de Voz (TTS)**
- `WindowsTextToSpeech.cs` - Text-to-Speech usando Windows SAPI
- Sistema de cola con prioridades (Urgente, Normal, Background)
- Soporte para voces en español

#### 3. **IA Conversacional**
- `BasicAIAssistant.cs` - Asistente basado en reglas (sin LLM por ahora)
- `ResponseParser.cs` - Extrae comandos de lenguaje natural
- `AIPromptTemplates.cs` - Plantillas de respuestas contextuales

#### 4. **Integración con el Juego**
- `GameContextProvider.cs` - Provee contexto del juego a la IA
- `AICommandExecutor.cs` - Ejecuta comandos extraídos por la IA
- `VoiceSystemManager.cs` - Manager central que coordina todo

#### 5. **Interfaz de Usuario**
- `VoiceSystemDebugUI.cs` - UI de debug completa para testing
- Muestra transcripciones, respuestas de IA, contexto y log de conversación

### 📁 Estructura del Proyecto

```
Assets/
├── Scripts/VoiceSystem/
│   ├── Core/
│   │   ├── Interfaces/          # Interfaces del sistema
│   │   ├── Data/                # Modelos de datos
│   │   └── VoiceSystemManager.cs
│   ├── Recognition/             # Reconocimiento de voz
│   ├── Synthesis/               # Text-to-Speech
│   ├── AI/                      # Asistente de IA
│   ├── GameIntegration/         # Integración con el juego
│   ├── UI/                      # UI de debug
│   └── Editor/                  # Herramientas de Unity Editor
├── Resources/VoiceSystem/
│   ├── Commands/                # Comandos de voz
│   ├── WhisperConfig.asset
│   ├── TTSConfig.asset
│   ├── AIResponseRules.asset
│   └── CommandLibrary.asset
├── AI/Models/Whisper/           # Modelos de Whisper
└── Scenes/
    └── VoiceSystemTest.unity    # Escena de prueba
```

### 🚀 Setup Inicial

#### Paso 1: Instalar Sentis
El package Sentis ya fue agregado a `Packages/manifest.json`:
```json
"com.unity.sentis": "2.2.0"
```

#### Paso 2: Crear Assets de Configuración
En Unity, ve al menú:
**`VoiceSystem > Setup > Create All Assets`**

Esto creará automáticamente:
- Configuraciones (Whisper, TTS, AI Templates)
- Biblioteca de comandos
- 12 comandos de voz predefinidos

#### Paso 3: Descargar Modelo Whisper
Necesitas descargar un modelo Whisper para Sentis:
1. Ve a [Hugging Face](https://huggingface.co/)
2. Busca "whisper-tiny" o "whisper-base"
3. Descarga y convierte a formato `.sentis`
4. Coloca el archivo en `Assets/AI/Models/Whisper/`

#### Paso 4: Configurar la Escena de Prueba
1. Abre `Assets/Scenes/VoiceSystemTest.unity`
2. Crea un GameObject llamado "VoiceSystemManager"
3. Agrega estos componentes:
   - `VoiceSystemManager`
   - `SentisWhisperRecognizer`
   - `WindowsTextToSpeech`
   - `BasicAIAssistant`
   - `GameContextProvider`
   - `AICommandExecutor`
4. Asigna las configuraciones en el Inspector
5. Crea una Canvas con UI para `VoiceSystemDebugUI`

### 🎮 Comandos Implementados

El sistema reconoce estos comandos en español:

**Movimiento:**
- Adelante, atrás, izquierda, derecha

**Interacción:**
- Inspeccionar, tomar, usar, comer, leer, dar

**Sistema:**
- Renacer, despertar

**Meta-comandos:**
- Ayuda, inventario, dónde estoy, qué puedo hacer

### 💬 Ejemplo de Conversación

```
Jugador: "¿Dónde estoy?"
IA: "Estás en la Sala Principal. Huele a polvo y madera vieja. Escuchas el eco de tus pasos. Puedes inspeccionar el cuadro familiar, avanzar hacia el pasillo, o ir hacia el comedor."

Jugador: "Inspeccionar cuadro"
IA: [Ejecuta CMD:inspeccionar] "Te acercas al cuadro. Percibes cuatro figuras... un padre, una madre, y dos niños. Uno de ellos sostiene algo suave... parece un peluche."

Jugador: "Tengo miedo"
IA: "Es normal tener miedo aquí. Respira profundo. Tienes 10 acciones para escapar. ¿Qué quieres hacer?"
```

### 🔧 Configuración de Player Settings

Para que funcione correctamente:
1. **Scripting Backend**: IL2CPP (requerido por Sentis)
2. **API Level**: .NET Standard 2.1
3. **Platform**: Windows Standalone (64-bit)
4. **Micrófono**: Permisos habilitados

### 📊 Arquitectura Modular

El sistema está diseñado para ser completamente modular:

```
ISpeechRecognizer ← SentisWhisperRecognizer
                  ← (futuro: WindowsSpeechRecognizer, AzureSpeechRecognizer)

ITextToSpeech ← WindowsTextToSpeech
              ← (futuro: AzureTTS, GoogleTTS)

IAIAssistant ← BasicAIAssistant
             ← (futuro: SentisLLMAssistant con LLM local)
```

### 🎯 Próximos Pasos

1. **Descargar modelo Whisper** y colocarlo en la carpeta correcta
2. **Configurar la escena de prueba** con UI completa
3. **Testear el sistema** con micrófono real
4. **Ajustar parámetros** de sensibilidad y confianza
5. **Integrar con tu juego** conectando el EventManager existente

### ⚠️ Notas Importantes

- **Modelo Whisper**: El sistema está preparado para usar Whisper, pero necesitas descargar el modelo
- **TTS Windows**: Requiere `System.Speech.Synthesis` (incluido en .NET Framework)
- **Micrófono**: Asegúrate de dar permisos de micrófono en Windows
- **Performance**: Whisper-tiny tiene ~2-3 seg de latencia (aceptable para tu juego)

### 📝 Próxima Fase (LLM Completo)

El sistema está arquitecturado para agregar fácilmente un LLM local con Sentis:
- Phi-2, TinyLlama o LLaMA-3.2
- Respuestas más naturales y contextuales
- Sistema de hints adaptativos
- Memoria de conversación persistente

### 🆘 Debug y Testing

Usa el menú de Unity:
- `VoiceSystem > Setup > Create All Assets` - Crear assets
- Context Menu en componentes para test individual
- `VoiceSystemDebugUI` muestra toda la información en tiempo real

---

## 🎉 ¡Todo Listo!

El sistema de voz está completamente implementado y listo para ser configurado en Unity. Solo faltan los pasos de configuración manual en el editor (crear GameObjects, asignar referencias, descargar modelo Whisper).

¿Necesitas ayuda con algún paso específico de la configuración?

