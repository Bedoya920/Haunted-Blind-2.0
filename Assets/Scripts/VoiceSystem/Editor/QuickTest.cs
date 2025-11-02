using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class QuickTest
    {
        [MenuItem("Tools/VoiceSystem/Quick Test")]
        public static void RunQuickTest()
        {
            Debug.Log("=== PRUEBA RÁPIDA DEL SISTEMA ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            Debug.Log($"✅ Manager encontrado - Inicializado: {manager.IsInitialized}");
            
            if (manager.IsInitialized)
            {
                Debug.Log("🔊 Probando TTS...");
                manager.SpeakToPlayer("Sistema funcionando. Habla comandos como adelante, atrás, ayuda.");
                Debug.Log("✅ TTS ejecutado");
            }
            else
            {
                Debug.Log("⚠️ Inicializando sistema...");
                manager.Initialize();
                Debug.Log($"✅ Sistema inicializado: {manager.IsInitialized}");
            }
        }
    }
}
