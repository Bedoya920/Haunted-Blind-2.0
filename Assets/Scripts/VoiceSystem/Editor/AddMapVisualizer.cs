using UnityEngine;
using UnityEditor;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Herramienta de editor para añadir MapVisualizer al GameManager
    /// </summary>
    public class AddMapVisualizer
    {
        [MenuItem("Tools/VoiceSystem/Add Map Visualizer", priority = 999)]
        public static void AddMapVisualizerToGameManager()
        {
            GameObject gameManager = GameObject.Find("GameManager");
            
            if (gameManager == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "No se encontró el GameManager. Por favor, abre la escena GameScene primero.", 
                    "OK");
                return;
            }
            
            // Verificar si ya tiene MapVisualizer
            MapVisualizer existing = gameManager.GetComponent<MapVisualizer>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Info", 
                    "El GameManager ya tiene un MapVisualizer.", 
                    "OK");
                return;
            }
            
            // Añadir componente
            MapVisualizer visualizer = gameManager.AddComponent<MapVisualizer>();
            
            // Auto-asignar referencias
            visualizer.GetType().GetField("roomGenerator", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(visualizer, gameManager.GetComponent<RoomGenerator3000>());
            
            // Marcar escena como modificada
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            
            Debug.Log("[AddMapVisualizer] ✅ MapVisualizer añadido al GameManager correctamente!");
            EditorUtility.DisplayDialog("Éxito", 
                "MapVisualizer añadido al GameManager.\n\nAhora verás el mapa visual con la posición del jugador cuando juegues.", 
                "OK");
        }
    }
}

