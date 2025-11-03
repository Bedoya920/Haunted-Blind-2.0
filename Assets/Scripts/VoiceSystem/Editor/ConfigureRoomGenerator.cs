using UnityEngine;
using UnityEditor;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Configura el RoomGenerator con valores seguros
    /// </summary>
    public class ConfigureRoomGenerator
    {
        [MenuItem("Tools/VoiceSystem/Configure Room Generator (Safe Values)", priority = 100)]
        public static void ConfigureSafeValues()
        {
            var generator = GameObject.FindFirstObjectByType<RoomGenerator3000>();
            
            if (generator == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "No se encontró RoomGenerator3000 en la escena.\n\nAsegúrate de abrir GameScene primero.", 
                    "OK");
                return;
            }
            
            // Configurar valores seguros
            generator.numeroHabitaciones = 8; // Valor conservador que siempre funciona
            
            // Marcar escena como modificada
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            
            Debug.Log("[ConfigureRoomGenerator] ✅ RoomGenerator configurado con 8 habitaciones (valor seguro)");
            EditorUtility.DisplayDialog("Éxito", 
                "RoomGenerator configurado con 8 habitaciones.\n\nEste es un valor seguro que garantiza generación exitosa.", 
                "OK");
        }
        
        [MenuItem("Tools/VoiceSystem/Test Room Generation", priority = 101)]
        public static void TestGeneration()
        {
            var generator = GameObject.FindFirstObjectByType<RoomGenerator3000>();
            
            if (generator == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "No se encontró RoomGenerator3000 en la escena.", 
                    "OK");
                return;
            }
            
            if (generator.numeroHabitaciones == 0)
            {
                generator.numeroHabitaciones = 8;
            }
            
            Debug.Log($"[Test] Generando casa con {generator.numeroHabitaciones} habitaciones...");
            generator.Iniciar();
            
            if (generator.casa != null && generator.casa.habitaciones.Count > 0)
            {
                Debug.Log($"[Test] ✅ Generación EXITOSA: {generator.casa.habitaciones.Count} habitaciones, {generator.casa.puertas.Count} puertas");
                EditorUtility.DisplayDialog("Éxito", 
                    $"Casa generada correctamente:\n\n• {generator.casa.habitaciones.Count} habitaciones\n• {generator.casa.puertas.Count} puertas\n\nRevisa la Console para más detalles.", 
                    "OK");
            }
            else
            {
                Debug.LogError("[Test] ❌ Generación FALLÓ - casa es null o sin habitaciones");
                EditorUtility.DisplayDialog("Error", 
                    "La generación falló.\n\nRevisa la Console para detalles.", 
                    "OK");
            }
        }
    }
}

