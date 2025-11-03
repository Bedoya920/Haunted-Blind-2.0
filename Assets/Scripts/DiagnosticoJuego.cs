using UnityEngine;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core.Data;

/// <summary>
/// Script de diagnóstico para verificar el estado del juego
/// Presiona D para ver el diagnóstico completo
/// </summary>
public class DiagnosticoJuego : MonoBehaviour
{
    private void Update()
    {
        // Presiona D para diagnóstico
        if (Input.GetKeyDown(KeyCode.D))
        {
            MostrarDiagnostico();
        }
        
        // Presiona L para ver logs de movimiento
        if (Input.GetKeyDown(KeyCode.L))
        {
            MostrarInfoMovimiento();
        }
        
        // Presiona E para ver eventos cargados
        if (Input.GetKeyDown(KeyCode.E))
        {
            MostrarEventos();
        }
    }
    
    private void MostrarDiagnostico()
    {
        Debug.Log("========== DIAGNÓSTICO DEL JUEGO ==========");
        
        // 1. Room System
        var roomBridge = RoomSystemBridge.Instance;
        if (roomBridge != null)
        {
            var currentRoom = roomBridge.GetCurrentRoom();
            if (currentRoom != null)
            {
                Debug.Log($"✅ HABITACIÓN ACTUAL: {currentRoom.roomName} (ID: {currentRoom.roomId})");
                Debug.Log($"   Descripción: {currentRoom.shortDescription}");
                Debug.Log($"   Puertas disponibles: {currentRoom.doors?.Count ?? 0}");
                
                if (currentRoom.doors != null)
                {
                    foreach (var door in currentRoom.doors)
                    {
                        Debug.Log($"   → Puerta: {door.doorName} | Dirección: {door.direction} | ID: {door.doorId} | Bloqueada: {door.isLocked}");
                    }
                }
                
                Debug.Log($"   Objetos visibles: {currentRoom.objects?.Count ?? 0}");
                if (currentRoom.objects != null && currentRoom.objects.Count > 0)
                {
                    Debug.Log($"   → {string.Join(", ", currentRoom.objects)}");
                }
            }
            else
            {
                Debug.LogError("❌ NO HAY HABITACIÓN ACTUAL");
            }
        }
        else
        {
            Debug.LogError("❌ RoomSystemBridge NO ENCONTRADO");
        }
        
        // 2. Player Data
        var playerData = Resources.Load<PlayerData>("Data/PlayerData");
        if (playerData != null)
        {
            Debug.Log($"✅ PLAYER DATA:");
            Debug.Log($"   Salud: {playerData.CurrentHealth}");
            Debug.Log($"   Fatiga: {playerData.CurrentFatigue}");
            Debug.Log($"   Habitación actual: {playerData.CurrentRoomId}");
            Debug.Log($"   Inventario: {playerData.Inventory.Count} items");
            if (playerData.Inventory.Count > 0)
            {
                Debug.Log($"   → {string.Join(", ", playerData.Inventory)}");
            }
            Debug.Log($"   Habitaciones visitadas: {playerData.VisitedRooms.Count}");
            if (playerData.VisitedRooms.Count > 0)
            {
                Debug.Log($"   → {string.Join(", ", playerData.VisitedRooms)}");
            }
        }
        else
        {
            Debug.LogError("❌ PlayerData NO CARGADO desde Resources/Data/");
        }
        
        // 3. Event Manager
        var eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            Debug.Log($"✅ EVENT MANAGER:");
            Debug.Log($"   Main Events: {eventManager.MainEvents?.Length ?? 0}");
            Debug.Log($"   Random Events: {eventManager.RandomEvents?.Length ?? 0}");
            Debug.Log($"   Story Events: {eventManager.StoryEvents?.Length ?? 0}");
            Debug.Log($"   Screamers: {eventManager.Screamers?.Length ?? 0}");
        }
        else
        {
            Debug.LogError("❌ EventManager NO ENCONTRADO");
        }
        
        // 4. Story Event Trigger
        var storyTrigger = FindFirstObjectByType<StoryEventTrigger>();
        if (storyTrigger != null)
        {
            Debug.Log($"✅ STORY EVENT TRIGGER encontrado");
        }
        else
        {
            Debug.LogError("❌ StoryEventTrigger NO ENCONTRADO - Los eventos de historia NO se dispararán");
        }
        
        // 5. Directional Movement
        var directionalMovement = FindFirstObjectByType<DirectionalMovement>();
        if (directionalMovement != null)
        {
            Debug.Log($"✅ DIRECTIONAL MOVEMENT encontrado");
        }
        else
        {
            Debug.LogError("❌ DirectionalMovement NO ENCONTRADO - Comandos direccionales NO funcionarán");
        }
        
        // 6. Fatigue System
        var fatigueSystem = FatigueSystem.Instance;
        if (fatigueSystem != null)
        {
            Debug.Log($"✅ FATIGUE SYSTEM:");
            Debug.Log($"   Vidas: {fatigueSystem.PlayerLives?.currentLives ?? 0}");
            Debug.Log($"   Fatiga: {fatigueSystem.NivelFatiga}");
        }
        
