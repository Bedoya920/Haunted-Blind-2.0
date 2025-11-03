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
        // Singleton
        private static RoomSystemBridge _instance;
        private static bool _isInitialized = false;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showCacheDebugLogs = false; // Logs de cache
        [SerializeField] private bool showDoorDebugLogs = false;  // Logs de búsqueda de puertas
        [SerializeField] private bool showMovementLogs = true;    // Logs de movimiento del jugador
        
        public static RoomSystemBridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RoomSystemBridge>();
                    if (_instance == null)
                    {
                        var go = new GameObject("RoomSystemBridge");
                        _instance = go.AddComponent<RoomSystemBridge>();
                        DontDestroyOnLoad(go);
                    }
                }
                
                // CRÍTICO: Asegurar que InitializeBridge se llame aunque Awake no haya corrido
                if (!_isInitialized && _instance != null)
                {
                    _instance.EnsureCachesInitialized();
                }
                
                return _instance;
            }
        }
        
        [Header("Referencias")]
        [SerializeField] private RoomGenerator3000 roomGenerator;
        [SerializeField] private FatigueSystem fatigueSystem;
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private RoomInventoryManager inventoryManager;
        
        [Header("Estado del Jugador")]
        [Tooltip("Posición actual del jugador en la matriz")]
        public Vector2Int currentPlayerPosition;
        
        [Header("Estado de Inicialización")]
        public bool isFullyInitialized = false; // Flag para saber si SetRoomGenerator() terminó
        
        // Eventos (implementados de IRoomSystemProvider)
        public event System.Action<RoomData> OnRoomChanged;
        public event System.Action<DoorData> OnDoorStateChanged;
        public event System.Action<string> OnItemCollected; // Se dispara en TryMoveThroughDoor cuando se usa llave
        
        // Cache para evitar conversiones repetidas
        private Dictionary<int, RoomData> roomDataCache;
        private Dictionary<int, DoorData> doorDataCache;
        private HashSet<Vector2Int> visitedRooms;
        
        void Awake()
        {
            // Singleton pattern
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeBridge();
                _isInitialized = true;
                Debug.Log("[RoomBridge] Awake() ejecutado - Singleton inicializado");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void InitializeBridge()
        {
            Debug.Log("[RoomBridge] InitializeBridge() llamado");
            
            roomDataCache = new Dictionary<int, RoomData>();
            doorDataCache = new Dictionary<int, DoorData>();
            visitedRooms = new HashSet<Vector2Int>();
            
            // Validar referencias
            if (roomGenerator == null)
                Debug.LogWarning("[RoomBridge] RoomGenerator3000 no asignado - será buscado en Start");
            
            // Obtener o crear RoomInventoryManager
            if (inventoryManager == null)
            {
                inventoryManager = RoomInventoryManager.Instance;
            }
            
            Debug.Log("[RoomBridge] ✅ Singleton inicializado - Caches creados");
        }
        
        /// <summary>
        /// Asegurar que los caches están inicializados (llamar al principio de métodos críticos)
        /// </summary>
        private void EnsureCachesInitialized()
        {
            bool needsInit = false;
            
            if (roomDataCache == null)
            {
                roomDataCache = new Dictionary<int, RoomData>();
                needsInit = true;
            }
            
            if (doorDataCache == null)
            {
                doorDataCache = new Dictionary<int, DoorData>();
                needsInit = true;
            }
            
            if (visitedRooms == null)
            {
                visitedRooms = new HashSet<Vector2Int>();
                needsInit = true;
            }
            
            if (needsInit)
            {
                Debug.LogWarning("[RoomBridge] ⚠️ Caches inicializados tardíamente (Awake no corrió primero)");
                _isInitialized = true;
            }
        }
        
        void Start()
        {
            // Solo buscar referencias, NO acceder a roomGenerator.casa
            if (roomGenerator == null)
                roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
            
            if (inventoryManager == null)
                inventoryManager = RoomInventoryManager.Instance;
            
            if (fatigueSystem == null)
                fatigueSystem = FatigueSystem.Instance;
                
            if (gameTimer == null)
                gameTimer = GameTimer.Instance;
            
            // NO inicializar posición aquí - esperar a SetRoomGenerator()
            Debug.Log("[RoomBridge] Start() completado - esperando inicialización de casa");
        }
        
        /// <summary>
        /// Asignar RoomGenerator manualmente (útil para GameInitializer)
        /// </summary>
        public void SetRoomGenerator(RoomGenerator3000 generator)
        {
            EnsureCachesInitialized(); // CRÍTICO: Asegurar caches antes de todo
            
            if (generator == null)
            {
                Debug.LogError("[RoomBridge] SetRoomGenerator recibió null generator");
                return;
            }
            
            if (generator.casa == null)
            {
                Debug.LogError("[RoomBridge] SetRoomGenerator: generator.casa es null");
                return;
            }
            
            roomGenerator = generator;
            currentPlayerPosition = generator.casa.habitacionInicial;
            
            visitedRooms.Clear();
            visitedRooms.Add(currentPlayerPosition);
            ClearCache();
            
            Debug.Log($"[RoomBridge] ✅ RoomGenerator asignado. Habitación inicial: {currentPlayerPosition}, Total habitaciones: {generator.casa.habitaciones.Count}, Total puertas: {generator.casa.puertas.Count}");
            
            // NUEVO: Verificar puertas de la habitación inicial
            var doorsInStart = GetDoorsForRoom(currentPlayerPosition);
            Debug.Log($"[RoomBridge] Puertas en habitación inicial: {doorsInStart.Count}");
            foreach (var door in doorsInStart)
            {
                Debug.Log($"  - {door.doorName} hacia {door.direction}");
            }
            
            // Marcar como completamente inicializado
            isFullyInitialized = true;
            Debug.Log("[RoomBridge] ✅ Completamente inicializado y listo");
            
            // DIAGNÓSTICO: Mostrar TODAS las puertas con sus direcciones
            DiagnosticarTodasLasPuertas();
        }
        
        /// <summary>
        /// Diagnóstico completo de todas las puertas y sus direcciones
        /// </summary>
        [ContextMenu("🔍 Diagnosticar Todas las Puertas")]
        public void DiagnosticarTodasLasPuertas()
        {
            if (roomGenerator == null || roomGenerator.casa == null)
            {
                Debug.LogError("[RoomBridge] No hay casa generada para diagnosticar");
                return;
            }
            
            Debug.Log($"\n========== 🚪 DIAGNÓSTICO DE PUERTAS ==========");
            Debug.Log($"Total de puertas: {roomGenerator.casa.puertas.Count}");
            Debug.Log($"Total de habitaciones: {roomGenerator.casa.habitaciones.Count}\n");
            
            foreach (var door in roomGenerator.casa.puertas)
            {
                var room1 = FindRoomAtPosition(door.cuarto1);
                var room2 = FindRoomAtPosition(door.cuarto2);
                
                string nombre1 = room1 != null ? room1.nombre : "???";
                string nombre2 = room2 != null ? room2.nombre : "???";
                
                string dir1to2 = CalculateDirection(door.cuarto1, door.cuarto2);
                string dir2to1 = CalculateDirection(door.cuarto2, door.cuarto1);
                
                Debug.Log($"Puerta #{door.id}: {nombre1}({door.cuarto1.x},{door.cuarto1.y}) ↔ {nombre2}({door.cuarto2.x},{door.cuarto2.y})");
                Debug.Log($"  Desde {nombre1}: ve hacia '{dir1to2}' para ir a {nombre2}");
                Debug.Log($"  Desde {nombre2}: ve hacia '{dir2to1}' para volver a {nombre1}");
                Debug.Log($"  Estado: {(door.abierta ? "🟢 ABIERTA" : "🔒 CERRADA")}\n");
            }
            
            Debug.Log($"========== FIN DIAGNÓSTICO ==========\n");
        }
        
        #region Conversión Room → RoomData
        
        /// <summary>
        /// Convierte Room del generador a RoomData del sistema de voz
        /// </summary>
        private RoomData ConvertToRoomData(Room generatorRoom, bool forceRecalculate = false)
        {
            EnsureCachesInitialized(); // CRÍTICO: Llamar siempre primero
            
            if (generatorRoom == null)
            {
                Debug.LogError("[RoomBridge] ConvertToRoomData: generatorRoom es null");
                return null;
            }
            
            // ⚠️ CACHE DESHABILITADO TEMPORALMENTE PARA DEBUG
            // Check cache (solo si no forzamos recálculo)
            //if (!forceRecalculate && roomDataCache.TryGetValue(generatorRoom.id, out RoomData cached))
            //{
            //    if (showCacheDebugLogs)
            //        Debug.Log($"[RoomBridge] ConvertToRoomData: Usando cache para room ID:{generatorRoom.id}");
            //    return cached;
            //}
            
            // Si forzamos recálculo, limpiar el cache de esta habitación
            if (forceRecalculate && roomDataCache.ContainsKey(generatorRoom.id))
            {
                roomDataCache.Remove(generatorRoom.id);
                Debug.Log($"[RoomBridge] 🔄 Cache invalidado para room ID:{generatorRoom.id} ({generatorRoom.nombre})");
            }
            
            Debug.Log($"[RoomBridge] 🏗️ ConvertToRoomData: Creando RoomData para {generatorRoom.nombre} (ID:{generatorRoom.id}) en posición {generatorRoom.posicion} | forceRecalculate={forceRecalculate}");
            
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
            
            Debug.Log($"[RoomBridge] ✅ RoomData creado: {roomData.roomName} con {roomData.doors.Count} puertas");
            foreach (var d in roomData.doors)
            {
                Debug.Log($"[RoomBridge]   → Puerta: {d.doorName} | Dir: {d.direction} | ID: {d.doorId}");
            }
            
            // ⚠️ CACHE DESHABILITADO TEMPORALMENTE PARA DEBUG
            // Cache
            //roomDataCache[generatorRoom.id] = roomData;
            return roomData;
        }
        
        #endregion
        
        #region Conversión Door → DoorData
        
        /// <summary>
        /// Obtiene todas las puertas conectadas a una posición
        /// </summary>
        private List<DoorData> GetDoorsForRoom(Vector2Int roomPosition)
        {
            EnsureCachesInitialized(); // CRÍTICO: Asegurar caches
            
            var doors = new List<DoorData>();
            
            if (roomGenerator == null || roomGenerator.casa == null || roomGenerator.casa.puertas == null)
            {
                Debug.LogWarning($"[RoomBridge] GetDoorsForRoom: generator, casa, o puertas es null");
                return doors;
            }
            
            if (showDoorDebugLogs)
                Debug.Log($"[RoomBridge] Buscando puertas para posición {roomPosition}. Total puertas en casa: {roomGenerator.casa.puertas.Count}");
            
            foreach (var door in roomGenerator.casa.puertas)
            {
                // DIAGNÓSTICO: Log de cada puerta
                bool matches1 = door.cuarto1 == roomPosition;
                bool matches2 = door.cuarto2 == roomPosition;
                
                if (showDoorDebugLogs)
                    Debug.Log($"[RoomBridge]   Puerta ID:{door.id} - cuarto1:{door.cuarto1} cuarto2:{door.cuarto2} | Match1:{matches1} Match2:{matches2}");
                
                if (matches1 || matches2)
                {
                    var doorData = ConvertToDoorData(door, roomPosition);
                    if (doorData != null)
                    {
                        doors.Add(doorData);
                        if (showDoorDebugLogs)
                            Debug.Log($"[RoomBridge]     ✅ Puerta añadida: {doorData.doorName} hacia {doorData.direction}");
                    }
                    else
                    {
                        Debug.LogWarning($"[RoomBridge]     ⚠️ ConvertToDoorData retornó null para puerta ID:{door.id}");
                    }
                }
            }
            
            if (showDoorDebugLogs)
                Debug.Log($"[RoomBridge] Total puertas encontradas: {doors.Count}");
            return doors;
        }
        
        /// <summary>
        /// Convierte Door del generador a DoorData del sistema de voz
        /// </summary>
        private DoorData ConvertToDoorData(Door generatorDoor, Vector2Int fromRoom)
        {
            EnsureCachesInitialized(); // CRÍTICO: Llamar siempre primero
            
            if (generatorDoor == null) return null;
            
            // ⚠️ CACHE DE PUERTAS DESHABILITADO - El cache anterior no consideraba fromRoom
            // Una puerta tiene DIFERENTE dirección dependiendo desde dónde la mires:
            //   Door #3 desde Hall(0,0) → "abajo" hacia Comedor
            //   Door #3 desde Comedor(0,1) → "arriba" hacia Hall
            // Por eso NO podemos cachear solo por door.id
            
            // Determinar habitación destino
            Vector2Int toRoom = (generatorDoor.cuarto1 == fromRoom) 
                ? generatorDoor.cuarto2 
                : generatorDoor.cuarto1;
            
            // Calcular dirección DESDE fromRoom HACIA toRoom
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
            
            // ⚠️ NO CACHEAR - Las puertas tienen diferente dirección según fromRoom
            // doorDataCache[generatorDoor.id] = doorData;
            return doorData;
        }
        
        /// <summary>
        /// Calcula dirección cardinal desde dos posiciones
        /// </summary>
        private string CalculateDirection(Vector2Int from, Vector2Int to)
        {
            int deltaX = to.x - from.x;
            int deltaY = to.y - from.y;
            
            /* SISTEMA DE NAVEGACIÓN INTUITIVO PARA JUGADORES CIEGOS
             * 
             * Mapa técnico (coordenadas Unity):
             *   X aumenta → DERECHA
             *   Y aumenta → ABAJO en pantalla (como índice de array)
             * 
             * Traducción a lenguaje natural del jugador:
             *   deltaX > 0  →  "derecha"   (Sala está a la derecha de Hall)
             *   deltaX < 0  →  "izquierda" (Hall está a la izquierda de Sala)
             *   deltaY > 0  →  "abajo"     (Comedor está abajo del Hall, avanzando en profundidad)
             *   deltaY < 0  →  "arriba"    (Hall está arriba del Comedor, retrocediendo)
             * 
             * Ejemplo práctico desde Hall(0,0):
             *   Sala(1,0)      → deltaX=+1, deltaY=0  → "derecha"
             *   Comedor(0,1)   → deltaX=0,  deltaY=+1 → "abajo"
             * 
             * Desde Comedor(0,1):
             *   Hall(0,0)      → deltaX=0,  deltaY=-1 → "arriba" (volver)
             *   Cocina(0,2)    → deltaX=0,  deltaY=+1 → "abajo" (avanzar)
             */
            
            // Priorizar eje más significativo
            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                // Movimiento HORIZONTAL predomina
                return deltaX > 0 ? "derecha" : "izquierda";
            }
            else if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
            {
                // Movimiento VERTICAL predomina
                // deltaY > 0 = aumenta Y = ABAJO en el eje vertical (avanzar en profundidad)
                // deltaY < 0 = disminuye Y = ARRIBA en el eje vertical (retroceder)
                return deltaY > 0 ? "abajo" : "arriba";
            }
            else if (deltaX != 0)
            {
                // Diagonal: priorizar horizontal
                return deltaX > 0 ? "derecha" : "izquierda";
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
            EnsureCachesInitialized(); // CRÍTICO: Asegurar caches
            
            if (roomGenerator == null || roomGenerator.casa == null)
            {
                Debug.LogError("[RoomBridge] GetCurrentRoom: roomGenerator o casa es null!");
                return null;
            }
            
            Room generatorRoom = FindRoomAtPosition(currentPlayerPosition);
            if (generatorRoom == null)
            {
                Debug.LogError($"[RoomBridge] GetCurrentRoom: No se encontró habitación en posición {currentPlayerPosition}");
                return null;
            }
            
            return ConvertToRoomData(generatorRoom);
        }
        
        /// <summary>
        /// Obtiene habitación actual SIEMPRE FRESCA (sin cache)
        /// Usa esto para consultas que necesitan datos actualizados de puertas
        /// </summary>
        public RoomData GetCurrentRoomFresh()
        {
            EnsureCachesInitialized();
            
            Debug.Log($"[RoomBridge] 🔄 GetCurrentRoomFresh() llamado - Posición actual: {currentPlayerPosition}");
            
            if (roomGenerator == null || roomGenerator.casa == null)
            {
                Debug.LogError("[RoomBridge] GetCurrentRoomFresh: roomGenerator o casa es null!");
                return null;
            }
            
            Room generatorRoom = FindRoomAtPosition(currentPlayerPosition);
            if (generatorRoom == null)
            {
                Debug.LogError($"[RoomBridge] GetCurrentRoomFresh: No se encontró habitación en posición {currentPlayerPosition}");
                return null;
            }
            
            Debug.Log($"[RoomBridge] 📍 Habitación encontrada: {generatorRoom.nombre} (ID:{generatorRoom.id}) en {currentPlayerPosition}");
            
            // FORZAR RECÁLCULO - siempre bypass cache
            var freshRoom = ConvertToRoomData(generatorRoom, forceRecalculate: true);
            
            Debug.Log($"[RoomBridge] ✅ GetCurrentRoomFresh retorna: {freshRoom.roomName} con {freshRoom.doors.Count} puertas");
            foreach (var door in freshRoom.doors)
            {
                Debug.Log($"[RoomBridge]   🚪 {door.doorName} → {door.direction} (ID: {door.doorId}, Locked: {door.isLocked})");
            }
            
            return freshRoom;
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
            
            Debug.Log($"[RoomBridge] 🚪 TryMoveThroughDoor: {doorId}");
            
            // Extraer ID numérico
            if (!doorId.StartsWith("door_")) 
            {
                failureReason = "ID de puerta inválido";
                Debug.LogWarning($"[RoomBridge] ❌ ID inválido: {doorId}");
                return false;
            }
            
            if (!int.TryParse(doorId.Substring(5), out int numericId))
            {
                failureReason = "No se pudo parsear ID de puerta";
                Debug.LogWarning($"[RoomBridge] ❌ No se pudo parsear: {doorId}");
                return false;
            }
            
            Debug.Log($"[RoomBridge] Buscando puerta con ID numérico: {numericId}");
            
            // Buscar puerta
            Door door = roomGenerator.casa.puertas.FirstOrDefault(d => d.id == numericId);
            if (door == null)
            {
                failureReason = "Puerta no encontrada";
                Debug.LogError($"[RoomBridge] ❌ Puerta {numericId} no existe en casa.puertas");
                
                // Diagnóstico: Mostrar todas las puertas disponibles
                Debug.Log($"[RoomBridge] Puertas disponibles: {string.Join(", ", roomGenerator.casa.puertas.Select(p => p.id))}");
                return false;
            }
            
            Debug.Log($"[RoomBridge] ✅ Puerta encontrada: ID={door.id}, cuarto1=({door.cuarto1.x},{door.cuarto1.y}), cuarto2=({door.cuarto2.x},{door.cuarto2.y}), abierta={door.abierta}");
            
            // NUEVO: Verificar si requiere tiempo específico
            if (door.timeToUnlock > 0)
            {
                var gameTimer = GameTimer.Instance;
                if (gameTimer != null)
                {
                    // Calcular tiempo transcurrido desde el inicio
                    float elapsedTime = gameTimer.GetTotalTime() - gameTimer.GetRemainingTime();
                    
                    if (elapsedTime < door.timeToUnlock)
                    {
                        float timeUntilUnlock = door.timeToUnlock - elapsedTime;
                        failureReason = !string.IsNullOrEmpty(door.mensajeBloqueada)
                            ? door.mensajeBloqueada
                            : $"Esta puerta se desbloqueará más adelante. Quedan {Mathf.CeilToInt(timeUntilUnlock)} segundos.";
                        Debug.Log($"[RoomBridge] ⏰ Puerta requiere tiempo {door.timeToUnlock}s. Transcurrido: {elapsedTime}s. Faltan: {timeUntilUnlock}s");
                        return false;
                    }
                    else
                    {
                        Debug.Log($"[RoomBridge] ⏰ Puerta desbloqueada por tiempo ({elapsedTime}s >= {door.timeToUnlock}s)");
                    }
                }
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
            
            if (showMovementLogs)
                Debug.Log($"[RoomBridge] 🎮 Jugador: {previousPosition} → {newPosition}");
            
            // Disparar evento - FORZAR RECÁLCULO para obtener puertas frescas
            RoomData newRoomData = ConvertToRoomData(destinationRoom, true);
            OnRoomChanged?.Invoke(newRoomData);
            
            // Sync with PlayerStateManager
            var playerState = PlayerStateManager.Instance;
            if (playerState != null)
            {
                playerState.UpdatePosition(newPosition, newRoomData);
            }
            
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
        
        /// <summary>
        /// Desbloquea puerta por ID de llave (llamado por RoomInventoryManager)
        /// </summary>
        public void UnlockDoorWithKeyId(int keyId)
        {
            if (roomGenerator == null || roomGenerator.casa == null)
            {
                Debug.LogWarning("[RoomBridge] No hay casa generada");
                return;
            }
            
            // Buscar puerta que requiere esta llave
            var door = roomGenerator.casa.puertas.Find(d => d.variableNecesaria == keyId);
            if (door != null)
            {
                door.abierta = true;
                Debug.Log($"[RoomBridge] 🔓 Puerta #{door.id} desbloqueada automáticamente con llave ID:{keyId}");
                
                // Limpiar cache de puertas para forzar recálculo
                ClearCache();
                
                // Notificar cambio de estado
                DoorData doorData = ConvertToDoorData(door, currentPlayerPosition);
                OnDoorStateChanged?.Invoke(doorData);
            }
            else
            {
                Debug.LogWarning($"[RoomBridge] No se encontró puerta que requiera llave ID:{keyId}");
            }
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

