using UnityEngine;

[CreateAssetMenu(fileName = "RandomSO", menuName = "EventosEspeciales/RandomEvent")]
public class RandomSO : ScriptableObject
{
    public string id;
    public string eventName;
    EventType type = EventType.Random;
    public string audioTxt;
    public float duration;

    //preguntar como se aplican los efectos (arreglo de Enum?)
    
}
