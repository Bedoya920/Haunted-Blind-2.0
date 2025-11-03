using UnityEngine;
using VoiceSystem.GameIntegration;

/// <summary>
/// Transforma la flor de loto de marchita a viva cuando se activa el flag
/// </summary>
public class LotusFlowerTransformation : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string masterBedroomRoomId = "room_7"; // Habitación Principal
    
    [Header("Estado")]
    [SerializeField] private bool transformationDone = false;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private PlayerStateManager playerState;
    private RoomInventoryManager inventoryManager;
    private bool lastFlagState = false;
    
    private void Start()
    {
        playerState = PlayerStateManager.Instance;
        inventoryManager = RoomInventoryManager.Instance;
    }
    
    private void Update()
    {
        if (transformationDone) return;
        if (playerState == null || inventoryManager == null) return;
        
        // Verificar si el flag se activó
        bool flagActive = playerState.HasSeenEvent("lotus_flower_activated");
        
        // Solo transformar cuando el flag cambia de false a true
        if (flagActive && !lastFlagState)
        {
            TransformFlower();
        }
        
        lastFlagState = flagActive;
    }
    
    /// <summary>
    /// Transforma la flor de marchita a viva
    /// </summary>
    private void TransformFlower()
    {
        transformationDone = true;
        LogDebug("[LotusFlower] 🌸 Transformando flor de loto: marchita → viva");
        
        var inventory = inventoryManager.GetRoomInventory(masterBedroomRoomId);
        
        // Ocultar flor marchita
        var deadFlower = inventory.GetItemById("lotus_flower_dead");
        if (deadFlower != null)
        {
            deadFlower.isVisible = false;
            LogDebug("[LotusFlower] ✅ Flor marchita ocultada");
        }
        
        // Mostrar flor viva
        var aliveFlower = inventory.GetItemById("lotus_flower_alive");
        if (aliveFlower != null)
        {
            aliveFlower.isVisible = true;
            LogDebug("[LotusFlower] ✅ Flor viva ahora visible y recogible");
        }
        
        LogDebug("[LotusFlower] 🎉 Transformación completa!");
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

