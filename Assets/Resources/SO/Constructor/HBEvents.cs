using UnityEngine;

public enum EventType
{
    Main,
    Random
}

[CreateAssetMenu(fileName = "HBEvents", menuName = "EventosEspeciales/MainEvent")]
public class HBEvents : ScriptableObject
{
    public int id;
    public string eventName;
    public EventType type = EventType.Main;
    public string audioTxt;
    public float duration;

    //public int indexStep; No necesario
    
    //Cómo deberia marcar en que habitación se va a aplicar este evento?
    //string idRoom; (?)
    
}
