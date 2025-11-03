using UnityEngine;
using VoiceSystem.GameIntegration;
using System.Linq;

/// <summary>
/// Diagnóstico específico para el problema de puertas
/// </summary>
public class DiagnosticoPuertas : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RoomGenerator3000 roomGenerator;
    [SerializeField] private RoomSystemBridge roomBridge;
    
    [ContextMenu("🔍 DIAGNOSTICAR PUERTAS ACTUAL")]
    public void DiagnosticarPuertasActual()
    {
        if (roomGenerator == null) roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        if (roomBridge == null) roomBridge = RoomSystemBridge.Instance;
        
        if (roomGenerator == null || roomGenerator.casa == null)
        {
            Debug.LogError("[Diagnóstico] Casa no cargada");
            return;
        }
        
        Debug.Log("╔══════════════════════════════════════════════════════╗");
        Debug.Log("║     DIAGNÓSTICO DE PUERTAS - HABITACIÓN ACTUAL      ║");
        Debug.Log("╚══════════════════════════════════════════════════════╝");
        
        // Obtener posición actual del jugador
        Vector2Int posActual = roomBridge.currentPlayerPosition;
        Debug.Log($"\n🎮 POSICIÓN ACTUAL DEL JUGADOR: ({posActual.x}, {posActual.y})");
        
        // Obtener habitación actual
        var habitacionActual = roomGenerator.casa.habitaciones.Find(h => h.posicion == posActual);
        if (habitacionActual == null)
        {
            Debug.LogError($"❌ No se encontró habitación en posición ({posActual.x}, {posActual.y})");
            return;
        }
        
        Debug.Log($"📍 Habitación: {habitacionActual.nombre} (ID: {habitacionActual.id})");
        Debug.Log($"   Puertas esperadas: {habitacionActual.puertas}");
        
        // Buscar TODAS las puertas que conectan con esta posición
        Debug.Log($"\n🚪 PUERTAS EN EL MAPA (Total: {roomGenerator.casa.puertas.Count}):");
        
        int puertasEncontradas = 0;
        foreach (var puerta in roomGenerator.casa.puertas)
        {
            bool conecta1 = puerta.cuarto1 == posActual;
            bool conecta2 = puerta.cuarto2 == posActual;
            
            if (conecta1 || conecta2)
            {
                puertasEncontradas++;
                Vector2Int otraHabitacion = conecta1 ? puerta.cuarto2 : puerta.cuarto1;
                
                // Calcular dirección
                int deltaX = otraHabitacion.x - posActual.x;
                int deltaY = otraHabitacion.y - posActual.y;
                
                string direccion = "???";
                if (deltaX > 0) direccion = "derecha";
                else if (deltaX < 0) direccion = "izquierda";
                else if (deltaY > 0) direccion = "abajo";
                else if (deltaY < 0) direccion = "arriba";
                
                // Buscar nombre de la otra habitación
                var otraRoom = roomGenerator.casa.habitaciones.Find(h => h.posicion == otraHabitacion);
                string nombreOtra = otraRoom != null ? otraRoom.nombre : "???";
                
                Debug.Log($"   ✅ Puerta ID:{puerta.id} → {nombreOtra} ({otraHabitacion.x}, {otraHabitacion.y}) | Dirección: {direccion} | Abierta: {puerta.abierta}");
            }
        }
        
        Debug.Log($"\n📊 RESUMEN:");
        Debug.Log($"   Puertas esperadas: {habitacionActual.puertas}");
        Debug.Log($"   Puertas encontradas: {puertasEncontradas}");
        
        if (puertasEncontradas != habitacionActual.puertas)
        {
            Debug.LogWarning($"⚠️ DISCREPANCIA: Esperadas {habitacionActual.puertas}, Encontradas {puertasEncontradas}");
        }
        
        // Ahora verificar qué ve el RoomSystemBridge
        Debug.Log($"\n🔍 VERIFICACIÓN DEL ROOMSYSTEMBRIDGE:");
        var currentRoom = roomBridge.GetCurrentRoom();
        if (currentRoom != null)
        {
            Debug.Log($"   RoomData.roomName: {currentRoom.roomName}");
            Debug.Log($"   RoomData.doors.Count: {currentRoom.doors.Count}");
            
            foreach (var door in currentRoom.doors)
            {
                Debug.Log($"      - {door.doorName} | Dirección: {door.direction} | Bloqueada: {door.isLocked}");
            }
        }
        else
        {
            Debug.LogError("❌ RoomSystemBridge.GetCurrentRoom() retornó null!");
        }
        
        Debug.Log("═══════════════════════════════════════════════════════");
    }
    
    [ContextMenu("🗺️ MAPA COMPLETO DE PUERTAS")]
    public void MostrarMapaCompletoPuertas()
    {
        if (roomGenerator == null) roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        
        if (roomGenerator == null || roomGenerator.casa == null)
        {
            Debug.LogError("[Diagnóstico] Casa no cargada");
            return;
        }
        
        Debug.Log("╔══════════════════════════════════════════════════════╗");
        Debug.Log("║           MAPA COMPLETO DE TODAS LAS PUERTAS        ║");
        Debug.Log("╚══════════════════════════════════════════════════════╝");
        
        foreach (var habitacion in roomGenerator.casa.habitaciones)
        {
            Debug.Log($"\n📍 {habitacion.nombre} (ID:{habitacion.id}) - Posición: ({habitacion.posicion.x}, {habitacion.posicion.y})");
            Debug.Log($"   Puertas declaradas: {habitacion.puertas}");
            
            // Buscar puertas que conectan con esta habitación
            var puertasHabitacion = roomGenerator.casa.puertas.Where(p => 
                p.cuarto1 == habitacion.posicion || p.cuarto2 == habitacion.posicion
            ).ToList();
            
            Debug.Log($"   Puertas reales: {puertasHabitacion.Count}");
            
            foreach (var puerta in puertasHabitacion)
            {
                Vector2Int otraPos = puerta.cuarto1 == habitacion.posicion ? puerta.cuarto2 : puerta.cuarto1;
                var otraHab = roomGenerator.casa.habitaciones.Find(h => h.posicion == otraPos);
                string nombreOtra = otraHab != null ? otraHab.nombre : "???";
                
                int deltaX = otraPos.x - habitacion.posicion.x;
                int deltaY = otraPos.y - habitacion.posicion.y;
                
                string dir = "???";
                if (deltaX > 0) dir = "derecha";
                else if (deltaX < 0) dir = "izquierda";
                else if (deltaY > 0) dir = "abajo";
                else if (deltaY < 0) dir = "arriba";
                
                Debug.Log($"      → {nombreOtra} | Dirección: {dir} | Abierta: {puerta.abierta}");
            }
        }
        
        Debug.Log("═══════════════════════════════════════════════════════");
    }
}

