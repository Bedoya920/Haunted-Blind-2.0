using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddDiagnosticoCompleto
{
    [MenuItem("Tools/VoiceSystem/Add Diagnostico Completo")]
    public static void AddComponent()
    {
        var gameManager = GameObject.Find("GameManager");
        if (gameManager == null)
        {
            Debug.LogError("[Editor] GameManager no encontrado");
            return;
        }
        
        var existing = gameManager.GetComponent<DiagnosticoCompleto>();
        if (existing != null)
        {
            Debug.LogWarning("[Editor] DiagnosticoCompleto ya existe");
            Selection.activeGameObject = gameManager;
            return;
        }
        
        gameManager.AddComponent<DiagnosticoCompleto>();
        Debug.Log("[Editor] ✅ DiagnosticoCompleto añadido");
        
        EditorUtility.SetDirty(gameManager);
        EditorSceneManager.MarkSceneDirty(gameManager.scene);
        Selection.activeGameObject = gameManager;
    }
}

