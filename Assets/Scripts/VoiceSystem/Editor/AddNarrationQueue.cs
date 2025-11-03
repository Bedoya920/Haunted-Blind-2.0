using UnityEngine;
using UnityEditor;
using VoiceSystem.GameIntegration;

namespace VoiceSystem.Editor
{
    public class AddNarrationQueue : UnityEditor.Editor
    {
        [MenuItem("Tools/VoiceSystem/Add Narration Queue")]
        public static void AddNarrationQueueComponent()
        {
            var gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                Debug.LogError("[Editor] GameManager no encontrado en la escena");
                return;
            }

            // Verificar si ya existe
            var existing = gameManager.GetComponent<NarrationQueue>();
            if (existing != null)
            {
                Debug.LogWarning("[Editor] NarrationQueue ya existe en GameManager");
                return;
            }

            // Añadir componente
            var narrationQueue = gameManager.AddComponent<NarrationQueue>();
            Debug.Log("[Editor] ✅ NarrationQueue añadido a GameManager");

            // Marcar como modificado
            EditorUtility.SetDirty(gameManager);
        }
    }
}

