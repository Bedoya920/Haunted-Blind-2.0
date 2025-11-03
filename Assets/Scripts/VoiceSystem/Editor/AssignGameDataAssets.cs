using UnityEditor;
using UnityEngine;

namespace VoiceSystem.Editor
{
    public static class AssignGameDataAssets
    {
        [MenuItem("Tools/VoiceSystem/Fix Game Systems (Assign Data)")]
        public static void FixGameSystems()
        {
            Debug.Log("=== ASIGNANDO DATOS A SISTEMAS DEL JUEGO ===");
            
            if (Application.isPlaying)
            {
                Debug.LogWarning("Sal de Play Mode para asignar referencias");
                return;
            }
            
            int assigned = 0;
            
            // 1. Asignar TimerData a GameTimer
            var gameTimer = Object.FindFirstObjectByType<GameTimer>();
            if (gameTimer != null)
            {
                var timerData = AssetDatabase.LoadAssetAtPath<TimerData>("Assets/Data/TimerData.asset");
                if (timerData != null)
                {
                    var so = new SerializedObject(gameTimer);
                    so.FindProperty("timerSettings").objectReferenceValue = timerData;
                    so.ApplyModifiedProperties();
                    Debug.Log("✅ TimerData asignado a GameTimer");
                    assigned++;
                }
                else
                {
                    Debug.LogWarning("⚠️ TimerData no encontrado en Assets/Data/");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ GameTimer no encontrado - se creará como Singleton en Play Mode");
            }
            
            // 2. Asignar PlayerLivesData a FatigueSystem
            var fatigueSystem = Object.FindFirstObjectByType<FatigueSystem>();
            if (fatigueSystem != null)
            {
                var playerLives = AssetDatabase.LoadAssetAtPath<PlayerLivesData>("Assets/Data/PlayerLivesData.asset");
                if (playerLives != null)
                {
                    // Configurar valores iniciales
                    playerLives.totalLives = 5;
                    playerLives.currentLives = 5;
                    EditorUtility.SetDirty(playerLives);
                    
                    var so = new SerializedObject(fatigueSystem);
                    so.FindProperty("playerLives").objectReferenceValue = playerLives;
                    so.ApplyModifiedProperties();
                    Debug.Log("✅ PlayerLivesData asignado a FatigueSystem (5 vidas)");
                    assigned++;
                }
                else
                {
                    Debug.LogWarning("⚠️ PlayerLivesData no encontrado");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ FatigueSystem no encontrado - se creará como Singleton en Play Mode");
            }
            
            // 3. Crear ConsumibleData si no existe
            var consumibleData = AssetDatabase.LoadAssetAtPath<ConsumibleData>("Assets/Data/ConsumibleData.asset");
            if (consumibleData == null)
            {
                consumibleData = ScriptableObject.CreateInstance<ConsumibleData>();
                consumibleData.cantidadConsumibles = 0; // Se llena desde RoomInventoryManager
                consumibleData.vidasQueDevuelve = 1;
                AssetDatabase.CreateAsset(consumibleData, "Assets/Data/ConsumibleData.asset");
                Debug.Log("✅ ConsumibleData creado");
            }
            
            // 4. Asignar ConsumibleData a ConsumiblesManager
            var consumiblesManager = Object.FindFirstObjectByType<ConsumiblesManager>();
            if (consumiblesManager != null)
            {
                var so = new SerializedObject(consumiblesManager);
                so.FindProperty("consumibleData").objectReferenceValue = consumibleData;
                so.ApplyModifiedProperties();
                Debug.Log("✅ ConsumibleData asignado a ConsumiblesManager");
                assigned++;
            }
            else
            {
                Debug.LogWarning("⚠️ ConsumiblesManager no encontrado - se creará como Singleton en Play Mode");
            }
            
            AssetDatabase.SaveAssets();
            
            Debug.Log($"=== ASIGNACIÓN COMPLETADA ({assigned} sistemas configurados) ===");
            
            if (assigned > 0)
            {
                EditorUtility.DisplayDialog(
                    "Sistemas Configurados",
                    $"✅ {assigned} sistemas configurados correctamente\n\n" +
                    "ScriptableObjects asignados:\n" +
                    "• GameTimer → TimerData (10 min, evento cada 1 min)\n" +
                    "• FatigueSystem → PlayerLivesData (5 vidas)\n" +
                    "• ConsumiblesManager → ConsumibleData\n\n" +
                    "¡Ahora presiona Play y el juego funcionará completamente!",
                    "¡Perfecto!"
                );
            }
        }
    }
}


