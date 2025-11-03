using UnityEngine;

public enum EventType
{
    Main = 0,
    Random = 1,
    Story = 2,      // Eventos narrativos del GDD
    Screamer = 3    // Eventos que reducen vida
}

[System.Serializable]
public class HBEvents
{
    public int id;
    public string eventName;
    public EventType type = EventType.Main;
    public string audioTxt;          // Texto para narración
    public float duration;
    
    [Header("Room Integration (Opcional)")]
    public string roomId = "";        // ID de habitación específica (vacío = cualquier habitación)
    public bool narrateWithVoice = true; // Narrar con TTS?
    
    [Header("Event Behavior")]
    public bool isUnique = false;     // ¿Solo se dispara una vez?
    public bool hasBeenTriggered = false; // Flag interno (no serializar en JSON si es false)
    
    [Header("Story Event Properties")]
    public string triggerCondition = ""; // firstEntry, inspect, take, read, interact, random, reentry
    public string requiredItem = "";     // Item necesario para activar
    public string requiredFlag = "";     // Flag que debe estar activo para disparar este evento
    public string setsFlag = "";         // Flag de PlayerData que se setea al activar
}
