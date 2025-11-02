using System.Collections.Generic;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Wrapper para serializar/deserializar todos los inventarios de habitaciones desde JSON
    /// Clase raíz del archivo JSON
    /// </summary>
    [System.Serializable]
    public class RoomInventoryDatabase
    {
        public List<RoomInventoryData> roomInventories; // Lista de inventarios por habitación
        
        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public RoomInventoryDatabase()
        {
            roomInventories = new List<RoomInventoryData>();
        }
        
        /// <summary>
        /// Obtiene inventario de una habitación específica por ID
        /// </summary>
        public RoomInventoryData GetRoomInventory(string roomId)
        {
            if (roomInventories == null)
            {
                return null;
            }
            
            return roomInventories.Find(r => r.roomId == roomId);
        }
        
        /// <summary>
        /// Agrega inventario de una habitación
        /// </summary>
        public void AddRoomInventory(RoomInventoryData roomInventory)
        {
            if (roomInventories == null)
            {
                roomInventories = new List<RoomInventoryData>();
            }
            
            // Verificar si ya existe
            var existing = GetRoomInventory(roomInventory.roomId);
            if (existing != null)
            {
                // Reemplazar
                roomInventories.Remove(existing);
            }
            
            roomInventories.Add(roomInventory);
        }
        
        /// <summary>
        /// Obtiene el total de habitaciones con inventarios definidos
        /// </summary>
        public int GetTotalRoomCount()
        {
            return roomInventories?.Count ?? 0;
        }
        
        /// <summary>
        /// Obtiene el total de items en todas las habitaciones
        /// </summary>
        public int GetTotalItemCount()
        {
            int total = 0;
            if (roomInventories != null)
            {
                foreach (var room in roomInventories)
                {
                    total += room.GetTotalItemCount();
                }
            }
            return total;
        }
    }
}

