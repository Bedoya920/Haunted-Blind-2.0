using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class ForceInitialize
    {
        [MenuItem("Tools/VoiceSystem/Force Initialize System")]
        public static void ForceInitializeSystem()
        {
            Debug.Log("=== FORZANDO INICIALIZACIÓN DEL SISTEMA ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            // Force stop everything first
            Debug.Log("🛑 Deteniendo todo...");
            manager.StopListening();
            if (manager.textToSpeech != null)
            {
                manager.textToSpeech.Stop();
            }
            
            // Wait a moment
            System.Threading.Thread.Sleep(1000);
            
            // Force reinitialize
            Debug.Log("🔄 Reinicializando sistema...");
            manager.Initialize();
            
            Debug.Log($"✅ Sistema reinicializado - Inicializado: {manager.IsInitialized}");
            
            if (manager.IsInitialized)
            {
                Debug.Log("🔊 Probando TTS...");
                manager.SpeakToPlayer("Sistema reinicializado correctamente. Ahora puedes hablar comandos.");
                Debug.Log("✅ Sistema funcionando");
            }
            else
            {
                Debug.LogError("❌ Sistema no se pudo inicializar");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Disable Auto Start")]
        public static void DisableAutoStart()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null)
            {
                manager.autoStartListening = false;
                Debug.Log("✅ Auto start deshabilitado");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Enable Auto Start")]
        public static void EnableAutoStart()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null)
            {
                manager.autoStartListening = true;
                Debug.Log("✅ Auto start habilitado");
            }
        }
    }
}
