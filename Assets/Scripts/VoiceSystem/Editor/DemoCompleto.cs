using UnityEditor;
using UnityEngine;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class DemoCompleto
    {
        [MenuItem("Tools/VoiceSystem/DEMO COMPLETO")]
        public static void RunDemoCompleto()
        {
            Debug.Log("=== DEMO COMPLETO DEL SISTEMA DE VOZ REAL ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("❌ VoiceSystemManager no encontrado. Ejecuta 'Setup Windows Voice System' primero.");
                return;
            }
            
            if (!manager.IsInitialized)
            {
                Debug.Log("🔄 Inicializando sistema...");
                manager.Initialize();
            }
            
            if (manager.IsInitialized)
            {
                Debug.Log("✅ Sistema inicializado correctamente");
                
                // Hablar mensaje de bienvenida completo
                string welcomeMessage = "¡Hola! Soy tu asistente de voz para el juego Haunted Blind. " +
                    "Estoy listo para ayudarte. " +
                    "Puedes decir comandos como: adelante, atrás, izquierda, derecha, inspeccionar, tomar, usar, comer, dar, leer, información, o ayuda. " +
                    "Di 'información' en cualquier momento para saber dónde estás y qué puedes hacer. " +
                    "Empezaré a escuchar en unos segundos.";
                
                manager.SpeakToPlayer(welcomeMessage);
                
                Debug.Log("🎤 Comandos disponibles:");
                Debug.Log("   - 'información' - Obtener contexto completo (dónde estás, qué tienes, qué puedes hacer)");
                Debug.Log("   - 'adelante' - Mover hacia adelante");
                Debug.Log("   - 'atrás' - Mover hacia atrás");
                Debug.Log("   - 'izquierda' - Girar a la izquierda");
                Debug.Log("   - 'derecha' - Girar a la derecha");
                Debug.Log("   - 'inspeccionar' - Inspeccionar el área");
                Debug.Log("   - 'tomar' - Tomar un objeto");
                Debug.Log("   - 'usar' - Usar un objeto");
                Debug.Log("   - 'comer' - Comer algo");
                Debug.Log("   - 'dar' - Dar un objeto");
                Debug.Log("   - 'leer' - Leer algo");
                Debug.Log("   - 'ayuda' - Obtener ayuda");
                
                // Esperar 12 segundos (tiempo para que termine el mensaje) y empezar a escuchar
                EditorApplication.delayCall += () =>
                {
                    System.Threading.Thread.Sleep(12000);
                    
                    if (!manager.IsListening)
                    {
                        manager.StartListening();
                        Debug.Log("🎤 Sistema escuchando tu voz AHORA - Puedes hablar");
                    }
                };
            }
            else
            {
                Debug.LogError("❌ Sistema no se pudo inicializar");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Detener Todo")]
        public static void DetenerTodo()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null)
            {
                manager.StopListening();
                manager.textToSpeech?.Stop();
                Debug.Log("✅ Todo detenido");
            }
        }
    }
}
