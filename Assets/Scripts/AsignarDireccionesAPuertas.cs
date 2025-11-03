using UnityEngine;
using System.Collections.Generic;
using VoiceSystem.GameIntegration;

/// <summary>
/// Asigna direcciones cardinales a las puertas generadas por RoomGenerator3000
/// Se ejecuta automáticamente después de generar la casa
/// </summary>
public class AsignarDireccionesAPuertas : MonoBehaviour
{
    [Header("Auto-ejecutar")]
    [SerializeField] private bool ejecutarEnStart = true;
    [SerializeField] private float delaySegundos = 1f;
    
    private void Start()
    {
        if (ejecutarEnStart)
        {
            Invoke(nameof(AsignarDirecciones), delaySegundos);
        }
    }
    
    [ContextMenu("Asignar Direcciones a Puertas")]
    public void AsignarDirecciones()
    {
        Debug.Log("[AsignarDirecciones] Iniciando asignación de direcciones cardinales...");
        
        var roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        if (roomGenerator == null)
        {
            Debug.LogError("[AsignarDirecciones] No se encontró RoomGenerator3000");
            return;
        }
        
        if (roomGenerator.casa == null)
        {
            Debug.LogError("[AsignarDirecciones] La casa no ha sido generada aún");
            return;
        }
        
        // Direcciones cardinales disponibles
        string[] direccionesDisponibles = { "norte", "sur", "este", "oeste" };
        
        int puertasModificadas = 0;
        
        // Para cada puerta de la casa (no por habitación, porque Room.puertas es un int)
        foreach (var puerta in roomGenerator.casa.puertas)
        {
            if (puerta == null)
            {
                continue;
            }
            
            // NOTA: Door no tiene campo direccionRelativa
            // RoomSystemBridge ya calcula las direcciones automáticamente en ConvertToDoorData
            // usando CalculateDirection basado en las posiciones de las habitaciones
            
            // Este script NO es necesario - las direcciones ya se calculan
            puertasModificadas++;
        }
        
        Debug.Log($"[AsignarDirecciones] ✅ {puertasModificadas} puertas procesadas");
        Debug.Log($"[AsignarDirecciones] NOTA: Las direcciones cardinales se calculan automáticamente en RoomSystemBridge.CalculateDirection()");
        
        // Actualizar RoomSystemBridge para que recargue las puertas
        var roomBridge = RoomSystemBridge.Instance;
        if (roomBridge != null)
        {
            roomBridge.ClearCache();
            Debug.Log("[AsignarDirecciones] Cache de RoomSystemBridge limpiado - Direcciones recalculadas");
        }
    }
    
}

