using UnityEngine;
using VoiceSystem.Core.Data;
using System.Linq;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Sistema que dispara eventos narrativos basados en condiciones específicas
    /// Maneja eventos de primera entrada, inspeccionar, tomar items, leer, etc.
    /// </summary>
    public class StoryEventTrigger : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private PlayerData playerData;
        [SerializeField] private EventManager eventManager;
        [SerializeField] private RoomSystemBridge roomBridge;
        
        [Header("Configuración")]
        [SerializeField] private bool enableDebugLogs = false; // Deshabilitado por defecto
        [SerializeField] private float reentryEventCooldown = 30f; // Cooldown en segundos para eventos reentry
        
        // Cooldowns para eventos repetibles
        private System.Collections.Generic.Dictionary<string, float> eventCooldowns = new System.Collections.Generic.Dictionary<string, float>();
        
        private void Start()
        {
            // Encontrar componentes si no están asignados
            if (eventManager == null)
            {
                eventManager = EventManager.Instance;
            }
            
            if (roomBridge == null)
            {
                roomBridge = RoomSystemBridge.Instance;
            }
            
            // Cargar PlayerData desde Resources si no está asignado
            if (playerData == null)
            {
                playerData = Resources.Load<PlayerData>("Data/PlayerData");
                if (playerData == null)
                {
                    Debug.LogWarning("[StoryEventTrigger] PlayerData no encontrado en Resources/Data/");
                }
            }
            
            // Suscribirse a eventos de cambio de habitación
            if (roomBridge != null)
            {
                roomBridge.OnRoomChanged += HandleRoomChanged;
            }
            
            LogDebug("[StoryEventTrigger] Inicializado");
        }
        
        private void OnDestroy()
        {
            if (roomBridge != null)
            {
                roomBridge.OnRoomChanged -= HandleRoomChanged;
            }
        }
        
        #region Room Entry Events
        
        /// <summary>
        /// Maneja eventos cuando el jugador cambia de habitación
        /// </summary>
        private void HandleRoomChanged(RoomData newRoom)
        {
            if (newRoom == null || string.IsNullOrEmpty(newRoom.roomId))
            {
                return;
            }
            
            LogDebug($"[StoryEventTrigger] Cambio de habitación: {newRoom.roomId} ({newRoom.roomName})");
            
            // IMPORTANTE: NO marcar como visitada ANTES del trigger
            // El trigger verifica si es primera visita, y LUEGO marca
            
            // Trigger entry events
            TriggerRoomEntry(newRoom.roomId);
            
            // AHORA marcar como visitada DESPUÉS del trigger
            if (playerData != null)
            {
                playerData.MarkRoomVisited(newRoom.roomId);
            }
        }
        
        /// <summary>
        /// Dispara eventos de primera entrada o reentrada
        /// </summary>
        public void TriggerRoomEntry(string roomId)
        {
            if (eventManager == null || playerData == null)
            {
                return;
            }
            
            bool isFirstVisit = !playerData.HasVisitedRoom(roomId);
            
            if (isFirstVisit)
            {
                // PRIMERA VISITA: Siempre disparar evento
                LogDebug($"[StoryEventTrigger] Trigger room entry: {roomId} (firstEntry)");
                
                var storyEvent = eventManager.GetStoryEventByTrigger(roomId, "firstEntry");
                if (storyEvent != null && CheckConditions(storyEvent))
                {
                    ExecuteEvent(storyEvent);
                }
            }
            else
            {
                // REENTRADA: Verificar cooldown
                string cooldownKey = $"{roomId}_reentry";
                
                if (!eventCooldowns.ContainsKey(cooldownKey) || 
                    Time.time - eventCooldowns[cooldownKey] > reentryEventCooldown)
                {
                    LogDebug($"[StoryEventTrigger] Trigger room entry: {roomId} (reentry - cooldown OK)");
                    
                    var storyEvent = eventManager.GetStoryEventByTrigger(roomId, "reentry");
                    if (storyEvent != null && CheckConditions(storyEvent))
                    {
                        // NUEVO: Solo disparar eventos de reentry DESPUÉS de tomar el oso
                        // Esto evita narraciones atmosféricas intensas antes del evento principal
                        var playerState = PlayerStateManager.Instance;
                        bool shouldNarrate = true;
                        
                        // Si el evento tiene descripciones post-corrupción, requiere el evento del niño
                        if (playerState != null && !playerState.HasSeenEvent("child_event_triggered"))
                        {
                            // Eventos que NO se narran antes del evento del niño
                            if (storyEvent.eventName.Contains("Narración Corta"))
                            {
                                shouldNarrate = false;
                                LogDebug($"[StoryEventTrigger] Evento '{storyEvent.eventName}' bloqueado - Requiere evento del niño");
                            }
                        }
                        
                        if (shouldNarrate)
                        {
                            ExecuteEvent(storyEvent);
                            eventCooldowns[cooldownKey] = Time.time; // Actualizar cooldown
                        }
                    }
                }
                else
                {
                    float timeLeft = reentryEventCooldown - (Time.time - eventCooldowns[cooldownKey]);
                    LogDebug($"[StoryEventTrigger] Reentry cooldown activo para {roomId} ({timeLeft:F1}s restantes)");
                }
            }
        }
        
        #endregion
        
        #region Action-Based Events
        
        /// <summary>
        /// Dispara eventos basados en acciones del jugador
        /// </summary>
        /// <param name="action">Tipo de acción: inspect, take, read, interact</param>
        /// <param name="roomId">ID de la habitación actual</param>
        /// <param name="itemId">ID del item involucrado (opcional)</param>
        public void TriggerAction(string action, string roomId, string itemId = "")
        {
            if (eventManager == null || playerData == null)
            {
                return;
            }
            
            LogDebug($"[StoryEventTrigger] Trigger action: {action} en {roomId} con item {itemId}");
            
            // Construir el trigger condition basado en la acción e item
            string triggerCondition = action;
            if (!string.IsNullOrEmpty(itemId))
            {
                triggerCondition = $"{action}_{itemId}";
            }
            
            // Buscar evento que coincida con esta acción
            var storyEvent = eventManager.GetStoryEventByTrigger(roomId, triggerCondition);
            
            if (storyEvent != null)
            {
                if (CheckConditions(storyEvent))
                {
                    ExecuteEvent(storyEvent);
                }
            }
            else
            {
                // Intentar buscar eventos genéricos de acción sin item específico
                storyEvent = eventManager.GetStoryEventByTrigger(roomId, action);
                if (storyEvent != null && CheckConditions(storyEvent))
                {
                    ExecuteEvent(storyEvent);
                }
            }
        }
        
        #endregion
        
        #region Condition Checking
        
        /// <summary>
        /// Verifica si se cumplen las condiciones para ejecutar un evento
        /// </summary>
        private bool CheckConditions(HBEvents evt)
        {
            if (evt == null)
            {
                return false;
            }
            
            // Si es único y ya fue disparado, no ejecutar
            if (evt.isUnique && evt.hasBeenTriggered)
            {
                LogDebug($"[StoryEventTrigger] Evento '{evt.eventName}' ya fue disparado (unique)");
                return false;
            }
            
            // Si es único y ya tiene un flag en PlayerData, no ejecutar
            if (evt.isUnique && !string.IsNullOrEmpty(evt.setsFlag))
            {
                if (playerData != null && playerData.HasSeenEvent(evt.setsFlag))
                {
                    LogDebug($"[StoryEventTrigger] Evento '{evt.eventName}' ya visto (flag: {evt.setsFlag})");
                    return false;
                }
            }
            
            // Verificar si requiere un item específico
            if (!string.IsNullOrEmpty(evt.requiredItem))
            {
                if (playerData == null || !playerData.HasItem(evt.requiredItem))
                {
                    LogDebug($"[StoryEventTrigger] Evento '{evt.eventName}' requiere item: {evt.requiredItem}");
                    return false;
                }
            }
            
            // NUEVO: Verificar si requiere un flag previo (para progresión secuencial)
            if (!string.IsNullOrEmpty(evt.requiredFlag))
            {
                bool hasFlag = playerData != null && playerData.HasSeenEvent(evt.requiredFlag);
                LogDebug($"[StoryEventTrigger] 🔍 Evento '{evt.eventName}' requiere flag '{evt.requiredFlag}' → {(hasFlag ? "✅ TIENE" : "❌ NO TIENE")}");
                
                if (!hasFlag)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region Event Execution
        
        /// <summary>
        /// Ejecuta un evento narrativo
        /// </summary>
        private void ExecuteEvent(HBEvents evt)
        {
            if (evt == null || eventManager == null)
            {
                return;
            }
            
            LogDebug($"[StoryEventTrigger] Ejecutando evento: {evt.eventName}");
            
            // Narrar el evento
            eventManager.NarrateEvent(evt);
            
            // Setear flag en PlayerData si corresponde
            if (!string.IsNullOrEmpty(evt.setsFlag) && playerData != null)
            {
                playerData.SetEventFlag(evt.setsFlag, true);
            }
            
            // Si es screamer (type 3), reducir vida
            if (evt.type == EventType.Screamer)
            {
                HandleScreamer(evt);
            }
        }
        
        /// <summary>
        /// Maneja un evento de screamer (reduce vida del jugador)
        /// </summary>
        private void HandleScreamer(HBEvents screamerEvent)
        {
            // Reproducir sonido de screamer con fade
            var soundManager = Audio.SoundManager.Instance;
            if (soundManager != null)
            {
                string soundId = GetEventScreamerSound(screamerEvent.id);
                soundManager.PlayScreamer(soundId, () => {
                    // Después del sonido, narrar
                    var confirmManager = ActionConfirmationManager.Instance;
                    if (confirmManager != null)
                    {
                        confirmManager.ConfirmScreamer(screamerEvent.audioTxt);
                    }
                });
            }
            else
            {
                // Fallback: solo narrar
                var confirmManager = ActionConfirmationManager.Instance;
                if (confirmManager != null)
                {
                    confirmManager.ConfirmScreamer(screamerEvent.audioTxt);
                }
            }
            
            // Reducir vida del jugador
            var fatigueSystem = FatigueSystem.Instance;
            if (fatigueSystem != null && fatigueSystem.PlayerLives != null)
            {
                bool stillAlive = fatigueSystem.PlayerLives.LoseLife();
                LogDebug($"[StoryEventTrigger] Screamer! Vida reducida a {fatigueSystem.PlayerLives.currentLives}. Vivo: {stillAlive}");
            }
        }
        
        /// <summary>
        /// Mapea ID de evento a ID de sonido de screamer
        /// </summary>
        private string GetEventScreamerSound(int eventId)
        {
            switch (eventId)
            {
                case 300: return "screamer_kitchen";     // Cocina Risa
                case 301: return "screamer_bathroom";    // Baño Espejo
                case 302: return "screamer_bedroom";     // Hab. Niños Caja Musical
                case 303: return "screamer_dining";      // Comedor Sombra
                case 304: return "screamer_library";     // Biblioteca Libros
                default: return "screamer_default";
            }
        }
        
        #endregion
        
        #region Public Trigger Methods
        
        /// <summary>
        /// Trigger para cuando el jugador inspecciona en una habitación
        /// </summary>
        public void TriggerInspect(string roomId)
        {
            TriggerAction("inspect", roomId);
        }
        
        /// <summary>
        /// Trigger para cuando el jugador toma un item
        /// </summary>
        public void TriggerTakeItem(string roomId, string itemId)
        {
            TriggerAction("take", roomId, itemId);
        }
        
        /// <summary>
        /// Trigger para cuando el jugador lee algo
        /// </summary>
        public void TriggerRead(string roomId, string readableId)
        {
            TriggerAction("read", roomId, readableId);
        }
        
        /// <summary>
        /// Trigger para interacciones genéricas
        /// </summary>
        public void TriggerInteract(string roomId, string targetId = "")
        {
            TriggerAction("interact", roomId, targetId);
        }
        
        #endregion
        
        #region Debug
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
        
        #endregion
    }
}

