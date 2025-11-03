using UnityEngine;
using System.Collections;
using VoiceSystem.Core;
using VoiceSystem.GameIntegration;

/// <summary>
/// Inicializa el juego automáticamente al cargar la escena.
/// Genera la casa, configura todos los sistemas, y narra el evento de inicio.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RoomGenerator3000 roomGenerator;
    
    [Header("Configuración")]
    [SerializeField] private bool loadFromJson = true; // NUEVO: Cargar mapa guardado en lugar de generar
    [SerializeField] private int numeroHabitaciones = 9; // Solo si loadFromJson = false
    [SerializeField] private float delayBeforeStart = 2f; // Espera antes de narrar inicio
    [SerializeField] private int maxGenerationRetries = 3; // Reintentos si falla
    
    [Header("Eventos de Inicio")]
    [SerializeField] private bool narrateWelcome = true;
    [SerializeField] private int welcomeEventId = 1; // "Inicio del Juego"
    
    void Start()
    {
        StartCoroutine(InitializeGameSequence());
    }
    
    private IEnumerator InitializeGameSequence()
    {
        Debug.Log("=== INICIANDO JUEGO ===");
        
        // 1. Generar la casa
        yield return StartCoroutine(GenerateHouse());
        
        // 2. Esperar a que todos los singletons se inicialicen
        yield return new WaitForSeconds(0.5f);
        
        // 3. Configurar integración del generador
        yield return StartCoroutine(SetupIntegration());
        
        // 3.5. Mapear habitaciones a nombres del GDD
        yield return StartCoroutine(MapearHabitaciones());
        
        // 4. Espera inicial antes de narrar
        yield return new WaitForSeconds(delayBeforeStart);
        
        // 5. Narrar evento de inicio
        if (narrateWelcome)
        {
            NarrateWelcomeEvent();
        }
        
        // 6. Iniciar timer del juego
        var gameTimer = GameTimer.Instance;
        if (gameTimer != null)
        {
            Debug.Log("[GameInit] GameTimer iniciado");
        }
        
        // 7. Activar reconocimiento de voz
        yield return new WaitForSeconds(1f);
        var voiceSystem = VoiceSystemManager.Instance;
        if (voiceSystem != null && !voiceSystem.IsListening)
        {
            voiceSystem.StartListening();
            Debug.Log("[GameInit] Sistema de voz activado - Puedes hablar ahora");
        }
        
        Debug.Log("=== JUEGO INICIADO - LISTO PARA JUGAR ===");
    }
    
    private IEnumerator GenerateHouse()
    {
        if (roomGenerator == null)
        {
            // Intentar encontrar el generador
            roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
            
            if (roomGenerator == null)
            {
                Debug.LogError("[GameInit] RoomGenerator3000 no encontrado en la escena!");
                yield break;
            }
        }
        
        // NUEVO: Cargar desde JSON o generar
        if (loadFromJson)
        {
            Debug.Log("[GameInit] Cargando casa desde JSON...");
            roomGenerator.CargarCasaJson();
            
            // Verificar que se cargó correctamente
            if (roomGenerator.casa != null && 
                roomGenerator.casa.habitaciones != null && 
                roomGenerator.casa.habitaciones.Count > 0)
            {
                Debug.Log($"[GameInit] ✅ Casa cargada desde JSON: {roomGenerator.casa.habitaciones.Count} habitaciones, {roomGenerator.casa.puertas.Count} puertas");
            }
            else
            {
                Debug.LogError("[GameInit] ❌ Error al cargar casa desde JSON. Generando nueva casa...");
                loadFromJson = false; // Fallback a generación
            }
        }
        
        // Generar nueva casa si no se cargó desde JSON
        if (!loadFromJson)
        {
            // Configurar número de habitaciones si está en 0
            if (roomGenerator.numeroHabitaciones == 0)
            {
                roomGenerator.numeroHabitaciones = numeroHabitaciones;
            }
            
            // Intentar generar con reintentos
            bool success = false;
            int retries = 0;
            
            while (!success && retries < maxGenerationRetries)
            {
                if (retries > 0)
                {
                    Debug.LogWarning($"[GameInit] Reintento {retries}/{maxGenerationRetries} de generación...");
                }
                
                Debug.Log($"[GameInit] Generando casa con {roomGenerator.numeroHabitaciones} habitaciones...");
                
                // Ejecutar generador
                roomGenerator.Iniciar();
                
                yield return new WaitForSeconds(0.5f);
                
                // Verificar éxito
                if (roomGenerator.casa != null && 
                    roomGenerator.casa.habitaciones != null && 
                    roomGenerator.casa.habitaciones.Count > 0)
                {
                    success = true;
                    Debug.Log($"[GameInit] ✅ Casa generada: {roomGenerator.casa.habitaciones.Count} habitaciones, {roomGenerator.casa.puertas.Count} puertas");
                }
                else
                {
                    retries++;
                    if (retries < maxGenerationRetries)
                    {
                        Debug.LogWarning($"[GameInit] ⚠️ Generación falló, reintentando...");
                    }
                    else
                    {
                        Debug.LogError("[GameInit] ❌ Error generando casa después de todos los reintentos");
                    }
                }
            }
        }
        
        yield return null;
    }
    
    private IEnumerator SetupIntegration()
    {
        Debug.Log("[GameInit] Configurando integración con RoomSystemBridge...");
        
        RoomGenerator3000 generator = roomGenerator != null ? roomGenerator : FindFirstObjectByType<RoomGenerator3000>();
        if (generator == null)
        {
            Debug.LogError("[GameInit] RoomGenerator3000 no encontrado!");
            yield break;
        }
        
        // Verificar que la casa fue generada
        if (generator.casa == null || generator.casa.habitaciones == null || generator.casa.habitaciones.Count == 0)
        {
            Debug.LogError("[GameInit] Casa no generada correctamente!");
            yield break;
        }
        
        yield return null; // Esperar un frame
        
        // Asignar al bridge
        var bridge = RoomSystemBridge.Instance;
        if (bridge != null)
        {
            bridge.SetRoomGenerator(generator);
            Debug.Log($"[GameInit] ✅ Bridge configurado - Posición inicial: {bridge.currentPlayerPosition}");
        }
        else
        {
            Debug.LogError("[GameInit] RoomSystemBridge.Instance es null!");
        }
        
        // Asegurar que GameContextProvider esté usando el generador real
        var contextProvider = FindFirstObjectByType<GameContextProvider>();
        if (contextProvider != null)
        {
            // Forzar actualización de contexto desde el generador
            var currentRoom = bridge?.GetCurrentRoom();
            if (currentRoom != null)
            {
                Debug.Log($"[GameInit] ✅ Habitación inicial: {currentRoom.roomName}");
            }
        }
        
        yield return null;
    }
    
    private IEnumerator MapearHabitaciones()
    {
        Debug.Log("[GameInit] Mapeando habitaciones a nombres del GDD...");
        
        var mapeo = FindFirstObjectByType<MapeoHabitacionesGDD>();
        if (mapeo != null)
        {
            mapeo.CrearMapeo();
            Debug.Log("[GameInit] ✅ Habitaciones mapeadas");
        }
        else
        {
            Debug.LogWarning("[GameInit] MapeoHabitacionesGDD no encontrado - Los eventos podrían no dispararse correctamente");
        }
        
        yield return new WaitForSeconds(0.2f);
    }
    
    private void NarrateWelcomeEvent()
    {
        var eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            var welcomeEvent = eventManager.GetEventById(welcomeEventId);
            if (welcomeEvent != null)
            {
                eventManager.NarrateEvent(welcomeEvent);
                Debug.Log($"[GameInit] Narrando evento de bienvenida: {welcomeEvent.eventName}");
            }
            else
            {
                Debug.LogWarning($"[GameInit] Evento de bienvenida ID {welcomeEventId} no encontrado");
            }
        }
    }
    
    /// <summary>
    /// Reiniciar el juego (útil para testing)
    /// </summary>
    [ContextMenu("Reiniciar Juego")]
    public void RestartGame()
    {
        // Reset de flags de eventos
        var eventManager = EventManager.Instance;
        if (eventManager != null)
        {
            eventManager.ResetEventFlags();
        }
        
        // Recargar escena
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

