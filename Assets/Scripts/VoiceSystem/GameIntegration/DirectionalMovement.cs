using UnityEngine;
using VoiceSystem.Core.Data;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Traduce comandos direccionales (adelante, atrás, derecha, izquierda)
    /// a puertas cardinales (norte, sur, este, oeste)
    /// </summary>
    public class DirectionalMovement : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private RoomSystemBridge roomBridge;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false; // Deshabilitado por defecto
        
        private void Start()
        {
            if (roomBridge == null)
            {
                roomBridge = RoomSystemBridge.Instance;
            }
        }
        
        /// <summary>
        /// Traduce una dirección relativa a una puerta cardinal
        /// </summary>
        /// <param name="direction">adelante, atras, derecha, izquierda</param>
        /// <returns>DoorData si existe una puerta en esa dirección, null si no</returns>
        public DoorData TranslateDirectionToDoor(string direction)
        {
            if (roomBridge == null)
            {
                Debug.LogWarning("[DirectionalMovement] RoomSystemBridge no encontrado");
                return null;
            }
            
            var currentRoom = roomBridge.GetCurrentRoom();
            if (currentRoom == null)
            {
                Debug.LogWarning("[DirectionalMovement] No hay habitación actual");
                return null;
            }
            
            // Traducir dirección relativa a cardinal
            string cardinalDirection = TranslateToCardinal(direction);
            
            if (string.IsNullOrEmpty(cardinalDirection))
            {
                LogDebug($"[DirectionalMovement] Dirección '{direction}' no reconocida");
                return null;
            }
            
            // Buscar puerta en la dirección cardinal
            var door = currentRoom.GetDoorByDirection(cardinalDirection);
            
            if (door != null)
            {
                LogDebug($"[DirectionalMovement] '{direction}' → '{cardinalDirection}' → Puerta encontrada: {door.doorName}");
            }
            else
            {
                LogDebug($"[DirectionalMovement] '{direction}' → '{cardinalDirection}' → No hay puerta en esa dirección");
            }
            
            return door;
        }
        
        /// <summary>
        /// Traduce dirección relativa a visual (del mapa)
        /// </summary>
        private string TranslateToCardinal(string direction)
        {
            string normalizedDirection = direction.ToLower().Trim();
            
            switch (normalizedDirection)
            {
                case "adelante":
                case "frente":
                case "enfrente":
                case "arriba":
                    return "arriba";
                    
                case "atras":
                case "atrás":
                case "detras":
                case "detrás":
                case "abajo":
                    return "abajo";
                    
                case "derecha":
                    return "derecha";
                    
                case "izquierda":
                    return "izquierda";
                    
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Verifica si el jugador puede moverse en una dirección
        /// </summary>
        /// <param name="direction">Dirección relativa</param>
        /// <param name="reason">Razón por la cual no puede moverse (si aplica)</param>
        /// <returns>True si puede moverse, false si no</returns>
        public bool CanMoveInDirection(string direction, out string reason)
        {
            reason = "";
            
            var door = TranslateDirectionToDoor(direction);
            
            if (door == null)
            {
                reason = $"No hay una puerta en esa dirección";
                return false;
            }
            
            // Verificar si la puerta está bloqueada
            if (door.isLocked)
            {
                if (!string.IsNullOrEmpty(door.keyItemId))
                {
                    reason = $"La puerta está bloqueada. Necesitas: {door.keyItemId}";
                }
                else
                {
                    reason = $"La puerta está bloqueada y no se puede abrir";
                }
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Obtiene descripción de las direcciones disponibles
        /// </summary>
        public string GetAvailableDirectionsDescription()
        {
            if (roomBridge == null)
            {
                return "No hay información de direcciones disponible";
            }
            
            var currentRoom = roomBridge.GetCurrentRoom();
            if (currentRoom == null || currentRoom.doors == null || currentRoom.doors.Count == 0)
            {
                return "No hay puertas disponibles";
            }
            
            var descriptions = new System.Text.StringBuilder();
            descriptions.AppendLine("Direcciones disponibles:");
            
            // Mapear cada puerta a una dirección
            var directionMap = new System.Collections.Generic.Dictionary<string, string>
            {
                {"norte", "adelante"},
                {"sur", "atrás"},
                {"este", "derecha"},
                {"oeste", "izquierda"}
            };
            
            foreach (var door in currentRoom.doors)
            {
                if (!door.isLocked && directionMap.ContainsKey(door.direction.ToLower()))
                {
                    string relativeDirection = directionMap[door.direction.ToLower()];
                    descriptions.AppendLine($"  - {relativeDirection} ({door.direction}): {door.doorName}");
                }
            }
            
            return descriptions.ToString();
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
    }
}

