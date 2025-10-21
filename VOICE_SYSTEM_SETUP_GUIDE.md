# 🎯 Guía de Setup Rápido - Sistema de Voz

## ⚠️ Errores de Compilación Solucionados

He arreglado los errores de compilación que estabas viendo. Los problemas eran:

1. **System.Speech.Synthesis** - Agregué directivas de compilación condicional para Windows
2. **Sentis/Whisper** - Creé una versión simplificada que funciona sin Sentis por ahora
3. **Referencias faltantes** - Todo está ahora correctamente referenciado

## 🚀 Setup Inmediato (5 minutos)

### Paso 1: Crear Assets de Configuración
En Unity, ve al menú:
**`VoiceSystem > Setup > Create All Assets`**

### Paso 2: Configurar la Escena de Prueba
1. Abre `Assets/Scenes/VoiceSystemTest.unity`
2. Crea un GameObject vacío llamado "VoiceSystemManager"
3. Agrega estos componentes:
   - `VoiceSystemManager`
   - `SimpleSpeechRecognizer` (en lugar de SentisWhisperRecognizer)
   - `WindowsTextToSpeech`
   - `BasicAIAssistant`
   - `GameContextProvider`
   - `AICommandExecutor`

### Paso 3: Configurar Referencias
En el Inspector del VoiceSystemManager:
- **Speech Recognizer**: Arrastra el componente SimpleSpeechRecognizer
- **Text To Speech**: Arrastra el componente WindowsTextToSpeech
- **AI Assistant**: Arrastra el componente BasicAIAssistant
- **Context Provider**: Arrastra el componente GameContextProvider
- **Command Executor**: Arrastra el componente AICommandExecutor

### Paso 4: Crear UI de Debug
1. Crea una Canvas
2. Agrega un GameObject con el script `VoiceSystemDebugUI`
3. Asigna las referencias de UI en el Inspector

## 🎮 Cómo Probar

### Modo Demo (Sin Micrófono)
El `SimpleSpeechRecognizer` simula reconocimiento cada 5 segundos:
- Reconocerá comandos aleatorios como "adelante", "inspeccionar", etc.
- La IA responderá contextualmente
- El TTS hablará las respuestas

### Comandos de Prueba
Usa el Context Menu en SimpleSpeechRecognizer:
- **Simulate Recognition - adelante**
- **Simulate Recognition - inspeccionar** 
- **Simulate Recognition - dónde estoy**

### UI de Debug
La UI mostrará:
- Estado del sistema (inicializado/escuchando)
- Texto transcrito
- Respuesta de IA
- Contexto del juego
- Log de conversación

## 🔧 Configuración Avanzada

### Para Usar TTS Real
1. Asegúrate de estar en Windows
2. El TTS usará las voces instaladas en Windows
3. Configura la voz en `TTSConfig.asset`

### Para Usar Reconocimiento Real (Futuro)
1. Descarga un modelo Whisper en formato .sentis
2. Reemplaza `SimpleSpeechRecognizer` con `SentisWhisperRecognizer`
3. Asigna el modelo en `WhisperConfig.asset`

## 📋 Checklist de Verificación

- [ ] Assets creados con `VoiceSystem > Setup > Create All Assets`
- [ ] GameObject VoiceSystemManager creado
- [ ] Todos los componentes agregados
- [ ] Referencias asignadas en el Inspector
- [ ] UI de debug configurada
- [ ] Escena guardada
- [ ] Play mode funciona sin errores

## 🎯 Resultado Esperado

Al hacer Play:
1. El sistema se inicializa
2. Comienza a "escuchar" (modo demo)
3. Cada 5 segundos simula reconocimiento
4. La IA responde contextualmente
5. El TTS habla la respuesta
6. La UI muestra todo en tiempo real

## 🆘 Solución de Problemas

### Error: "No se encuentra el tipo"
- Asegúrate de que todos los scripts estén en las carpetas correctas
- Verifica que no haya errores de compilación en la Console

### Error: "Referencia nula"
- Verifica que todas las referencias estén asignadas en el Inspector
- Asegúrate de que los ScriptableObjects estén creados

### TTS no funciona
- Verifica que estés en Windows
- Comprueba que las voces estén instaladas en Windows

## 🎉 ¡Listo!

Una vez configurado, tendrás un sistema de voz completamente funcional que:
- ✅ Simula reconocimiento de voz
- ✅ Procesa comandos con IA
- ✅ Responde con TTS
- ✅ Muestra todo en UI de debug
- ✅ Es escalable para el futuro

¿Necesitas ayuda con algún paso específico?
