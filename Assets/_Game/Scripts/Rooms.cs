using System.Collections.Generic;
using UnityEngine;

public class Rooms : MonoBehaviour
{
    public Room room;
    public GameObject puertaFrente;
    public GameObject puertaAtras;
    public GameObject puertaIzquierda;
    public GameObject puertaDerecha;


    void Start()
    {
        if (room != null)
        {
            //if(room.puertas != null)
            //{
            //    puertaFrente.SetActive(true);
            //}
            //if (room.puertaAtras != null)
            //{
            //    puertaAtras.SetActive(true);
            //}
            //if (room.puertaIzquierda != null)
            //{
            //    puertaIzquierda.SetActive(true);
            //}
            //if (room.puertaDerecha != null)
            //{
            //    puertaDerecha.SetActive(true);
            //}
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class Room
{
    public int      id;
    public string   nombre;
    public string   descripLarga;
    public string   descripCorta;
    public int      puertas;
    public Vector2Int posicion;
    
    // NUEVOS CAMPOS (opcionales, pueden estar vacíos)
    [Header("Voice System Integration (Opcional)")]
    public List<string> objetosEnHabitacion = new List<string>(); // Para sistema de voz

    public Room()
    {
        // No generar ID aquí - se genera en el RoomGenerator al crear habitaciones
        id = 0;
    }
    public Room(Vector2Int pos)
    {
        // No generar ID aquí - se genera en el RoomGenerator al crear habitaciones
        id = 0;
        posicion = pos;
    }

}

[System.Serializable]
public class Door
{
    public int id;
    public bool abierta;
    public int variableNecesaria;
    public string mensajeBloqueada;
    public string mensajeAbrir;
    public Vector2Int cuarto1, cuarto2;
    
    // NUEVO CAMPO (opcional)
    [Header("Voice System Integration (Opcional)")]
    public string descripcionPuerta = ""; // Descripción detallada para inspeccionar
    
    public Door()
    {
        // No generar ID aquí - se genera en el RoomGenerator al crear puertas
        id = 0;
        abierta = true;
        variableNecesaria = -1;
    }
}


[System.Serializable]
public class House
{
    public int id;
    public List<Room> habitaciones;
    public Vector2Int habitacionInicial;
    public List<Door> puertas;

    public House()
    {
        // No generar ID aquí - se genera en el RoomGenerator al crear la casa
        id = 0;
        habitaciones = new List<Room>();
        puertas = new List<Door>();
        
    }
}