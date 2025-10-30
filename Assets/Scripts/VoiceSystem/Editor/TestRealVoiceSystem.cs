using UnityEngine;
using UnityEditor;
using VoiceSystem.Core;
using VoiceSystem.Recognition;
using VoiceSystem.Synthesis;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Script para probar el sistema de voz real
    /// </summary>
    public static class TestRealVoiceSystem
    {
        [MenuItem("Tools/VoiceSystem/Test Real Voice System")]
        public static void RunTestRealVoiceSystem()
        {
            Debug.Log("=== PROBANDO SISTEMA DE VOZ REAL ===");
            
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog(
                    "Error", 
                    "VoiceSystemManager no encontrado.\n\n" +
                    "Ejecuta: Tools > VoiceSystem > Setup Windows Voice System", 
                    "OK"
                );
                return;
            }
            
            // Verificar componentes
            var recognizer = manager.GetComponent<WindowsSpeechRecognizer>();
            var tts = manager.GetComponent<UnityTextToSpeech>();
            
            if (recognizer == null || tts == null)
            {
                EditorUtility.DisplayDialog(
                    "Componentes Faltantes", 
                    "Faltan componentes reales:\n\n" +
                    $"WindowsSpeechRecognizer: {(recognizer != null ? "✅" : "❌")}\n" +
                    $"UnityTextToSpeech: {(tts != null ? "✅" : "❌")}\n\n" +
                    "Ejecuta: Tools > VoiceSystem > Setup Windows Voice System", 
                    "OK"
                );
                return;
            }
            
            // Probar TTS
            if (tts != null)
            {
                tts.Speak("Sistema de voz real funcionando. Ahora puedes hablar y te escucharé.", VoiceSystem.Core.Interfaces.TTSPriority.Normal);
                Debug.Log("✅ TTS real probado");
            }
            
            // Iniciar reconocimiento
            if (recognizer != null)
            {
                recognizer.StartListening();
                Debug.Log("✅ Reconocimiento real iniciado - habla ahora");
            }
            
            EditorUtility.DisplayDialog(
                "Sistema Real Funcionando", 
                "El sistema de voz real está funcionando:\n\n" +
                "✅ WindowsSpeechRecognizer - Escuchando tu voz\n" +
                "✅ UnityTextToSpeech - Hablando de verdad\n\n" +
                "AHORA PUEDES:\n" +
                "• Hablar al micrófono - te escuchará\n" +
                "• El sistema responderá con voz real\n" +
                "• Los comandos se ejecutarán en el juego\n\n" +
                "¡Prueba hablando ahora!", 
                "¡Perfecto!"
            );
        }
        
        [MenuItem("Tools/VoiceSystem/Stop Voice System")]
        public static void StopVoiceSystem()
        {
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager != null)
            {
                var recognizer = manager.GetComponent<WindowsSpeechRecognizer>();
                var tts = manager.GetComponent<UnityTextToSpeech>();
                
                if (recognizer != null)
                {
                    recognizer.StopListening();
                    Debug.Log("✅ Reconocimiento detenido");
                }
                
                if (tts != null)
                {
                    tts.Stop();
                    Debug.Log("✅ TTS detenido");
                }
                
                Debug.Log("✅ Sistema de voz detenido");
            }
        }
    }
}
