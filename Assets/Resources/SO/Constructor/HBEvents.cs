using UnityEngine;

public enum EventType
{
    Main,
    Random
}

[System.Serializable]
public class HBEvents
{
    public int id;
    public string eventName;
    public EventType type = EventType.Main;
    public string audioTxt;
    public float duration;
    
}
