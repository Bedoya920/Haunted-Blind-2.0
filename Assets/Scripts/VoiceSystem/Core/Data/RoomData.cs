using System.Collections.Generic;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Represents a door in a room
    /// </summary>
    [System.Serializable]
    public class DoorData
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this door")]
        public string doorId;           // Ej: "door_kitchen_north"
        
        [Tooltip("Display name for the door")]
        public string doorName;         // Ej: "Puerta del Sótano"
        
        [Tooltip("Cardinal direction of the door")]
        public string direction;        // Ej: "norte", "sur", "este", "oeste"
        
        [Header("State")]
        [Tooltip("Is the door locked?")]
        public bool isLocked;           // Bloqueada o no
        
        [Header("Connection")]
        [Tooltip("Room ID this door leads to")]
        public string leadsToRoomId;    // ID de habitación destino
        
        [Tooltip("Item ID required to unlock (optional)")]
        public string keyItemId;        // Item necesario para abrir (opcional)
        
        [Header("Description")]
        [Tooltip("Description when inspecting the door")]
        public string description;      // Descripción al inspeccionar
        
        public DoorData()
        {
            doorId = "";
            doorName = "Puerta";
            direction = "norte";
            isLocked = false;
            leadsToRoomId = "";
            keyItemId = "";
            description = "Una puerta común";
        }
        
        /// <summary>
        /// Get a formatted description of the door
        /// </summary>
        public string GetFormattedDescription()
        {
            string desc = $"{doorName} ({direction})";
            if (isLocked)
            {
                desc += " - BLOQUEADA";
                if (!string.IsNullOrEmpty(keyItemId))
                {
                    desc += $" (requiere: {keyItemId})";
                }
            }
            return desc;
        }
        
        /// <summary>
        /// Check if door can be opened with given item
        /// </summary>
        public bool CanUnlockWith(string itemId)
        {
            if (!isLocked) return true;
            if (string.IsNullOrEmpty(keyItemId)) return false;
            return keyItemId.Equals(itemId, System.StringComparison.OrdinalIgnoreCase);
        }
    }
    
    /// <summary>
    /// Represents a room in the game
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this room")]
        public string roomId;           // Ej: "room_kitchen_01"
        
        [Tooltip("Display name of the room")]
        public string roomName;         // Ej: "Cocina"
        
        [Header("Descriptions")]
        [Tooltip("Short description (one line)")]
        [TextArea(2, 3)]
        public string shortDescription; // Ej: "Una cocina oscura"
        
        [Tooltip("Long description (detailed)")]
        [TextArea(4, 8)]
        public string longDescription;  // Ej: "Una habitación llena de..."
        
        [Header("Doors")]
        [Tooltip("Doors in this room (1-4 doors)")]
        public List<DoorData> doors;    // 1-4 puertas
        
        [Header("Objects")]
        [Tooltip("Objects present in this room")]
        public List<string> objects;    // Objetos en la habitación
        
        [Header("Metadata")]
        [Tooltip("Custom data for this room")]
        public Dictionary<string, string> metadata; // Datos adicionales
        
        public RoomData()
        {
            roomId = "";
            roomName = "Habitación Desconocida";
            shortDescription = "Una habitación misteriosa";
            longDescription = "Una habitación oscura y silenciosa. No puedes ver mucho en la penumbra.";
            doors = new List<DoorData>();
            objects = new List<string>();
            metadata = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Get door by direction (norte, sur, este, oeste)
        /// </summary>
        public DoorData GetDoorByDirection(string direction)
        {
            if (doors == null) return null;
            
            direction = direction.ToLower().Trim();
            return doors.Find(d => d.direction.ToLower().Trim() == direction);
        }
        
        /// <summary>
        /// Get door by name
        /// </summary>
        public DoorData GetDoorByName(string name)
        {
            if (doors == null) return null;
            
            name = name.ToLower().Trim();
            return doors.Find(d => d.doorName.ToLower().Contains(name) || d.doorId.ToLower().Contains(name));
        }
        
        /// <summary>
        /// Get door by ID
        /// </summary>
        public DoorData GetDoorById(string doorId)
        {
            if (doors == null) return null;
            
            return doors.Find(d => d.doorId == doorId);
        }
        
        /// <summary>
        /// Get all unlocked doors
        /// </summary>
        public List<DoorData> GetUnlockedDoors()
        {
            if (doors == null) return new List<DoorData>();
            
            return doors.FindAll(d => !d.isLocked);
        }
        
        /// <summary>
        /// Get formatted list of doors
        /// </summary>
        public string GetDoorsDescription()
        {
            if (doors == null || doors.Count == 0)
            {
                return "No hay puertas visibles";
            }
            
            List<string> doorDescs = new List<string>();
            foreach (var door in doors)
            {
                doorDescs.Add(door.GetFormattedDescription());
            }
            
            return string.Join(", ", doorDescs);
        }
        
        /// <summary>
        /// Get formatted list of objects
        /// </summary>
        public string GetObjectsDescription()
        {
            if (objects == null || objects.Count == 0)
            {
                return "No hay objetos notables";
            }
            
            return string.Join(", ", objects);
        }
        
        /// <summary>
        /// Check if room has a specific object
        /// </summary>
        public bool HasObject(string objectName)
        {
            if (objects == null) return false;
            
            objectName = objectName.ToLower().Trim();
            return objects.Exists(o => o.ToLower().Contains(objectName));
        }
        
        /// <summary>
        /// Add object to room
        /// </summary>
        public void AddObject(string objectName)
        {
            if (objects == null)
            {
                objects = new List<string>();
            }
            
            if (!objects.Contains(objectName))
            {
                objects.Add(objectName);
            }
        }
        
        /// <summary>
        /// Remove object from room
        /// </summary>
        public bool RemoveObject(string objectName)
        {
            if (objects == null) return false;
            
            return objects.Remove(objectName);
        }
    }
}

