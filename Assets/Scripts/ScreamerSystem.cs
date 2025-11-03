using UnityEngine;
using VoiceSystem.GameIntegration;

/// <summary>
/// Sistema de screamers que se activan tras tomar el oso de peluche
/// </summary>
public class ScreamerSystem : MonoBehaviour
{
    private static ScreamerSystem instance;
    public static ScreamerSystem Instance => instance;
    
    [Header("Configuración")]
    [SerializeField] private bool screamersActive = false;
    [SerializeField] [Range(0f, 1f)] private float screamerChance = 0.3f; // 30% chance por defecto
    
    [Header("Cooldown")]
    [SerializeField] private float screamerCooldown = 30f; // Segundos entre screamers
    private float lastScreamerTime = -999f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private RoomSystemBridge roomBridge;
    private string lastRoomId = "";
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        roomBridge = RoomSystemBridge.Instance;
        
        if (roomBridge != null)
        {
            roomBridge.OnRoomChanged += OnRoomChanged;
            LogDebug("[ScreamerSystem] Suscrito a cambios de habitación");
        }
    }
    
    private void OnDestroy()
    {
        if (roomBridge != null)
        {
            roomBridge.OnRoomChanged -= OnRoomChanged;
        }
    }
    
    /// <summary>
    /// Activa el sistema de screamers (llamado al tomar el oso)
    /// </summary>
    public void ActivateScreamers()
    {
        screamersActive = true;
        LogDebug("[ScreamerSystem] 🔴 SCREAMERS ACTIVADOS - Fase de corrupción iniciada");
    }
    
    /// <summary>
    /// Desactiva los screamers
    /// </summary>
    public void DeactivateScreamers()
    {
        screamersActive = false;
        LogDebug("[ScreamerSystem] 🟢 Screamers desactivados");
    }
    
    /// <summary>
    /// Llamado cuando el jugador entra a una nueva habitación
    /// </summary>
    private void OnRoomChanged(VoiceSystem.Core.Data.RoomData newRoom)
    {
        if (!screamersActive) return;
        if (newRoom == null) return;
        if (newRoom.roomId == lastRoomId) return; // Evitar duplicados
        
        lastRoomId = newRoom.roomId;
        
        // Verificar cooldown
        if (Time.time - lastScreamerTime < screamerCooldown)
        {
            LogDebug($"[ScreamerSystem] Cooldown activo, skipping screamer en {newRoom.roomName}");
            return;
        }
        
        // Roll para screamer
        float roll = Random.Range(0f, 1f);
        if (roll < screamerChance)
        {
            TriggerScreamer(newRoom);
        }
        else
        {
            LogDebug($"[ScreamerSystem] No screamer en {newRoom.roomName} (roll: {roll:F2} vs {screamerChance:F2})");
        }
    }
    
    /// <summary>
    /// Dispara un screamer específico según la habitación
    /// </summary>
    private void TriggerScreamer(VoiceSystem.Core.Data.RoomData room)
    {
        lastScreamerTime = Time.time;
        
        string screamerNarration = GetScreamerForRoom(room.roomId);
        
        if (string.IsNullOrEmpty(screamerNarration))
        {
            LogDebug($"[ScreamerSystem] No hay screamer definido para {room.roomName}");
            return;
        }
        
        LogDebug($"[ScreamerSystem] 👻 SCREAMER disparado en {room.roomName}!");
        
        // Narrar screamer con prioridad URGENTE
        var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
        if (voiceSystem?.textToSpeech != null)
        {
            voiceSystem.textToSpeech.Speak(screamerNarration, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
        }
        
        // Reducir vida del jugador
        var fatigueSystem = FatigueSystem.Instance;
        if (fatigueSystem != null && fatigueSystem.PlayerLives != null)
        {
            fatigueSystem.PlayerLives.currentLives = Mathf.Max(0, fatigueSystem.PlayerLives.currentLives - 1);
            LogDebug($"[ScreamerSystem] ❤️ Jugador perdió 1 vida por screamer (Vidas restantes: {fatigueSystem.PlayerLives.currentLives})");
        }
    }
    
    /// <summary>
    /// Retorna el screamer específico para cada habitación
    /// </summary>
    private string GetScreamerForRoom(string roomId)
    {
        switch (roomId)
        {
            case "room_6": // Baño
                return "Cuando te acercas al espejo, el vapor se disuelve apenas un poco. Por un instante ves tu reflejo, y justo detrás, una figura femenina, pálida, inclinada sobre la bañera cuyo cabello cae como cascada sobre el agua que comienza a vibrar, tiñéndose lentamente de rojo. El sonido es leve, como un suspiro.";
            
            case "room_5": // Cocina
                return "Abres los cajones, y el sonido de la madera al ceder retumba en la cocina. Una risa breve, femenina, se escucha cerca de tu oído, casi susurrada: Siempre servía para cuatro.";
            
            case "room_4": // Comedor
                return "Una sombra cruza al fondo del comedor. Escuchas el arrastre de una silla que se mueve sola. Los platos tiemblan en la mesa.";
            
            case "room_9": // Hab. Niños
                return "La caja musical gira más rápido ahora, con notas que se rompen y retuercen. El aire es denso, y escuchas una respiración pequeña, como si el niño estuviera aquí, esperando.";
            
            case "room_3": // Biblioteca (tras evento)
                return "Un par de libros caen por su cuenta desde los estantes con un golpe seco. Las páginas se abren solas, pasando rápidamente como si alguien invisible las leyera con urgencia.";
            
            default:
                return ""; // Sin screamer para esta habitación
        }
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

