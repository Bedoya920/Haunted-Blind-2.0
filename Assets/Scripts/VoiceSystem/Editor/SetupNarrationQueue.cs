using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VoiceSystem.GameIntegration;

namespace VoiceSystem.Editor
{
    public class SetupNarrationQueue
    {
        [MenuItem("Tools/VoiceSystem/Setup Narration Queue")]
        public static void Setup()
        {
            Debug.Log("=== CONFIGURANDO NARRATION QUEUE ===");

            // Buscar GameManager en la escena activa
            var gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                Debug.LogError("[Setup] GameManager no encontrado en la escena. Asegúrate de que la escena GameScene esté abierta.");
                return;
            }

            // Verificar si ya existe NarrationQueue
            var existing = gameManager.GetComponent<NarrationQueue>();
            if (existing != null)
            {
                Debug.LogWarning("[Setup] NarrationQueue ya existe en GameManager");
                Selection.activeGameObject = gameManager;
                return;
            }

            // Añadir NarrationQueue
            var narrationQueue = gameManager.AddComponent<NarrationQueue>();
            Debug.Log("[Setup] ✅ NarrationQueue añadido a GameManager");

            // Marcar escena como modificada
            EditorUtility.SetDirty(gameManager);
            EditorSceneManager.MarkSceneDirty(gameManager.scene);

            // Seleccionar GameManager en el editor
            Selection.activeGameObject = gameManager;

            Debug.Log("=== NARRATION QUEUE CONFIGURADO ===");
            Debug.Log("Guarda la escena para preservar los cambios.");
        }
    }
}

