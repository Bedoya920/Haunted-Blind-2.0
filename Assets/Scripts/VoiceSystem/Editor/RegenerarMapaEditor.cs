using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script para regenerar el mapa
/// </summary>
public class RegenerarMapaEditor : EditorWindow
{
    [MenuItem("Tools/Regenerar Mapa Completo")]
    public static void RegenerarMapa()
    {
        Debug.Log("[RegenerarMapa] Buscando RoomGenerator3000...");
        
        var generator = FindFirstObjectByType<RoomGenerator3000>();
        if (generator == null)
        {
            Debug.LogError("[RegenerarMapa] No se encontró RoomGenerator3000 en la escena!");
            EditorUtility.DisplayDialog("Error", "No se encontró RoomGenerator3000. Asegúrate de tener GameScene abierta.", "OK");
            return;
        }
        
        Debug.Log("[RegenerarMapa] Generando mapa de campaña...");
        generator.GenerarMapaCampana();
        
        Debug.Log("[RegenerarMapa] Guardando JSON...");
        generator.GuardarCasaJson();
        
        Debug.Log("[RegenerarMapa] ✅ Mapa regenerado y guardado exitosamente");
        EditorUtility.DisplayDialog("Éxito", "Mapa regenerado con narraciones del GDD y guardado en houseData.json", "OK");
    }
}

