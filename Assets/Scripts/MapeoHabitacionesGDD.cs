using UnityEngine;
using System.Collections.Generic;
using VoiceSystem.GameIntegration;
using System.Linq;

/// <summary>
/// Mapea las habitaciones generadas aleatoriamente a los nombres del GDD
/// para que los eventos de historia se disparen correctamente
/// </summary>
public class MapeoHabitacionesGDD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RoomGenerator3000 roomGenerator;
    [SerializeField] private EventManager eventManager;
    
    [Header("Auto-ejecutar")]
    [SerializeField] private bool ejecutarEnStart = true;
    [SerializeField] private float delaySegundos = 1.5f;
    
    // Mapeo de nombres GDD a roomIds generados
    private Dictionary<string, string> nombreAId = new Dictionary<string, string>();
    
    // Nombres de habitaciones del GDD que necesitan eventos
    private List<string> habitacionesGDD = new List<string>
    {
        "hall",
        "sala",
        "biblioteca",
        "comedor",
        "cocina",
        "baño",
        "habitacion_niños",
        "sotano",
        "habitacion_principal"
    };
    
    private void Start()
    {
        if (ejecutarEnStart)
        {
            Invoke(nameof(CrearMapeo), delaySegundos);
        }
    }
    
    [ContextMenu("Crear Mapeo de Habitaciones")]
    public void CrearMapeo()
    {
        Debug.Log("[MapeoHabitaciones] Iniciando mapeo de habitaciones GDD...");
        
        if (roomGenerator == null)
        {
            roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        }
        
        if (eventManager == null)
        {
            eventManager = EventManager.Instance;
        }
        
        if (roomGenerator == null || roomGenerator.casa == null)
        {
            Debug.LogError("[MapeoHabitaciones] Casa no generada aún");
            return;
        }
        
        if (eventManager == null)
        {
            Debug.LogError("[MapeoHabitaciones] EventManager no encontrado");
            return;
        }
        
        // Limpiar mapeo anterior
        nombreAId.Clear();
        
        // Obtener todas las habitaciones generadas
        var habitacionesDisponibles = new List<Room>(roomGenerator.casa.habitaciones);
        
        // ✅ CRÍTICO: Asignar la habitación INICIAL como "hall" (punto de entrada)
        var habitacionInicial = roomGenerator.casa.habitaciones.Find(h => 
            h.posicion == roomGenerator.casa.habitacionInicial
        );
        
        if (habitacionInicial != null)
        {
            nombreAId["hall"] = "room_" + habitacionInicial.id;
            habitacionInicial.nombre = "Hall"; // Actualizar nombre
            habitacionesDisponibles.Remove(habitacionInicial);
            Debug.Log($"[MapeoHabitaciones] ✅ HALL (habitación inicial) → room_{habitacionInicial.id}");
        }
        else
        {
            Debug.LogWarning("[MapeoHabitaciones] ⚠️ No se encontró habitación inicial!");
        }
        
        // Mapear habitaciones específicas del GDD
        MapearHabitacionPorObjetos("sala", habitacionesDisponibles, new[] { "piano", "retrato", "cuadro" });
        MapearHabitacionPorObjetos("biblioteca", habitacionesDisponibles, new[] { "libro", "estante", "libros" });
        MapearHabitacionPorObjetos("comedor", habitacionesDisponibles, new[] { "mesa", "silla", "vajilla" });
        MapearHabitacionPorObjetos("cocina", habitacionesDisponibles, new[] { "estufa", "refrigerador", "cocina" });
        MapearHabitacionPorNombre("baño", habitacionesDisponibles);
        MapearHabitacionPorNombre("sótano", habitacionesDisponibles);
        MapearHabitacionPorNombre("sotano", habitacionesDisponibles); // Sin acento
        MapearHabitacionPorObjetos("habitacion_niños", habitacionesDisponibles, new[] { "cama", "juguete", "niño" });
        MapearHabitacionPorNombre("habitacion principal", habitacionesDisponibles);
        
        // Asignar habitaciones restantes aleatoriamente
        AsignarHabitacionesRestantes(habitacionesDisponibles);
        
        // Ahora REESCRIBIR los roomIds en los eventos
        ReescribirEventosConIds();
        
        Debug.Log($"[MapeoHabitaciones] ✅ Mapeo completado: {nombreAId.Count} habitaciones mapeadas");
        MostrarMapeo();
    }
    
    private void MapearHabitacionPorNombre(string nombreGDD, List<Room> habitaciones)
    {
        if (nombreAId.ContainsKey(nombreGDD))
        {
            return; // Ya mapeada
        }
        
        var habitacion = habitaciones.FirstOrDefault(h => 
            h.nombre.ToLower().Contains(nombreGDD.ToLower())
        );
        
        if (habitacion != null)
        {
            nombreAId[nombreGDD] = "room_" + habitacion.id;
            habitacion.nombre = CapitalizarNombre(nombreGDD); // Actualizar nombre
            habitaciones.Remove(habitacion);
            Debug.Log($"[MapeoHabitaciones] '{nombreGDD}' → room_{habitacion.id} ({habitacion.nombre})");
        }
    }
    
    private void MapearHabitacionPorObjetos(string nombreGDD, List<Room> habitaciones, string[] objetosClave)
    {
        if (nombreAId.ContainsKey(nombreGDD))
        {
            return; // Ya mapeada
        }
        
        var habitacion = habitaciones.FirstOrDefault(h => 
        {
            if (h.objetosEnHabitacion == null || h.objetosEnHabitacion.Count == 0)
                return false;
                
            foreach (var objeto in h.objetosEnHabitacion)
            {
                foreach (var clave in objetosClave)
                {
                    if (objeto.ToLower().Contains(clave.ToLower()))
                        return true;
                }
            }
            return false;
        });
        
        if (habitacion != null)
        {
            nombreAId[nombreGDD] = "room_" + habitacion.id;
            habitacion.nombre = CapitalizarNombre(nombreGDD); // Actualizar nombre
            habitaciones.Remove(habitacion);
            Debug.Log($"[MapeoHabitaciones] '{nombreGDD}' → room_{habitacion.id} ({habitacion.nombre}) [Por objetos]");
        }
    }
    
    private void AsignarHabitacionesRestantes(List<Room> habitaciones)
    {
        // Las habitaciones que no se pudieron mapear por nombre u objetos
        // se asignan a nombres GDD que quedaron sin mapear
        
        var nombresRestantes = new List<string>(habitacionesGDD);
        foreach (var nombre in nombreAId.Keys)
        {
            nombresRestantes.Remove(nombre);
        }
        
        for (int i = 0; i < Mathf.Min(habitaciones.Count, nombresRestantes.Count); i++)
        {
            string nombreGDD = nombresRestantes[i];
            Room habitacion = habitaciones[i];
            
            nombreAId[nombreGDD] = "room_" + habitacion.id;
            
            // Actualizar nombre de la habitación para mejor debugging
            habitacion.nombre = CapitalizarNombre(nombreGDD);
            
            Debug.Log($"[MapeoHabitaciones] '{nombreGDD}' → room_{habitacion.id} (Asignación aleatoria)");
        }
    }
    
    private void ReescribirEventosConIds()
    {
        if (eventManager == null || eventManager.StoryEvents == null)
        {
            Debug.LogWarning("[MapeoHabitaciones] No hay story events para reescribir");
            return;
        }
        
        int eventosActualizados = 0;
        
        // Reescribir roomIds en todos los story events
        foreach (var evt in eventManager.StoryEvents)
        {
            if (!string.IsNullOrEmpty(evt.roomId) && nombreAId.ContainsKey(evt.roomId))
            {
                string oldId = evt.roomId;
                evt.roomId = nombreAId[evt.roomId];
                Debug.Log($"[MapeoHabitaciones] Evento '{evt.eventName}': '{oldId}' → '{evt.roomId}'");
                eventosActualizados++;
            }
        }
        
        // También reescribir screamers
        if (eventManager.Screamers != null)
        {
            foreach (var evt in eventManager.Screamers)
            {
                if (!string.IsNullOrEmpty(evt.roomId) && nombreAId.ContainsKey(evt.roomId))
                {
                    string oldId = evt.roomId;
                    evt.roomId = nombreAId[evt.roomId];
                    Debug.Log($"[MapeoHabitaciones] Screamer '{evt.eventName}': '{oldId}' → '{evt.roomId}'");
                    eventosActualizados++;
                }
            }
        }
        
        Debug.Log($"[MapeoHabitaciones] ✅ {eventosActualizados} eventos actualizados con roomIds correctos");
    }
    
    private void MostrarMapeo()
    {
        Debug.Log("========== MAPEO DE HABITACIONES ==========");
        foreach (var kvp in nombreAId)
        {
            Debug.Log($"  {kvp.Key} → {kvp.Value}");
        }
        Debug.Log("==========================================");
    }
    
    private string CapitalizarNombre(string nombreGDD)
    {
        // Convertir nombres del GDD a nombres legibles
        switch (nombreGDD.ToLower())
        {
            case "hall": return "Hall";
            case "sala": return "Sala";
            case "biblioteca": return "Biblioteca";
            case "comedor": return "Comedor";
            case "cocina": return "Cocina";
            case "baño": return "Baño";
            case "sotano": 
            case "sótano": return "Sótano";
            case "habitacion_niños": return "Habitación de los Niños";
            case "habitacion_principal": return "Habitación Principal";
            default: return "Habitación";
        }
    }
    
    /// <summary>
    /// Obtiene el roomId real de una habitación GDD
    /// </summary>
    public string GetRoomIdFromGDDName(string nombreGDD)
    {
        if (nombreAId.ContainsKey(nombreGDD))
        {
            return nombreAId[nombreGDD];
        }
        return null;
    }
}

