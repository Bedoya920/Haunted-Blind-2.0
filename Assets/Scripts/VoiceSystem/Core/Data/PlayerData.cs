using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// ScriptableObject que persiste el estado del jugador entre sesiones
    /// Contiene inventario, salud, fatiga, flags de eventos y habitaciones visitadas
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Haunted Blind/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Health & Fatigue")]
        [SerializeField] private int currentHealth = 5;
        [SerializeField] private int currentFatigue = 0;
        
        [Header("Location")]
        [SerializeField] private string currentRoomId = "";
        [SerializeField] private List<string> visitedRooms = new List<string>();
        
        [Header("Inventory")]
        [SerializeField] private List<string> inventory = new List<string>();
        
        [Header("Story Progress")]
        [SerializeField] private SerializableDictionary<string, bool> eventFlags = new SerializableDictionary<string, bool>();
        
        // Public Properties
        public int CurrentHealth 
        { 
            get => currentHealth; 
            set => currentHealth = Mathf.Max(0, value); 
        }
        
        public int CurrentFatigue 
        { 
            get => currentFatigue; 
            set => currentFatigue = Mathf.Max(0, value); 
        }
        
        public string CurrentRoomId 
        { 
            get => currentRoomId; 
            set => currentRoomId = value; 
        }
        
        public List<string> VisitedRooms => visitedRooms;
        public List<string> Inventory => inventory;
        
        #region Inventory Methods
        
        /// <summary>
        /// Añade un item al inventario si no existe ya
        /// </summary>
        public bool AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[PlayerData] Intento de añadir item vacío");
                return false;
            }
            
            if (inventory.Contains(itemId))
            {
                Debug.LogWarning($"[PlayerData] Item '{itemId}' ya está en el inventario");
                return false;
            }
            
            inventory.Add(itemId);
            Debug.Log($"[PlayerData] Item '{itemId}' añadido al inventario");
            return true;
        }
        
        /// <summary>
        /// Remueve un item del inventario
        /// </summary>
        public bool RemoveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }
            
            bool removed = inventory.Remove(itemId);
            if (removed)
            {
                Debug.Log($"[PlayerData] Item '{itemId}' removido del inventario");
            }
            return removed;
        }
        
        /// <summary>
        /// Verifica si el jugador tiene un item específico
        /// </summary>
        public bool HasItem(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && inventory.Contains(itemId);
        }
        
        /// <summary>
        /// Limpia el inventario completamente
        /// </summary>
        public void ClearInventory()
        {
            inventory.Clear();
            Debug.Log("[PlayerData] Inventario limpiado");
        }
        
        #endregion
        
        #region Event Flag Methods
        
        /// <summary>
        /// Marca un evento como visto/completado
        /// </summary>
        public void SetEventFlag(string eventId, bool value = true)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return;
            }
            
            if (eventFlags.ContainsKey(eventId))
            {
                eventFlags[eventId] = value;
            }
            else
            {
                eventFlags.Add(eventId, value);
            }
            
            Debug.Log($"[PlayerData] Event flag '{eventId}' = {value}");
        }
        
        /// <summary>
        /// Verifica si un evento ya fue visto/completado
        /// </summary>
        public bool HasSeenEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return false;
            }
            
            return eventFlags.ContainsKey(eventId) && eventFlags[eventId];
        }
        
        /// <summary>
        /// Limpia todos los flags de eventos
        /// </summary>
        public void ClearEventFlags()
        {
            eventFlags.Clear();
            Debug.Log("[PlayerData] Event flags limpiados");
        }
        
        #endregion
        
        #region Room Tracking Methods
        
        /// <summary>
        /// Marca una habitación como visitada
        /// </summary>
        public void MarkRoomVisited(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                return;
            }
            
            if (!visitedRooms.Contains(roomId))
            {
                visitedRooms.Add(roomId);
                Debug.Log($"[PlayerData] Habitación '{roomId}' marcada como visitada");
            }
        }
        
        /// <summary>
        /// Verifica si una habitación ya fue visitada antes
        /// </summary>
        public bool HasVisitedRoom(string roomId)
        {
            return !string.IsNullOrEmpty(roomId) && visitedRooms.Contains(roomId);
        }
        
        /// <summary>
        /// Limpia el historial de habitaciones visitadas
        /// </summary>
        public void ClearVisitedRooms()
        {
            visitedRooms.Clear();
            Debug.Log("[PlayerData] Historial de habitaciones limpiado");
        }
        
        #endregion
        
        #region Reset & Initialize
        
        /// <summary>
        /// Reinicia todos los datos del jugador a valores iniciales
        /// </summary>
        public void ResetToDefault()
        {
            currentHealth = 5;
            currentFatigue = 0;
            currentRoomId = "";
            
            ClearInventory();
            ClearEventFlags();
            ClearVisitedRooms();
            
            Debug.Log("[PlayerData] Datos del jugador reseteados a valores iniciales");
        }
        
        /// <summary>
        /// Obtiene un resumen del estado actual del jugador
        /// </summary>
        public string GetSummary()
        {
            return $"[PlayerData]\n" +
                   $"  Salud: {currentHealth}\n" +
                   $"  Fatiga: {currentFatigue}\n" +
                   $"  Habitación: {currentRoomId}\n" +
                   $"  Inventario: {inventory.Count} items\n" +
                   $"  Habitaciones visitadas: {visitedRooms.Count}\n" +
                   $"  Eventos vistos: {eventFlags.Count(kv => kv.Value)}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Diccionario serializable para Unity Inspector
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();
        
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            
            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
        
        public void OnAfterDeserialize()
        {
            Clear();
            
            int count = Mathf.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}

