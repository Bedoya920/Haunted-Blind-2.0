using System.Collections.Generic;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Almacena todos los items de UNA habitación específica
    /// Serializable para JSON
    /// </summary>
    [System.Serializable]
    public class RoomInventoryData
    {
        public string roomId;                    // ID de la habitación ("room_12345")
        public List<RoomItem> items;             // Todos los items (visibles + ocultos)
        public List<string> availableActions;    // ["tomar", "buscar", "inspeccionar", "leer"]
        
        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public RoomInventoryData()
        {
            roomId = "";
            items = new List<RoomItem>();
            availableActions = new List<string>();
        }
        
        /// <summary>
        /// Constructor con roomId
        /// </summary>
        public RoomInventoryData(string id)
        {
            roomId = id;
            items = new List<RoomItem>();
            availableActions = new List<string> { "inspeccionar", "tomar", "buscar" }; // Acciones por defecto
        }
        
        /// <summary>
        /// Obtiene solo los items visibles y no recogidos
        /// </summary>
        public List<RoomItem> GetVisibleItems()
        {
            return items.FindAll(i => i.isVisible && !i.isCollected);
        }
        
        /// <summary>
        /// Obtiene solo los items ocultos y no recogidos
        /// </summary>
        public List<RoomItem> GetHiddenItems()
        {
            return items.FindAll(i => !i.isVisible && !i.isCollected);
        }
        
        /// <summary>
        /// Obtiene item por ID exacto
        /// </summary>
        public RoomItem GetItemById(string itemId)
        {
            return items.Find(i => i.itemId == itemId);
        }
        
        /// <summary>
        /// Obtiene item por nombre (búsqueda parcial, case-insensitive)
        /// </summary>
        public RoomItem GetItemByName(string itemName)
        {
            string searchName = itemName.ToLower().Trim();
            return items.Find(i => i.itemName.ToLower().Contains(searchName));
        }
        
        /// <summary>
        /// Cuenta total de items en la habitación
        /// </summary>
        public int GetTotalItemCount()
        {
            return items.Count;
        }
        
        /// <summary>
        /// Cuenta items no recogidos
        /// </summary>
        public int GetUncollectedItemCount()
        {
            return items.FindAll(i => !i.isCollected).Count;
        }
        
        /// <summary>
        /// Verifica si una acción está disponible en esta habitación
        /// </summary>
        public bool IsActionAvailable(string action)
        {
            return availableActions.Contains(action.ToLower());
        }
        
        /// <summary>
        /// Agrega un item a la habitación
        /// </summary>
        public void AddItem(RoomItem item)
        {
            if (item != null && !items.Exists(i => i.itemId == item.itemId))
            {
                items.Add(item);
            }
        }
        
        /// <summary>
        /// Remueve un item de la habitación
        /// </summary>
        public bool RemoveItem(string itemId)
        {
            var item = GetItemById(itemId);
            if (item != null)
            {
                return items.Remove(item);
            }
            return false;
        }
    }
}

