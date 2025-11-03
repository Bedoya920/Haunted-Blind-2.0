using UnityEngine;

public class ConsumiblesManager : MonoBehaviour
{
    // Singleton
    private static ConsumiblesManager _instance;
    public static ConsumiblesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ConsumiblesManager");
                _instance = go.AddComponent<ConsumiblesManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [Header("Referencia al ScriptableObject del consumible")]
    [SerializeField] private ConsumibleData consumibleData;
    
    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Devuelve la cantidad actual de consumibles.
    /// </summary>
    public int GetCantidadConsumibles()
    {
        return consumibleData.cantidadConsumibles;
    }

    /// <summary>
    /// Aumenta el n�mero de consumibles.
    /// </summary>
    public void SumarConsumible(int cantidad = 1)
    {
        consumibleData.cantidadConsumibles += cantidad;
        Debug.Log($"Se a�adieron {cantidad} consumible(s). Total: {consumibleData.cantidadConsumibles}");
    }

    /// <summary>
    /// Resta consumibles (no baja de 0).
    /// </summary>
    public void RestarConsumible(int cantidad = 1)
    {
        consumibleData.cantidadConsumibles = Mathf.Max(0, consumibleData.cantidadConsumibles - cantidad);
        Debug.Log($"Se us� {cantidad} consumible. Restan: {consumibleData.cantidadConsumibles}");
    }

    /// <summary>
    /// Devuelve el ScriptableObject asociado, para acceder a sus propiedades.
    /// </summary>
    public ConsumibleData GetConsumibleData()
    {
        return consumibleData;
    }
    
    /// <summary>
    /// Sincroniza cantidad con RoomInventoryManager
    /// Cuenta consumibles en inventario del jugador
    /// </summary>
    public void SyncWithRoomInventory()
    {
        var inventoryManager = VoiceSystem.GameIntegration.RoomInventoryManager.Instance;
            var contextProvider = FindFirstObjectByType<VoiceSystem.GameIntegration.GameContextProvider>();
        
        if (inventoryManager != null && contextProvider != null && consumibleData != null)
        {
            var context = contextProvider.GetCurrentContext();
            int consumableCount = 0;
            
            // Contar consumibles en inventario
            foreach (string itemId in context.inventory)
            {
                var item = inventoryManager.FindItemInAllRooms(itemId);
                if (item != null && item.IsConsumable())
                {
                    consumableCount++;
                }
            }
            
            consumibleData.cantidadConsumibles = consumableCount;
            Debug.Log($"[ConsumiblesManager] Sincronizado: {consumableCount} consumibles en inventario");
        }
    }
}
