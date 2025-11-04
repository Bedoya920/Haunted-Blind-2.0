using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Represents the current state of the game for AI context
    /// </summary>
    [System.Serializable]
    public class GameContext
    {
        [Header("Location")]
        public string currentLocation = "Sala Principal";
        public List<string> nearbyObjects = new List<string>();
        public List<string> availableExits = new List<string>();
        
        [Header("Room System")]
        [Tooltip("Current room data with doors and details")]
        public RoomData currentRoom;
        
        [Tooltip("All rooms in the game (for future module integration)")]
        public Dictionary<string, RoomData> allRooms = new Dictionary<string, RoomData>();
        
        [Header("Player State")]
        public int health = 5;
        public int maxHealth = 5;
        public int fatigue = 0;
        public int maxFatigue = 25;
        public int actions = 9999; // INFINITAS - GDD no especifica límite
        public int maxActions = 9999;
        
        [Header("Inventory")]
        public List<string> inventory = new List<string>();
        
        [Header("Time")]
        public string gameTime = "2:00 AM";
        public float timeRemaining = 12f; // hours
        
        [Header("Game State")]
        public List<string> availableCommands = new List<string>();
        public List<string> recentEvents = new List<string>();
        public bool isInCombat = false;
        public bool hasEncounteredChild = false;
        public bool hasLotusFlower = false;
        
        [Header("Room Specific")]
        public Dictionary<string, object> roomData = new Dictionary<string, object>();
        
        public GameContext()
        {
            // Initialize default commands
            availableCommands.AddRange(new string[] 
            {
                // Direcciones (arriba/abajo son los comandos principales, adelante/atrás son alias)
                "arriba", "abajo", "izquierda", "derecha",
                "adelante", "atrás", "atras", "frente",
                // Acciones de interacción
                "inspeccionar", "tomar", "usar", "comer", "leer", "dar",
                // Comandos especiales
                "renacer", "despertar", "ayuda", "inventario"
            });
        }
        
        /// <summary>
        /// Add a recent event (keeps only last 5)
        /// </summary>
        public void AddEvent(string eventDescription)
        {
            recentEvents.Insert(0, eventDescription);
            if (recentEvents.Count > 5)
            {
                recentEvents.RemoveAt(recentEvents.Count - 1);
            }
        }
        
        /// <summary>
        /// Check if player can perform an action
        /// </summary>
        public bool CanPerformAction(int actionCost = 1)
        {
            return actions >= actionCost;
        }
        
        /// <summary>
        /// Consume actions
        /// NOTA: La lógica de fatiga está manejada por FatigueSystem
        /// Este método solo actualiza el contador de acciones
        /// </summary>
        public void ConsumeActions(int amount = 1)
        {
            actions = Mathf.Max(0, actions - amount);
            // La fatiga es manejada por FatigueSystem.AddFatigue() en GameContextProvider
            // NO duplicar la lógica aquí
        }
        
        /// <summary>
        /// Restore health (eating)
        /// </summary>
        public void RestoreHealth(int amount = 1)
        {
            health = Mathf.Min(maxHealth, health + amount);
        }
        
        /// <summary>
        /// Get context summary for AI
        /// </summary>
        public string GetContextSummary()
        {
            return $"Ubicación: {currentLocation}\n" +
                   $"Vida: {health}/{maxHealth}\n" +
                   $"Fatiga: {fatigue}\n" +
                   $"Acciones: {actions}\n" +
                   $"Tiempo: {gameTime}\n" +
                   $"Inventario: {(inventory.Count > 0 ? string.Join(", ", inventory) : "vacío")}\n" +
                   $"Objetos cercanos: {(nearbyObjects.Count > 0 ? string.Join(", ", nearbyObjects) : "ninguno")}";
        }
        
        #region Room System Helper Methods
        
        /// <summary>
        /// Get all available doors in current room
        /// </summary>
        public List<DoorData> GetAvailableDoors()
        {
            if (currentRoom == null || currentRoom.doors == null)
            {
                return new List<DoorData>();
            }
            
            return currentRoom.doors;
        }
        
        /// <summary>
        /// Get door by direction (norte, sur, este, oeste)
        /// </summary>
        public DoorData GetDoorByDirection(string direction)
        {
            if (currentRoom == null)
            {
                return null;
            }
            
            return currentRoom.GetDoorByDirection(direction);
        }
        
        /// <summary>
        /// Get door by name or partial name
        /// </summary>
        public DoorData GetDoorByName(string name)
        {
            if (currentRoom == null)
            {
                return null;
            }
            
            return currentRoom.GetDoorByName(name);
        }
        
        /// <summary>
        /// Check if a door is accessible (exists and not locked, or player has key)
        /// </summary>
        public bool IsDoorAccessible(string doorId)
        {
            if (currentRoom == null)
            {
                return false;
            }
            
            var door = currentRoom.GetDoorById(doorId);
            if (door == null)
            {
                return false;
            }
            
            // Door is accessible if not locked
            if (!door.isLocked)
            {
                return true;
            }
            
            // Check if player has the required key
            if (!string.IsNullOrEmpty(door.keyItemId))
            {
                return inventory.Contains(door.keyItemId);
            }
            
            // Locked with no key specified - inaccessible
            return false;
        }
        
        /// <summary>
        /// Get all unlocked doors in current room
        /// </summary>
        public List<DoorData> GetUnlockedDoors()
        {
            if (currentRoom == null)
            {
                return new List<DoorData>();
            }
            
            return currentRoom.GetUnlockedDoors();
        }
        
        /// <summary>
        /// Get room by ID from allRooms dictionary
        /// </summary>
        public RoomData GetRoomById(string roomId)
        {
            if (allRooms == null || !allRooms.ContainsKey(roomId))
            {
                return null;
            }
            
            return allRooms[roomId];
        }
        
        /// <summary>
        /// Get formatted description of all doors in current room
        /// </summary>
        public string GetDoorsDescription()
        {
            if (currentRoom == null)
            {
                return "No hay información sobre puertas disponible";
            }
            
            return currentRoom.GetDoorsDescription();
        }
        
        /// <summary>
        /// Check if player can use a specific door
        /// </summary>
        public bool CanUseDoor(DoorData door, out string reason)
        {
            reason = "";
            
            if (door == null)
            {
                reason = "La puerta no existe";
                return false;
            }
            
            if (!door.isLocked)
            {
                return true;
            }
            
            // Door is locked
            if (string.IsNullOrEmpty(door.keyItemId))
            {
                reason = $"{door.doorName} está bloqueada y no se puede abrir";
                return false;
            }
            
            // Check if player has key
            if (inventory.Contains(door.keyItemId))
            {
                return true;
            }
            
            reason = $"{door.doorName} está bloqueada. Necesitas: {door.keyItemId}";
            return false;
        }
        
        #endregion
    }
}