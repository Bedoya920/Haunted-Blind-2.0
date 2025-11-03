using UnityEditor;
using UnityEngine;

namespace VoiceSystem.Editor
{
    public static class QuickFixGame
    {
        [MenuItem("Tools/VoiceSystem/QUICK FIX - Preparar Juego Completo")]
        public static void QuickFixEverything()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Advertencia", "Sal de Play Mode primero", "OK");
                return;
            }
            
            Debug.Log("=== QUICK FIX - CONFIGURANDO TODO ===");
            
            // 1. Verificar/Crear ScriptableObjects
            CreateScriptableObjectsIfNeeded();
            
            // 2. Asignar a Singletons en escena
            AssignToSceneSystems();
            
            // 3. Guardar
            AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            
            Debug.Log("=== QUICK FIX COMPLETADO ===");
            
            EditorUtility.DisplayDialog(
                "Juego Configurado",
                "✅ TODOS LOS SISTEMAS CONFIGURADOS\n\n" +
                "Configuración aplicada:\n" +
                "• TimerData: 10 minutos, eventos cada 60s\n" +
                "• PlayerLivesData: 5 vidas iniciales\n" +
                "• ConsumibleData: Sistema de comida\n\n" +
                "IMPORTANTE - Verifica Windows Speech:\n" +
                "1. Configuración de Windows\n" +
                "2. Privacidad → Voz\n" +
                "3. Activa 'Reconocimiento de voz en línea'\n" +
                "4. Activa 'Conocerme' en 'Entradas de lápiz y escritura'\n\n" +
                "Ahora PRESIONA PLAY y di comandos como:\n" +
                "• información\n" +
                "• qué puertas hay\n" +
                "• buscar\n\n" +
                "Si no detecta tu voz, revisa configuración de Windows.",
                "Entendido"
            );
        }
        
        private static void CreateScriptableObjectsIfNeeded()
        {
            // TimerData
            var timerData = AssetDatabase.LoadAssetAtPath<TimerData>("Assets/Data/TimerData.asset");
            if (timerData == null)
            {
                timerData = ScriptableObject.CreateInstance<TimerData>();
                timerData.totalDuration = 600f;  // 10 minutos
                timerData.interval = 60f;        // Evento cada minuto
                AssetDatabase.CreateAsset(timerData, "Assets/Data/TimerData.asset");
                Debug.Log("✅ TimerData creado");
            }
            else
            {
                Debug.Log("✅ TimerData ya existe");
            }
            
            // PlayerLivesData
            var playerLives = AssetDatabase.LoadAssetAtPath<PlayerLivesData>("Assets/Data/PlayerLivesData.asset");
            if (playerLives == null)
            {
                playerLives = ScriptableObject.CreateInstance<PlayerLivesData>();
                playerLives.totalLives = 5;
                playerLives.currentLives = 5;
                AssetDatabase.CreateAsset(playerLives, "Assets/Data/PlayerLivesData.asset");
                Debug.Log("✅ PlayerLivesData creado");
            }
            else
            {
                // Asegurar valores correctos
                playerLives.totalLives = 5;
                playerLives.currentLives = 5;
                EditorUtility.SetDirty(playerLives);
                Debug.Log("✅ PlayerLivesData actualizado a 5 vidas");
            }
            
            // ConsumibleData
            var consumibleData = AssetDatabase.LoadAssetAtPath<ConsumibleData>("Assets/Data/ConsumibleData.asset");
            if (consumibleData == null)
            {
                consumibleData = ScriptableObject.CreateInstance<ConsumibleData>();
                consumibleData.cantidadConsumibles = 0; // Se sincroniza con RoomInventory
                consumibleData.vidasQueDevuelve = 1;
                AssetDatabase.CreateAsset(consumibleData, "Assets/Data/ConsumibleData.asset");
                Debug.Log("✅ ConsumibleData creado");
            }
            else
            {
                Debug.Log("✅ ConsumibleData ya existe");
            }
        }
        
        private static void AssignToSceneSystems()
        {
            // GameTimer
            var gameTimer = Object.FindFirstObjectByType<GameTimer>();
            if (gameTimer != null)
            {
                var timerData = AssetDatabase.LoadAssetAtPath<TimerData>("Assets/Data/TimerData.asset");
                var so = new SerializedObject(gameTimer);
                so.FindProperty("timerSettings").objectReferenceValue = timerData;
                so.ApplyModifiedProperties();
                Debug.Log("✅ GameTimer configurado");
            }
            
            // FatigueSystem
            var fatigueSystem = Object.FindFirstObjectByType<FatigueSystem>();
            if (fatigueSystem != null)
            {
                var playerLives = AssetDatabase.LoadAssetAtPath<PlayerLivesData>("Assets/Data/PlayerLivesData.asset");
                var so = new SerializedObject(fatigueSystem);
                so.FindProperty("playerLives").objectReferenceValue = playerLives;
                so.ApplyModifiedProperties();
                Debug.Log("✅ FatigueSystem configurado");
            }
            
            // ConsumiblesManager
            var consumiblesManager = Object.FindFirstObjectByType<ConsumiblesManager>();
            if (consumiblesManager != null)
            {
                var consumibleData = AssetDatabase.LoadAssetAtPath<ConsumibleData>("Assets/Data/ConsumibleData.asset");
                var so = new SerializedObject(consumiblesManager);
                so.FindProperty("consumibleData").objectReferenceValue = consumibleData;
                so.ApplyModifiedProperties();
                Debug.Log("✅ ConsumiblesManager configurado");
            }
        }
    }
}


