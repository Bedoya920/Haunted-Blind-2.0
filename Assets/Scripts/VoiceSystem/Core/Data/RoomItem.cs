using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Tipos de items en el juego
    /// </summary>
    public enum ItemType
    {
        Key,           // Llaves para puertas
        Consumable,    // Comida, pociones
        Quest,         // Items de misión (flor de loto)
        Readable,      // Libros, notas
        Decorative,    // Objetos ambientales
        Hidden         // Items ocultos que requieren "buscar"
    }
    
    /// <summary>
    /// Representa un item en una habitación
    /// Serializable para JSON
    /// </summary>
    [System.Serializable]
    public class RoomItem
    {
        [Header("Identification")]
        public string itemId;           // "key_biblioteca", "food_bread"
        public string itemName;         // "Llave de la Biblioteca"
        public ItemType itemType;       // Tipo de item
        
        [Header("State")]
        public bool isVisible;          // false = oculto, true = visible
        public bool isCollected;        // Ya fue recogido?
        
        [Header("Descriptions")]
        public string shortDescription; // "Una llave oxidada"
        public string longDescription;  // "Una llave antigua con grabados..."
        
        [Header("Effects (for consumables)")]
        public int healthRestore;       // Cuánta vida restaura
        public int fatigueReduction;    // Cuánta fatiga reduce
        
        [Header("Usage")]
        public string useMessage;       // Mensaje al usar
        public string failMessage;      // Mensaje si falla
        
        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public RoomItem()
        {
            itemId = "";
            itemName = "Item";
            itemType = ItemType.Decorative;
            isVisible = true;
            isCollected = false;
            shortDescription = "";
            longDescription = "";
            healthRestore = 0;
            fatigueReduction = 0;
            useMessage = "";
            failMessage = "";
        }
        
        /// <summary>
        /// Constructor con parámetros básicos
        /// </summary>
        public RoomItem(string id, string name, ItemType type, bool visible = true)
        {
            itemId = id;
            itemName = name;
            itemType = type;
            isVisible = visible;
            isCollected = false;
            shortDescription = "";
            longDescription = "";
            healthRestore = 0;
            fatigueReduction = 0;
            useMessage = "";
            failMessage = "";
        }
        
        // Helper methods
        
        /// <summary>
        /// Verifica si el item puede ser recogido
        /// </summary>
        public bool CanBeCollected()
        {
            return isVisible && !isCollected;
        }
        
        /// <summary>
        /// Verifica si es una llave
        /// </summary>
        public bool IsKey()
        {
            return itemType == ItemType.Key;
        }
        
        /// <summary>
        /// Verifica si es un consumible
        /// </summary>
        public bool IsConsumable()
        {
            return itemType == ItemType.Consumable;
        }
        
        /// <summary>
        /// Verifica si está oculto
        /// </summary>
        public bool IsHidden()
        {
            return !isVisible;
        }
        
        /// <summary>
        /// Verifica si es legible
        /// </summary>
        public bool IsReadable()
        {
            return itemType == ItemType.Readable;
        }
        
        /// <summary>
        /// Verifica si es un item de quest
        /// </summary>
        public bool IsQuestItem()
        {
            return itemType == ItemType.Quest;
        }
        
        /// <summary>
        /// Obtiene descripción apropiada según contexto
        /// </summary>
        public string GetDescription(bool detailed = false)
        {
            if (detailed && !string.IsNullOrEmpty(longDescription))
            {
                return longDescription;
            }
            
            return string.IsNullOrEmpty(shortDescription) ? itemName : shortDescription;
        }
    }
}

