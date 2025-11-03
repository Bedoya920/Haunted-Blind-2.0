using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RoomGenerator3000 : MonoBehaviour
{
    public int numeroHabitaciones;
    public House casa;
    
    [Header("Serialización")]
    [Tooltip("Nombre del archivo JSON para guardar/cargar")]
    public string mapFileName = "houseData.json";

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
    
    /// <summary>
    /// Generar un mapa predefinido y lógico para la campaña
    /// Layout garantizado con todas las habitaciones conectadas
    /// </summary>
    [ContextMenu("Generar Mapa Campaña")]
    public void GenerarMapaCampana()
    {
        casa = new House();
        casa.id = 100001; // ID fijo para la campaña
        casa.habitaciones = new List<Room>();
        casa.puertas = new List<Door>();
        
        // LAYOUT EN L INVERTIDA - Navegación Simple y Clara
        // 9 habitaciones en forma de L con camino directo al sótano
        //
        //  X: 0       1       2       3
        // Y:
        // 0  [Hall]─────→[Sala]─────→[Biblioteca]
        //     ↓                          
        // 1  [Comedor]                  
        //     ↓                          
        // 2  [Cocina]                   
        //     ↓                          
        // 3  [Baño]                     
        //     ↓                          
        // 4  [Hab.Principal]            
        //     ↓                          
        // 5  [Hab.Niños]        ← INTERCAMBIADO (ahora tiene el oso)
        //     ↓
        // 6  [Sótano]           ← INTERCAMBIADO (ahora al final)
        
        // Crear habitaciones (9 total)
        Room hall = CrearHabitacion(1, new Vector2Int(0, 0), "Hall");                    // INICIO
        Room sala = CrearHabitacion(2, new Vector2Int(1, 0), "Sala");
        Room biblioteca = CrearHabitacion(3, new Vector2Int(2, 0), "Biblioteca");
        Room comedor = CrearHabitacion(4, new Vector2Int(0, 1), "Comedor");
        Room cocina = CrearHabitacion(5, new Vector2Int(0, 2), "Cocina");
        Room bano = CrearHabitacion(6, new Vector2Int(0, 3), "Baño");
        Room habitacionPrincipal = CrearHabitacion(7, new Vector2Int(0, 4), "Habitación Principal");
        Room habitacionNinos = CrearHabitacion(9, new Vector2Int(0, 5), "Habitación de los Niños");  // INTERCAMBIADO
        Room sotano = CrearHabitacion(8, new Vector2Int(0, 6), "Sótano");  // INTERCAMBIADO
        
        // Añadir a la lista (9 habitaciones)
        casa.habitaciones.Add(hall);
        casa.habitaciones.Add(sala);
        casa.habitaciones.Add(biblioteca);
        casa.habitaciones.Add(comedor);
        casa.habitaciones.Add(cocina);
        casa.habitaciones.Add(bano);
        casa.habitaciones.Add(habitacionPrincipal);
        casa.habitaciones.Add(habitacionNinos);  // INTERCAMBIADO
        casa.habitaciones.Add(sotano);  // INTERCAMBIADO
        
        // Establecer habitación inicial (Hall)
        casa.habitacionInicial = hall.posicion;
        
        // Crear puertas - CAMINO EN L SIMPLE Y DIRECTO
        int doorId = 1;
        
        // Pasillo horizontal superior (Hall → Sala → Biblioteca)
        CrearPuerta(ref doorId, hall.posicion, sala.posicion);         // 1: Hall → derecha → Sala
        CrearPuerta(ref doorId, sala.posicion, biblioteca.posicion);   // 2: Sala → derecha → Biblioteca
        
        // Pasillo vertical (Hall → Comedor → Cocina → Baño → Hab.Principal → Hab.Niños → Sótano)
        CrearPuerta(ref doorId, hall.posicion, comedor.posicion);                   // 3: Hall → abajo → Comedor
        CrearPuerta(ref doorId, comedor.posicion, cocina.posicion);                 // 4: Comedor → abajo → Cocina
        CrearPuerta(ref doorId, cocina.posicion, bano.posicion);                    // 5: Cocina → abajo → Baño
        CrearPuerta(ref doorId, bano.posicion, habitacionPrincipal.posicion);       // 6: Baño → abajo → Hab.Principal
        CrearPuerta(ref doorId, habitacionPrincipal.posicion, habitacionNinos.posicion);  // 7: Hab.Principal → abajo → Hab.Niños (INTERCAMBIADO)
        CrearPuerta(ref doorId, habitacionNinos.posicion, sotano.posicion, true, "La puerta está cerrada con llave. Algo te dice que debes esperar a que el reloj marque las 2 AM.");              // 8: Hab.Niños → abajo → Sótano (BLOQUEADA hasta las 2 AM)
        
        Debug.Log($"[RoomGenerator] ✅ Mapa de campaña generado: {casa.habitaciones.Count} habitaciones, {casa.puertas.Count} puertas");
    }
    
    private Room CrearHabitacion(int id, Vector2Int posicion, string nombre)
    {
        Room room = new Room();
        room.id = id;
        room.nombre = nombre;
        room.posicion = posicion;
        
        // Añadir descripciones según el nombre
        switch (nombre.ToLower())
        {
            case "hall":
                room.descripLarga = "Entras al hall de entrada. El aire huele a madera vieja y polvo acumulado. Tus pasos resuenan sobre el suelo de tablones agrietados. A tu alrededor, el silencio es denso, interrumpido solo por el crujido lejano de la estructura. La oscuridad es absoluta, pero percibes el espacio amplio que te rodea.";
                room.descripCorta = "Estás en el hall de entrada. El silencio es denso y el aire huele a abandono.";
                room.narrationFirstEntry = "En el hall principal el aire es espeso, pesado, como si la casa llevara demasiado tiempo conteniendo la respiración. El suelo cruje bajo tus pies y, al fondo, el reloj marca eternamente las 2:00 a.m. Su tic tac no avanza, repite el mismo sonido una y otra vez. El eco parece acompañarte desde lo alto de las escaleras, como si alguien invisible observara tus movimientos. A cada paso, el reloj suena más cerca, aunque no te hayas movido.";
                room.narrationShort = "En el hall principal te acercas al reloj. La manecilla vibra, intentando avanzar, pero algo la retiene en las 2 de la madrugada. El tiempo avanza sólo cuando no lo miras.";
                room.puertas = 2; // Sala (derecha), Comedor (abajo)
                break;
            case "biblioteca":
                room.descripLarga = "Una habitación llena de libros antiguos. El aire huele a papel viejo y humedad. Escuchas el crujir de las estanterías cuando te mueves. Hay un ligero susurro, como si las páginas hablaran entre sí. Algunos volúmenes parecen tener sellos extraños en sus portadas que puedes sentir al tacto.";
                room.descripCorta = "La biblioteca. Huele a libros antiguos y humedad. Las estanterías crujen a tu alrededor.";
                room.narrationFirstEntry = "La puerta de la Biblioteca se abre con un lamento seco. Un olor a humedad, cuero viejo y polvo te envuelve. Las ventanas están cubiertas con cortinas gruesas que no dejan pasar la luz, apenas un hilo pálido se filtra desde la parte superior, iluminando partículas flotantes en el aire. El silencio aquí no es vacío; parece lleno de murmullos apagados, como si las páginas aún conservaran la voz de quien las escribió.";
                room.narrationShort = "El aire está espeso. La biblioteca te recibe con su silencio pesado. El aire huele a papel húmedo y madera vieja. Algo parece haberse movido entre los estantes, aunque todo está quieto.";
                room.narrationAfterEvent = "Cuando vuelves a la biblioteca, el aire parece más denso, casi irrespirable. Las páginas de los libros tiemblan con un murmullo bajo, como si las voces dentro discutieran entre sí. Los sellos en las portadas están calientes al tacto, y un par de libros caen por su cuenta desde los estantes. Algo invisible camina entre los pasillos, rozando las cubiertas con suavidad. Quizás esté buscando quién leyó demasiado.";
                room.puertas = 1; // Sala (izquierda)
                break;
            case "sala":
                room.descripLarga = "Una habitación amplia y polvorienta. Hueles a madera vieja y polvo. Escuchas el eco de tus pasos en el espacio vacío. Hay algo que suena como un reloj de péndulo marcando el tiempo con un tic-tac irregular. Percibes muebles cubiertos de sábanas y, en la pared, el marco de lo que parece ser un cuadro grande.";
                room.descripCorta = "La sala principal. Polvorienta y silenciosa, excepto por el tic-tac de un reloj.";
                room.narrationFirstEntry = "Entras a la sala principal. Es amplia pero no lo suficiente, el polvo cubre la tela de viejos sofás y la atmósfera se vuelve densa. El piano al fondo tiene las teclas amarillentas, y cada tanto una nota vibra por sí sola. Frente a ti, un retrato familiar: el padre, rígido y serio, con un reloj de bolsillo abierto; la madre, serena, sostiene un ramo de flores de loto; y entre ellos, los gemelos, uno sonríe, el otro mira al suelo, abrazando un oso rojo de peluche.";
                room.narrationShort = "Sala principal polvorienta, densa y amplia. El piano vibra solo ocasionalmente.";
                room.narrationAfterEvent = "Regresas a la sala principal, pero algo en el aire ha cambiado. El polvo ya no cubre los muebles; parece suspendido, flotando como si el tiempo mismo respirara. El retrato familiar es distinto. La madre ya no sostiene las flores de loto, y en su lugar, de sus manos abiertas gotea una línea oscura que se extiende hasta el suelo. El padre mira directamente hacia ti, y el niño que sostenía el oso ahora parece ausente, como si hubiera salido del cuadro. Al fondo, el reloj suena de nuevo, un tic tac irregular, como un corazón roto.";
                room.puertas = 2; // Hall (izquierda), Biblioteca (derecha)
                break;
            case "comedor":
                room.descripLarga = "Una mesa larga domina el centro de la habitación. Tus manos encuentran platos y cubiertos desordenados, cubiertos de polvo. El aire tiene un olor extraño, mezcla de comida rancia y humedad. Escuchas el goteo constante de agua desde algún lugar. Una de las sillas parece estar separada del resto, como si alguien acabara de levantarse.";
                room.descripCorta = "El comedor. Una mesa con platos desordenados y el sonido constante de agua goteando.";
                room.narrationFirstEntry = "El comedor se abre ante ti con una mesa larga cubierta por un mantel que alguna vez fue blanco. Los cubiertos están perfectamente dispuestos, pero cada plato está vacío, salvo por un anillo de humedad que parece reciente. Una lámpara de cristal cuelga sobre la mesa, balanceándose con un leve chirrido, aunque el aire está quieto. A un lado, una silla está retirada, como si alguien se hubiera levantado de prisa y al pie de la silla, migajas. El silencio aquí tiene peso; huele a hierro, a sopa rancia y algo más, dulzón.";
                room.narrationShort = "El comedor huele a comida y las sillas parecen girarse hacia donde caminas, te observan; el mantel tiembla apenas cuando pasas pero el candelabro tintinea.";
                room.narrationAfterEvent = "El comedor ha cambiado. Los platos ya no están vacíos: hay restos de comida en descomposición, moscas que zumban en la penumbra. La lámpara parpadea, y las sillas se han movido; todas miran hacia ti. En la cabecera de la mesa, un plato limpio, cubierto por una servilleta roja.";
                room.puertas = 2; // Hall (arriba), Cocina (abajo)
                break;
            case "cocina":
                room.descripLarga = "La cocina está fría. Tus pasos resuenan sobre las baldosas agrietadas. Sientes la estufa oxidada y el refrigerador que ya no funciona. Hay un olor persistente a comida podrida mezclado con algo metálico. Escuchas un zumbido bajo, como si algo eléctrico estuviera a punto de encenderse. Los cajones están entreabiertos, como si alguien hubiera estado buscando algo con urgencia.";
                room.descripCorta = "La cocina. Fría, con olor a comida podrida y un zumbido eléctrico constante.";
                room.narrationFirstEntry = "El olor es lo primero que te golpea: humedad, óxido y grasa rancia. Las ollas aún cuelgan sobre la estufa, cubiertas de una capa negra que brilla con el reflejo tenue de la linterna. El fregadero está lleno de agua turbia y utensilios viejos; cada tanto, una burbuja asciende y revienta con un sonido hueco.";
                room.narrationShort = "El aire en la cocina es irrespirable. Agua del fregadero gotea y se escuchan ollas invisibles chirriar con humeante comida que destila putrefacción.";
                room.puertas = 2; // Comedor (arriba), Baño (abajo)
                break;
            case "baño":
                room.descripLarga = "El baño es pequeño y claustrofóbico. El aire está cargado de humedad y moho. Escuchas el goteo constante de un grifo roto que resuena contra la porcelana. Tus dedos encuentran el lavabo agrietado y el espejo frío. Hay un olor a agua estancada. Por un momento, crees escuchar una respiración que no es la tuya.";
                room.descripCorta = "El baño. Húmedo, con olor a moho y el constante goteo de un grifo.";
                room.narrationFirstEntry = "En el baño el aire es frío, más que en el resto de la casa. La puerta chirría al abrirse y un olor metálico se mezcla con el del moho. El espejo, completamente empañado, refleja solo una sombra borrosa: tú. Una bañera al fondo, llena hasta la mitad con un agua estancada, oscura e inmóvil. Se escuchan gotas, pero dónde.";
                room.narrationShort = "La bañera conserva la huella húmeda de un cuerpo recostado. El espejo, agrietado, no refleja tu figura; solo una sombra que parpadea detrás de ti antes de desvanecerse en el silencio.";
                room.puertas = 2; // Cocina (arriba), Hab.Principal (abajo)
                break;
            case "habitación principal":
                room.descripLarga = "La habitación principal. El aire es pesado, casi sofocante. Tus manos encuentran una cama grande con sábanas húmedas y frías. Hay un tocador con objetos personales: cepillos, frascos de perfume vacíos. En un rincón, percibes algo suave y marchito: flores secas. El silencio aquí es diferente, como si la habitación guardara secretos. Escuchas un susurro muy lejano, casi imperceptible.";
                room.descripCorta = "La habitación principal. Aire pesado, cama con sábanas frías y un susurro lejano.";
                room.narrationFirstEntry = "Empujas la puerta de la habitación principal y un olor tenue a madera vieja y perfume marchito llena el aire. La habitación es amplia, con una cama de dosel cubriéndose de polvo, las cortinas pesadas cayendo como velos de luto. Frente a la cama, un espejo ovalado refleja una imagen que tarda en parecer tuya. En un escritorio descansa una flor de loto marchita. Sus pétalos ennegrecidos se curvan hacia adentro, secos, quebradizos, como si hubieran querido proteger algo que ya no existe.";
                room.narrationShort = "El aire conserva el aroma del loto. La flor parece más viva que la casa misma. El espejo refleja un par de sombras donde no hay nadie.";
                room.narrationAfterEvent = "Vuelves a la habitación y el aire ha cambiado: ya no hay olor a polvo, sino a tierra húmeda y flores recién abiertas. La flor de loto, antes seca, ahora irradia un brillo pálido, como si respirara. Sus pétalos están limpios, firmes, suaves, y en su centro parece latir una luz débil, un corazón que no termina de apagarse.";
                room.puertas = 2; // Baño (arriba), Hab.Niños (abajo)
                break;
            case "sótano":
                room.descripLarga = "Desciendes al sótano. El aire es denso, húmedo y huele a tierra mojada. Cada paso en las escaleras de madera cruje peligrosamente. Abajo, el frío es penetrante. Escuchas el goteo de agua filtrándose por las paredes de piedra. Hay cajas apiladas, viejas herramientas oxidadas. En el fondo, percibes algo que late suavemente, como un corazón enterrado. Este lugar guarda los secretos más oscuros de la casa.";
                room.descripCorta = "El sótano. Frío, húmedo, con olor a tierra mojada y un latido lejano.";
                room.narrationFirstEntry = "Llegas al final del pasillo. La puerta del sótano se alza frente a ti: madera gruesa, húmeda, con un cerrojo oxidado que parece latir al ritmo del reloj de la casa. Del otro lado se escucha un leve murmullo, como si alguien respirara muy cerca, apenas contenido por la madera. No hay luz, salvo una rendija diminuta por la que pasa un hilo de aire frío que huele a tierra mojada, como de cementerio. El suelo vibra, débilmente, con cada tic del reloj.";
                room.narrationShort = "La puerta sigue ahí. El aire se inunda de un fuerte olor de flores y tierra de cementerio, y un hilo de podredumbre. El tic tac del reloj proviene desde abajo, constante, recordando que el tiempo en esta casa ya no le pertenece.";
                room.puertas = 2; // Hab.Niños (arriba), FINAL
                break;
            case "habitación de los niños":
                room.descripLarga = "Entras a la habitación de los niños. El aire huele a polvo y juguetes olvidados. Tus pies pisan sobre algo suave, quizás peluches tirados en el suelo. Escuchas el crujido de una cama pequeña y el tintineo de una caja de música que suena sola, repitiendo una melodía infantil distorsionada. Las paredes están frías. Hay dibujos colgados que puedes sentir bajo tus dedos: trazos irregulares, como hechos con urgencia.";
                room.descripCorta = "La habitación de los niños. Huele a juguetes viejos y suena una caja de música distorsionada.";
                room.narrationFirstEntry = "Empujas la puerta con suavidad. Un leve olor a madera húmeda y polvo te recibe, mezclado con el perfume añejo de algo que alguna vez fue infancia. Juguetes regados, una caja musical y dos camas pequeñas, perfectamente paralelas, aún conservan la forma de cuerpos diminutos que ya no están. El papel tapiz muestra dibujos hechos con crayones: una madre de flores blancas, un padre con un reloj brillante y dos niños que se toman de la mano frente a una casa. Pero una de las figuras ha sido borrada con trazos negros, como si alguien hubiera intentado arrancarla del recuerdo.";
                room.narrationShort = "La habitación de los niños. La caja musical sigue sonando con su melodía quebrada. Los dibujos en la pared parecen moverse cuando no los miras.";
                room.narrationAfterEvent = "Vuelves a la habitación de los niños. La caja musical gira más rápido ahora, con notas que se rompen y retuercen. El aire es denso, y escuchas una respiración pequeña, como si el niño estuviera aquí, esperando.";
                room.puertas = 2; // Hab.Principal (arriba), Sótano (abajo)
                break;
        }
        
        return room;
    }
    
    private void CrearPuerta(ref int doorId, Vector2Int cuarto1, Vector2Int cuarto2, bool bloqueada = false, string mensajeBloqueada = "")
    {
        Door door = new Door();
        door.id = doorId++;
        door.cuarto1 = cuarto1;
        door.cuarto2 = cuarto2;
        door.abierta = !bloqueada; // Si bloqueada=true, abierta=false
        door.variableNecesaria = 0; // Sin requisitos
        door.mensajeBloqueada = bloqueada ? mensajeBloqueada : "";
        door.mensajeAbrir = "";
        casa.puertas.Add(door);
    }
    
    /// <summary>
    /// Guardar el mapa generado en JSON
    /// </summary>
    [ContextMenu("Guardar Casa JSON")]
    public void GuardarCasaJson()
    {
        if (casa == null)
        {
            Debug.LogWarning("No hay una casa generada para guardar.");
            return;
        }
        
        string json = JsonUtility.ToJson(casa, true);
        string ruta = Path.Combine(Application.dataPath, mapFileName);
        File.WriteAllText(ruta, json);
        Debug.Log($"Casa guardada en: {ruta}\nHabitaciones: {casa.habitaciones.Count}, Puertas: {casa.puertas.Count}");
    }
    
    /// <summary>
    /// Cargar el mapa desde JSON
    /// </summary>
    [ContextMenu("Cargar Casa JSON")]
    public void CargarCasaJson()
    {
        string ruta = Path.Combine(Application.dataPath, mapFileName);
        
        if (!File.Exists(ruta))
        {
            Debug.LogWarning($"No existe ningún archivo JSON para cargar: {ruta}");
            return;
        }
        
        string json = File.ReadAllText(ruta);
        casa = JsonUtility.FromJson<House>(json);
        Debug.Log($"Casa cargada correctamente desde {ruta}.\nHabitaciones: {casa.habitaciones.Count}, Puertas: {casa.puertas.Count}");
    }
}
