using UnityEngine;
using UnityEditor;
using VoiceSystem.Core;
using VoiceSystem.Recognition;
using VoiceSystem.Synthesis;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Script para verificar el estado del sistema de voz
    /// </summary>
    public static class TestSystemStatus
    {
        [MenuItem("Tools/VoiceSystem/Check System Status")]
        public static void CheckSystemStatus()
        {
            Debug.Log("=== VERIFICANDO ESTADO DEL SISTEMA DE VOZ ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog(
                    "Sistema No Encontrado", 
                    "VoiceSystemManager no encontrado.\n\n" +
                    "Ejecuta: Tools > VoiceSystem > Setup Windows Voice System", 
                    "OK"
                );
                return;
            }
            
            // Verificar componentes
            var recognizer = manager.GetComponent<WindowsSpeechRecognizer>();
            var tts = manager.GetComponent<UnityTextToSpeech>();
            var ai = manager.GetComponent<VoiceSystem.AI.BasicAIAssistant>();
            var context = manager.GetComponent<VoiceSystem.GameIntegration.GameContextProvider>();
            var executor = manager.GetComponent<VoiceSystem.GameIntegration.AICommandExecutor>();
            
            string status = "=== ESTADO DEL SISTEMA ===\n\n";
            
            // VoiceSystemManager
            status += $"VoiceSystemManager: {(manager.IsInitialized ? "✅ Inicializado" : "❌ No inicializado")}\n";
            status += $"Escuchando: {(manager.IsListening ? "✅ Sí" : "❌ No")}\n\n";
            
            // Componentes
            status += "COMPONENTES:\n";
            status += $"WindowsSpeechRecognizer: {(recognizer != null ? "✅ Presente" : "❌ Faltante")}\n";
            status += $"UnityTextToSpeech: {(tts != null ? "✅ Presente" : "❌ Faltante")}\n";
            status += $"BasicAIAssistant: {(ai != null ? "✅ Presente" : "❌ Faltante")}\n";
            status += $"GameContextProvider: {(context != null ? "✅ Presente" : "❌ Faltante")}\n";
            status += $"AICommandExecutor: {(executor != null ? "✅ Presente" : "❌ Faltante")}\n\n";
            
            // Estado de componentes
            if (recognizer != null)
            {
                status += $"Reconocimiento: {(recognizer.IsInitialized ? "✅ Inicializado" : "❌ No inicializado")}\n";
                status += $"Escuchando: {(recognizer.IsListening ? "✅ Sí" : "❌ No")}\n";
            }
            
            if (tts != null)
            {
                status += $"TTS: {(tts.IsInitialized ? "✅ Inicializado" : "❌ No inicializado")}\n";
                status += $"Hablando: {(tts.IsSpeaking ? "✅ Sí" : "❌ No")}\n";
            }
            
            // Configuraciones
            status += "\nCONFIGURACIONES:\n";
            status += $"TTSConfig: {(manager.ttsConfig != null ? "✅ Asignada" : "❌ Faltante")}\n";
            status += $"CommandLibrary: {(manager.commandLibrary != null ? "✅ Asignada" : "❌ Faltante")}\n";
            status += $"PromptTemplates: {(manager.promptTemplates != null ? "✅ Asignada" : "❌ Faltante")}\n";
            
            Debug.Log(status);
            
            EditorUtility.DisplayDialog(
                "Estado del Sistema", 
                status, 
                "OK"
            );
        }
        
        [MenuItem("Tools/VoiceSystem/Test TTS Fallback")]
        public static void TestTTSFallback()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null)
            {
                var tts = manager.GetComponent<UnityTextToSpeech>();
                if (tts != null)
                {
                    tts.Speak("Probando sistema de voz en modo fallback. Si escuchas esto, el sistema está funcionando.", VoiceSystem.Core.Interfaces.TTSPriority.Normal);
                    Debug.Log("✅ TTS fallback probado");
                }
            }
        }
    }
}
