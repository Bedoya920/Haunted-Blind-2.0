using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Sistema que dispara eventos y acciones basados en el tiempo de juego
    /// Permite abrir puertas, revelar objetos o disparar eventos a cierto tiempo
    /// </summary>
    public class TimeBasedEventSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string jsonFileName = "time_based_events";
        [SerializeField] private bool loadFromJson = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("References")]
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private RoomSystemBridge roomBridge;
        [SerializeField] private RoomInventoryManager inventoryManager;
        
        // Lista de triggers
        private List<TimeBasedTrigger> triggers = new List<TimeBasedTrigger>();
        private HashSet<int> executedTriggers = new HashSet<int>(); // Para no ejecutar dos veces
        
        // Singleton
        private static TimeBasedEventSystem _instance;
        public static TimeBasedEventSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TimeBasedEventSystem>();
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            // Buscar referencias si no están asignadas
            if (gameTimer == null) gameTimer = GameTimer.Instance;
            if (roomBridge == null) roomBridge = RoomSystemBridge.Instance;
            if (inventoryManager == null) inventoryManager = RoomInventoryManager.Instance;
        }
        
        private void Start()
        {
            LoadTriggers();
            SubscribeToTimer();
        }
        
        private void LoadTriggers()
        {
            if (!loadFromJson)
            {
                CreateDemoTriggers();
                return;
            }
            
            TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
            if (jsonFile == null)
            {
                Debug.LogWarning($"[TimeBasedEvents] No se encontró {jsonFileName}.json, usando datos demo");
                CreateDemoTriggers();
                return;
            }
            
            try
            {
                TimeBasedTriggerDatabase database = JsonUtility.FromJson<TimeBasedTriggerDatabase>(jsonFile.text);
                if (database != null && database.triggers != null)
                {
                    triggers = database.triggers;
                    LogDebug($"[TimeBasedEvents] Cargados {triggers.Count} triggers desde JSON");
                }
                else
                {
                    Debug.LogWarning("[TimeBasedEvents] JSON vacío o inválido");
                    CreateDemoTriggers();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimeBasedEvents] Error al cargar JSON: {e.Message}");
                CreateDemoTriggers();
            }
        }
        
        private void CreateDemoTriggers()
        {
            triggers = new List<TimeBasedTrigger>
            {
                // Ejemplo: A los 2 minutos (120s), desbloquear puerta del sótano
                new TimeBasedTrigger
                {
                    triggerId = 1,
                    triggerTime = 120f,
                    actionType = "unlock_door",
                    targetId = 7,
                    eventToTrigger = "basement_door_unlocked"
                }
            };
            
            LogDebug($"[TimeBasedEvents] Creados {triggers.Count} triggers demo");
        }
        
        private void SubscribeToTimer()
        {
            if (gameTimer == null)
            {
                Debug.LogWarning("[TimeBasedEvents] GameTimer no encontrado");
                return;
            }
            
            // Suscribirse a intervalos de tiempo
            gameTimer.OnIntervalReached += OnTimeIntervalReached;
            LogDebug("[TimeBasedEvents] Suscrito a GameTimer");
        }
        
        private void OnDestroy()
        {
            if (gameTimer != null)
            {
                gameTimer.OnIntervalReached -= OnTimeIntervalReached;
            }
        }
        
        /// <summary>
        /// Llamado cada segundo por GameTimer
        /// </summary>
        private void OnTimeIntervalReached()
        {
            // Obtener tiempo actual del GameTimer
            if (gameTimer == null) return;
            
            float elapsedTime = gameTimer.GetTotalTime() - gameTimer.GetRemainingTime();
            
            // Buscar triggers que deberían ejecutarse
            foreach (var trigger in triggers)
            {
                if (executedTriggers.Contains(trigger.triggerId))
                    continue; // Ya ejecutado
                
                if (elapsedTime >= trigger.triggerTime)
                {
                    ExecuteTrigger(trigger);
                    executedTriggers.Add(trigger.triggerId);
                }
            }
        }
        
        private void ExecuteTrigger(TimeBasedTrigger trigger)
        {
            LogDebug($"[TimeBasedEvents] ⏰ Ejecutando trigger #{trigger.triggerId} - Tipo: {trigger.actionType}");
            
            switch (trigger.actionType.ToLower())
            {
                case "unlock_door":
                    UnlockDoor(trigger.targetId, trigger.eventToTrigger);
                    break;
                    
                case "reveal_item":
                    RevealItem(trigger.targetId, trigger.eventToTrigger);
                    break;
                    
                case "trigger_event":
                    TriggerEvent(trigger.eventToTrigger);
                    break;
                    
                default:
                    Debug.LogWarning($"[TimeBasedEvents] Tipo de acción desconocido: {trigger.actionType}");
                    break;
            }
        }
        
        private void UnlockDoor(int doorId, string eventName)
        {
            var roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
            if (roomGenerator == null || roomGenerator.casa == null)
            {
                Debug.LogWarning("[TimeBasedEvents] RoomGenerator no disponible");
                return;
            }
            
            var door = roomGenerator.casa.puertas.Find(d => d.id == doorId);
            if (door != null)
            {
                door.abierta = true;
                LogDebug($"[TimeBasedEvents] 🔓 Puerta #{doorId} desbloqueada por tiempo");
                
                // Limpiar cache
                roomBridge.ClearCache();
                
                // Disparar evento si existe
                if (!string.IsNullOrEmpty(eventName))
                {
                    TriggerEvent(eventName);
                }
            }
            else
            {
                Debug.LogWarning($"[TimeBasedEvents] No se encontró puerta con ID:{doorId}");
            }
        }
        
        private void RevealItem(int itemIdNumeric, string eventName)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning("[TimeBasedEvents] RoomInventoryManager no disponible");
                return;
            }
            
            string itemId = "item_" + itemIdNumeric;
            
            // Buscar item en todas las habitaciones
            var item = inventoryManager.FindItemInAllRooms(itemId);
            if (item != null)
            {
                item.isVisible = true;
                LogDebug($"[TimeBasedEvents] 👁️ Item revelado por tiempo: {item.itemName}");
                
                // Disparar evento si existe
                if (!string.IsNullOrEmpty(eventName))
                {
                    TriggerEvent(eventName);
                }
            }
            else
            {
                Debug.LogWarning($"[TimeBasedEvents] No se encontró item con ID:{itemId}");
            }
        }
        
        private void TriggerEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            
            var eventManager = EventManager.Instance;
            if (eventManager == null)
            {
                Debug.LogWarning("[TimeBasedEvents] EventManager no disponible");
                return;
            }
            
            var evt = eventManager.GetEventByName(eventName);
            if (evt != null)
            {
                eventManager.NarrateEvent(evt);
                LogDebug($"[TimeBasedEvents] 🎬 Evento disparado: {eventName}");
            }
            else
            {
                Debug.LogWarning($"[TimeBasedEvents] Evento '{eventName}' no encontrado");
            }
        }
        
        /// <summary>
        /// Resetea triggers ejecutados (para reiniciar el juego)
        /// </summary>
        public void ResetExecutedTriggers()
        {
            executedTriggers.Clear();
            LogDebug("[TimeBasedEvents] Triggers reseteados");
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
    }
    
    /// <summary>
    /// Trigger individual basado en tiempo
    /// </summary>
    [System.Serializable]
    public class TimeBasedTrigger
    {
        public int triggerId;           // ID único del trigger
        public float triggerTime;       // Tiempo en segundos desde inicio (ej: 120 = 2 minutos)
        public string actionType;       // "unlock_door", "reveal_item", "trigger_event"
        public int targetId;            // ID de puerta/objeto afectado
        public string eventToTrigger;   // Nombre del evento a disparar (opcional)
        public string description;      // Descripción para debugging
    }
    
    /// <summary>
    /// Base de datos de triggers para JSON
    /// </summary>
    [System.Serializable]
    public class TimeBasedTriggerDatabase
    {
        public List<TimeBasedTrigger> triggers = new List<TimeBasedTrigger>();
    }
}
