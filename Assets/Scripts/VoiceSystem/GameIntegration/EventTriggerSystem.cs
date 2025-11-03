using UnityEngine;
using VoiceSystem.Core;
using VoiceSystem.Core.Data;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Sistema de triggers automáticos para eventos del EventManager.
    /// Conecta GameTimer y RoomSystemBridge con EventManager para narración automática.
    /// </summary>
    public class EventTriggerSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool triggerOnInterval = true;
        [SerializeField] private bool triggerOnRoomChange = true;
        [SerializeField] [Range(0f, 1f)] private float randomEventChance = 0.3f; // 30% chance
        
        private EventManager eventManager;
        private GameTimer gameTimer;
        private RoomSystemBridge roomBridge;
        
        void Start()
        {
            // Obtener singletons
            eventManager = EventManager.Instance;
            gameTimer = GameTimer.Instance;
            roomBridge = FindFirstObjectByType<RoomSystemBridge>();
            
            // Suscribirse a eventos
            if (gameTimer != null && triggerOnInterval)
            {
                gameTimer.OnIntervalReached += OnTimerInterval;
                Debug.Log("[EventTrigger] Suscrito a GameTimer.OnIntervalReached");
            }
            else if (triggerOnInterval)
            {
                Debug.LogWarning("[EventTrigger] GameTimer no encontrado - triggers de intervalo deshabilitados");
            }
            
            if (roomBridge != null && triggerOnRoomChange)
            {
                roomBridge.OnRoomChanged += OnPlayerChangedRoom;
                Debug.Log("[EventTrigger] Suscrito a RoomBridge.OnRoomChanged");
            }
            else if (triggerOnRoomChange)
            {
                Debug.LogWarning("[EventTrigger] RoomSystemBridge no encontrado - triggers de cambio de habitación deshabilitados");
            }
            
            Debug.Log($"[EventTrigger] Sistema inicializado (Interval: {triggerOnInterval}, RoomChange: {triggerOnRoomChange}, Chance: {randomEventChance * 100}%)");
        }
        
        void OnDestroy()
        {
            // Desuscribirse de eventos
            if (gameTimer != null)
            {
                gameTimer.OnIntervalReached -= OnTimerInterval;
            }
            
            if (roomBridge != null)
            {
                roomBridge.OnRoomChanged -= OnPlayerChangedRoom;
            }
        }
        
        private void OnTimerInterval()
        {
            if (Random.value < randomEventChance)
            {
                TriggerRandomEvent();
            }
        }
        
        private void OnPlayerChangedRoom(RoomData newRoom)
        {
            // Posibilidad de evento al entrar a habitación
            if (Random.value < randomEventChance)
            {
                TriggerRandomEvent(newRoom.roomId);
            }
        }
        
        private void TriggerRandomEvent(string roomId = "")
        {
            if (eventManager == null) 
            {
                Debug.LogWarning("[EventTrigger] EventManager no disponible");
                return;
            }
            
            var randomEvent = eventManager.GetRandomEvent(EventType.Random, roomId);
            if (randomEvent != null)
            {
                eventManager.NarrateEvent(randomEvent);
                Debug.Log($"[EventTrigger] Evento disparado: {randomEvent.eventName} (Room: {roomId})");
            }
            else
            {
                Debug.LogWarning($"[EventTrigger] No se encontró evento aleatorio para roomId: {roomId}");
            }
        }
        
        /// <summary>
        /// Trigger manual de evento específico por ID
        /// </summary>
        public void TriggerEventById(int eventId)
        {
            if (eventManager == null) 
            {
                Debug.LogWarning("[EventTrigger] EventManager no disponible");
                return;
            }
            
            var hbEvent = eventManager.GetEventById(eventId);
            if (hbEvent != null)
            {
                eventManager.NarrateEvent(hbEvent);
                Debug.Log($"[EventTrigger] Evento manual disparado: {hbEvent.eventName} (ID: {eventId})");
            }
            else
            {
                Debug.LogWarning($"[EventTrigger] Evento con ID {eventId} no encontrado");
            }
        }
        
        /// <summary>
        /// Trigger manual de evento aleatorio en habitación actual
        /// </summary>
        public void TriggerRandomEventInCurrentRoom()
        {
            if (roomBridge != null)
            {
                var currentRoom = roomBridge.GetCurrentRoom();
                if (currentRoom != null)
                {
                    TriggerRandomEvent(currentRoom.roomId);
                }
                else
                {
                    TriggerRandomEvent();
                }
            }
            else
            {
                TriggerRandomEvent();
            }
        }
    }
}

