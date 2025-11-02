using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator3000 : MonoBehaviour
{
    public int numeroHabitaciones;
    public House casa;

    int tamMatriz;
    int[,] habitaciones;
    Room[,] habitacionesRoom;
    List<(Vector2Int desde, Vector2Int hasta)> puertasBase;

    [ContextMenu("Iniciar")]
    public void Iniciar()
    {
        tamMatriz = Mathf.RoundToInt(Mathf.Sqrt(numeroHabitaciones) + 1);
        habitaciones = new int[tamMatriz, tamMatriz];
        habitacionesRoom = new Room[tamMatriz, tamMatriz];
        puertasBase = new List<(Vector2Int, Vector2Int)>();

        int x = tamMatriz / 2;
        int y = tamMatriz - 1;
        casa = new House();
        casa.habitacionInicial = new Vector2Int(x, y);

        int habiCreadas = 1;
        habitaciones[x, y] = 1;

        Room room = new Room();
        room.nombre = "Sala";
        room.posicion = new Vector2Int(x, y);
        habitacionesRoom[x, y] = room;

        int intentos = 0;
        while (habiCreadas < numeroHabitaciones && intentos < 10000)
        {
            intentos++;
            int nx = x;
            int ny = y;

            if (Random.Range(0, 101) < 50)
                nx = Mathf.Clamp(x + Random.Range(-1, 2), 0, tamMatriz - 1);
            else
                ny = Mathf.Clamp(y + Random.Range(-1, 2), 0, tamMatriz - 1);

            if (habitaciones[nx, ny] == 0)
            {
                habitaciones[nx, ny] = 2;
                habiCreadas++;

                Room _room = new Room(new Vector2Int(nx, ny));
                _room.nombre = "Cuarto";
                habitacionesRoom[nx, ny] = _room;

                puertasBase.Add((new Vector2Int(x, y), new Vector2Int(nx, ny)));

                Door puerta = new Door();

                x = nx;
                y = ny;
            }
            else
            {
                if (x != nx || y != ny)
                    puertasBase.Add((new Vector2Int(x, y), new Vector2Int(nx, ny)));
            }
        }
        LimpiarPuertasDuplicadas();

        if (intentos >= 10000)
            Debug.LogWarning("Se alcanzó el límite de intentos sin completar la generación.");



        /////////////////////////////// Rellenar los cuartos y puertas de la casa
        ///

        for (int i = 0; i < tamMatriz; i++)
        {
            for (int j = 0; j < tamMatriz; j++)
            {
                if (habitaciones[i, j] != 0 && habitacionesRoom[i, j] != null)
                {
                    casa.habitaciones.Add(habitacionesRoom[i, j]);
                }
            }
        }

        foreach (var puerta in puertasBase)
        {
            Door door = new Door();
            door.cuarto1 = puerta.desde;
            door.cuarto2 = puerta.hasta;

            casa.puertas.Add(door);
        }
    }

    public void LimpiarPuertasDuplicadas()
    {
        if (puertasBase == null || puertasBase.Count == 0)
        {
            Debug.Log("No hay puertas para limpiar.");
            return;
        }

        var nuevasPuertas = new List<(Vector2Int desde, Vector2Int hasta)>();

        foreach (var puerta in puertasBase)
        {
            // Verificamos si ya existe la puerta (en ambos sentidos)
            bool yaExiste = nuevasPuertas.Exists(p =>
                (p.desde == puerta.desde && p.hasta == puerta.hasta) ||
                (p.desde == puerta.hasta && p.hasta == puerta.desde)
            );

            if (!yaExiste)
                nuevasPuertas.Add(puerta);
        }

        int eliminadas = puertasBase.Count - nuevasPuertas.Count;
        puertasBase = nuevasPuertas;

        Debug.Log($"Puertas duplicadas eliminadas: {eliminadas}. Total final: {puertasBase.Count}");
    }


    private void OnDrawGizmosSelected()
    {
        float tamañoCelda = 1f;
        if (habitaciones == null) return;

        int ancho = habitaciones.GetLength(0);
        int alto = habitaciones.GetLength(1);

        // Dibujar habitaciones
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Gizmos.color = (habitaciones[x, y] == 0) ? Color.black : Color.white;
                if (habitaciones[x, y] == 1) Gizmos.color = Color.green;

                Vector3 posicion = new Vector3(x * tamañoCelda, -y * tamañoCelda, 0);
                Gizmos.DrawCube(posicion, Vector3.one * (tamañoCelda * 0.9f));
            }
        }

        // Dibujar puertas (conexiones)
        if (puertasBase != null)
        {
            Gizmos.color = Color.blue;
            foreach (var puerta in puertasBase)
            {
                Vector3 desde = new Vector3(puerta.desde.x * tamañoCelda, -puerta.desde.y * tamañoCelda, 0);
                Vector3 hasta = new Vector3(puerta.hasta.x * tamañoCelda, -puerta.hasta.y * tamañoCelda, 0);
                Vector3 centroPuerta = (desde + hasta) / 2f;
                Gizmos.DrawCube(centroPuerta, Vector3.one * (tamañoCelda * 0.3f));
            }
        }
    }
}
