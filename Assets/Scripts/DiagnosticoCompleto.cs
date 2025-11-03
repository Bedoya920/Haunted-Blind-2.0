using UnityEngine;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core.Data;
using System.Linq;

/// <summary>
/// Diagnóstico completo del sistema de mapeo, navegación y narrativa
/// </summary>
public class DiagnosticoCompleto : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RoomGenerator3000 roomGenerator;
    [SerializeField] private RoomSystemBridge roomBridge;
    [SerializeField] private MapeoHabitacionesGDD mapeoGDD;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private PlayerData playerData;
    
    [ContextMenu("🔍 DIAGNÓSTICO COMPLETO")]
    public void DiagnosticoCompleto_Ejecutar()
    {
        Debug.Log("╔════════════════════════════════════════════════════════╗");
        Debug.Log("║         DIAGNÓSTICO COMPLETO DEL SISTEMA              ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝");
        
        BuscarReferencias();
        
        DiagnosticarGeneracion();
        DiagnosticarMapeo();
        DiagnosticarNavegacion();
        DiagnosticarNarrativa();
        
        Debug.Log("╔════════════════════════════════════════════════════════╗");
        Debug.Log("║              DIAGNÓSTICO COMPLETADO                    ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝");
    }
    
    private void BuscarReferencias()
    {
        if (roomGenerator == null)
            roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        
        if (roomBridge == null)
            roomBridge = RoomSystemBridge.Instance;
        
        if (mapeoGDD == null)
            mapeoGDD = FindFirstObjectByType<MapeoHabitacionesGDD>();
        
        if (eventManager == null)
            eventManager = EventManager.Instance;
        
        if (playerData == null)
            playerData = Resources.Load<PlayerData>("Data/PlayerData");
    }
    
    private void DiagnosticarGeneracion()
    {
        Debug.Log("\n━━━ 1. GENERACIÓN DEL MAPA ━━━");
        
        if (roomGenerator == null || roomGenerator.casa == null)
        {
            Debug.LogError("❌ RoomGenerator o Casa es null!");
            return;
        }
        
        var casa = roomGenerator.casa;
        Debug.Log($"✅ Casa generada correctamente");
        Debug.Log($"   📍 Habitación inicial: {casa.habitacionInicial}");
        Debug.Log($"   🏠 Total habitaciones: {casa.habitaciones.Count}");
        Debug.Log($"   🚪 Total puertas: {casa.puertas.Count}");
        
        // Listar todas las habitaciones con sus nombres
        Debug.Log("\n   📋 LISTA DE HABITACIONES:");
        foreach (var room in casa.habitaciones)
        {
            bool esInicial = room.posicion == casa.habitacionInicial;
            string marca = esInicial ? " ⭐ INICIAL" : "";
            Debug.Log($"      - ID:{room.id} | Pos:{room.posicion} | Nombre:'{room.nombre}'{marca}");
        }
        
        // Listar todas las puertas
        Debug.Log("\n   📋 LISTA DE PUERTAS:");
        foreach (var puerta in casa.puertas)
        {
            Debug.Log($"      - ID:{puerta.id} | {puerta.cuarto1} ↔ {puerta.cuarto2}");
        }
    }
    
    private void DiagnosticarMapeo()
    {
        Debug.Log("\n━━━ 2. MAPEO GDD → IDs ━━━");
        
        if (mapeoGDD == null)
        {
            Debug.LogError("❌ MapeoHabitacionesGDD no encontrado!");
            return;
        }
        
        // Verificar mapeo de habitaciones clave
        string[] habitacionesClave = new string[] 
        {
            "hall", "sala", "biblioteca", "comedor", "cocina", 
            "baño", "sotano", "habitacion_niños", "habitacion_principal"
        };
        
        Debug.Log("   🔑 MAPEO DE HABITACIONES CLAVE:");
        foreach (var nombreGDD in habitacionesClave)
        {
            string roomId = mapeoGDD.GetRoomIdFromGDDName(nombreGDD);
            if (!string.IsNullOrEmpty(roomId))
            {
                // Buscar la habitación real
                int realId = int.Parse(roomId.Replace("room_", ""));
                var room = roomGenerator.casa.habitaciones.Find(r => r.id == realId);
                if (room != null)
                {
                    bool esInicial = room.posicion == roomGenerator.casa.habitacionInicial;
                    string marca = esInicial ? " ⭐" : "";
                    Debug.Log($"      ✅ '{nombreGDD}' → {roomId} ({room.nombre}) Pos:{room.posicion}{marca}");
                }
                else
                {
                    Debug.LogWarning($"      ⚠️ '{nombreGDD}' → {roomId} (habitación NO ENCONTRADA)");
                }
            }
            else
            {
                Debug.LogWarning($"      ❌ '{nombreGDD}' → NO MAPEADO");
            }
        }
    }
    
    private void DiagnosticarNavegacion()
    {
        Debug.Log("\n━━━ 3. SISTEMA DE NAVEGACIÓN ━━━");
        
        if (roomBridge == null)
        {
            Debug.LogError("❌ RoomSystemBridge no encontrado!");
            return;
        }
        
        Debug.Log($"   📍 Posición actual del jugador: {roomBridge.currentPlayerPosition}");
        
        var currentRoom = roomBridge.GetCurrentRoom();
        if (currentRoom != null)
        {
            Debug.Log($"   🏠 Habitación actual: {currentRoom.roomName} (ID: {currentRoom.roomId})");
            Debug.Log($"   🚪 Puertas en habitación actual: {currentRoom.doors.Count}");
            
            if (currentRoom.doors.Count > 0)
            {
                Debug.Log("\n   📋 DETALLE DE PUERTAS:");
                foreach (var door in currentRoom.doors)
                {
                    Debug.Log($"      - {door.doorName} | Dirección: {door.direction} | ID: {door.doorId}");
                }
            }
            else
            {
                Debug.LogWarning("      ⚠️ NO HAY PUERTAS (esto no debería pasar!)");
            }
        }
        else
        {
            Debug.LogError("   ❌ GetCurrentRoom() retornó null!");
        }
        
        // Probar comandos de dirección
        Debug.Log("\n   🧭 PRUEBA DE DIRECCIONES:");
        var directionalMovement = FindFirstObjectByType<DirectionalMovement>();
        if (directionalMovement != null && currentRoom != null)
        {
            string[] direcciones = { "adelante", "arriba", "atrás", "abajo", "derecha", "izquierda" };
            foreach (var dir in direcciones)
            {
                var door = directionalMovement.TranslateDirectionToDoor(dir);
                string resultado = door != null ? $"✅ {door.doorName}" : "❌ Sin puerta";
                Debug.Log($"      '{dir}' → {resultado}");
            }
        }
    }
    
    private void DiagnosticarNarrativa()
    {
        Debug.Log("\n━━━ 4. SISTEMA NARRATIVO ━━━");
        
        if (eventManager == null)
        {
            Debug.LogError("❌ EventManager no encontrado!");
            return;
        }
        
        Debug.Log($"   📖 Eventos de historia cargados: {eventManager.StoryEvents?.Length ?? 0}");
        
        // Verificar eventos de la habitación actual
        var currentRoom = roomBridge?.GetCurrentRoom();
        if (currentRoom != null)
        {
            Debug.Log($"\n   🎭 EVENTOS PARA HABITACIÓN ACTUAL ({currentRoom.roomName} - {currentRoom.roomId}):");
            
            // firstEntry
            var firstEntryEvent = eventManager.GetStoryEventByTrigger(currentRoom.roomId, "firstEntry");
            if (firstEntryEvent != null)
            {
                Debug.Log($"      ✅ firstEntry: '{firstEntryEvent.eventName}' ({firstEntryEvent.duration}s)");
                Debug.Log($"         Texto: {firstEntryEvent.audioTxt.Substring(0, System.Math.Min(80, firstEntryEvent.audioTxt.Length))}...");
            }
            else
            {
                Debug.LogWarning($"      ⚠️ firstEntry: NO HAY EVENTO");
            }
            
            // reentry
            var reentryEvent = eventManager.GetStoryEventByTrigger(currentRoom.roomId, "reentry");
            if (reentryEvent != null)
            {
                string flagInfo = !string.IsNullOrEmpty(reentryEvent.requiredFlag) 
                    ? $" (requiere: {reentryEvent.requiredFlag})" 
                    : "";
                Debug.Log($"      ✅ reentry: '{reentryEvent.eventName}' ({reentryEvent.duration}s){flagInfo}");
            }
            else
            {
                Debug.LogWarning($"      ⚠️ reentry: NO HAY EVENTO");
            }
        }
        
        // Verificar progresión secuencial
        Debug.Log("\n   🔗 PROGRESIÓN SECUENCIAL:");
        if (playerData != null)
        {
            bool hasChildDialogue = playerData.HasSeenEvent("child_dialogue_complete");
            Debug.Log($"      child_dialogue_complete: {(hasChildDialogue ? "✅ Completado" : "❌ Pendiente")}");
            Debug.Log($"      Habitaciones visitadas: {playerData.VisitedRooms.Count}");
        }
        
        // Verificar eventos con requiredFlag
        Debug.Log("\n   🔐 EVENTOS CON DEPENDENCIAS:");
        int eventosConFlag = 0;
        foreach (var evt in eventManager.StoryEvents)
        {
            if (!string.IsNullOrEmpty(evt.requiredFlag))
            {
                eventosConFlag++;
                bool cumplido = playerData != null && playerData.HasSeenEvent(evt.requiredFlag);
                string status = cumplido ? "✅ Disponible" : "🔒 Bloqueado";
                Debug.Log($"      {status} '{evt.eventName}' (requiere: {evt.requiredFlag})");
            }
        }
        Debug.Log($"   Total eventos con dependencias: {eventosConFlag}");
    }
    
    [ContextMenu("🗺️ Ver Mapa Completo")]
    public void MostrarMapaCompleto()
    {
        if (roomGenerator == null || roomGenerator.casa == null)
        {
            Debug.LogError("Casa no generada!");
            return;
        }
        
        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║                   MAPA COMPLETO                        ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝");
        
        // Calcular bounds
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        
        foreach (var room in roomGenerator.casa.habitaciones)
        {
            if (room.posicion.x < minX) minX = room.posicion.x;
            if (room.posicion.y < minY) minY = room.posicion.y;
            if (room.posicion.x > maxX) maxX = room.posicion.x;
            if (room.posicion.y > maxY) maxY = room.posicion.y;
        }
        
        // Dibujar mapa ASCII
        for (int y = maxY; y >= minY; y--)
        {
            string linea = "";
            for (int x = minX; x <= maxX; x++)
            {
                var room = roomGenerator.casa.habitaciones.Find(r => r.posicion == new Vector2Int(x, y));
                if (room != null)
                {
                    bool esInicial = room.posicion == roomGenerator.casa.habitacionInicial;
                    bool esJugador = roomBridge != null && roomBridge.currentPlayerPosition == room.posicion;
                    
                    if (esJugador)
                        linea += "[P]";
                    else if (esInicial)
                        linea += "[I]";
                    else
                        linea += "[·]";
                }
                else
                {
                    linea += "   ";
                }
            }
            Debug.Log(linea);
        }
        
        Debug.Log("\nLeyenda: [P]=Jugador, [I]=Inicial, [·]=Habitación");
    }
}

