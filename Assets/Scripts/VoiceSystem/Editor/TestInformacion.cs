using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class TestInformacion
    {
        [MenuItem("Tools/VoiceSystem/Test Comando Información")]
        public static void TestComandoInformacion()
        {
            Debug.Log("=== PROBANDO COMANDO 'INFORMACIÓN' ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado");
                return;
            }
            
            if (!manager.IsInitialized)
            {
                Debug.Log("🔄 Inicializando sistema...");
                manager.Initialize();
            }
            
            if (manager.IsInitialized)
            {
                Debug.Log("✅ Sistema inicializado");
                
                // Simular comando "información"
                Debug.Log("🎤 Simulando comando 'información'...");
                
                var context = manager.GetCurrentContext();
                if (context != null)
                {
                    manager.aiAssistant.ProcessInput("información", context);
                    Debug.Log("✅ Comando procesado - Escucha la respuesta");
                }
                else
                {
                    Debug.LogError("❌ Context provider no disponible");
                }
            }
            else
            {
                Debug.LogError("❌ Sistema no se pudo inicializar");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Test Comando Info")]
        public static void TestComandoInfo()
        {
            Debug.Log("=== PROBANDO COMANDO 'INFO' ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null && manager.IsInitialized)
            {
                var context = manager.GetCurrentContext();
                if (context != null)
                {
                    manager.aiAssistant.ProcessInput("info", context);
                    Debug.Log("✅ Comando 'info' procesado");
                }
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Test Comando Estado")]
        public static void TestComandoEstado()
        {
            Debug.Log("=== PROBANDO COMANDO 'ESTADO' ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null && manager.IsInitialized)
            {
                var context = manager.GetCurrentContext();
                if (context != null)
                {
                    manager.aiAssistant.ProcessInput("estado", context);
                    Debug.Log("✅ Comando 'estado' procesado");
                }
            }
        }
    }
}
