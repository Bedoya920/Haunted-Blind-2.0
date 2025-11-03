using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace VoiceSystem.Editor
{
    public class RegenerarMapaCampana
    {
        [MenuItem("Tools/VoiceSystem/Regenerar Mapa de Campaña")]
        public static void Regenerar()
        {
            Debug.Log("=== REGENERANDO MAPA DE CAMPAÑA ===");
            
            // Buscar RoomGenerator3000
            var roomGenerator = Object.FindFirstObjectByType<RoomGenerator3000>();
            if (roomGenerator == null)
            {
                Debug.LogError("[Editor] RoomGenerator3000 no encontrado en la escena");
                return;
            }
            
            Debug.Log("[Editor] 1. Generando mapa de campaña...");
            roomGenerator.GenerarMapaCampana();
            
            if (roomGenerator.casa == null)
            {
                Debug.LogError("[Editor] Error: casa es null después de generar");
                return;
            }
            
            Debug.Log($"[Editor] ✅ Mapa generado: {roomGenerator.casa.habitaciones.Count} habitaciones, {roomGenerator.casa.puertas.Count} puertas");
            
            Debug.Log("[Editor] 2. Guardando a JSON...");
            roomGenerator.GuardarCasaJson();
            
            Debug.Log("[Editor] ✅ MAPA REGENERADO Y GUARDADO");
            Debug.Log("[Editor] Presiona Play para probar el nuevo mapa");
            
            EditorUtility.SetDirty(roomGenerator);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}

