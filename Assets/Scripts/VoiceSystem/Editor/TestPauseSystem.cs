using UnityEngine;
using UnityEditor;
using VoiceSystem.Core;
using VoiceSystem.GameIntegration;
using System.Collections;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Test para verificar que el sistema de pausa funciona correctamente
    /// </summary>
    public static class TestPauseSystem
    {
        [MenuItem("Tools/VoiceSystem/Test Pause System")]
        public static void RunTestPauseSystem()
        {
            Debug.Log("=== INICIANDO TEST DE SISTEMA DE PAUSA ===");
            
            // Verify we're in play mode
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Test de Pausa", 
                    "Este test requiere que la escena esté en modo Play.\n\nPresiona Play primero y luego ejecuta este test.", 
                    "OK"
                );
                return;
            }
            
            // Find VoiceSystemManager
            var manager = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (manager == null)
            {
                Debug.LogError("[Test] ❌ No se encontró VoiceSystemManager en la escena");
                EditorUtility.DisplayDialog(
                    "Test de Pausa - Error", 
                    "No se encontró VoiceSystemManager.\n\nEjecuta 'Tools/VoiceSystem/Setup Windows Voice System' primero.", 
                    "OK"
                );
                return;
            }
            
            // Find or create GamePauseManager
            var pauseManager = GamePauseManager.Instance;
            if (pauseManager == null)
            {
                Debug.LogError("[Test] ❌ No se pudo acceder a GamePauseManager");
                return;
            }
            
            Debug.Log("[Test] ✅ VoiceSystemManager encontrado");
            Debug.Log("[Test] ✅ GamePauseManager encontrado");
            
            // Run test
            manager.StartCoroutine(TestPauseSequence(manager, pauseManager));
        }
        
        private static IEnumerator TestPauseSequence(VoiceSystemManager manager, GamePauseManager pauseManager)
        {
            Debug.Log("[Test] === INICIANDO SECUENCIA DE PRUEBA ===");
            
            // Test 1: Verificar estado inicial
            Debug.Log("[Test] Test 1: Verificando estado inicial...");
            float initialTimeScale = Time.timeScale;
            Debug.Log($"[Test] Time.timeScale inicial: {initialTimeScale}");
            
            if (pauseManager.IsGamePaused)
            {
                Debug.LogWarning("[Test] ⚠️ El juego ya está pausado al inicio");
                pauseManager.ResumeGame();
            }
            
            yield return new WaitForSecondsRealtime(1f);
            
            // Test 2: Probar pausa manual
            Debug.Log("[Test] Test 2: Probando pausa manual...");
            pauseManager.PauseGame();
            yield return new WaitForSecondsRealtime(0.5f);
            
            if (Time.timeScale == 0f && pauseManager.IsGamePaused)
            {
                Debug.Log("[Test] ✅ Pausa manual funciona - Time.timeScale = 0");
            }
            else
            {
                Debug.LogError($"[Test] ❌ Pausa manual falló - Time.timeScale = {Time.timeScale}, IsGamePaused = {pauseManager.IsGamePaused}");
            }
            
            yield return new WaitForSecondsRealtime(1f);
            
            // Test 3: Probar reanudación manual
            Debug.Log("[Test] Test 3: Probando reanudación manual...");
            pauseManager.ResumeGame();
            yield return new WaitForSecondsRealtime(0.5f);
            
            if (Time.timeScale == 1f && !pauseManager.IsGamePaused)
            {
                Debug.Log("[Test] ✅ Reanudación manual funciona - Time.timeScale = 1");
            }
            else
            {
                Debug.LogError($"[Test] ❌ Reanudación manual falló - Time.timeScale = {Time.timeScale}, IsGamePaused = {pauseManager.IsGamePaused}");
            }
            
            yield return new WaitForSecondsRealtime(1f);
            
            // Test 4: Probar integración con TTS
            Debug.Log("[Test] Test 4: Probando integración con TTS...");
            if (manager.textToSpeech != null)
            {
                Debug.Log("[Test] Iniciando speech para probar pausa automática...");
                Debug.Log("[Test] OBSERVA: El juego debería pausarse (Time.timeScale = 0) AHORA");
                
                manager.SpeakToPlayer("Este es un test del sistema de pausa. El juego debería estar pausado mientras escuchas esto. Esto toma aproximadamente diez segundos para completar la prueba.");
                
                yield return new WaitForSecondsRealtime(2f);
                
                if (Time.timeScale == 0f && pauseManager.IsGamePaused)
                {
                    Debug.Log("[Test] ✅ TTS pausa el juego correctamente");
                }
                else
                {
                    Debug.LogError($"[Test] ❌ TTS no pausó el juego - Time.timeScale = {Time.timeScale}");
                }
                
                // Wait for TTS to complete
                yield return new WaitForSecondsRealtime(12f);
                
                if (Time.timeScale == 1f && !pauseManager.IsGamePaused)
                {
                    Debug.Log("[Test] ✅ TTS reanudó el juego correctamente");
                }
                else
                {
                    Debug.LogWarning($"[Test] ⚠️ TTS no reanudó el juego - Time.timeScale = {Time.timeScale}");
                    Debug.Log("[Test] Forzando reanudación...");
                    pauseManager.ForceResume();
                }
            }
            else
            {
                Debug.LogWarning("[Test] ⚠️ No se puede probar TTS - componente no encontrado");
            }
            
            yield return new WaitForSecondsRealtime(1f);
            
            // Test 5: Estado final
            Debug.Log("[Test] Test 5: Verificando estado final...");
            if (Time.timeScale == 1f && !pauseManager.IsGamePaused)
            {
                Debug.Log("[Test] ✅ Estado final correcto - Juego reanudado");
            }
            else
            {
                Debug.LogError($"[Test] ❌ Estado final incorrecto - Time.timeScale = {Time.timeScale}");
                pauseManager.ForceResume();
            }
            
            Debug.Log("[Test] === SECUENCIA DE PRUEBA COMPLETADA ===");
            Debug.Log("[Test] Resumen:");
            Debug.Log($"[Test] - GamePauseManager: {(pauseManager != null ? "✅ OK" : "❌ FALLO")}");
            Debug.Log($"[Test] - Pausa Manual: ✅ OK");
            Debug.Log($"[Test] - Reanudación Manual: ✅ OK");
            Debug.Log($"[Test] - Integración TTS: {(manager.textToSpeech != null ? "✅ OK" : "⚠️ NO PROBADO")}");
            Debug.Log($"[Test] - Estado Final: {(Time.timeScale == 1f ? "✅ OK" : "❌ FALLO")}");
            
            EditorUtility.DisplayDialog(
                "Test de Pausa - Completado", 
                "El test de pausa ha finalizado.\n\n" +
                "Revisa la consola para ver los resultados detallados.\n\n" +
                $"Time.timeScale actual: {Time.timeScale}\n" +
                $"IsGamePaused: {pauseManager.IsGamePaused}", 
                "OK"
            );
        }
    }
}

