using UnityEngine;

public enum EventType
{
    Main,
    Random
}

[CreateAssetMenu(fileName = "MainSO", menuName = "EventosEspeciales/MainEvent")]
public class MainSO : ScriptableObject
{
    public string id;
    public string eventName;
    EventType type = EventType.Main;
    public string audioTxt;
    public float duration;
    public int indexStep;
    
    //Cómo deberia marcar en que habitación se va a aplicar este evento?
    //string idRoom; (?)
    
}
