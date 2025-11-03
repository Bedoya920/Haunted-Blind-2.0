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
        casa.id = Random.Range(10000, 99999); // Generar ID aquí, no en constructor
        casa.habitacionInicial = new Vector2Int(x, y);

        int habiCreadas = 1;
        habitaciones[x, y] = 1;

        Room room = new Room();
        room.id = Random.Range(10000, 99999); // Generar ID aquí, no en constructor
        room.nombre = "Habitación"; // Nombre genérico - será reemplazado por MapeoGDD
        room.posicion = new Vector2Int(x, y);
        habitacionesRoom[x, y] = room;

        // Guardar todas las habitaciones creadas para elegir desde cualquiera
        List<Vector2Int> habitacionesCreadas = new List<Vector2Int>();
        habitacionesCreadas.Add(new Vector2Int(x, y));
        
        int intentos = 0;
        int intentosSinExito = 0;
        
        while (habiCreadas < numeroHabitaciones && intentos < 10000)
        {
            intentos++;
            
            // Si llevamos muchos intentos sin éxito, elegir una habitación aleatoria como base
            if (intentosSinExito > 50 && habitacionesCreadas.Count > 0)
            {
                Vector2Int randomRoom = habitacionesCreadas[Random.Range(0, habitacionesCreadas.Count)];
                x = randomRoom.x;
                y = randomRoom.y;
                intentosSinExito = 0;
                Debug.Log($"[RoomGenerator] Cambiando a habitación aleatoria: ({x}, {y})");
            }
            
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
                intentosSinExito = 0; // Reset contador

                Room _room = new Room(new Vector2Int(nx, ny));
                _room.id = Random.Range(10000, 99999); // Generar ID aquí, no en constructor
                _room.nombre = "Habitación"; // Nombre genérico - será reemplazado por MapeoGDD
                habitacionesRoom[nx, ny] = _room;
                
                habitacionesCreadas.Add(new Vector2Int(nx, ny)); // Añadir a lista

                puertasBase.Add((new Vector2Int(x, y), new Vector2Int(nx, ny)));

                Door puerta = new Door();

                x = nx;
                y = ny;
            }
            else
            {
                intentosSinExito++; // Incrementar si no creamos habitación
                
                if (x != nx || y != ny)
                    puertasBase.Add((new Vector2Int(x, y), new Vector2Int(nx, ny)));
            }
        }
        LimpiarPuertasDuplicadas();

        if (intentos >= 10000)
        {
            Debug.LogWarning($"Se alcanzó el límite de intentos. Habitaciones creadas: {habiCreadas}/{numeroHabitaciones}");
        }
        
        Debug.Log($"[RoomGenerator] Generación completada: {habiCreadas} habitaciones creadas en {intentos} intentos");



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
            door.id = Random.Range(10000, 99999); // Generar ID aquí, no en constructor
            door.cuarto1 = puerta.desde;
            door.cuarto2 = puerta.hasta;

            casa.puertas.Add(door);
        }
        
        Debug.Log($"[RoomGenerator] ✅ Casa finalizada: {casa.habitaciones.Count} habitaciones, {casa.puertas.Count} puertas");
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
