using UnityEngine;
using VoiceSystem.Core.Data;
using VoiceSystem.GameIntegration;
using System.Collections.Generic;

/// <summary>
/// Single source of truth para TODO el estado del jugador
/// Sincroniza PlayerData, FatigueSystem, RoomGenerator, y GameContext
/// </summary>
public class PlayerStateManager : MonoBehaviour
{
    // Singleton
    private static PlayerStateManager _instance;
    public static PlayerStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("PlayerStateManager");
                _instance = go.AddComponent<PlayerStateManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [Header("Referencias")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private RoomGenerator3000 roomGenerator;
    
    [Header("Debug")]
    [SerializeField] private bool showSyncLogs = false; // Logs de sincronización
    
    // Cache de sistemas
    private FatigueSystem fatigueSystem;
    private RoomSystemBridge roomBridge;
    private GameContext gameContext;
    
    // Estado actual (authoritative)
    private Vector2Int currentPosition;
    private RoomData currentRoomData;
    
    void Awake()
    {
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
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        // Load PlayerData
        if (playerData == null)
        {
            playerData = Resources.Load<PlayerData>("Data/PlayerData");
            if (playerData == null)
            {
                Debug.LogError("[PlayerState] ❌ PlayerData no encontrado en Resources/Data/");
                return;
            }
        }
        
        // Find systems
        roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        fatigueSystem = FatigueSystem.Instance;
        roomBridge = RoomSystemBridge.Instance;
        
        // Subscribe to events
        if (roomBridge != null)
        {
            roomBridge.OnRoomChanged += OnRoomChangedHandler;
        }
        
        // Initial sync
        SyncAllSystems();
        
        if (showSyncLogs)
            Debug.Log($"[PlayerState] ✅ Inicializado - Pos: {currentPosition}, Salud: {Health}, Fatiga: {Fatigue}, Inventario: {Inventory.Count} items");
    }
    
    /// <summary>
    /// Master sync method - Sincroniza TODOS los sistemas
    /// </summary>
    public void SyncAllSystems()
    {
        // VERIFICACIÓN TEMPRANA: Asegurar que el generador está listo
        bool isGeneratorReady = roomGenerator != null && 
                                roomGenerator.casa != null && 
                                roomGenerator.casa.habitaciones != null &&
                                roomGenerator.casa.habitaciones.Count > 0;
        
        if (!isGeneratorReady)
        {
            Debug.LogWarning("[PlayerState] SyncAllSystems: Casa aún no generada, saltando sync completo");
            
            // Solo sincronizar FatigueSystem (no depende de habitaciones)
            if (fatigueSystem != null)
            {
                fatigueSystem.SyncWithPlayerData();
            }
            
            Debug.Log("[PlayerState] Sync parcial completado (sin habitaciones)");
            return;
        }
        
        // Si llegamos aquí, el generador está listo
        if (roomGenerator != null && roomBridge != null)
        {
            // Get position from current room
            var currentRoom = roomBridge.GetCurrentRoom();
            if (currentRoom != null && currentRoom.metadata != null)
            {
                if (currentRoom.metadata.TryGetValue("positionX", out string xStr) &&
                    currentRoom.metadata.TryGetValue("positionY", out string yStr))
                {
                    if (int.TryParse(xStr, out int x) && int.TryParse(yStr, out int y))
                    {
                        currentPosition = new Vector2Int(x, y);
                    }
                }
            }
        }
        
        if (roomBridge != null)
        {
            currentRoomData = roomBridge.GetCurrentRoom();
            
            if (currentRoomData != null && playerData != null)
            {
                playerData.CurrentRoomId = currentRoomData.roomId;
            }
        }
        
        if (fatigueSystem != null)
        {
            fatigueSystem.SyncWithPlayerData();
        }
        
        Debug.Log("[PlayerState] Todos los sistemas sincronizados");
    }
    
    /// <summary>
    /// Update position (master method) - Sincroniza posición a TODOS los sistemas
    /// </summary>
    public void UpdatePosition(Vector2Int newPosition, RoomData newRoom)
    {
        currentPosition = newPosition;
        currentRoomData = newRoom;
        
        // Store position (RoomGenerator3000 doesn't have posicionJugador field)
        // Position is tracked through RoomData.metadata
        
        if (playerData != null)
        {
            playerData.CurrentRoomId = newRoom.roomId;
            playerData.MarkRoomVisited(newRoom.roomId);
            if (showSyncLogs)
                Debug.Log($"[PlayerState] PlayerData actualizado: roomId = {newRoom.roomId}");
        }
        
        Debug.Log($"[PlayerState] 🎮 Posición: {newPosition} → {newRoom.roomName}"); // Log importante
    }
    
    private void OnRoomChangedHandler(RoomData newRoom)
    {
        // This is called by RoomSystemBridge
        // Just update our cache
        currentRoomData = newRoom;
        
        if (playerData != null)
        {
            playerData.CurrentRoomId = newRoom.roomId;
            playerData.MarkRoomVisited(newRoom.roomId);
        }
    }
    
    // Properties (authoritative)
    public Vector2Int CurrentPosition => currentPosition;
    public RoomData CurrentRoom => currentRoomData;
    public int Health => fatigueSystem?.PlayerLives?.currentLives ?? 0;
    public int Fatigue => fatigueSystem?.NivelFatiga ?? 0;
    public List<string> Inventory => playerData?.Inventory ?? new List<string>();
    
    /// <summary>
    /// Añade item al inventario (master method)
    /// </summary>
    public bool AddItem(string itemId)
    {
        if (playerData != null)
        {
            bool added = playerData.AddItem(itemId);
            if (added)
            {
                if (showSyncLogs)
                    Debug.Log($"[PlayerState] ✅ Item '{itemId}' añadido al inventario");
            }
            return added;
        }
        return false;
    }
    
    /// <summary>
    /// Remueve item del inventario (master method)
    /// </summary>
    public bool RemoveItem(string itemId)
    {
        if (playerData != null)
        {
            bool removed = playerData.RemoveItem(itemId);
            if (removed)
            {
                if (showSyncLogs)
                    Debug.Log($"[PlayerState] ✅ Item '{itemId}' removido del inventario");
            }
            return removed;
        }
        return false;
    }
    
    /// <summary>
    /// Verifica si el jugador tiene un item
    /// </summary>
    public bool HasItem(string itemId)
    {
        if (playerData != null)
        {
            return playerData.HasItem(itemId);
        }
        return false;
    }
    
    /// <summary>
    /// Verifica si un flag de evento está marcado
    /// </summary>
    public bool HasSeenEvent(string eventFlag)
    {
        if (playerData != null)
        {
            return playerData.HasSeenEvent(eventFlag);
        }
        return false;
    }
    
    /// <summary>
    /// Marca un flag de evento
    /// </summary>
    public void SetEventFlag(string eventFlag)
    {
        if (playerData != null)
        {
            playerData.SetEventFlag(eventFlag, true);
        }
    }
    
    /// <summary>
    /// Obtiene resumen del estado del jugador
    /// </summary>
    public string GetStateSummary()
    {
        return $"[PlayerState]\n" +
               $"  Posición: {currentPosition}\n" +
               $"  Habitación: {currentRoomData?.roomName ?? "Desconocida"} (ID: {currentRoomData?.roomId ?? "N/A"})\n" +
               $"  Salud: {Health}\n" +
               $"  Fatiga: {Fatigue}\n" +
               $"  Inventario: {Inventory.Count} items\n" +
               $"  Items: {(Inventory.Count > 0 ? string.Join(", ", Inventory) : "vacío")}";
    }
}

