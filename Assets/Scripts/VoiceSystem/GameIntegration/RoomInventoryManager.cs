using UnityEngine;
using System.Collections.Generic;
using VoiceSystem.Core.Data;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Gestor centralizado de inventarios de habitaciones
    /// Carga items desde JSON y maneja acciones de items
    /// </summary>
    public class RoomInventoryManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string jsonFileName = "room_inventories"; // En Resources/
        [SerializeField] private bool loadFromJson = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        // Diccionario principal: roomId → items
        public Dictionary<string, RoomInventoryData> roomInventories { get; private set; }
        
        // Singleton
        private static RoomInventoryManager _instance;
        public static RoomInventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("RoomInventoryManager");
                    _instance = go.AddComponent<RoomInventoryManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInventories();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeInventories()
        {
            roomInventories = new Dictionary<string, RoomInventoryData>();
            
            if (loadFromJson)
            {
                LoadFromJson();
            }
            else
            {
                CreateDemoData();
            }
            
            LogDebug($"[RoomInventory] Inicializado con {roomInventories.Count} habitaciones");
        }
        
        #region JSON Loading
        
        private void LoadFromJson()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
            if (jsonFile == null)
            {
                Debug.LogWarning($"[RoomInventory] No se encontró {jsonFileName}.json en Resources/, usando datos demo");
                CreateDemoData();
                return;
            }
            
            try
            {
                RoomInventoryDatabase database = JsonUtility.FromJson<RoomInventoryDatabase>(jsonFile.text);
                
                if (database != null && database.roomInventories != null)
                {
                    foreach (var roomInv in database.roomInventories)
                    {
                        roomInventories[roomInv.roomId] = roomInv;
                    }
                    LogDebug($"[RoomInventory] Cargados inventarios de {roomInventories.Count} habitaciones desde JSON");
                    LogDebug($"[RoomInventory] Total de items: {database.GetTotalItemCount()}");
                }
                else
                {
                    Debug.LogWarning("[RoomInventory] JSON vacío o inválido, usando datos demo");
                    CreateDemoData();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RoomInventory] Error al cargar JSON: {e.Message}");
                CreateDemoData();
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Obtiene los items de una habitación (convierte int ID a string)
        /// </summary>
        public RoomInventoryData GetRoomInventory(int roomId)
        {
            string stringId = "room_" + roomId;
            return GetRoomInventory(stringId);
        }
        
        /// <summary>
        /// Obtiene los items de una habitación por string ID
        /// </summary>
        public RoomInventoryData GetRoomInventory(string roomId)
        {
            if (roomInventories.TryGetValue(roomId, out RoomInventoryData inventory))
            {
                return inventory;
            }
            
            // Si no existe, crear uno vacío con acciones por defecto
            var newInventory = new RoomInventoryData(roomId);
            roomInventories[roomId] = newInventory;
            
            LogDebug($"[RoomInventory] Creado inventario vacío para {roomId}");
            
            return newInventory;
        }
        
        /// <summary>
        /// Buscar en habitación - revela items ocultos
        /// </summary>
        public List<RoomItem> SearchRoom(string roomId)
        {
            var inventory = GetRoomInventory(roomId);
            var foundItems = new List<RoomItem>();
            
            foreach (var item in inventory.GetHiddenItems())
            {
                // Revelar item oculto
                item.isVisible = true;
                foundItems.Add(item);
                
                LogDebug($"[RoomInventory] Item revelado: {item.itemName} en {roomId}");
            }
            
            return foundItems;
        }
        
        /// <summary>
        /// Tomar item de habitación
        /// </summary>
        public bool TryTakeItem(string roomId, string itemId, out RoomItem item)
        {
            item = null;
            var inventory = GetRoomInventory(roomId);
            
            item = inventory.GetItemById(itemId);
            if (item == null)
            {
                LogDebug($"[RoomInventory] Item {itemId} no encontrado en {roomId}");
                return false;
            }
            
            if (!item.CanBeCollected())
            {
                LogDebug($"[RoomInventory] Item {itemId} no puede ser recogido (visible:{item.isVisible}, collected:{item.isCollected})");
                return false;
            }
            
            // Marcar como recogido
            item.isCollected = true;
            
            // Añadir a PlayerStateManager (sincroniza PlayerData + GameContext)
            var playerState = PlayerStateManager.Instance;
            if (playerState != null)
            {
                playerState.AddItem(itemId);
            }
            
            LogDebug($"[RoomInventory] ✅ Item recogido: {item.itemName} de {roomId} → Añadido al inventario persistente");
            
            return true;
        }
        
        /// <summary>
        /// Obtiene lista de items visibles en habitación
        /// </summary>
        public List<RoomItem> GetVisibleItems(string roomId)
        {
            var inventory = GetRoomInventory(roomId);
            return inventory.GetVisibleItems();
        }
        
        /// <summary>
        /// Obtiene acciones disponibles en habitación
        /// </summary>
        public List<string> GetAvailableActions(string roomId)
        {
            var inventory = GetRoomInventory(roomId);
            return inventory.availableActions;
        }
        
        /// <summary>
        /// Busca un item en todas las habitaciones por ID
        /// </summary>
        public RoomItem FindItemInAllRooms(string itemId)
        {
            foreach (var kvp in roomInventories)
            {
                var item = kvp.Value.GetItemById(itemId);
                if (item != null)
                {
                    return item;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Verifica si una habitación tiene items ocultos
        /// </summary>
        public bool HasHiddenItems(string roomId)
        {
            var inventory = GetRoomInventory(roomId);
            return inventory.GetHiddenItems().Count > 0;
        }
        
        #endregion
        
        #region Demo Data
        
        private void CreateDemoData()
        {
            // Sala Principal
            var salaInventory = new RoomInventoryData
            {
                roomId = "room_sala",
                availableActions = new List<string> { "inspeccionar", "tomar", "buscar" },
                items = new List<RoomItem>
                {
                    new RoomItem 
                    { 
                        itemId = "item_cuadro", 
                        itemName = "Cuadro Familiar",
                        itemType = ItemType.Decorative,
                        isVisible = true,
                        shortDescription = "Un retrato familiar antiguo",
                        longDescription = "Un retrato de una familia que vivió aquí hace décadas. Sus ojos parecen seguirte."
                    },
                    new RoomItem 
                    { 
                        itemId = "key_biblioteca", 
                        itemName = "Llave de la Biblioteca",
                        itemType = ItemType.Key,
                        isVisible = false,  // OCULTA - requiere "buscar"
                        shortDescription = "Una llave dorada escondida detrás del piano",
                        longDescription = "Una llave antigua con forma de libro grabado. Debe abrir la biblioteca."
                    }
                }
            };
            
            // Comedor
            var comedorInventory = new RoomInventoryData
            {
                roomId = "room_comedor",
                availableActions = new List<string> { "inspeccionar", "tomar", "buscar" },
                items = new List<RoomItem>
                {
                    new RoomItem 
                    { 
                        itemId = "food_apple", 
                        itemName = "Manzana",
                        itemType = ItemType.Consumable,
                        isVisible = true,
                        healthRestore = 1,
                        fatigueReduction = 1,
                        shortDescription = "Una manzana marchita sobre la mesa",
                        longDescription = "Una manzana que parece haber estado aquí por años, pero aún es comestible.",
                        useMessage = "Comiste la manzana y recuperaste vida"
                    }
                }
            };
            
            // Biblioteca
            var bibliotecaInventory = new RoomInventoryData
            {
                roomId = "room_biblioteca",
                availableActions = new List<string> { "inspeccionar", "tomar", "leer", "buscar" },
                items = new List<RoomItem>
                {
                    new RoomItem 
                    { 
                        itemId = "key_sotano", 
                        itemName = "Llave del Sótano",
                        itemType = ItemType.Key,
                        isVisible = true,
                        shortDescription = "Una llave oxidada sobre la mesa",
                        longDescription = "Una llave pesada y oxidada que huele a humedad. Tiene grabados extraños."
                    },
                    new RoomItem 
                    { 
                        itemId = "food_bread", 
                        itemName = "Pan",
                        itemType = ItemType.Consumable,
                        isVisible = false, // OCULTO - requiere "buscar"
                        healthRestore = 1,
                        fatigueReduction = 2,
                        shortDescription = "Pan duro pero comestible",
                        longDescription = "Un trozo de pan duro encontrado entre los libros. Parece seguro comerlo.",
                        useMessage = "Comiste el pan y te sientes mejor"
                    },
                    new RoomItem 
                    { 
                        itemId = "readable_diary", 
                        itemName = "Diario",
                        itemType = ItemType.Readable,
                        isVisible = false, // OCULTO
                        shortDescription = "Un diario viejo y polvoriento",
                        longDescription = "El diario de la familia que vivió aquí. Las últimas entradas hablan de extraños sucesos."
                    }
                }
            };
            
            // Cocina
            var cocinaInventory = new RoomInventoryData
            {
                roomId = "room_cocina",
                availableActions = new List<string> { "inspeccionar", "tomar", "buscar" },
                items = new List<RoomItem>
                {
                    new RoomItem 
                    { 
                        itemId = "food_water", 
                        itemName = "Agua",
                        itemType = ItemType.Consumable,
                        isVisible = true,
                        healthRestore = 0,
                        fatigueReduction = 3,
                        shortDescription = "Una botella de agua",
                        longDescription = "Agua embotellada que parece estar en buen estado.",
                        useMessage = "Bebiste agua y reduciste tu fatiga"
                    }
                }
            };
            
            roomInventories["room_sala"] = salaInventory;
            roomInventories["room_comedor"] = comedorInventory;
            roomInventories["room_biblioteca"] = bibliotecaInventory;
            roomInventories["room_cocina"] = cocinaInventory;
            
            LogDebug("[RoomInventory] Datos demo creados para 4 habitaciones");
        }
        
        #endregion
        
        #region Utility
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
        
        /// <summary>
        /// Limpia y reinicia todos los inventarios
        /// </summary>
        [ContextMenu("Reset All Inventories")]
        public void ResetAllInventories()
        {
            foreach (var kvp in roomInventories)
            {
                foreach (var item in kvp.Value.items)
                {
                    item.isCollected = false;
                    // Mantener isVisible como estaba originalmente
                }
            }
            
            Debug.Log("[RoomInventory] Todos los inventarios reiniciados");
        }
        
        /// <summary>
        /// Muestra estadísticas de todos los inventarios
        /// </summary>
        [ContextMenu("Show Inventory Stats")]
        public void ShowInventoryStats()
        {
            Debug.Log("=== ESTADÍSTICAS DE INVENTARIOS ===");
            Debug.Log($"Total habitaciones: {roomInventories.Count}");
            
            int totalItems = 0;
            int totalHidden = 0;
            int totalCollected = 0;
            
            foreach (var kvp in roomInventories)
            {
                var inv = kvp.Value;
                int hidden = inv.GetHiddenItems().Count;
                int collected = inv.items.FindAll(i => i.isCollected).Count;
                
                totalItems += inv.items.Count;
                totalHidden += hidden;
                totalCollected += collected;
                
                Debug.Log($"  {kvp.Key}: {inv.items.Count} items ({hidden} ocultos, {collected} recogidos)");
            }
            
            Debug.Log($"TOTALES: {totalItems} items, {totalHidden} ocultos, {totalCollected} recogidos");
        }
        
        #endregion
    }
}

