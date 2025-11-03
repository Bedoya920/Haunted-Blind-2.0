using System.Collections.Generic;
using UnityEngine;
using VoiceSystem.Core.Interfaces;
using VoiceSystem.Core.Data;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Provides game context to the AI system
    /// </summary>
    public class GameContextProvider : MonoBehaviour, IGameContextProvider
    {
        [Header("Room System Integration")]
        [SerializeField] private RoomSystemBridge roomBridge;
        
        [Header("Game References")]
        public EventManager eventManager;
        
        // Current context
        private GameContext currentContext;
        
        private void Awake()
        {
            // Initialize context
            currentContext = new GameContext();
        }
        
        private void Start()
        {
            StartCoroutine(SetupAfterGeneration());
        }
        
        private System.Collections.IEnumerator SetupAfterGeneration()
        {
            // Esperar a que GameInitializer termine de configurar RoomSystemBridge
            Debug.Log("[GameContext] Esperando a que RoomSystemBridge esté listo...");
            
            // Esperar hasta que isFullyInitialized sea true
            int maxAttempts = 20; // 10 segundos máximo
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                if (roomBridge != null && roomBridge.isFullyInitialized)
                {
                    Debug.Log($"[GameContext] ✅ RoomSystemBridge listo en intento {attempts + 1}");
                    break;
                }
                
                yield return new WaitForSeconds(0.5f);
                attempts++;
            }
            
            if (attempts >= maxAttempts)
            {
                Debug.LogError("[GameContext] ❌ Timeout esperando RoomSystemBridge");
                Debug.LogError($"[GameContext] roomBridge null? {roomBridge == null}, isFullyInitialized? {roomBridge?.isFullyInitialized}");
                yield break;
            }
            
            if (roomBridge != null)
            {
                SetupFromRoomGenerator();
                
                // Verify we got a room
                if (currentContext.currentRoom != null)
                {
                    Debug.Log($"[GameContext] ✅ Configurado correctamente: {currentContext.currentRoom.roomName} (ID: {currentContext.currentRoom.roomId})");
                }
                else
                {
                    Debug.LogError("[GameContext] ❌ No se pudo obtener habitación inicial - Reintentando...");
                    yield return new WaitForSeconds(1f);
                    SetupFromRoomGenerator();
                }
            }
            else
            {
                Debug.LogError("[GameContextProvider] RoomSystemBridge no asignado!");
            }
        }
        
        
        private void SetupFromRoomGenerator()
        {
            var roomBridge = RoomSystemBridge.Instance;
            if (roomBridge == null)
            {
                Debug.LogError("[GameContext] RoomSystemBridge.Instance es null!");
                return;
            }
            
            // YA NO VERIFICAMOS aquí porque SetupAfterGeneration() ya esperó
            // Asumimos que roomBridge.currentPlayerPosition es válido
            
            Debug.Log($"[GameContext] Configurando desde posición: {roomBridge.currentPlayerPosition}");
            
            // Obtener habitación actual del generador
            currentContext.currentRoom = roomBridge.GetCurrentRoom();
            
            if (currentContext.currentRoom == null)
            {
                Debug.LogError("[GameContext] GetCurrentRoom() retornó null!");
                return;
            }
            
            currentContext.currentLocation = currentContext.currentRoom.roomName;
            currentContext.nearbyObjects.Clear();
            currentContext.nearbyObjects.AddRange(currentContext.currentRoom.objects);
            
            // Sincronizar estado del jugador
            roomBridge.SyncPlayerStateToContext(currentContext);
            
            // Suscribirse a cambios de habitación
            roomBridge.OnRoomChanged += OnRoomChangedFromGenerator;
            
            Debug.Log($"[GameContext] ✅ Setup completado - Habitación: {currentContext.currentRoom.roomName}, Puertas: {currentContext.currentRoom.doors.Count}");
        }
        
        private void OnRoomChangedFromGenerator(RoomData newRoom)
        {
            // CRÍTICO: Forzar recálculo para asegurar datos frescos
            var freshRoom = roomBridge?.GetCurrentRoomFresh();
            if (freshRoom != null)
            {
                currentContext.currentRoom = freshRoom;
                currentContext.currentLocation = freshRoom.roomName;
                Debug.Log($"[GameContext] ✅ Actualizado con datos FRESCOS: {freshRoom.roomName} con {freshRoom.doors.Count} puertas");
            }
            else
            {
                // Fallback a los datos del evento
                currentContext.currentRoom = newRoom;
                currentContext.currentLocation = newRoom.roomName;
                Debug.LogWarning($"[GameContext] ⚠️ Usando datos del evento (GetCurrentRoomFresh falló)");
            }
            
            currentContext.nearbyObjects.Clear();
            currentContext.nearbyObjects.AddRange(currentContext.currentRoom.objects);
            
            currentContext.AddEvent($"Entraste a {currentContext.currentRoom.roomName}");
            
            Debug.Log($"[GameContext] Cambio de habitación: {currentContext.currentRoom.roomName}");
        }
        
        
        public GameContext GetCurrentContext()
        {
            return currentContext;
        }
        
        public void UpdateContext(GameContext context)
        {
            currentContext = context;
            Debug.Log($"[GameContext] Context updated: {context.currentLocation}");
        }
        
        public bool IsCommandValid(string commandId)
        {
            if (currentContext == null)
                return false;
                
            return currentContext.availableCommands.Contains(commandId) && 
                   currentContext.CanPerformAction();
        }
        
        public void ExecuteCommand(string commandId)
        {
            Debug.Log($"[GameContext] Executing command: {commandId}");
            
            // Obtener sistemas una sola vez
            var confirmationManager = ActionConfirmationManager.Instance;
            var fatigueSystem = FatigueSystem.Instance;
            
            // Si es comando de puerta
            if (commandId.StartsWith("usar_puerta_") && roomBridge != null)
            {
                string doorId = commandId.Replace("usar_puerta_", "");
                
                if (roomBridge.TryMoveThroughDoor(doorId, out string reason))
                {
                    currentContext.ConsumeActions();
                    currentContext.AddEvent("Atravesaste la puerta");
                    
                    // Confirmar con voz
                    if (confirmationManager != null)
                    {
                        // Obtener dirección de la puerta para confirmación
                        var door = currentContext.currentRoom?.doors.Find(d => d.doorId == doorId);
                        if (door != null)
                        {
                            confirmationManager.ConfirmUseDoor(door.direction);
                        }
                    }
                    
                    // Sincronizar con FatigueSystem
                    if (fatigueSystem != null)
                    {
                        fatigueSystem.AddFatigue(1);
                        SyncWithFatigueSystem();
                    }
                    
                    // roomBridge.OnRoomChanged se dispara automáticamente
                }
                else
                {
                    currentContext.AddEvent($"No puedes usar esa puerta: {reason}");
                    
                    // Confirmar con voz
                    if (confirmationManager != null)
                    {
                        confirmationManager.ConfirmDoorLocked(reason);
                    }
                }
                
                return;
            }
            
            // Validación para comandos normales
            if (!IsCommandValid(commandId))
            {
                Debug.LogWarning($"[GameContext] Invalid command: {commandId}");
                return;
            }
            
            // Consume actions
            currentContext.ConsumeActions();
            
            // Sincronizar con FatigueSystem (cada acción = +1 fatiga)
            if (fatigueSystem != null)
            {
                fatigueSystem.AddFatigue(1);
                SyncWithFatigueSystem();
            }
            
            // Add event
            currentContext.AddEvent($"Ejecutaste: {commandId}");
            
            // Handle specific commands
            switch (commandId)
            {
                // Comandos de movimiento direccional
                case "arriba":
                case "abajo":
                case "adelante":
                case "atras":
                case "atrás":
                case "frente":
                case "derecha":
                case "izquierda":
                    HandleDirectionalMovement(commandId);
                    break;
                case "comer":
                    {
                        var validator = CommandValidator.Instance;
                        if (validator != null && !validator.CanEat(out string errorMsg))
                        {
                            var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                            voiceSystem?.textToSpeech?.Speak(errorMsg);
                            Debug.Log($"[Command Rejected] comer: {errorMsg}");
                            return;
                        }
                        EatFood();
                    }
                    break;
                case "inspeccionar":
                    InspectLocation();
                    break;
                case "tomar":
                    TakeObject();
                    break;
                case "usar":
                    UseObject();
                    break;
                case "leer":
                    ReadObject();
                    break;
                case "dar":
                    GiveObject();
                    break;
                case "renacer":
                    AttemptRebirth();
                    break;
                case "despertar":
                    AttemptAwakening();
                    break;
                case "buscar":
                    {
                        var validator = CommandValidator.Instance;
                        if (validator != null && !validator.CanSearch(out string errorMsg))
                        {
                            var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                            voiceSystem?.textToSpeech?.Speak(errorMsg);
                            Debug.Log($"[Command Rejected] buscar: {errorMsg}");
                            return;
                        }
                        SearchCurrentRoom();
                    }
                    break;
            }
            
            // Procesar comandos de "tomar [item]"
            if (commandId.StartsWith("tomar_"))
            {
                string itemName = commandId.Substring(6); // Remover "tomar_"
                
                // Validar antes de ejecutar
                var validator = CommandValidator.Instance;
                if (validator != null && !validator.CanTakeItem(itemName, out string errorMsg))
                {
                    var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                    voiceSystem?.textToSpeech?.Speak(errorMsg);
                    Debug.Log($"[Command Rejected] tomar {itemName}: {errorMsg}");
                    return;
                }
                
                TakeItemFromRoom(itemName);
                return;
            }
            
            // Update time
            UpdateGameTime();
        }
        
        /// <summary>
        /// Maneja movimiento direccional usando DirectionalMovement
        /// </summary>
        private void HandleDirectionalMovement(string direction)
        {
            var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
            if (voiceSystem == null) return;
            
            // Buscar DirectionalMovement component
            var directionalMovement = FindFirstObjectByType<DirectionalMovement>();
            if (directionalMovement == null)
            {
                voiceSystem.textToSpeech.Speak("Sistema de movimiento direccional no disponible");
                Debug.LogWarning("[GameContextProvider] DirectionalMovement component no encontrado");
                return;
            }
            
            // Traducir dirección a puerta
            var door = directionalMovement.TranslateDirectionToDoor(direction);
            
            if (door == null)
            {
                voiceSystem.textToSpeech.Speak($"No hay una puerta hacia {direction}");
                Debug.Log($"[GameContextProvider] No hay puerta en dirección: {direction}");
                return;
            }
            
            // Verificar si la puerta está accesible
            if (!directionalMovement.CanMoveInDirection(direction, out string reason))
            {
                voiceSystem.textToSpeech.Speak(reason);
                Debug.Log($"[GameContextProvider] No se puede mover {direction}: {reason}");
                return;
            }
            
            // Intentar moverse a través de la puerta
            if (roomBridge == null)
            {
                voiceSystem.textToSpeech.Speak("Sistema de habitaciones no disponible");
                return;
            }
            
            if (roomBridge.TryMoveThroughDoor(door.doorId, out string failureReason))
            {
                // IMPORTANTE: Usar GetCurrentRoomFresh() para obtener puertas actualizadas
                var newRoom = roomBridge.GetCurrentRoomFresh();
                
                // Actualizar el contexto INMEDIATAMENTE con los datos frescos
                if (newRoom != null)
                {
                    currentContext.currentRoom = newRoom;
                    currentContext.currentLocation = newRoom.roomName;
                }
                
                // Confirmar movimiento
                var confirmManager = ActionConfirmationManager.Instance;
                if (confirmManager != null && newRoom != null)
                {
                    confirmManager.ConfirmMove(direction, newRoom.roomName);
                }
                
                // Trigger story event de entrada a la habitación
                var storyTrigger = FindFirstObjectByType<StoryEventTrigger>();
                if (storyTrigger != null && newRoom != null)
                {
                    storyTrigger.TriggerRoomEntry(newRoom.roomId);
                }
                
                Debug.Log($"[GameContextProvider] Movimiento exitoso {direction} → {newRoom?.roomName}");
            }
            else
            {
                voiceSystem.textToSpeech.Speak(failureReason);
                Debug.LogWarning($"[GameContextProvider] TryMoveThroughDoor falló: {failureReason}");
            }
        }
        
        
        private void EatFood()
        {
            // Usar sistema de consumibles real
            ConsumeFirstConsumable();
        }
        
        private void ConsumeFirstConsumable()
        {
            var confirmationManager = ActionConfirmationManager.Instance;
            
            // Verificar si ya tiene vida completa
            if (currentContext.health >= currentContext.maxHealth)
            {
                if (confirmationManager != null)
                {
                    confirmationManager.ConfirmHealthFull();
                }
                currentContext.AddEvent("Ya tienes la vida completa");
                return;
            }
            
            // Obtener sistemas necesarios
            var fatigueSys = FatigueSystem.Instance;
            var inventoryManager = RoomInventoryManager.Instance;
            
            // Buscar en inventario del jugador items de tipo Consumable
            if (inventoryManager != null)
            {
                foreach (string itemId in currentContext.inventory)
                {
                    // Buscar el item en TODAS las habitaciones para obtener su data
                    RoomItem consumable = inventoryManager.FindItemInAllRooms(itemId);
                    
                    if (consumable != null && consumable.IsConsumable())
                    {
                        // Consumir
                        currentContext.inventory.Remove(itemId);
                        currentContext.RestoreHealth(consumable.healthRestore);
                        currentContext.fatigue = Mathf.Max(0, currentContext.fatigue - consumable.fatigueReduction);
                        
                        currentContext.AddEvent($"Comiste {consumable.itemName}");
                        
                        // Confirmar con voz
                        if (confirmationManager != null)
                        {
                            confirmationManager.ConfirmEat(consumable.itemName, consumable.healthRestore, consumable.fatigueReduction);
                        }
                        
                        // Sincronizar con FatigueSystem si existe
                        if (fatigueSys != null)
                        {
                            SyncWithFatigueSystem();
                        }
                        
                        Debug.Log($"[GameContext] Consumible usado: {consumable.itemName} (+{consumable.healthRestore} vida, -{consumable.fatigueReduction} fatiga)");
                        return;
                    }
                }
            }
            
            // No hay consumibles
            currentContext.AddEvent("No tienes comida para comer");
            if (confirmationManager != null)
            {
                confirmationManager.ConfirmNoConsumables();
            }
        }
        
        /// <summary>
        /// Sincroniza estado desde FatigueSystem (vida/fatiga)
        /// </summary>
        private void SyncWithFatigueSystem()
        {
            var fatigueSystem = FatigueSystem.Instance;
            if (fatigueSystem != null && fatigueSystem.PlayerLives != null)
            {
                currentContext.health = fatigueSystem.PlayerLives.currentLives;
                currentContext.maxHealth = fatigueSystem.PlayerLives.totalLives;
                currentContext.fatigue = fatigueSystem.NivelFatiga;
            }
        }
        
        private void InspectLocation()
        {
            string inspection = $"Inspeccionas {currentContext.currentLocation}. ";
            
            if (currentContext.nearbyObjects.Count > 0)
            {
                inspection += $"Ves: {string.Join(", ", currentContext.nearbyObjects)}.";
            }
            else
            {
                inspection += "No hay nada notable aquí.";
            }
            
            currentContext.AddEvent(inspection);
        }
        
        private void TakeObject()
        {
            if (currentContext.nearbyObjects.Count > 0)
            {
                string objectToTake = currentContext.nearbyObjects[0];
                currentContext.nearbyObjects.RemoveAt(0);
                currentContext.inventory.Add(objectToTake);
                currentContext.AddEvent($"Tomaste: {objectToTake}");
            }
            else
            {
                currentContext.AddEvent("No hay nada que tomar aquí");
            }
        }
        
        private void UseObject()
        {
            if (currentContext.inventory.Count > 0)
            {
                string objectToUse = currentContext.inventory[0];
                currentContext.AddEvent($"Usaste: {objectToUse}");
            }
            else
            {
                currentContext.AddEvent("No tienes nada que usar");
            }
        }
        
        private void ReadObject()
        {
            if (currentContext.nearbyObjects.Contains("libros"))
            {
                currentContext.AddEvent("Lees un libro. Contiene información sobre la historia de la casa.");
            }
            else
            {
                currentContext.AddEvent("No hay nada que leer aquí");
            }
        }
        
        private void GiveObject()
        {
            if (currentContext.inventory.Count > 0)
            {
                string objectToGive = currentContext.inventory[0];
                currentContext.inventory.RemoveAt(0);
                currentContext.AddEvent($"Diste: {objectToGive}");
            }
            else
            {
                currentContext.AddEvent("No tienes nada que dar");
            }
        }
        
        private void AttemptRebirth()
        {
            if (currentContext.inventory.Contains("flor de loto"))
            {
                currentContext.AddEvent("¡Usaste la flor de loto y lograste renacer! ¡Has ganado!");
                // Game win condition
            }
            else
            {
                currentContext.AddEvent("No tienes la flor de loto necesaria para renacer");
            }
        }
        
        private void AttemptAwakening()
        {
            currentContext.AddEvent("Intentas despertar, pero algo te mantiene atrapado en este lugar");
        }
        
        #region Item Actions
        
        private void SearchCurrentRoom()
        {
            var confirmationManager = ActionConfirmationManager.Instance;
            
            if (roomBridge != null)
            {
                var inventoryManager = RoomInventoryManager.Instance;
                if (inventoryManager != null)
                {
                    var foundItems = inventoryManager.SearchRoom(currentContext.currentRoom.roomId);
                
                if (foundItems.Count > 0)
                {
                    string itemNames = string.Join(", ", foundItems.ConvertAll(i => i.itemName));
                    currentContext.AddEvent($"Buscaste y encontraste: {itemNames}");
                    
                    // Confirmar con voz
                    if (confirmationManager != null)
                    {
                        List<string> itemNamesList = foundItems.ConvertAll(i => i.itemName);
                        confirmationManager.ConfirmSearch(itemNamesList);
                    }
                    
                    // Actualizar objetos visibles en currentRoom
                    foreach (var item in foundItems)
                    {
                        if (!currentContext.currentRoom.objects.Contains(item.itemName))
                        {
                            currentContext.currentRoom.objects.Add(item.itemName);
                        }
                    }
                    
                    // Limpiar cache del bridge para que recargue los objetos
                    if (roomBridge != null)
                    {
                        roomBridge.ClearCache();
                    }
                }
                else
                {
                    currentContext.AddEvent("Buscaste cuidadosamente pero no encontraste nada oculto");
                    
                    // Confirmar con voz
                    if (confirmationManager != null)
                    {
                        confirmationManager.ConfirmSearch(null);
                    }
                }
                
                // Trigger story event de inspección
                var storyTrigger = FindFirstObjectByType<StoryEventTrigger>();
                if (storyTrigger != null && currentContext.currentRoom != null)
                {
                    storyTrigger.TriggerInspect(currentContext.currentRoom.roomId);
                }
                }
            }
            else
            {
                // Modo demo simple
                currentContext.AddEvent("Buscaste en la habitación");
            }
        }
        
        private void TakeItemFromRoom(string itemName)
        {
            var confirmationManager = ActionConfirmationManager.Instance;
            
            if (roomBridge != null)
            {
                var inventoryManager = RoomInventoryManager.Instance;
                if (inventoryManager != null)
                {
                    // Buscar item por nombre en la habitación actual
                    var roomInventory = inventoryManager.GetRoomInventory(currentContext.currentRoom.roomId);
                    var item = roomInventory.items.Find(i => 
                        i.itemName.ToLower().Contains(itemName.ToLower()) && i.CanBeCollected()
                    );
                    
                    if (item != null)
                    {
                        if (inventoryManager.TryTakeItem(currentContext.currentRoom.roomId, item.itemId, out RoomItem takenItem))
                        {
                            // Agregar al inventario del jugador
                            currentContext.inventory.Add(takenItem.itemId);
                            currentContext.AddEvent($"Tomaste: {takenItem.itemName}");
                            
                            // Confirmar con voz
                            if (confirmationManager != null)
                            {
                                confirmationManager.ConfirmTakeItem(takenItem.itemName, takenItem.itemType);
                            }
                            
                            // Remover de objetos visibles
                            currentContext.currentRoom.objects.Remove(takenItem.itemName);
                            currentContext.nearbyObjects.Remove(takenItem.itemName);
                            
                            // Limpiar cache del bridge
                            if (roomBridge != null)
                            {
                                roomBridge.ClearCache();
                            }
                            
                            // Trigger story event de "tomar item"
                            var storyTrigger = FindFirstObjectByType<StoryEventTrigger>();
                            if (storyTrigger != null && currentContext.currentRoom != null)
                            {
                                storyTrigger.TriggerTakeItem(currentContext.currentRoom.roomId, takenItem.itemId);
                            }
                        }
                    }
                    else
                    {
                        currentContext.AddEvent($"No puedes tomar {itemName}");
                        
                        // Confirmar con voz
                        if (confirmationManager != null)
                        {
                            confirmationManager.ConfirmItemNotFound(itemName);
                        }
                    }
                }
            }
            else
            {
                // Lógica demo existente
                TakeObject();
            }
        }
        
        #endregion
        
        private void UpdateGameTime()
        {
            // Simulate time passing
            currentContext.timeRemaining -= 0.1f;
            
            if (currentContext.timeRemaining <= 0)
            {
                currentContext.timeRemaining = 0;
                currentContext.AddEvent("¡Se acabó el tiempo! La casa te ha atrapado para siempre.");
                // Game over condition
            }
        }
        
        [ContextMenu("Add Food")]
        public void DemoAddFood()
        {
            currentContext.inventory.Add("comida");
            Debug.Log("[GameContext] Added food to inventory");
        }
        
        [ContextMenu("Take Damage")]
        public void DemoTakeDamage()
        {
            currentContext.health = Mathf.Max(0, currentContext.health - 1);
            Debug.Log($"[GameContext] Health reduced to {currentContext.health}");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from room bridge events
            if (roomBridge != null)
            {
                roomBridge.OnRoomChanged -= OnRoomChangedFromGenerator;
            }
        }
    }
}