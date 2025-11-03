using UnityEngine;
using VoiceSystem.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Valida TODOS los comandos antes de ejecutarlos
    /// Proporciona mensajes de error específicos
    /// </summary>
    public class CommandValidator : MonoBehaviour
    {
        // Singleton
        private static CommandValidator _instance;
        public static CommandValidator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CommandValidator");
                    _instance = go.AddComponent<CommandValidator>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private DirectionalMovement directionalMovement;
        private RoomInventoryManager inventoryManager;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            directionalMovement = FindFirstObjectByType<DirectionalMovement>();
            inventoryManager = RoomInventoryManager.Instance;
            
            Debug.Log("[CommandValidator] Inicializado");
        }
        
        #region Movement Validation
        
        /// <summary>
        /// Valida si el jugador puede moverse en una dirección
        /// </summary>
        public bool CanMoveDirection(string direction, out string errorMessage)
        {
            errorMessage = "";
            
            if (directionalMovement == null)
            {
                errorMessage = "Sistema de movimiento no disponible";
                return false;
            }
            
            var door = directionalMovement.TranslateDirectionToDoor(direction);
            if (door == null)
            {
                errorMessage = $"No hay puertas hacia {direction}";
                return false;
            }
            
            if (door.isLocked)
            {
                if (!string.IsNullOrEmpty(door.keyItemId))
                {
                    string keyName = GetItemName(door.keyItemId);
                    errorMessage = $"La puerta está bloqueada. Necesitas {keyName}";
                }
                else
                {
                    errorMessage = "La puerta está bloqueada y no se puede abrir";
                }
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Item Validation
        
        /// <summary>
        /// Valida si el jugador puede tomar un item
        /// </summary>
        public bool CanTakeItem(string itemName, out string errorMessage)
        {
            errorMessage = "";
            
            var playerState = PlayerStateManager.Instance;
            if (playerState == null || playerState.CurrentRoom == null)
            {
                errorMessage = "No se puede determinar la ubicación actual";
                return false;
            }
            
            if (inventoryManager == null)
            {
                errorMessage = "Sistema de inventario no disponible";
                return false;
            }
            
            var currentRoom = playerState.CurrentRoom;
            var roomInventory = inventoryManager.GetRoomInventory(currentRoom.roomId);
            
            if (roomInventory == null)
            {
                errorMessage = "No hay items en esta habitación";
                return false;
            }
            
            var item = roomInventory.GetItemByName(itemName);
            
            if (item == null)
            {
                // List available items instead
                var availableItems = roomInventory.GetVisibleItems();
                if (availableItems != null && availableItems.Count > 0)
                {
                    var itemNames = string.Join(", ", availableItems.ConvertAll(i => i.itemName));
                    errorMessage = $"No ves {itemName} aquí. Items disponibles: {itemNames}";
                }
                else
                {
                    errorMessage = $"No ves {itemName} aquí. No hay items visibles en esta habitación";
                }
                return false;
            }
            
            if (!item.CanBeCollected())
            {
                if (item.isCollected)
                {
                    errorMessage = $"Ya tomaste {itemName}";
                }
                else if (!item.isVisible)
                {
                    errorMessage = $"{itemName} está oculto. Primero debes buscar en la habitación";
                }
                else
                {
                    errorMessage = $"No puedes tomar {itemName}";
                }
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Action Validation
        
        /// <summary>
        /// Valida si el jugador puede comer
        /// </summary>
        public bool CanEat(out string errorMessage)
        {
            errorMessage = "";
            
            var playerState = PlayerStateManager.Instance;
            if (playerState == null)
            {
                errorMessage = "Estado del jugador no disponible";
                return false;
            }
            
            // Check health
            if (playerState.Health >= 5)
            {
                errorMessage = "Ya tienes la vida completa, no necesitas comer ahora";
                return false;
            }
            
            // Check for consumables in inventory
            var consumables = GetConsumablesInInventory();
            if (consumables.Count == 0)
            {
                errorMessage = "No tienes comida. Debes buscar en las habitaciones";
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Valida si el jugador puede buscar
        /// </summary>
        public bool CanSearch(out string errorMessage)
        {
            errorMessage = "";
            
            var playerState = PlayerStateManager.Instance;
            if (playerState == null || playerState.CurrentRoom == null)
            {
                errorMessage = "No se puede determinar la ubicación actual";
                return false;
            }
            
            if (inventoryManager == null)
            {
                errorMessage = "Sistema de inventario no disponible";
                return false;
            }
            
            var currentRoom = playerState.CurrentRoom;
            var roomInventory = inventoryManager.GetRoomInventory(currentRoom.roomId);
            var hiddenItems = roomInventory?.GetHiddenItems();
            
            if (hiddenItems == null || hiddenItems.Count == 0)
            {
                errorMessage = "No hay nada oculto en esta habitación";
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Obtiene consumibles en el inventario del jugador
        /// </summary>
        private List<RoomItem> GetConsumablesInInventory()
        {
            var consumables = new List<RoomItem>();
            
            if (inventoryManager == null)
            {
                return consumables;
            }
            
            var playerState = PlayerStateManager.Instance;
            if (playerState == null)
            {
                return consumables;
            }
            
            // Buscar items consumibles en el inventario
            foreach (var itemId in playerState.Inventory)
            {
                // Buscar el item en todas las habitaciones
                foreach (var roomInv in inventoryManager.roomInventories.Values)
                {
                    var item = roomInv.GetItemById(itemId);
                    if (item != null && item.IsConsumable())
                    {
                        consumables.Add(item);
                        break;
                    }
                }
            }
            
            return consumables;
        }
        
        /// <summary>
        /// Obtiene nombre legible de un item por su ID
        /// </summary>
        private string GetItemName(string itemId)
        {
            if (inventoryManager == null)
            {
                return itemId;
            }
            
            // Buscar en todas las habitaciones
            foreach (var roomInv in inventoryManager.roomInventories.Values)
            {
                var item = roomInv.GetItemById(itemId);
                if (item != null)
                {
                    return item.itemName;
                }
            }
            
            return itemId; // Fallback al ID
        }
        
        #endregion
        
        #region Get Valid Commands
        
        /// <summary>
        /// Obtiene lista de comandos que son VÁLIDOS en este momento
        /// </summary>
        public List<string> GetValidCommandsNow()
        {
            var validCommands = new List<string>();
            
            // Always valid
            validCommands.Add("información");
            validCommands.Add("ayuda");
            validCommands.Add("hora");
            validCommands.Add("tiempo");
            validCommands.Add("reloj");
            
            // Movement - check each direction
            if (CanMoveDirection("adelante", out _)) validCommands.Add("adelante");
            if (CanMoveDirection("atrás", out _)) validCommands.Add("atrás");
            if (CanMoveDirection("derecha", out _)) validCommands.Add("derecha");
            if (CanMoveDirection("izquierda", out _)) validCommands.Add("izquierda");
            
            // Actions
            if (CanSearch(out _)) validCommands.Add("buscar");
            if (CanEat(out _)) validCommands.Add("comer");
            
            // Items (check what's in room)
            var playerState = PlayerStateManager.Instance;
            if (playerState != null && playerState.CurrentRoom != null && inventoryManager != null)
            {
                var items = inventoryManager.GetRoomInventory(playerState.CurrentRoom.roomId);
                if (items != null && items.GetVisibleItems().Count > 0)
                {
                    validCommands.Add("tomar");
                }
            }
            
            return validCommands;
        }
        
        /// <summary>
        /// Obtiene descripción de comandos válidos para el AI
        /// </summary>
        public string GetValidCommandsDescription()
        {
            var validCommands = GetValidCommandsNow();
            
            if (validCommands.Count == 0)
            {
                return "No hay comandos disponibles en este momento";
            }
            
            return $"Puedes hacer: {string.Join(", ", validCommands)}";
        }
        
        #endregion
    }
}

