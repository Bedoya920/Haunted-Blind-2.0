using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public EventLists eventos;

    [Header("Archivo JSON en Resources (sin extensión)")]
    public string jsonFileName = "events_data";

    // Estas listas se llenarán directamente desde el JSON
    private HBEvents[] mainEvents;
    private HBEvents[] randomEvents;

    void Awake()
    {
        CargarDesdeJSON();
        FillMainEventList();
        FillRandomEventList(eventos.randomEventAmount);
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
}

[System.Serializable]
public class EventLists
{
    public HBEvents[] mainEvents;
    public HBEvents[] randomEvents;
    public int randomEventAmount;
}