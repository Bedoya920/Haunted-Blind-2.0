using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    public static class SetupGameScene
    {
        [MenuItem("Tools/VoiceSystem/Setup Complete Game Scene")]
        public static void SetupCompleteGameScene()
        {
            Debug.Log("=== CONFIGURANDO ESCENA DE JUEGO COMPLETA ===");
            
            if (!Application.isPlaying)
            {
                // Crear nueva escena
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                scene.name = "GameScene";
                
                Debug.Log($"✅ Escena creada: {scene.name}");
            }
            else
            {
                Debug.LogWarning("[SetupGameScene] No se puede configurar en Play Mode. Sal de Play Mode primero.");
                EditorUtility.DisplayDialog("Advertencia", "Sal de Play Mode para configurar la escena.", "OK");
                return;
            }
            
            try
            {
                // 1. GameManager con GameInitializer y RoomGenerator
                var gameManager = Object.FindFirstObjectByType<GameInitializer>();
                if (gameManager == null)
                {
                    var gmGO = new GameObject("GameManager");
                    gameManager = gmGO.AddComponent<GameInitializer>();
                    var roomGen = gmGO.AddComponent<RoomGenerator3000>();
                    
                    // Configurar RoomGenerator
                    roomGen.numeroHabitaciones = 12; // Casa mediana
                    
                    // Asignar referencia
                    var gmSO = new SerializedObject(gameManager);
                    gmSO.FindProperty("roomGenerator").objectReferenceValue = roomGen;
                    gmSO.FindProperty("numeroHabitaciones").intValue = 12;
                    gmSO.FindProperty("delayBeforeStart").floatValue = 3f;
                    gmSO.FindProperty("narrateWelcome").boolValue = true;
                    gmSO.FindProperty("welcomeEventId").intValue = 1;
                    gmSO.ApplyModifiedProperties();
                    
                    Debug.Log("✅ GameManager creado con GameInitializer y RoomGenerator3000");
                }
                else
                {
                    Debug.Log("✅ GameManager ya existe");
                }
                
                // 2. Asegurar Main Camera
                var camera = Camera.main;
                if (camera == null)
                {
                    var camGO = new GameObject("Main Camera");
                    camGO.tag = "MainCamera";
                    camera = camGO.AddComponent<Camera>();
                    camGO.AddComponent<AudioListener>();
                    Debug.Log("✅ Main Camera creada");
                }
                else
                {
                    Debug.Log("✅ Main Camera ya existe");
                }
                
                // 3. Guardar escena
                string scenePath = "Assets/Scenes/GameScene.unity";
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
                
                Debug.Log($"=== ESCENA CONFIGURADA Y GUARDADA EN {scenePath} ===");
                
                EditorUtility.DisplayDialog(
                    "Escena Lista",
                    "✅ GameScene.unity configurada completamente\n\n" +
                    "Componentes creados:\n" +
                    "• GameManager con GameInitializer\n" +
                    "• RoomGenerator3000 (12 habitaciones)\n" +
                    "• Main Camera con AudioListener\n\n" +
                    "Al presionar Play:\n" +
                    "1. Se genera la casa automáticamente\n" +
                    "2. Se narran eventos de inicio\n" +
                    "3. El sistema de voz comienza a escuchar\n" +
                    "4. Puedes empezar a hablar comandos\n\n" +
                    "Sistemas Singletons (se crean automáticamente):\n" +
                    "• VoiceSystemManager\n" +
                    "• RoomSystemBridge\n" +
                    "• EventManager\n" +
                    "• GameTimer, FatigueSystem, etc.\n\n" +
                    "¡LISTO PARA JUGAR!",
                    "¡Perfecto!"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SetupGameScene] Error: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Error configurando escena: {e.Message}", "OK");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Load Game Scene")]
        public static void LoadGameScene()
        {
            string scenePath = "Assets/Scenes/GameScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Debug.Log("✅ GameScene cargada");
            }
            else
            {
                Debug.LogError($"GameScene no existe en {scenePath}. Ejecuta 'Setup Complete Game Scene' primero.");
                EditorUtility.DisplayDialog("Error", "GameScene no existe. Ejecuta 'Setup Complete Game Scene' primero.", "OK");
            }
        }
    }
}