        // 7. Voice System
        var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
        if (voiceSystem != null)
        {
            Debug.Log($"✅ VOICE SYSTEM:");
            Debug.Log($"   Escuchando: {voiceSystem.IsListening}");
        }
        
        Debug.Log("========== FIN DEL DIAGNÓSTICO ==========");
    }
    
    private void MostrarInfoMovimiento()
    {
        Debug.Log("========== INFO DE MOVIMIENTO ==========");
        
        var roomBridge = RoomSystemBridge.Instance;
        if (roomBridge == null)
        {
            Debug.LogError("❌ No hay RoomSystemBridge");
            return;
        }
        
        var currentRoom = roomBridge.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogError("❌ No hay habitación actual");
            return;
        }
        
        Debug.Log($"Estás en: {currentRoom.roomName}");
        Debug.Log("Puertas disponibles:");
        
        if (currentRoom.doors == null || currentRoom.doors.Count == 0)
        {
            Debug.LogWarning("⚠️ NO HAY PUERTAS - No puedes moverte");
            return;
        }
        
        foreach (var door in currentRoom.doors)
        {
            string direccionRelativa = "";
            switch (door.direction.ToLower())
            {
                case "norte": direccionRelativa = "adelante"; break;
                case "sur": direccionRelativa = "atrás"; break;
                case "este": direccionRelativa = "derecha"; break;
                case "oeste": direccionRelativa = "izquierda"; break;
                default: direccionRelativa = door.direction; break;
            }
            
            Debug.Log($"  {direccionRelativa} ({door.direction}) → {door.doorName} (ID: {door.doorId})");
            if (door.isLocked)
            {
                Debug.Log($"    ⚠️ BLOQUEADA - Necesitas: {door.keyItemId}");
            }
        }
        
        Debug.Log("========================================");
    }
    
    private void MostrarEventos()
    {
        Debug.Log("========== EVENTOS DISPONIBLES ==========");
        
        var eventManager = EventManager.Instance;
        if (eventManager == null)
        {
            Debug.LogError("❌ No hay EventManager");
            return;
        }
        
        var roomBridge = RoomSystemBridge.Instance;
        var currentRoom = roomBridge?.GetCurrentRoom();
        string roomId = currentRoom?.roomId ?? "desconocido";
        
        Debug.Log($"Buscando eventos para habitación: {roomId}");
        
        // Buscar story events para esta habitación
        if (eventManager.StoryEvents != null)
        {
            Debug.Log($"\nSTORY EVENTS en esta habitación:");
            int count = 0;
            foreach (var evt in eventManager.StoryEvents)
            {
                if (evt.roomId == roomId)
                {
                    Debug.Log($"  [{evt.id}] {evt.eventName}");
                    Debug.Log($"      Trigger: {evt.triggerCondition} | Único: {evt.isUnique} | Disparado: {evt.hasBeenTriggered}");
                    count++;
                }
            }
            if (count == 0)
            {
                Debug.LogWarning($"  ⚠️ No hay story events para roomId '{roomId}'");
                Debug.Log("  Eventos disponibles por habitación:");
                var roomIds = new System.Collections.Generic.HashSet<string>();
                foreach (var evt in eventManager.StoryEvents)
                {
                    if (!roomIds.Contains(evt.roomId))
                    {
                        roomIds.Add(evt.roomId);
                        Debug.Log($"    - {evt.roomId}");
                    }
                }
            }
        }
        
        Debug.Log("========================================");
    }
    
    [ContextMenu("Diagnosticar Puertas Detallado")]
    public void DiagnosticarPuertasDetallado()
    {
        var bridge = RoomSystemBridge.Instance;
        if (bridge == null)
        {
            Debug.LogError("RoomSystemBridge no encontrado");
            return;
        }
        
        var generator = FindFirstObjectByType<RoomGenerator3000>();
        if (generator == null || generator.casa == null)
        {
            Debug.LogError("RoomGenerator o casa no encontrados");
            return;
        }
        
        Debug.Log("========== DIAGNÓSTICO DE PUERTAS ==========");
        Debug.Log($"Posición actual del jugador: {bridge.currentPlayerPosition}");
        Debug.Log($"Total puertas en casa: {generator.casa.puertas.Count}");
        Debug.Log($"Total habitaciones: {generator.casa.habitaciones.Count}");
        
        Debug.Log("\n--- TODAS LAS PUERTAS ---");
        foreach (var door in generator.casa.puertas)
        {
            Debug.Log($"Puerta ID:{door.id} | Conecta: {door.cuarto1} <-> {door.cuarto2} | Abierta:{door.abierta}");
        }
        
        Debug.Log("\n--- PUERTAS DE LA HABITACIÓN ACTUAL ---");
        var currentRoom = bridge.GetCurrentRoom();
        if (currentRoom != null)
        {
            Debug.Log($"Habitación: {currentRoom.roomName}");
            Debug.Log($"Puertas detectadas: {currentRoom.doors.Count}");
            foreach (var door in currentRoom.doors)
            {
                Debug.Log($"  - {door.doorName} hacia {door.direction} | Bloqueada:{door.isLocked}");
            }
        }
        else
        {
            Debug.LogError("currentRoom es null!");
        }
        
        Debug.Log("==========================================");
    }
}

