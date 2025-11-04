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
        
        [Header("Reading State")]
        private Dictionary<string, int> roomReadingIndex = new Dictionary<string, int>(); // Tracking de qué libro sigue por habitación
        
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
                case "hora":
                case "tiempo":
                case "reloj":
                    CheckCurrentTime();
                    break;
                case "volumen_subir":
                    AdjustVolume(0.1f);
                    break;
                case "volumen_bajar":
                    AdjustVolume(-0.1f);
                    break;
                case "volumen_silenciar":
                    MuteAudio(true);
                    break;
                case "volumen_activar":
                    MuteAudio(false);
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
                // REMOVIDO: StoryEventTrigger ya se dispara automáticamente vía evento OnRoomChanged
                // No es necesario llamarlo manualmente aquí (causaba eventos duplicados)
                
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
                        Debug.Log($"[GameContext] Consumiendo: {consumable.itemName} (Health +{consumable.healthRestore}, Fatigue -{consumable.fatigueReduction})");
                        
                        // Remover del inventario
                        currentContext.inventory.Remove(itemId);
                        
                        // Aplicar DIRECTAMENTE a FatigueSystem (authoritative)
                        if (fatigueSys != null && fatigueSys.PlayerLives != null)
                        {
                            // Restaurar salud
                            if (consumable.healthRestore > 0)
                            {
                                fatigueSys.PlayerLives.currentLives = Mathf.Min(
                                    fatigueSys.PlayerLives.currentLives + consumable.healthRestore,
                                    fatigueSys.PlayerLives.totalLives
                                );
                                Debug.Log($"[GameContext] Salud actualizada: {fatigueSys.PlayerLives.currentLives}/{fatigueSys.PlayerLives.totalLives}");
                            }
                            
                            // Reducir fatiga DIRECTAMENTE
                            if (consumable.fatigueReduction > 0)
                            {
                                fatigueSys.ReduceFatigue(consumable.fatigueReduction);
                                Debug.Log($"[GameContext] Fatiga reducida en {consumable.fatigueReduction} puntos");
                            }
                        }
                        
                        // Sincronizar de vuelta al contexto
                        SyncWithFatigueSystem();
                        
                        currentContext.AddEvent($"Consumiste {consumable.itemName}");
                        
                        // Confirmar con voz
                        if (confirmationManager != null)
                        {
                            confirmationManager.ConfirmEat(consumable.itemName, consumable.healthRestore, consumable.fatigueReduction);
                        }
                        
                        Debug.Log($"[GameContext] ✅ Consumible aplicado: {consumable.itemName} | Salud: {currentContext.health}, Fatiga: {currentContext.fatigue}");
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
            // DISPARAR eventos de "inspect" en StoryEventTrigger
            // Los eventos "inspect" tienen sus propias narraciones detalladas
            var storyTrigger = FindFirstObjectByType<StoryEventTrigger>();
            if (storyTrigger != null)
            {
                string currentRoomId = currentContext.currentRoom?.roomId ?? "";
                if (!string.IsNullOrEmpty(currentRoomId))
                {
                    storyTrigger.TriggerInspect(currentRoomId);
                    Debug.Log($"[GameContext] 🔍 Evento 'inspect' disparado en {currentRoomId}");
                    
                    // NO narrar lista genérica después de evento "inspect"
                    // El evento ya proporciona una narración detallada y específica
                    currentContext.AddEvent($"Inspeccionas {currentContext.currentLocation}.");
                    return;
                }
            }
            
            // FALLBACK: Si NO hay evento "inspect", narrar lista genérica de objetos
            var inventoryManager = RoomInventoryManager.Instance;
            string roomId = currentContext.currentRoom?.roomId ?? "";
            
            string inspection = $"Inspeccionas {currentContext.currentLocation}. ";
            
            // Obtener objetos visibles del inventario
            if (inventoryManager != null && !string.IsNullOrEmpty(roomId))
            {
                var inventory = inventoryManager.GetRoomInventory(roomId);
                var visibleItems = inventory.GetVisibleItems();
                
                if (visibleItems.Count > 0)
                {
                    var itemNames = visibleItems.ConvertAll(i => i.itemName);
                    inspection += $"Ves: {string.Join(", ", itemNames)}.";
                    
                    // Narrar con voz (solo si NO hubo evento "inspect")
                    var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                    if (voiceSystem?.textToSpeech != null)
                    {
                        Debug.Log($"[GameContext] 🔍 Narrando lista de objetos: {inspection}");
                        voiceSystem.textToSpeech.Speak(inspection, VoiceSystem.Core.Interfaces.TTSPriority.Normal);
                    }
                }
                else if (currentContext.nearbyObjects.Count > 0)
                {
                    inspection += $"Ves: {string.Join(", ", currentContext.nearbyObjects)}.";
                }
                else
                {
                    inspection += "No hay nada notable aquí.";
                }
            }
            else if (currentContext.nearbyObjects.Count > 0)
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
            // NUEVO: Usar RoomInventoryManager en lugar de nearbyObjects
            var inventoryManager = RoomInventoryManager.Instance;
            var roomBridge = RoomSystemBridge.Instance;
            
            if (inventoryManager == null || roomBridge == null)
            {
                currentContext.AddEvent("No puedes tomar nada ahora");
                return;
            }
            
            string currentRoomId = currentContext.currentRoom?.roomId ?? "";
            
            // Obtener objetos visibles que se pueden tomar
            var inventory = inventoryManager.GetRoomInventory(currentRoomId);
            var takableItems = inventory.GetVisibleItems().FindAll(i => !i.isCollected);
            
            if (takableItems.Count == 0)
            {
                currentContext.AddEvent("No hay nada que tomar aquí");
                
                // Narrar con voz
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                voiceSystem?.textToSpeech?.Speak("No hay nada que tomar aquí");
                return;
            }
            
            // Tomar el primer objeto disponible
            var itemToTake = takableItems[0];
            if (inventoryManager.TryTakeItem(currentRoomId, itemToTake.itemId, out RoomItem item, out string failReason))
            {
                currentContext.AddEvent($"Tomaste: {item.itemName}");
                
                // Narrar confirmación
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                voiceSystem?.textToSpeech?.Speak($"Tomaste {item.itemName}");
                
                Debug.Log($"[GameContext] ✅ Objeto recogido: {item.itemName}");
            }
            else
            {
                currentContext.AddEvent($"No puedes tomar eso: {failReason}");
                
                // Narrar razón del fallo
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                voiceSystem?.textToSpeech?.Speak(failReason);
                
                Debug.LogWarning($"[GameContext] ❌ No se pudo tomar {itemToTake.itemName}: {failReason}");
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
            // NUEVO: Usar RoomInventoryManager para inspeccionar objetos
            var inventoryManager = RoomInventoryManager.Instance;
            var roomBridge = RoomSystemBridge.Instance;
            
            if (inventoryManager == null || roomBridge == null)
            {
                currentContext.AddEvent("No puedes leer nada ahora");
                return;
            }
            
            string currentRoomId = currentContext.currentRoom?.roomId ?? "";
            
            // Buscar objetos legibles en la habitación
            var inventory = inventoryManager.GetRoomInventory(currentRoomId);
            var readableItems = inventory.GetVisibleItems().FindAll(i => 
                i.itemId.Contains("diary") || i.itemId.Contains("book") || i.itemId.Contains("note") || i.itemId.Contains("readable") || i.itemId.Contains("letter"));
            
            if (readableItems.Count == 0)
            {
                currentContext.AddEvent("No hay nada que leer aquí");
                
                // Narrar con voz
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                voiceSystem?.textToSpeech?.Speak("No hay nada que leer aquí");
                return;
            }
            
            // Si hay múltiples libros, leer el siguiente en secuencia
            if (readableItems.Count > 1)
            {
                // Obtener índice actual de lectura para esta habitación
                if (!roomReadingIndex.ContainsKey(currentRoomId))
                {
                    roomReadingIndex[currentRoomId] = 0;
                }
                
                int currentIndex = roomReadingIndex[currentRoomId];
                
                // Si ya leyó todos los libros, informar
                if (currentIndex >= readableItems.Count)
                {
                    var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                    voiceSystem?.textToSpeech?.Speak("Ya leíste todos los libros aquí.", VoiceSystem.Core.Interfaces.TTSPriority.Normal);
                    return;
                }
                
                // Leer el libro actual
                var bookToRead = readableItems[currentIndex];
                ReadSpecificItem(bookToRead, currentRoomId, inventoryManager);
                
                // Incrementar índice para la próxima lectura
                roomReadingIndex[currentRoomId] = currentIndex + 1;
                
                // Informar cuántos libros quedan
                int remaining = readableItems.Count - (currentIndex + 1);
                if (remaining > 0)
                {
                    Debug.Log($"[GameContext] 📚 Quedan {remaining} libros por leer en esta habitación");
                }
            }
            else
            {
                // Leer el único objeto legible
                ReadSpecificItem(readableItems[0], currentRoomId, inventoryManager);
            }
        }
        
        /// <summary>
        /// Lee un objeto específico
        /// </summary>
        private void ReadSpecificItem(RoomItem itemToRead, string roomId, RoomInventoryManager inventoryManager)
        {
            if (inventoryManager.TryInspectItem(roomId, itemToRead.itemId, out RoomItem item, out string description))
            {
                currentContext.AddEvent($"Lees {item.itemName}: {description}");
                
                // Narrar con voz (prioridad NORMAL para no bloquear el juego)
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                if (voiceSystem?.textToSpeech != null)
                {
                    Debug.Log($"[GameContext] 📖 Narrando lectura de {item.itemName} ({description.Length} caracteres)");
                    
                    // Primero anunciar el título del libro con prioridad Urgent
                    string announcement = $"Lees el {item.itemName}.";
                    voiceSystem.textToSpeech.Speak(announcement, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                    
                    // Luego narrar el contenido con prioridad Normal (para no bloquear)
                    voiceSystem.textToSpeech.Speak(description, VoiceSystem.Core.Interfaces.TTSPriority.Normal);
                }
                else
                {
                    Debug.LogWarning("[GameContext] ⚠️ VoiceSystem o TTS es null, no se puede narrar");
                }
            }
            else
            {
                currentContext.AddEvent("No puedes leer eso ahora");
            }
        }
        
        private void GiveObject()
        {
            // Verificar si el jugador está intentando dar la flor al retrato
            var playerState = PlayerStateManager.Instance;
            
            if (playerState != null && playerState.HasItem("lotus_flower_alive"))
            {
                // El jugador tiene la flor viva
                var currentRoom = roomBridge?.GetCurrentRoom();
                
                if (currentRoom != null && currentRoom.roomId == "room_2") // Sala Principal
                {
                    // Está en la Sala Principal con la flor
                    currentContext.AddEvent("Extiendes la flor de loto viva hacia el retrato familiar. Los pétalos brillan con intensidad.");
                    
                    // Narrar
                    var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                    voiceSystem?.textToSpeech?.Speak("Extiendes la flor hacia el retrato. Los pétalos tocan la pintura y comienzan a brillar. Ahora di Renacer para completar el ritual.", VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                    
                    // Marcar que dio la flor (pero NO remover del inventario - se necesita para la victoria)
                    playerState.SetEventFlag("gave_flower_to_portrait");
                    
                    Debug.Log("[GameContext] ✅ Flor dada al retrato - Flag 'gave_flower_to_portrait' activado");
                    
                    return;
                }
            }
            
            // Comportamiento por defecto
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
            var playerState = PlayerStateManager.Instance;
            var winManager = WinConditionManager.Instance;
            
            // Verificar que haya dado la flor al retrato primero
            if (playerState != null && playerState.HasSeenEvent("gave_flower_to_portrait"))
            {
                // Intentar victoria
                if (winManager != null)
                {
                    winManager.AttemptVictory();
                }
                else
                {
                    currentContext.AddEvent("¡Usaste la flor de loto y lograste renacer! ¡Has ganado!");
                }
            }
            else if (playerState != null && playerState.HasItem("lotus_flower_alive"))
            {
                currentContext.AddEvent("Primero debes dar la flor al retrato familiar. Di Dar.");
            }
            else
            {
                currentContext.AddEvent("No tienes la flor de loto necesaria para renacer");
            }
        }
        
        private void AttemptAwakening()
        {
            // Verificar si estamos en el diálogo del niño en el sótano
            var basementDialogue = FindFirstObjectByType<BasementDoorDialogue>();
            if (basementDialogue != null)
            {
                // Llamar al final malo
                basementDialogue.OnPlayerSaidDespertar();
                currentContext.AddEvent("Dijiste 'Despertar'... el niño responde");
            }
            else
            {
                currentContext.AddEvent("Intentas despertar, pero algo te mantiene atrapado en este lugar");
            }
        }
        
        /// <summary>
        /// Verificar qué hora es en el juego
        /// </summary>
        private void CheckCurrentTime()
        {
            Debug.Log("[GameContext] CheckCurrentTime() llamado");
            
            var gameTimer = GameTimer.Instance;
            if (gameTimer != null)
            {
                Debug.Log($"[GameContext] GameTimer encontrado. IsRunning: {gameTimer.IsRunning()}");
                
                string currentTime = gameTimer.GetCurrentTimeString();
                int currentHour = gameTimer.GetCurrentHour();
                
                Debug.Log($"[GameContext] Hora actual: {currentTime} ({currentHour})");
                
                string narration = $"El reloj marca las {currentTime}.";
                
                // Añadir contexto especial para ciertas horas
                if (currentHour == 2)
                {
                    narration += " Una sensación extraña te recorre. Es la hora en que los muertos hablan.";
                }
                else if (currentHour >= 0 && currentHour < 6)
                {
                    narration += " La madrugada avanza. El amanecer se acerca.";
                }
                else if (currentHour >= 18 && currentHour < 24)
                {
                    narration += " La noche acaba de empezar.";
                }
                
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                if (voiceSystem?.textToSpeech != null)
                {
                    Debug.Log($"[GameContext] Hablando: {narration}");
                    voiceSystem.textToSpeech.Speak(narration, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                }
                else
                {
                    Debug.LogWarning("[GameContext] VoiceSystem o TTS es null");
                }
                
                currentContext.AddEvent($"Revisaste el reloj: {currentTime}");
                
                Debug.Log($"[GameContext] 🕐 Jugador revisó la hora: {currentTime}");
            }
            else
            {
                Debug.LogError("[GameContext] GameTimer.Instance es NULL!");
                currentContext.AddEvent("No puedes ver la hora");
                
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                if (voiceSystem?.textToSpeech != null)
                {
                    voiceSystem.textToSpeech.Speak("No puedes ver la hora", VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                }
            }
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
                        if (inventoryManager.TryTakeItem(currentContext.currentRoom.roomId, item.itemId, out RoomItem takenItem, out string failReason))
                        {
                            // Agregar al inventario del jugador
                            currentContext.inventory.Add(takenItem.itemId);
                            currentContext.AddEvent($"Tomaste: {takenItem.itemName}");
                            
                            // Confirmar con voz
                            if (confirmationManager != null)
                            {
                                confirmationManager.ConfirmTakeItem(takenItem.itemName, takenItem.itemType);
                            }
                        }
                        else
                        {
                            // No se pudo tomar - narrar razón
                            var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                            if (voiceSystem != null && !string.IsNullOrEmpty(failReason))
                            {
                                voiceSystem.textToSpeech?.Speak(failReason);
                                Debug.Log($"[GameContext] No se pudo tomar {itemName}: {failReason}");
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
        
        #region Audio Control Methods
        
        private void AdjustVolume(float delta)
        {
            var soundManager = Audio.SoundManager.Instance;
            if (soundManager != null)
            {
                soundManager.AdjustMasterVolume(delta);
                float currentVolume = soundManager.GetMasterVolume();
                
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                int displayVolume = Mathf.FloorToInt(currentVolume * 100);
                voiceSystem?.textToSpeech?.Speak($"Volumen al {displayVolume} por ciento", VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                
                Debug.Log($"[GameContext] Volumen: {currentVolume:F2}");
            }
            else
            {
                Debug.LogWarning("[GameContext] SoundManager no disponible para ajustar volumen");
            }
        }
        
        private void MuteAudio(bool mute)
        {
            var soundManager = Audio.SoundManager.Instance;
            if (soundManager != null)
            {
                soundManager.SetMute(mute);
                
                var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
                string message = mute ? "Audio silenciado" : "Audio activado";
                voiceSystem?.textToSpeech?.Speak(message, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                
                Debug.Log($"[GameContext] Audio: {(mute ? "Silenciado" : "Activado")}");
            }
            else
            {
                Debug.LogWarning("[GameContext] SoundManager no disponible para silenciar audio");
            }
        }
        
        #endregion
        
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