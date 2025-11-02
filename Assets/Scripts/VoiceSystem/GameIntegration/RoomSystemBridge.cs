using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VoiceSystem.Core.Data;
using VoiceSystem.Core.Interfaces;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Puente entre RoomGenerator3000 y VoiceSystem
    /// Convierte estructuras del generador a formato del sistema de voz
    /// </summary>
    public class RoomSystemBridge : MonoBehaviour, IRoomSystemProvider
    {
        [Header("Referencias")]
        [SerializeField] private RoomGenerator3000 roomGenerator;
        [SerializeField] private FatigueSystem fatigueSystem;
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private RoomInventoryManager inventoryManager;
        
        [Header("Estado del Jugador")]
        [Tooltip("Posición actual del jugador en la matriz")]
        public Vector2Int currentPlayerPosition;
        
        // Eventos
        public event System.Action<RoomData> OnRoomChanged;
        public event System.Action<DoorData> OnDoorStateChanged;
        
        // Cache para evitar conversiones repetidas
        private Dictionary<int, RoomData> roomDataCache;
        private Dictionary<int, DoorData> doorDataCache;
        private HashSet<Vector2Int> visitedRooms;
        
        void Awake()
        {
            roomDataCache = new Dictionary<int, RoomData>();
            doorDataCache = new Dictionary<int, DoorData>();
            visitedRooms = new HashSet<Vector2Int>();
            
            // Validar referencias
            if (roomGenerator == null)
                Debug.LogError("[RoomBridge] Falta asignar RoomGenerator3000");
            
            // Obtener o crear RoomInventoryManager
            if (inventoryManager == null)
            {
                inventoryManager = RoomInventoryManager.Instance;
            }
        }
        
        void Start()
        {
            // Esperar a que el generador cree la casa
            if (roomGenerator != null && roomGenerator.casa != null)
            {
                // Posición inicial del jugador = habitación inicial de la casa
                currentPlayerPosition = roomGenerator.casa.habitacionInicial;
                visitedRooms.Add(currentPlayerPosition);
                
                Debug.Log($"[RoomBridge] Jugador inicia en: {currentPlayerPosition}");
            }
        }
        
        #region Conversión Room → RoomData
        
        /// <summary>
        /// Convierte Room del generador a RoomData del sistema de voz
        /// </summary>
        private RoomData ConvertToRoomData(Room generatorRoom)
        {
            if (generatorRoom == null) return null;
            
            // Check cache
            if (roomDataCache.TryGetValue(generatorRoom.id, out RoomData cached))
                return cached;
            
            var roomData = new RoomData
            {
                roomId = "room_" + generatorRoom.id,
                roomName = string.IsNullOrEmpty(generatorRoom.nombre) ? "Habitación" : generatorRoom.nombre,
                shortDescription = string.IsNullOrEmpty(generatorRoom.descripCorta) 
                    ? "Una habitación misteriosa" 
                    : generatorRoom.descripCorta,
                longDescription = string.IsNullOrEmpty(generatorRoom.descripLarga) 
                    ? "No puedes ver mucho en la oscuridad" 
                    : generatorRoom.descripLarga,
                objects = new List<string>(generatorRoom.objetosEnHabitacion),
                doors = GetDoorsForRoom(generatorRoom.posicion),
                metadata = new Dictionary<string, string>
                {
                    { "positionX", generatorRoom.posicion.x.ToString() },
                    { "positionY", generatorRoom.posicion.y.ToString() }
                }
            };
            
            // NUEVO: Obtener items de la habitación desde InventoryManager
            if (inventoryManager != null)
            {
                var roomInventory = inventoryManager.GetRoomInventory(generatorRoom.id);
                
                // Agregar items visibles a objects
                var visibleItems = roomInventory.GetVisibleItems();
                foreach (var item in visibleItems)
                {
                    if (!roomData.objects.Contains(item.itemName))
                    {
                        roomData.objects.Add(item.itemName);
                    }
                }
                
                // Almacenar información de items ocultos en metadata
                int hiddenCount = roomInventory.GetHiddenItems().Count;
                roomData.metadata["hasHiddenItems"] = hiddenCount.ToString();
                roomData.metadata["totalItems"] = roomInventory.items.Count.ToString();
            }
            
            // Cache
            roomDataCache[generatorRoom.id] = roomData;
            return roomData;
        }
        
        #endregion
        
        #region Conversión Door → DoorData
        
        /// <summary>
        /// Obtiene todas las puertas conectadas a una posición
        /// </summary>
        private List<DoorData> GetDoorsForRoom(Vector2Int roomPosition)
        {
            var doors = new List<DoorData>();
            
            if (roomGenerator == null || roomGenerator.casa == null)
                return doors;
            
            foreach (var door in roomGenerator.casa.puertas)
            {
                // Verificar si esta puerta conecta con esta habitación
                if (door.cuarto1 == roomPosition || door.cuarto2 == roomPosition)
                {
                    var doorData = ConvertToDoorData(door, roomPosition);
                    if (doorData != null)
                        doors.Add(doorData);
                }
            }
            
            return doors;
        }
        
        /// <summary>
        /// Convierte Door del generador a DoorData del sistema de voz
        /// </summary>
        private DoorData ConvertToDoorData(Door generatorDoor, Vector2Int fromRoom)
        {
            if (generatorDoor == null) return null;
            
            // Check cache
            if (doorDataCache.TryGetValue(generatorDoor.id, out DoorData cached))
                return cached;
            
            // Determinar habitación destino
            Vector2Int toRoom = (generatorDoor.cuarto1 == fromRoom) 
                ? generatorDoor.cuarto2 
                : generatorDoor.cuarto1;
            
            // Calcular dirección
            string direction = CalculateDirection(fromRoom, toRoom);
            
            // Buscar nombre de habitación destino
            Room destinationRoom = FindRoomAtPosition(toRoom);
            string destinationName = destinationRoom != null ? destinationRoom.nombre : "Desconocido";
            
            var doorData = new DoorData
            {
                doorId = "door_" + generatorDoor.id,
                doorName = $"Puerta hacia {destinationName}",
                direction = direction,
                isLocked = !generatorDoor.abierta, // Invertir lógica
                leadsToRoomId = destinationRoom != null ? "room_" + destinationRoom.id : "",
                keyItemId = (generatorDoor.variableNecesaria == -1) 
                    ? "" 
                    : "key_" + generatorDoor.variableNecesaria,
                description = string.IsNullOrEmpty(generatorDoor.descripcionPuerta)
                    ? $"Una puerta de madera que lleva hacia {direction}"
                    : generatorDoor.descripcionPuerta
            };
            
            // Si está bloqueada y tiene mensaje, usarlo como descripción
            if (doorData.isLocked && !string.IsNullOrEmpty(generatorDoor.mensajeBloqueada))
            {
                doorData.description += $". {generatorDoor.mensajeBloqueada}";
            }
            
            // Cache
            doorDataCache[generatorDoor.id] = doorData;
            return doorData;
        }
        
        /// <summary>
        /// Calcula dirección cardinal desde dos posiciones
        /// </summary>
        private string CalculateDirection(Vector2Int from, Vector2Int to)
        {
            int deltaX = to.x - from.x;
            int deltaY = to.y - from.y;
            
            // Priorizar eje más significativo
            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                return deltaX > 0 ? "este" : "oeste";
            }
            else if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
            {
                return deltaY > 0 ? "sur" : "norte"; // Y aumenta hacia abajo en matriz
            }
            else if (deltaX != 0)
            {
                return deltaX > 0 ? "este" : "oeste";
            }
            
            return "aquí"; // Misma posición (no debería pasar)
        }
        
        /// <summary>
        /// Encuentra habitación en una posición específica
        /// </summary>
        private Room FindRoomAtPosition(Vector2Int position)
        {
            if (roomGenerator == null || roomGenerator.casa == null)
                return null;
            
            return roomGenerator.casa.habitaciones.FirstOrDefault(r => r.posicion == position);
        }
        
        #endregion
        
        #region IRoomSystemProvider Implementation
        
        public RoomData GetCurrentRoom()
        {
            Room generatorRoom = FindRoomAtPosition(currentPlayerPosition);
            return ConvertToRoomData(generatorRoom);
        }
        
        public RoomData GetRoomById(string roomId)
        {
            // Extraer ID numérico del string "room_12345"
            if (!roomId.StartsWith("room_")) return null;
            
            if (int.TryParse(roomId.Substring(5), out int numericId))
            {
                Room generatorRoom = roomGenerator.casa.habitaciones.FirstOrDefault(r => r.id == numericId);
                return ConvertToRoomData(generatorRoom);
            }
            
            return null;
        }
        
        public bool TryMoveThroughDoor(string doorId, out string failureReason)
        {
            failureReason = "";
            
            // Extraer ID numérico
            if (!doorId.StartsWith("door_")) 
            {
                failureReason = "ID de puerta inválido";
                return false;
            }
            
            if (!int.TryParse(doorId.Substring(5), out int numericId))
            {
                failureReason = "No se pudo parsear ID de puerta";
                return false;
            }
            
            // Buscar puerta
            Door door = roomGenerator.casa.puertas.FirstOrDefault(d => d.id == numericId);
            if (door == null)
            {
                failureReason = "Puerta no encontrada";
                return false;
            }
            
            // Verificar si está abierta
            if (!door.abierta)
            {
                failureReason = string.IsNullOrEmpty(door.mensajeBloqueada) 
                    ? "La puerta está bloqueada" 
                    : door.mensajeBloqueada;
                return false;
            }
            
            // Determinar nueva posición
            Vector2Int newPosition = (door.cuarto1 == currentPlayerPosition) 
                ? door.cuarto2 
                : door.cuarto1;
            
            // Verificar que existe habitación destino
            Room destinationRoom = FindRoomAtPosition(newPosition);
            if (destinationRoom == null)
            {
                failureReason = "No hay habitación al otro lado de la puerta";
                return false;
            }
            
            // MOVER JUGADOR
            Vector2Int previousPosition = currentPlayerPosition;
            currentPlayerPosition = newPosition;
            visitedRooms.Add(newPosition);
            
            Debug.Log($"[RoomBridge] Jugador se movió de {previousPosition} a {newPosition}");
            
            // Disparar evento
            RoomData newRoomData = ConvertToRoomData(destinationRoom);
            OnRoomChanged?.Invoke(newRoomData);
            
            return true;
        }
        
        public void UpdateRoomState(RoomData room)
        {
            // Sincronizar cambios de VoiceSystem de vuelta al generador
            // Por ahora, solo lectura (el generador es la fuente de verdad)
            Debug.Log($"[RoomBridge] UpdateRoomState llamado para {room.roomId} (solo lectura por ahora)");
        }
        
        public bool TryUnlockDoor(string doorId, string keyItemId)
        {
            if (!doorId.StartsWith("door_")) return false;
            if (!int.TryParse(doorId.Substring(5), out int numericId)) return false;
            
            Door door = roomGenerator.casa.puertas.FirstOrDefault(d => d.id == numericId);
            if (door == null) return false;
            
            // Verificar que la llave coincide
            string expectedKey = door.variableNecesaria == -1 ? "" : "key_" + door.variableNecesaria;
            if (expectedKey != keyItemId) return false;
            
            // ABRIR PUERTA
            door.abierta = true;
            
            Debug.Log($"[RoomBridge] Puerta {doorId} desbloqueada con {keyItemId}");
            
            // Disparar evento
            DoorData doorData = ConvertToDoorData(door, currentPlayerPosition);
            OnDoorStateChanged?.Invoke(doorData);
            
            // Limpiar cache de esta puerta
            doorDataCache.Remove(numericId);
            
            return true;
        }
        
        public DoorData[] GetCurrentRoomDoors()
        {
            return GetDoorsForRoom(currentPlayerPosition).ToArray();
        }
        
        public bool HasVisitedRoom(string roomId)
        {
            if (!roomId.StartsWith("room_")) return false;
            if (!int.TryParse(roomId.Substring(5), out int numericId)) return false;
            
            Room room = roomGenerator.casa.habitaciones.FirstOrDefault(r => r.id == numericId);
            if (room == null) return false;
            
            return visitedRooms.Contains(room.posicion);
        }
        
        public int GetTotalRoomCount()
        {
            return roomGenerator.casa.habitaciones.Count;
        }
        
        public int GetVisitedRoomCount()
        {
            return visitedRooms.Count;
        }
        
        #endregion
        
        #region Sincronización con Otros Sistemas
        
        /// <summary>
        /// Sincroniza vida/fatiga/tiempo del jugador al GameContext
        /// Usa Singletons para acceso optimizado
        /// </summary>
        public void SyncPlayerStateToContext(GameContext context)
        {
            if (context == null) return;
            
            // Sincronizar vida/fatiga (desde FatigueSystem Singleton)
            var fatigueSys = fatigueSystem ?? FatigueSystem.Instance;
            if (fatigueSys != null && fatigueSys.PlayerLives != null)
            {
                context.health = fatigueSys.PlayerLives.currentLives;
                context.maxHealth = fatigueSys.PlayerLives.totalLives;
                context.fatigue = fatigueSys.NivelFatiga;
            }
            
            // Sincronizar tiempo (desde GameTimer Singleton)
            var timer = gameTimer ?? GameTimer.Instance;
            if (timer != null)
            {
                float remainingMinutes = timer.GetRemainingTime() / 60f;
                context.timeRemaining = remainingMinutes;
                
                // Calcular hora del juego (asumiendo inicio a las 2:00 AM)
                float elapsedMinutes = (timer.GetTotalTime() - timer.GetRemainingTime()) / 60f;
                int hour = 2 + Mathf.FloorToInt(elapsedMinutes / 60f);
                int minute = Mathf.FloorToInt(elapsedMinutes % 60f);
                context.gameTime = $"{hour:D2}:{minute:D2} AM";
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Limpia el cache de conversiones (llamar si se regenera la casa)
        /// </summary>
        public void ClearCache()
        {
            roomDataCache.Clear();
            doorDataCache.Clear();
            Debug.Log("[RoomBridge] Cache limpiado");
        }
        
        /// <summary>
        /// Reinicia tracking de habitaciones visitadas
        /// </summary>
        public void ResetVisitedRooms()
        {
            visitedRooms.Clear();
            visitedRooms.Add(currentPlayerPosition);
            Debug.Log("[RoomBridge] Habitaciones visitadas reiniciadas");
        }
        
        #endregion
        
        // Debug
        [ContextMenu("Debug: Mostrar Habitación Actual")]
        private void DebugCurrentRoom()
        {
            RoomData current = GetCurrentRoom();
            if (current != null)
            {
                Debug.Log($"=== HABITACIÓN ACTUAL ===\n" +
                         $"ID: {current.roomId}\n" +
                         $"Nombre: {current.roomName}\n" +
                         $"Descripción: {current.shortDescription}\n" +
                         $"Puertas: {current.doors.Count}\n" +
                         $"Objetos: {string.Join(", ", current.objects)}");
            }
        }
        
        [ContextMenu("Debug: Mostrar Todas las Puertas")]
        private void DebugDoors()
        {
            var doors = GetCurrentRoomDoors();
            foreach (var door in doors)
            {
                Debug.Log($"Puerta: {door.doorName} ({door.direction}) - " +
                         $"{(door.isLocked ? "BLOQUEADA" : "Abierta")}");
            }
        }
    }
}

