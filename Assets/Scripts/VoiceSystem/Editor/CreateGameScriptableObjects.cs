using UnityEditor;
using UnityEngine;

namespace VoiceSystem.Editor
{
    public static class CreateGameScriptableObjects
    {
        [MenuItem("Tools/VoiceSystem/Create Game ScriptableObjects")]
        public static void CreateAllGameSOs()
        {
            Debug.Log("=== CREANDO SCRIPTABLEOBJECTS DEL JUEGO ===");
            
            // 1. TimerData
            CreateTimerData();
            
            // 2. PlayerLivesData
            CreatePlayerLivesData();
            
            // 3. ConsumibleData
            CreateConsumibleData();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("=== SCRIPTABLEOBJECTS CREADOS ===");
            
            EditorUtility.DisplayDialog(
                "ScriptableObjects Creados",
                "✅ TimerData creado en Resources/SO/\n" +
                "✅ PlayerLivesData creado en Assets/Data/\n" +
                "✅ ConsumibleData creado en Assets/Data/\n\n" +
                "Ahora ejecuta:\n" +
                "Tools → VoiceSystem → Assign Game ScriptableObjects\n\n" +
                "Para asignarlos a los sistemas automáticamente.",
                "OK"
            );
        }
        
        private static void CreateTimerData()
        {
            string path = "Assets/Resources/SO/TimerData.asset";
            
            // Verificar si ya existe
            var existing = AssetDatabase.LoadAssetAtPath<TimerData>(path);
            if (existing != null)
            {
                Debug.Log("✅ TimerData ya existe");
                return;
            }
            
            // Crear carpeta si no existe
            if (!AssetDatabase.IsValidFolder("Assets/Resources/SO"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "SO");
            }
            
            // Crear asset
            var timerData = ScriptableObject.CreateInstance<TimerData>();
            timerData.totalDuration = 600f;  // 10 minutos
            timerData.interval = 60f;        // Evento cada 1 minuto
            
            AssetDatabase.CreateAsset(timerData, path);
            Debug.Log($"✅ TimerData creado: {path}");
        }
        
        private static void CreatePlayerLivesData()
        {
            string path = "Assets/Data/PlayerLivesData.asset";
            
            // Verificar si ya existe
            var existing = AssetDatabase.LoadAssetAtPath<PlayerLivesData>(path);
            if (existing != null)
            {
                Debug.Log("✅ PlayerLivesData ya existe");
                return;
            }
            
            // Crear carpeta si no existe
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }
            
            // Crear asset
            var playerLives = ScriptableObject.CreateInstance<PlayerLivesData>();
            playerLives.totalLives = 5;
            playerLives.currentLives = 5;
            
            AssetDatabase.CreateAsset(playerLives, path);
            Debug.Log($"✅ PlayerLivesData creado: {path}");
        }
        
        private static void CreateConsumibleData()
        {
            string path = "Assets/Data/ConsumibleData.asset";
            
            // Verificar si ya existe
            var existing = AssetDatabase.LoadAssetAtPath<ConsumibleData>(path);
            if (existing != null)
            {
                Debug.Log("✅ ConsumibleData ya existe");
                return;
            }
            
            // Crear carpeta si no existe
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }
            
            // Crear asset
            var consumible = ScriptableObject.CreateInstance<ConsumibleData>();
            consumible.cantidadConsumibles = 3; // Empezar con 3 consumibles
            consumible.vidasQueDevuelve = 1;   // Cada consumible da 1 vida
            
            AssetDatabase.CreateAsset(consumible, path);
            Debug.Log($"✅ ConsumibleData creado: {path}");
        }
        
        [MenuItem("Tools/VoiceSystem/Assign Game ScriptableObjects")]
        public static void AssignGameSOs()
        {
            Debug.Log("=== ASIGNANDO SCRIPTABLEOBJECTS A SISTEMAS ===");
            
            // 1. Asignar TimerData a GameTimer
            var gameTimer = Object.FindFirstObjectByType<GameTimer>();
            if (gameTimer != null)
            {
                var timerData = AssetDatabase.LoadAssetAtPath<TimerData>("Assets/Resources/SO/TimerData.asset");
                if (timerData != null)
                {
                    var so = new SerializedObject(gameTimer);
                    so.FindProperty("timerSettings").objectReferenceValue = timerData;
                    so.ApplyModifiedProperties();
                    Debug.Log("✅ TimerData asignado a GameTimer");
                }
                else
                {
                    Debug.LogError("❌ TimerData no encontrado");
                }
            }
            
            // 2. Asignar PlayerLivesData a FatigueSystem
            var fatigueSystem = Object.FindFirstObjectByType<FatigueSystem>();
            if (fatigueSystem != null)
            {
                var playerLives = AssetDatabase.LoadAssetAtPath<PlayerLivesData>("Assets/Data/PlayerLivesData.asset");
                if (playerLives != null)
                {
                    var so = new SerializedObject(fatigueSystem);
                    so.FindProperty("playerLives").objectReferenceValue = playerLives;
                    so.ApplyModifiedProperties();
                    Debug.Log("✅ PlayerLivesData asignado a FatigueSystem");
                }
                else
                {
                    Debug.LogError("❌ PlayerLivesData no encontrado");
                }
            }
            
            // 3. Asignar ConsumibleData a ConsumiblesManager
            var consumiblesManager = Object.FindFirstObjectByType<ConsumiblesManager>();
            if (consumiblesManager != null)
            {
                var consumibleData = AssetDatabase.LoadAssetAtPath<ConsumibleData>("Assets/Data/ConsumibleData.asset");
                if (consumibleData != null)
                {
                    var so = new SerializedObject(consumiblesManager);
                    so.FindProperty("consumibleData").objectReferenceValue = consumibleData;
                    so.ApplyModifiedProperties();
                    Debug.Log("✅ ConsumibleData asignado a ConsumiblesManager");
                }
                else
                {
                    Debug.LogError("❌ ConsumibleData no encontrado");
                }
            }
            
            Debug.Log("=== ASIGNACIÓN COMPLETADA ===");
            
            EditorUtility.DisplayDialog(
                "Asignación Completa",
                "✅ TimerData asignado a GameTimer\n" +
                "✅ PlayerLivesData asignado a FatigueSystem\n" +
                "✅ ConsumibleData asignado a ConsumiblesManager\n\n" +
                "Ahora presiona Play de nuevo para probar con todos los sistemas configurados.",
                "OK"
            );
        }
    }
}


