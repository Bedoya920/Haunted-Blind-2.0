using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using VoiceSystem.GameIntegration;

namespace VoiceSystem.Editor
{
    public class EnableDebugLogs
    {
        [MenuItem("Tools/VoiceSystem/Enable All Debug Logs")]
        public static void EnableAllLogs()
        {
            var gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
            {
                Debug.LogError("[Editor] GameManager no encontrado");
                return;
            }
            
            // DirectionalMovement
            var directional = gameManager.GetComponent<DirectionalMovement>();
            if (directional != null)
            {
                var serializedObject = new SerializedObject(directional);
                var prop = serializedObject.FindProperty("enableDebugLogs");
                if (prop != null)
                {
                    prop.boolValue = true;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log("[Editor] ✅ DirectionalMovement debug habilitado");
                }
            }
            
            // RoomSystemBridge
            var bridge = Object.FindFirstObjectByType<RoomSystemBridge>();
            if (bridge != null)
            {
                var serializedObject = new SerializedObject(bridge);
                
                var cacheLog = serializedObject.FindProperty("showCacheDebugLogs");
                if (cacheLog != null) cacheLog.boolValue = true;
                
                var doorLog = serializedObject.FindProperty("showDoorDebugLogs");
                if (doorLog != null) doorLog.boolValue = true;
                
                var moveLog = serializedObject.FindProperty("showMovementLogs");
                if (moveLog != null) moveLog.boolValue = true;
                
                serializedObject.ApplyModifiedProperties();
                Debug.Log("[Editor] ✅ RoomSystemBridge debug habilitado");
            }
            
            // GameContextProvider
            var contextProvider = gameManager.GetComponent<GameContextProvider>();
            if (contextProvider != null)
            {
                Debug.Log("[Editor] GameContextProvider no tiene flag de debug (logs siempre activos)");
            }
            
            EditorUtility.SetDirty(gameManager);
            if (bridge != null) EditorUtility.SetDirty(bridge.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            Debug.Log("[Editor] ✅ TODOS los debug logs habilitados para diagnóstico");
        }
    }
}

