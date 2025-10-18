using System.Collections.Generic;
using UnityEngine;


public class EventManager : MonoBehaviour
{
    [SerializeField]private ScriptableObject[] mainEvents;
    [SerializeField]private ScriptableObject[] randomEvents;

    public int randomEventAmount;

    public ScriptableObject[] MainEvents => mainEvents;
    public ScriptableObject[] RandomEvents => randomEvents;

    void Awake()
    {
        FillMainEventList();
        FillRandomEventList(randomEventAmount);

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            int a = Random.Range(0, randomEventAmount);
            print(RequestRandomEvent(a));

        }
    }

    public void FillMainEventList()
    {
        mainEvents = LoadAllSO("SO/SO_Main");

        if (mainEvents == null || mainEvents.Length == 0)
        {
            print("No hay SO en Assets/Resources/SO/SO_Main");
        }
    }

    public void FillRandomEventList(int randomAmount)
    {
        if (randomAmount <= 0)
        {
            print("El número no puede ser 0 o negativo");
        }

        var allRandom = LoadAllSO("SO/SO_Random");

        if (allRandom == null || allRandom.Length == 0)
        {
            print("la carpeta Assets/SO/SO_Random está vacia");
        }

        var list = new List<ScriptableObject>(allRandom);
        Shuffle(list);

        int take = Mathf.Clamp(randomAmount, 0, list.Count);
        randomEvents = list.GetRange(0, take).ToArray();
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
            Debug.LogWarning("Index fuera de rango mano");
            return null;
        }

        //Necesario borra ese evento?
        return randomEvents[index].name;
    }


    private ScriptableObject[] LoadAllSO(string folder)
    {
        ScriptableObject[] loadedObjects = Resources.LoadAll<ScriptableObject>(folder);
        
        if (loadedObjects == null || loadedObjects.Length == 0)
        {
            print($"No se encontraron ScriptableObjects en Resources/{folder}");
            return new ScriptableObject[0];
        }
        return loadedObjects;
    }

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
}
