using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class FinalDemo
    {
        [MenuItem("Tools/VoiceSystem/Final Demo")]
        public static void RunFinalDemo()
        {
            Debug.Log("=== DEMO FINAL DEL SISTEMA DE VOZ ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            Debug.Log("🔧 Configurando sistema...");
            
            // Disable auto start to prevent conflicts
            manager.autoStartListening = false;
            
            // Initialize system
            manager.Initialize();
            Debug.Log($"✅ Sistema inicializado: {manager.IsInitialized}");
            
            if (manager.IsInitialized)
            {
                Debug.Log("🔊 Probando síntesis de voz...");
                manager.SpeakToPlayer("¡Hola! Soy tu asistente de voz. El sistema está funcionando correctamente.");
                
                Debug.Log("🎤 Para probar reconocimiento de voz:");
                Debug.Log("   1. Ve a Tools/VoiceSystem/Start Listening Manually");
                Debug.Log("   2. Habla comandos como: adelante, atrás, izquierda, derecha, ayuda");
                Debug.Log("   3. El sistema te responderá con voz real");
                
                Debug.Log("✅ DEMO COMPLETO - Sistema funcionando");
            }
            else
            {
                Debug.LogError("❌ Sistema no se pudo inicializar");
            }
        }
    }
}
