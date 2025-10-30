using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class SystemRepair
    {
        [MenuItem("Tools/VoiceSystem/Repair System")]
        public static void RepairSystem()
        {
            Debug.Log("=== REPARANDO SISTEMA DE VOZ ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            // Step 1: Disable auto start
            manager.autoStartListening = false;
            Debug.Log("✅ Auto start deshabilitado");
            
            // Step 2: Stop everything
            manager.StopListening();
            if (manager.textToSpeech != null)
            {
                manager.textToSpeech.Stop();
            }
            Debug.Log("✅ Todo detenido");
            
            // Step 3: Force reinitialize
            manager.Initialize();
            Debug.Log($"✅ Sistema reinicializado: {manager.IsInitialized}");
            
            // Step 4: Test TTS
            if (manager.IsInitialized)
            {
                manager.SpeakToPlayer("Sistema reparado. Para empezar a escuchar, usa 'Start Listening' manualmente.");
                Debug.Log("✅ TTS funcionando");
            }
            
            Debug.Log("=== REPARACIÓN COMPLETA ===");
        }
        
        [MenuItem("Tools/VoiceSystem/Start Listening Manually")]
        public static void StartListeningManually()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null && manager.IsInitialized)
            {
                manager.StartListening();
                Debug.Log("✅ Escuchando iniciado manualmente");
            }
            else
            {
                Debug.LogError("❌ Sistema no inicializado");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Stop Listening")]
        public static void StopListening()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null)
            {
                manager.StopListening();
                Debug.Log("✅ Escuchando detenido");
            }
        }
    }
}
