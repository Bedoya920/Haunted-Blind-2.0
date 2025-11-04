using UnityEngine;
using System.Collections.Generic;
using VoiceSystem.Core.Data;
using Object = UnityEngine.Object;

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
        /// Inspecciona un item sin recogerlo (solo lee descripción)
        /// </summary>
        public bool TryInspectItem(string roomId, string itemId, out RoomItem item, out string description)
        {
            item = null;
            description = "";
            var inventory = GetRoomInventory(roomId);
            
            item = inventory.GetItemById(itemId);
            if (item == null)
            {
                description = $"No hay ningún {itemId} aquí";
                LogDebug($"[RoomInventory] Item {itemId} no encontrado en {roomId} para inspeccionar");
                return false;
            }
            
            // Verificar si es visible
            if (!item.isVisible)
            {
                description = "No ves nada especial aquí";
                LogDebug($"[RoomInventory] Item {itemId} no visible para inspeccionar");
                return false;
            }
            
            // Si ya fue recogido, indicarlo
            if (item.isCollected)
            {
                description = $"Ya recogiste {item.itemName}";
                LogDebug($"[RoomInventory] Item {itemId} ya recogido");
                return false;
            }
            
            // Retornar descripción del objeto (usar longDescription si existe)
            description = !string.IsNullOrEmpty(item.longDescription) 
                ? item.longDescription 
                : (!string.IsNullOrEmpty(item.shortDescription) 
                    ? item.shortDescription 
                    : $"Ves {item.itemName}. Parece que podrías recogerlo");
            
            LogDebug($"[RoomInventory] ✅ Inspeccionado {itemId}: {description}");
            return true;
        }
        
        /// <summary>
        /// Tomar item de habitación con verificación de requisitos
        /// </summary>
        public bool TryTakeItem(string roomId, string itemId, out RoomItem item, out string failReason)
        {
            item = null;
            failReason = "";
            var inventory = GetRoomInventory(roomId);
            
            item = inventory.GetItemById(itemId);
            if (item == null)
            {
                failReason = $"No se encontró {itemId} en esta habitación";
                LogDebug($"[RoomInventory] Item {itemId} no encontrado en {roomId}");
                return false;
            }
            
            if (!item.CanBeCollected())
            {
                failReason = item.isCollected ? "Ya recogiste este objeto" : "Este objeto no está visible aún";
                LogDebug($"[RoomInventory] Item {itemId} no puede ser recogido (visible:{item.isVisible}, collected:{item.isCollected})");
                return false;
            }
            
            // NUEVO: Verificar si es un objeto solo legible (tipo Readable o Decorative)
            if (item.itemType == ItemType.Readable || item.itemType == ItemType.Decorative)
            {
                failReason = !string.IsNullOrEmpty(item.failMessage)
                    ? item.failMessage
                    : "No puedes tomar este objeto. Intenta leerlo o inspeccionarlo.";
                LogDebug($"[RoomInventory] Item {itemId} es de tipo Readable/Decorative - No se puede tomar");
                return false;
            }
            
            // Obtener PlayerStateManager una sola vez
            var playerState = PlayerStateManager.Instance;
            
            // NUEVO: Verificar requiredFlag
            if (!string.IsNullOrEmpty(item.requiredFlag))
            {
                if (playerState != null && !playerState.HasSeenEvent(item.requiredFlag))
                {
                    failReason = !string.IsNullOrEmpty(item.failMessage) 
                        ? item.failMessage 
                        : "Aún no puedes tomar este objeto";
                    LogDebug($"[RoomInventory] Item {itemId} requiere flag '{item.requiredFlag}' que el jugador no tiene");
                    return false;
                }
            }
            
            // NUEVO: Verificar requiredItemId
            if (item.requiredItemId != -1)
            {
                string requiredItemIdStr = "item_" + item.requiredItemId;
                
                if (playerState != null && !playerState.HasItem(requiredItemIdStr))
                {
                    failReason = !string.IsNullOrEmpty(item.failMessage)
                        ? item.failMessage
                        : $"Necesitas otro objeto para tomar {item.itemName}";
                    LogDebug($"[RoomInventory] Item {itemId} requiere objeto ID:{item.requiredItemId} que el jugador no tiene");
                    return false;
                }
            }
            
            // Marcar como recogido
            item.isCollected = true;
            
            // Añadir al inventario del jugador
            if (playerState != null)
            {
                playerState.AddItem(itemId);
                
                // NUEVO: Marcar flag si existe
                if (!string.IsNullOrEmpty(item.flagToSetOnCollect))
                {
                    playerState.SetEventFlag(item.flagToSetOnCollect);
                    LogDebug($"[RoomInventory] ✅ Flag marcado: {item.flagToSetOnCollect}");
                }
            }
            
            // NUEVO: Disparar evento al recoger
            if (!string.IsNullOrEmpty(item.eventToTriggerOnCollect))
            {
                TriggerCollectionEvent(item);
            }
            
            // EVENTO ESPECIAL: Oso de peluche activa evento del niño
            if (item.itemId == "toy_bear")
            {
                TriggerBearEvent(item);
            }
            
            // NUEVO: Si es llave, desbloquear puerta automáticamente
            if (item.IsKey())
            {
                UnlockDoorWithKey(item);
            }
            
            LogDebug($"[RoomInventory] ✅ Item recogido: {item.itemName} de {roomId} → Añadido al inventario persistente");
            
            return true;
        }
        
        /// <summary>
        /// Dispara evento cuando se recoge un objeto
        /// </summary>
        private void TriggerCollectionEvent(RoomItem item)
        {
            var eventManager = EventManager.Instance;
            if (eventManager == null)
            {
                Debug.LogWarning("[RoomInventory] EventManager no disponible para disparar evento de recolección");
                return;
            }
            
            // Buscar evento por nombre
            var evt = eventManager.GetEventByName(item.eventToTriggerOnCollect);
            if (evt != null)
            {
                eventManager.NarrateEvent(evt);
                LogDebug($"[RoomInventory] 🎬 Evento disparado: {item.eventToTriggerOnCollect}");
            }
            else
            {
                Debug.LogWarning($"[RoomInventory] Evento '{item.eventToTriggerOnCollect}' no encontrado");
            }
        }
        
        /// <summary>
        /// Desbloquea puerta automáticamente al recoger llave
        /// </summary>
        private void UnlockDoorWithKey(RoomItem keyItem)
        {
            // Extraer ID numérico de la llave (ej: "key_100" -> 100)
            if (!keyItem.itemId.StartsWith("key_"))
            {
                Debug.LogWarning($"[RoomInventory] Item {keyItem.itemId} es tipo Key pero no tiene formato 'key_XXX'");
                return;
            }
            
            if (!int.TryParse(keyItem.itemId.Substring(4), out int keyId))
            {
                Debug.LogWarning($"[RoomInventory] No se pudo extraer ID numérico de {keyItem.itemId}");
                return;
            }
            
            var roomBridge = RoomSystemBridge.Instance;
            if (roomBridge != null)
            {
                roomBridge.UnlockDoorWithKeyId(keyId);
                LogDebug($"[RoomInventory] 🔓 Puerta desbloqueada con llave ID:{keyId}");
            }
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
        
        #region Eventos Especiales
        
        /// <summary>
        /// Evento especial al tomar el oso de peluche
        /// </summary>
        private void TriggerBearEvent(RoomItem bearItem)
        {
            Debug.Log("[RoomInventory] 🧸 EVENTO DEL OSO ACTIVADO");
            
            // Marcar flag del evento del niño
            var playerState = PlayerStateManager.Instance;
            if (playerState != null)
            {
                playerState.SetEventFlag("child_event_triggered");
                Debug.Log("[RoomInventory] ✅ Flag 'child_event_triggered' marcado");
            }
            
            // Narrar el evento del oso (versión MUY CORTA para evitar bloqueos completos)
            var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
            if (voiceSystem?.textToSpeech != null)
            {
                // CRÍTICO: Usar narración EXTREMADAMENTE corta para evitar bloqueos
                string narration = "Tomaste el oso de peluche. Una voz infantil susurra: Lo encontraste. " +
                                 "Llévalo al lugar de su descanso.";
                
                voiceSystem.textToSpeech.Speak(narration, VoiceSystem.Core.Interfaces.TTSPriority.Normal);
                Debug.Log($"[RoomInventory] 📖 Narración del oso INICIADA: {narration.Length} caracteres");
            }
            else
            {
                Debug.LogWarning("[RoomInventory] ⚠️ VoiceSystem TTS no disponible para narrar evento del oso");
            }
            
            // Activar sistema de screamers usando Singleton (más confiable que SendMessage)
            var screamerSystem = ScreamerSystem.Instance;
            if (screamerSystem != null)
            {
                screamerSystem.ActivateScreamers();
                Debug.Log("[RoomInventory] ✅ ScreamerSystem activado correctamente");
            }
            else
            {
                Debug.LogWarning("[RoomInventory] ⚠️ ScreamerSystem.Instance no encontrado - ¿GameInitializer lo creó?");
            }
            
            // Cambiar música inmediatamente
            var ambientController = Audio.AmbientMusicController.Instance;
            if (ambientController != null)
            {
                ambientController.CheckForChildEvent();
                Debug.Log("[RoomInventory] ✅ Música cambiada a post-2 AM inmediatamente");
            }
        }
        
        #endregion
    }
}

