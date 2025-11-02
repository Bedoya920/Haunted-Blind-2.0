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
        [Header("Demo Configuration")]
        public bool useDemoData = true;
        
        [Header("Room System Integration")]
        [SerializeField] private RoomSystemBridge roomBridge;
        [SerializeField] private bool useRealRoomGenerator = false; // Toggle demo vs real
        
        [Header("Game References")]
        public EventManager eventManager;
        
        // Current context
        private GameContext currentContext;
        
        // Demo data for testing
        private string[] demoLocations = {
            "Sala Principal",
            "Biblioteca", 
            "Comedor",
            "Cocina",
            "Habitación Principal",
            "Habitación de los Niños",
            "Baño",
            "Sótano"
        };
        
        private int currentLocationIndex = 0;
        
        private void Awake()
        {
            // Initialize context
            currentContext = new GameContext();
            
            // Set up demo data
            if (useDemoData)
            {
                SetupDemoContext();
            }
        }
        
        private void SetupDemoContext()
        {
            // Set up basic stats
            currentContext.health = 5;
            currentContext.maxHealth = 5;
            currentContext.fatigue = 0;
            currentContext.actions = 10;
            currentContext.maxActions = 10;
            currentContext.gameTime = "2:00 AM";
            currentContext.timeRemaining = 12f;
            
            // Demo inventory
            currentContext.inventory.Clear();
            currentContext.inventory.Add("comida");
            currentContext.inventory.Add("agua");
            
            // Decidir fuente de habitaciones
            if (useRealRoomGenerator && roomBridge != null)
            {
                SetupFromRoomGenerator();
            }
            else
            {
                // Setup room system with demo data
                SetupDemoRooms();
                
                // Set current location from currentRoom
                if (currentContext.currentRoom != null)
                {
                    currentContext.currentLocation = currentContext.currentRoom.roomName;
                    
                    // Update nearby objects from room
                    currentContext.nearbyObjects.Clear();
                    if (currentContext.currentRoom.objects != null)
                    {
                        currentContext.nearbyObjects.AddRange(currentContext.currentRoom.objects);
                    }
                }
                else
                {
                    currentContext.currentLocation = demoLocations[currentLocationIndex];
                    UpdateNearbyObjects();
                }
            }
            
            // Demo recent events
            currentContext.recentEvents.Clear();
            currentContext.AddEvent("Llegaste a la casa");
            currentContext.AddEvent("El reloj marca las 2 AM");
            
            Debug.Log($"[GameContext] Demo context set up for room: {currentContext.currentRoom?.roomName ?? currentContext.currentLocation}");
        }
        
        private void SetupFromRoomGenerator()
        {
            // Obtener habitación actual del generador
            currentContext.currentRoom = roomBridge.GetCurrentRoom();
            
            if (currentContext.currentRoom != null)
            {
                currentContext.currentLocation = currentContext.currentRoom.roomName;
                currentContext.nearbyObjects.Clear();
                currentContext.nearbyObjects.AddRange(currentContext.currentRoom.objects);
            }
            
            // Sincronizar estado del jugador
            roomBridge.SyncPlayerStateToContext(currentContext);
            
            // Suscribirse a cambios de habitación
            roomBridge.OnRoomChanged += OnRoomChangedFromGenerator;
            
            Debug.Log($"[GameContext] Usando RoomGenerator real: {currentContext.currentRoom?.roomName}");
        }
        
        private void OnRoomChangedFromGenerator(RoomData newRoom)
        {
            currentContext.currentRoom = newRoom;
            currentContext.currentLocation = newRoom.roomName;
            currentContext.nearbyObjects.Clear();
            currentContext.nearbyObjects.AddRange(newRoom.objects);
            
            currentContext.AddEvent($"Entraste a {newRoom.roomName}");
            
            Debug.Log($"[GameContext] Cambio de habitación: {newRoom.roomName}");
        }
        
        private void SetupDemoRooms()
        {
            // Create demo rooms with doors
            var salaRoom = new RoomData
            {
                roomId = "room_sala",
                roomName = "Sala Principal",
                shortDescription = "Una habitación amplia y polvorienta",
                longDescription = "Una sala amplia con muebles cubiertos de polvo. Las cortinas rasgadas dejan pasar rayos de luz tenue. Hueles a madera vieja y polvo. Escuchas el eco de tus pasos.",
                doors = new List<DoorData>
                {
                    new DoorData 
                    { 
                        doorId = "door_sala_north", 
                        doorName = "Puerta al Comedor", 
                        direction = "norte", 
                        isLocked = false, 
                        leadsToRoomId = "room_comedor",
                        description = "Una puerta de madera oscura con tallados ornamentales"
                    },
                    new DoorData 
                    { 
                        doorId = "door_sala_east", 
                        doorName = "Puerta a la Biblioteca", 
                        direction = "este", 
                        isLocked = true, 
                        leadsToRoomId = "room_biblioteca",
                        keyItemId = "llave_biblioteca",
                        description = "Una puerta pesada con cerradura dorada. Está firmemente cerrada"
                    },
                    new DoorData 
                    { 
                        doorId = "door_sala_west", 
                        doorName = "Puerta a la Cocina", 
                        direction = "oeste", 
                        isLocked = false, 
                        leadsToRoomId = "room_cocina",
                        description = "Una puerta entreabiert que conduce a la cocina"
                    }
                },
                objects = new List<string> { "cuadro familiar", "piano", "reloj" }
            };
            
            var comedorRoom = new RoomData
            {
                roomId = "room_comedor",
                roomName = "Comedor",
                shortDescription = "Una mesa larga cubierta de polvo",
                longDescription = "Una gran mesa de madera domina el centro de la habitación. Los platos y cubiertos están desordenados, como si alguien hubiera abandonado la cena repentinamente.",
                doors = new List<DoorData>
                {
                    new DoorData 
                    { 
                        doorId = "door_comedor_south", 
                        doorName = "Puerta a la Sala", 
                        direction = "sur", 
                        isLocked = false, 
                        leadsToRoomId = "room_sala",
                        description = "La puerta por donde entraste"
                    },
                    new DoorData 
                    { 
                        doorId = "door_comedor_east", 
                        doorName = "Puerta a la Habitación Principal", 
                        direction = "este", 
                        isLocked = false, 
                        leadsToRoomId = "room_habitacion_principal",
                        description = "Una puerta elegante con marco dorado"
                    }
                },
                objects = new List<string> { "mesa", "sillas", "vajilla" }
            };
            
            var bibliotecaRoom = new RoomData
            {
                roomId = "room_biblioteca",
                roomName = "Biblioteca",
                shortDescription = "Una habitación llena de libros antiguos",
                longDescription = "Estantes altos repletos de libros antiguos cubren las paredes. El aire huele a papel viejo y humedad. Una lámpara de pie proyecta sombras inquietantes.",
                doors = new List<DoorData>
                {
                    new DoorData 
                    { 
                        doorId = "door_biblioteca_west", 
                        doorName = "Puerta a la Sala", 
                        direction = "oeste", 
                        isLocked = false, 
                        leadsToRoomId = "room_sala",
                        description = "La puerta por donde entraste"
                    }
                },
                objects = new List<string> { "libros", "estantes", "mesa", "llave_sotano" }
            };
            
            var cocinaRoom = new RoomData
            {
                roomId = "room_cocina",
                roomName = "Cocina",
                shortDescription = "Un lugar oscuro con utensilios oxidados",
                longDescription = "Una cocina abandonada con ollas y sartenes oxidadas. El refrigerador está abierto y vacío. El aire es pesado y húmedo.",
                doors = new List<DoorData>
                {
                    new DoorData 
                    { 
                        doorId = "door_cocina_east", 
                        doorName = "Puerta a la Sala", 
                        direction = "este", 
                        isLocked = false, 
                        leadsToRoomId = "room_sala",
                        description = "La puerta por donde entraste"
                    },
                    new DoorData 
                    { 
                        doorId = "door_cocina_down", 
                        doorName = "Puerta al Sótano", 
                        direction = "abajo", 
                        isLocked = true, 
                        leadsToRoomId = "room_sotano",
                        keyItemId = "llave_sotano",
                        description = "Una puerta de metal pesada con cerrojo oxidado. Algo susurra detrás de ella"
                    }
                },
                objects = new List<string> { "estufa", "refrigerador", "utensilios" }
            };
            
            // Set current room
            currentContext.currentRoom = salaRoom;
            
            // Add all rooms to dictionary for future reference
            currentContext.allRooms.Clear();
            currentContext.allRooms.Add(salaRoom.roomId, salaRoom);
            currentContext.allRooms.Add(comedorRoom.roomId, comedorRoom);
            currentContext.allRooms.Add(bibliotecaRoom.roomId, bibliotecaRoom);
            currentContext.allRooms.Add(cocinaRoom.roomId, cocinaRoom);
        }
        
        private void UpdateNearbyObjects()
        {
            currentContext.nearbyObjects.Clear();
            
            switch (currentContext.currentLocation)
            {
                case "Sala Principal":
                    currentContext.nearbyObjects.AddRange(new string[] { "cuadro familiar", "piano", "reloj" });
                    break;
                case "Biblioteca":
                    currentContext.nearbyObjects.AddRange(new string[] { "libros", "estantes", "mesa" });
                    break;
                case "Comedor":
                    currentContext.nearbyObjects.AddRange(new string[] { "mesa", "sillas", "vajilla" });
                    break;
                case "Cocina":
                    currentContext.nearbyObjects.AddRange(new string[] { "estufa", "refrigerador", "utensilios" });
                    break;
                case "Habitación Principal":
                    currentContext.nearbyObjects.AddRange(new string[] { "cama", "espejo", "flor marchita" });
                    break;
                case "Habitación de los Niños":
                    currentContext.nearbyObjects.AddRange(new string[] { "camas", "juguetes", "oso rojo" });
                    break;
                case "Baño":
                    currentContext.nearbyObjects.AddRange(new string[] { "bañera", "espejo", "lavabo" });
                    break;
                case "Sótano":
                    currentContext.nearbyObjects.AddRange(new string[] { "puerta", "cerrojo" });
                    break;
            }
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
            
            // Si es comando de puerta y estamos usando generador real
            if (commandId.StartsWith("usar_puerta_") && useRealRoomGenerator && roomBridge != null)
            {
                string doorId = commandId.Replace("usar_puerta_", "");
                
                if (roomBridge.TryMoveThroughDoor(doorId, out string reason))
                {
                    currentContext.ConsumeActions();
                    currentContext.AddEvent("Atravesaste la puerta");
                    // roomBridge.OnRoomChanged se dispara automáticamente
                }
                else
                {
                    currentContext.AddEvent($"No puedes usar esa puerta: {reason}");
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
            
            // Add event
            currentContext.AddEvent($"Ejecutaste: {commandId}");
            
            // Handle specific commands
            switch (commandId)
            {
                case "adelante":
                    MoveToNextLocation();
                    break;
                case "atrás":
                    MoveToPreviousLocation();
                    break;
                case "comer":
                    EatFood();
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
                    SearchCurrentRoom();
                    break;
            }
            
            // Procesar comandos de "tomar [item]"
            if (commandId.StartsWith("tomar_"))
            {
                string itemName = commandId.Substring(6); // Remover "tomar_"
                TakeItemFromRoom(itemName);
                return;
            }
            
            // Update time
            UpdateGameTime();
        }
        
        private void MoveToNextLocation()
        {
            currentLocationIndex = (currentLocationIndex + 1) % demoLocations.Length;
            currentContext.currentLocation = demoLocations[currentLocationIndex];
            UpdateNearbyObjects();
            currentContext.AddEvent($"Te moviste a {currentContext.currentLocation}");
        }
        
        private void MoveToPreviousLocation()
        {
            currentLocationIndex = (currentLocationIndex - 1 + demoLocations.Length) % demoLocations.Length;
            currentContext.currentLocation = demoLocations[currentLocationIndex];
            UpdateNearbyObjects();
            currentContext.AddEvent($"Te moviste a {currentContext.currentLocation}");
        }
        
        private void EatFood()
        {
            // Intentar sistema nuevo de items
            if (useRealRoomGenerator)
            {
                ConsumeFirstConsumable();
            }
            else
            {
                // Modo demo legacy
                if (currentContext.inventory.Contains("comida"))
                {
                    currentContext.inventory.Remove("comida");
                    currentContext.RestoreHealth(1);
                    currentContext.AddEvent("Comiste comida y recuperaste 1 de vida");
                }
                else
                {
                    currentContext.AddEvent("No tienes comida para comer");
                }
            }
        }
        
        private void ConsumeFirstConsumable()
        {
            var inventoryManager = RoomInventoryManager.Instance;
            
            if (inventoryManager != null)
            {
                // Buscar en inventario del jugador items de tipo Consumable
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
                        
                        string message = !string.IsNullOrEmpty(consumable.useMessage)
                            ? consumable.useMessage
                            : $"Comiste {consumable.itemName} y recuperaste {consumable.healthRestore} de vida";
                        
                        currentContext.AddEvent(message);
                        
                        Debug.Log($"[GameContext] Consumible usado: {consumable.itemName} (+{consumable.healthRestore} vida, -{consumable.fatigueReduction} fatiga)");
                        return;
                    }
                }
            }
            
            currentContext.AddEvent("No tienes comida para comer");
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
            if (useRealRoomGenerator && roomBridge != null)
            {
                var inventoryManager = RoomInventoryManager.Instance;
                if (inventoryManager != null)
                {
                    var foundItems = inventoryManager.SearchRoom(currentContext.currentRoom.roomId);
                    
                    if (foundItems.Count > 0)
                    {
                        string itemNames = string.Join(", ", foundItems.ConvertAll(i => i.itemName));
                        currentContext.AddEvent($"Buscaste y encontraste: {itemNames}");
                        
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
            if (useRealRoomGenerator && roomBridge != null)
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
                            
                            // Remover de objetos visibles
                            currentContext.currentRoom.objects.Remove(takenItem.itemName);
                            currentContext.nearbyObjects.Remove(takenItem.itemName);
                            
                            // Limpiar cache del bridge
                            if (roomBridge != null)
                            {
                                roomBridge.ClearCache();
                            }
                        }
                    }
                    else
                    {
                        currentContext.AddEvent($"No puedes tomar {itemName}. Quizás necesitas buscarlo primero.");
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
        
        // Demo methods for testing
        [ContextMenu("Move to Next Location")]
        public void DemoMoveNext()
        {
            MoveToNextLocation();
        }
        
        [ContextMenu("Move to Previous Location")]
        public void DemoMovePrevious()
        {
            MoveToPreviousLocation();
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