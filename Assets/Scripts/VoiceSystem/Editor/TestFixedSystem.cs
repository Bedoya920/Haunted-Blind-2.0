using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class TestFixedSystem
    {
        [MenuItem("Tools/VoiceSystem/Test Fixed System")]
        public static void RunTestFixedSystem()
        {
            Debug.Log("=== PROBANDO SISTEMA ARREGLADO ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado. Ejecuta 'Setup Windows Voice System' primero.");
                return;
            }
            
            Debug.Log($"✅ VoiceSystemManager encontrado");
            Debug.Log($"✅ Inicializado: {manager.IsInitialized}");
            Debug.Log($"✅ Escuchando: {manager.IsListening}");
            
            if (manager.IsInitialized)
            {
                Debug.Log("✅ Sistema inicializado correctamente");
                
                // Test TTS
                Debug.Log("🔊 Probando TTS...");
                manager.SpeakToPlayer("Sistema de voz funcionando correctamente. Ahora puedes hablar.");
                
                // Test recognition status
                if (manager.speechRecognizer != null)
                {
                    Debug.Log($"✅ Reconocedor: {manager.speechRecognizer.IsInitialized}");
                    Debug.Log($"✅ Escuchando: {manager.speechRecognizer.IsListening}");
                }
                
                Debug.Log("✅ Sistema funcionando - Habla comandos como 'adelante', 'atrás', 'ayuda'");
            }
            else
            {
                Debug.LogError("❌ Sistema no inicializado");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Force Restart System")]
        public static void ForceRestartSystem()
        {
            Debug.Log("=== REINICIANDO SISTEMA FORZADAMENTE ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            // Stop everything
            manager.StopListening();
            if (manager.textToSpeech != null)
            {
                manager.textToSpeech.Stop();
            }
            
            // Wait a moment
            System.Threading.Thread.Sleep(500);
            
            // Reinitialize
            manager.Initialize();
            
            Debug.Log("✅ Sistema reiniciado");
        }
        
        [MenuItem("Tools/VoiceSystem/Check System Health")]
        public static void CheckSystemHealth()
        {
            Debug.Log("=== VERIFICANDO SALUD DEL SISTEMA ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            Debug.Log($"Manager inicializado: {manager.IsInitialized}");
            Debug.Log($"Manager escuchando: {manager.IsListening}");
            
            if (manager.speechRecognizer != null)
            {
                Debug.Log($"Reconocedor inicializado: {manager.speechRecognizer.IsInitialized}");
                Debug.Log($"Reconocedor escuchando: {manager.speechRecognizer.IsListening}");
            }
            else
            {
                Debug.LogError("❌ Reconocedor de voz no encontrado");
            }
            
            if (manager.textToSpeech != null)
            {
                Debug.Log($"TTS hablando: {manager.textToSpeech.IsSpeaking}");
            }
            else
            {
                Debug.LogError("❌ TTS no encontrado");
            }
            
            Debug.Log("=== VERIFICACIÓN COMPLETA ===");
        }
    }
}
