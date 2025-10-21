using UnityEditor;
using UnityEngine;
using VoiceSystem.Synthesis;

namespace VoiceSystem.Editor
{
    public static class TestRealTTS
    {
        [MenuItem("Tools/VoiceSystem/Test REAL TTS")]
        public static void TestTTS()
        {
            Debug.Log("=== PROBANDO TTS REAL CON POWERSHELL ===");
            
            // Crear GameObject temporal
            var go = new GameObject("TTS_Test");
            var tts = go.AddComponent<WindowsTTSPlugin>();
            tts.Initialize();
            
            // Hablar
            tts.Speak("Hola, esta es una prueba de síntesis de voz real usando Windows SAPI. Si escuchas esto, el sistema está funcionando correctamente.");
            
            Debug.Log("✅ TTS ejecutado - Deberías escuchar audio ahora");
            
            // Destruir después de 10 segundos
            EditorApplication.delayCall += () =>
            {
                System.Threading.Thread.Sleep(10000);
                Object.DestroyImmediate(go);
                Debug.Log("✅ Test completado");
            };
        }
    }
}
