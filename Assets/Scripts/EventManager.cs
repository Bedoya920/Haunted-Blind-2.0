using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Singleton
    private static EventManager _instance;
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("EventManager");
                _instance = go.AddComponent<EventManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    public EventLists eventos;

    [Header("Archivo JSON en Resources (sin extensión)")]
    public string jsonFileName = "events_data";

    // Estas listas se llenarán directamente desde el JSON
    private HBEvents[] mainEvents;
    private HBEvents[] randomEvents;
    private HBEvents[] storyEvents;   // Eventos narrativos del GDD
    private HBEvents[] screamers;     // Eventos que reducen vida
    
    // Propiedades públicas
    public HBEvents[] MainEvents => mainEvents;
    public HBEvents[] RandomEvents => randomEvents;
    public HBEvents[] StoryEvents => storyEvents;
    public HBEvents[] Screamers => screamers;

    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEvents();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void InitializeEvents()
    {
        CargarDesdeJSON();
        FillMainEventList();
        FillRandomEventList(eventos?.randomEventAmount ?? 5);
        FillStoryEventList();
        FillScreamerList();
        
        Debug.Log($"[EventManager] Singleton inicializado - {mainEvents?.Length ?? 0} main, {randomEvents?.Length ?? 0} random, {storyEvents?.Length ?? 0} story, {screamers?.Length ?? 0} screamers");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            int a = Random.Range(0, randomEvents.Length);
            print(RequestRandomEvent(a));
        }
    }

    private void CargarDesdeJSON()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
        if (jsonFile == null)
        {
            Debug.LogError($"No se encontró el archivo JSON en Resources/{jsonFileName}.json");
            return;
        }

        eventos = JsonUtility.FromJson<EventLists>(jsonFile.text);

        if (eventos == null)
            Debug.LogError("Error al deserializar el archivo JSON.");
        else
            Debug.Log("Eventos cargados correctamente desde JSON.");
    }

    private void FillMainEventList()
    {
        if (eventos == null || eventos.mainEvents == null || eventos.mainEvents.Length == 0)
        {
            Debug.LogWarning("No hay eventos principales en el JSON.");
            mainEvents = new HBEvents[0];
            return;
        }

        mainEvents = new HBEvents[eventos.mainEvents.Length];

        for (int i = 0; i < eventos.mainEvents.Length; i++)
        {
            mainEvents[i] = eventos.mainEvents[i];
        }

        Debug.Log($"Se llenaron {mainEvents.Length} eventos principales desde el JSON.");
    }

    private void FillRandomEventList(int randomAmount)
    {
        if (eventos == null || eventos.randomEvents == null || eventos.randomEvents.Length == 0)
        {
            Debug.LogWarning("No hay eventos aleatorios en el JSON.");
            randomEvents = new HBEvents[0];
            return;
        }

        List<HBEvents> list = new List<HBEvents>(eventos.randomEvents);

        Shuffle(list); 

        int take = Mathf.Clamp(randomAmount, 0, list.Count);
        randomEvents = list.GetRange(0, take).ToArray();

        Debug.Log($"Se llenaron {randomEvents.Length} eventos aleatorios desde el JSON.");
    }
    
    private void FillStoryEventList()
    {
        if (eventos == null || eventos.storyEvents == null || eventos.storyEvents.Length == 0)
        {
            Debug.LogWarning("No hay eventos de historia en el JSON.");
            storyEvents = new HBEvents[0];
            return;
        }

        storyEvents = new HBEvents[eventos.storyEvents.Length];

        for (int i = 0; i < eventos.storyEvents.Length; i++)
        {
            storyEvents[i] = eventos.storyEvents[i];
        }

        Debug.Log($"Se llenaron {storyEvents.Length} eventos de historia desde el JSON.");
    }
    
    private void FillScreamerList()
    {
        if (eventos == null || eventos.screamers == null || eventos.screamers.Length == 0)
        {
            Debug.LogWarning("No hay screamers en el JSON.");
            screamers = new HBEvents[0];
            return;
        }

        screamers = new HBEvents[eventos.screamers.Length];

        for (int i = 0; i < eventos.screamers.Length; i++)
        {
            screamers[i] = eventos.screamers[i];
        }

        Debug.Log($"Se llenaron {screamers.Length} screamers desde el JSON.");
    }

    public string RequestRandomEvent(int index)
    {
        if (randomEvents == null || randomEvents.Length == 0)
        {
            Debug.LogWarning("Lista randomEvents vacía");
            return null;
        }

        if (index < 0 || index >= randomEvents.Length)
        {
            Debug.LogWarning("Index fuera de rango");
            return null;
        }

        return randomEvents[index].eventName;
    }

    // Barajar lista
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Obtiene evento aleatorio filtrado por tipo y habitación opcional
    /// </summary>
    public HBEvents GetRandomEvent(EventType eventType = EventType.Random, string roomId = "")
    {
        HBEvents[] pool = eventType == EventType.Main ? mainEvents : randomEvents;
        
        if (pool == null || pool.Length == 0)
        {
            return null;
        }
        
        // Filtrar por roomId y eventos no disparados (si son únicos)
        if (!string.IsNullOrEmpty(roomId))
        {
            var filtered = System.Array.FindAll(pool, e => 
                (string.IsNullOrEmpty(e.roomId) || e.roomId == roomId) &&
                (!e.isUnique || !e.hasBeenTriggered) // Excluir eventos únicos ya disparados
            );
            
            if (filtered.Length > 0)
            {
                return filtered[Random.Range(0, filtered.Length)];
            }
        }
        
        // Sin filtro de habitación, pero aún filtrar eventos únicos
        var availableEvents = System.Array.FindAll(pool, e => 
            !e.isUnique || !e.hasBeenTriggered
        );
        
        if (availableEvents.Length > 0)
        {
            return availableEvents[Random.Range(0, availableEvents.Length)];
        }
        
        return null; // No hay eventos disponibles
    }
    
    /// <summary>
    /// Obtiene evento por ID específico
    /// </summary>
    public HBEvents GetEventById(int eventId)
    {
        // Buscar en main
        if (mainEvents != null)
        {
            var mainEvent = System.Array.Find(mainEvents, e => e.id == eventId);
            if (mainEvent != null) return mainEvent;
        }
        
        // Buscar en random
        if (randomEvents != null)
        {
            var randomEvent = System.Array.Find(randomEvents, e => e.id == eventId);
            if (randomEvent != null) return randomEvent;
        }
        
        // Buscar en story
        if (storyEvents != null)
        {
            var storyEvent = System.Array.Find(storyEvents, e => e.id == eventId);
            if (storyEvent != null) return storyEvent;
        }
        
        // Buscar en screamers
        if (screamers != null)
        {
            return System.Array.Find(screamers, e => e.id == eventId);
        }
        
        return null;
    }
    
    /// <summary>
    /// Busca un evento de historia por habitación y condición de disparo
    /// </summary>
    public HBEvents GetStoryEventByTrigger(string roomId, string triggerType)
    {
        if (storyEvents == null || storyEvents.Length == 0)
        {
            return null;
        }
        
        // Buscar evento que coincida con roomId y triggerCondition
        return System.Array.Find(storyEvents, e => 
            e.roomId == roomId && 
            e.triggerCondition == triggerType &&
            (!e.isUnique || !e.hasBeenTriggered)
        );
    }
    
    /// <summary>
    /// Dispara un screamer y reduce la vida del jugador
    /// </summary>
    public void TriggerScreamer(HBEvents screamerEvent)
    {
        if (screamerEvent == null || screamerEvent.type != EventType.Screamer)
        {
            return;
        }
        
        // Narrar el screamer
        NarrateEvent(screamerEvent);
        
        // Reducir vida del jugador vía FatigueSystem
        var fatigueSystem = FatigueSystem.Instance;
        if (fatigueSystem != null && fatigueSystem.PlayerLives != null)
        {
            bool stillAlive = fatigueSystem.PlayerLives.LoseLife();
            Debug.Log($"[EventManager] Screamer! Vida reducida a {fatigueSystem.PlayerLives.currentLives}. Vivo: {stillAlive}");
        }
    }
    
    /// <summary>
    /// Busca un evento por su nombre en todas las categorías
    /// </summary>
    public HBEvents GetEventByName(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return null;
        
        // Buscar en main events
        if (mainEvents != null)
        {
            foreach (var evt in mainEvents)
            {
                if (evt.eventName == eventName) return evt;
            }
        }
        
        // Buscar en story events
        if (storyEvents != null)
        {
            foreach (var evt in storyEvents)
            {
                if (evt.eventName == eventName) return evt;
            }
        }
        
        // Buscar en random events
        if (randomEvents != null)
        {
            foreach (var evt in randomEvents)
            {
                if (evt.eventName == eventName) return evt;
            }
        }
        
        // Buscar en screamers
        if (screamers != null)
        {
            foreach (var evt in screamers)
            {
                if (evt.eventName == eventName) return evt;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Narra un evento usando el sistema de voz
    /// </summary>
    public void NarrateEvent(HBEvents hbEvent)
    {
        if (hbEvent == null || !hbEvent.narrateWithVoice)
        {
            return;
        }
        
        // Marcar como disparado si es único
        if (hbEvent.isUnique)
        {
            hbEvent.hasBeenTriggered = true;
            Debug.Log($"[EventManager] Evento único '{hbEvent.eventName}' marcado como disparado");
        }
        
        var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
        if (voiceSystem != null && voiceSystem.textToSpeech != null)
        {
            voiceSystem.textToSpeech.Speak(
                hbEvent.audioTxt, 
                VoiceSystem.Core.Interfaces.TTSPriority.Urgent // Eventos son urgentes
            );
            
            Debug.Log($"[EventManager] Narrando evento: {hbEvent.eventName}");
        }
        else
        {
            Debug.LogWarning($"[EventManager] No se pudo narrar evento: VoiceSystem no disponible");
        }
    }
    
    /// <summary>
    /// Resetear flags de eventos (útil para debug o nuevo juego)
    /// </summary>
    public void ResetEventFlags()
    {
        if (mainEvents != null)
        {
            foreach (var evt in mainEvents)
            {
                evt.hasBeenTriggered = false;
            }
        }
        
        if (randomEvents != null)
        {
            foreach (var evt in randomEvents)
            {
                evt.hasBeenTriggered = false;
            }
        }
        
        if (storyEvents != null)
        {
            foreach (var evt in storyEvents)
            {
                evt.hasBeenTriggered = false;
            }
        }
        
        if (screamers != null)
        {
            foreach (var evt in screamers)
            {
                evt.hasBeenTriggered = false;
            }
        }
        
        Debug.Log("[EventManager] Flags de eventos reseteados");
    }
    
    #endregion
}

[System.Serializable]
public class EventLists
{
    public HBEvents[] mainEvents;
    public HBEvents[] randomEvents;
    public HBEvents[] storyEvents;  // Eventos narrativos del GDD
    public HBEvents[] screamers;    // Eventos tipo screamer
    public int randomEventAmount;
}