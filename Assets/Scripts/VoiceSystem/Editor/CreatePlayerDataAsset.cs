using UnityEditor;
using UnityEngine;
using VoiceSystem.Core.Data;

namespace VoiceSystem.Editor
{
    public static class CreatePlayerDataAsset
    {
        [MenuItem("Tools/VoiceSystem/Create Player Data Asset")]
        public static void CreatePlayerData()
        {
            // Verificar que la carpeta Data existe
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
                Debug.Log("[CreatePlayerData] Carpeta Assets/Data creada");
            }
            
            string assetPath = "Assets/Data/PlayerData.asset";
            
            // Verificar si ya existe
            var existing = AssetDatabase.LoadAssetAtPath<PlayerData>(assetPath);
            if (existing != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "PlayerData ya existe",
                    "Ya existe un PlayerData.asset en Assets/Data/.\n\n¿Deseas sobrescribirlo? (Se perderán los datos actuales)",
                    "Sobrescribir",
                    "Cancelar"
                );
                
                if (!overwrite)
                {
                    Debug.Log("[CreatePlayerData] Operación cancelada por el usuario");
                    return;
                }
            }
            
            // Crear nuevo PlayerData
            PlayerData playerData = ScriptableObject.CreateInstance<PlayerData>();
            
            // Configurar valores iniciales
            playerData.ResetToDefault();
            
            // Guardar asset
            if (existing != null)
            {
                EditorUtility.CopySerialized(playerData, existing);
                EditorUtility.SetDirty(existing);
                Debug.Log("[CreatePlayerData] PlayerData existente actualizado");
            }
            else
            {
                AssetDatabase.CreateAsset(playerData, assetPath);
                Debug.Log($"[CreatePlayerData] PlayerData creado en {assetPath}");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Seleccionar el asset en el Project window
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<PlayerData>(assetPath));
            
            EditorUtility.DisplayDialog(
                "PlayerData Creado",
                $"✅ PlayerData.asset creado exitosamente en:\n{assetPath}\n\n" +
                "Valores iniciales:\n" +
                "• Salud: 5\n" +
                "• Fatiga: 0\n" +
                "• Inventario: Vacío\n" +
                "• Flags: Vacíos\n\n" +
                "Siguiente paso:\n" +
                "Asigna este asset a:\n" +
                "• FatigueSystem.playerData\n" +
                "• StoryEventTrigger.playerData\n" +
                "• RoomSystemBridge.playerData (si aplica)",
                "Entendido"
            );
        }
    }
}

