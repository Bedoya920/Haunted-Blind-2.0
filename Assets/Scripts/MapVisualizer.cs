using UnityEngine;
using VoiceSystem.GameIntegration;

/// <summary>
/// Visualizador del mapa generado con indicador de posición del jugador
/// </summary>
public class MapVisualizer : MonoBehaviour
{
    [Header("Mapa")]
    [SerializeField] private RoomGenerator3000 roomGenerator;
    [SerializeField] private RoomSystemBridge roomBridge;
    
    [Header("Configuración Visual")]
    [SerializeField] private int cellSize = 60; // Celdas más grandes estilo imagen
    [SerializeField] private int cellSpacing = 4; // Espacio entre celdas
    [SerializeField] private int doorSize = 12; // Tamaño de las puertas (más visibles)
    [SerializeField] private int mapOffsetX = 30;
    [SerializeField] private int mapOffsetY = 30;
    
    [Header("Colores - Estilo Pixel Art (Como la Imagen)")]
    [SerializeField] private Color backgroundColor = new Color(0.18f, 0.18f, 0.22f); // Fondo azul oscuro
    [SerializeField] private Color emptyColor = new Color(0.1f, 0.1f, 0.1f); // Negro para vacío
    [SerializeField] private Color roomColor = new Color(0.55f, 0.55f, 0.55f); // Gris claro para habitaciones
    [SerializeField] private Color initialRoomColor = new Color(0.5f, 0.85f, 0.5f); // Verde claro para inicio
    [SerializeField] private Color currentRoomColor = new Color(0.4f, 0.75f, 0.4f); // Verde para jugador
    [SerializeField] private Color doorColor = new Color(0.4f, 0.7f, 0.95f); // Azul claro para puertas
    [SerializeField] private Color playerTextColor = Color.yellow; // Amarillo para "YOU"
    
    private Texture2D emptyTexture;
    private Texture2D roomTexture;
    private Texture2D initialRoomTexture;
    private Texture2D currentRoomTexture;
    private Texture2D doorTexture;
    
    private Vector2Int lastPlayerPosition = new Vector2Int(-999, -999);
    private Vector2Int initialRoomPosition;
    
    private void Awake()
    {
        // Auto-asignar referencias
        if (roomGenerator == null)
            roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        
        if (roomBridge == null)
            roomBridge = RoomSystemBridge.Instance;
        
        // Crear texturas
        CreateTextures();
    }
    
    private void CreateTextures()
    {
        emptyTexture = MakeTexture(2, 2, emptyColor);
        roomTexture = MakeTexture(2, 2, roomColor);
        initialRoomTexture = MakeTexture(2, 2, initialRoomColor);
        currentRoomTexture = MakeTexture(2, 2, currentRoomColor);
        doorTexture = MakeTexture(2, 2, doorColor);
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private void OnGUI()
    {
        if (roomGenerator == null || roomGenerator.casa == null || 
            roomGenerator.casa.habitaciones == null || roomGenerator.casa.habitaciones.Count == 0)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Esperando generación del mapa...");
            return;
        }
        
        // Verificar que el bridge también está listo
        if (roomBridge == null || roomBridge.currentPlayerPosition == Vector2Int.zero)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "Inicializando sistemas...");
            return;
        }
        
        DrawMap();
        DrawPlayerPosition();
        DrawLegend();
        DrawCurrentRoomInfo();
    }
    
    private void DrawMap()
    {
        // El RoomGenerator usa una matriz interna, necesitamos obtener bounds
        if (roomGenerator.casa.habitaciones == null || roomGenerator.casa.habitaciones.Count == 0)
            return;
        
        // Guardar posición inicial
        if (initialRoomPosition == Vector2Int.zero || initialRoomPosition == new Vector2Int(-999, -999))
        {
            initialRoomPosition = roomGenerator.casa.habitacionInicial;
        }
        
        // NUEVO: Reorganizar posiciones visuales para forma de T con DOS HORIZONTALES
        // Línea superior: 3 habitaciones (Hall, Sala, Biblioteca)
        // Línea inferior: 3 habitaciones (Comedor, Cocina, Baño)
        // Vertical: 3 habitaciones (Hab.Principal, Sótano, Hab.Niños)
        int visualWidth = 3; // 3 columnas
        int visualHeight = 5; // 0:Superior, 1:Inferior, 2:Hab.Principal, 3:Sótano, 4:Hab.Niños
        
        // Dibujar fondo completo
        int totalWidth = visualWidth * (cellSize + cellSpacing) + mapOffsetX * 2;
        int totalHeight = visualHeight * (cellSize + cellSpacing) + mapOffsetY * 2;
        GUI.DrawTexture(new Rect(0, 0, totalWidth, totalHeight), emptyTexture);
        
        // Dibujar habitaciones con reorganización visual
        foreach (var room in roomGenerator.casa.habitaciones)
        {
            // Mapear coordenadas del generador a posiciones visuales en forma de T
            Vector2Int visualPos = GetVisualPosition(room.posicion);
            
            Rect roomRect = new Rect(
                mapOffsetX + visualPos.x * (cellSize + cellSpacing),
                mapOffsetY + visualPos.y * (cellSize + cellSpacing),
                cellSize,
                cellSize
            );
            
            // Color según estado de la habitación
            Texture2D texture;
            if (room.posicion == roomBridge.currentPlayerPosition)
            {
                texture = currentRoomTexture; // Verde intenso para jugador actual
            }
            else if (room.posicion == initialRoomPosition)
            {
                texture = initialRoomTexture; // Verde claro para inicio
            }
            else
            {
                texture = roomTexture; // Gris para habitaciones normales
            }
            
            GUI.DrawTexture(roomRect, texture);
        }
        
        // Dibujar puertas con posiciones reorganizadas
        DrawDoorsReorganized();
    }
    
    /// <summary>
    /// Mapea coordenadas del generador a posiciones visuales - Forma de T con DOS HORIZONTALES
    /// </summary>
    private Vector2Int GetVisualPosition(Vector2Int generatorPos)
    {
        // FORMA DE T CON DOS LÍNEAS HORIZONTALES:
        // Y=0: [Hall(0,0)] → [Sala(1,0)] → [Biblioteca(2,0)]    ← Horizontal superior
        //                       ↓
        // Y=1:              [Comedor(0,1)] → [Cocina(0,2)] → [Baño(0,3)]  ← Horizontal inferior (centrada)
        //                       ↓
        // Y=2:              [Hab.Principal(0,4)]  (centrado)
        //                       ↓
        // Y=3:                 [Sótano(0,5)]
        //                       ↓
        // Y=4:               [Hab.Niños(0,6)]
        
        // Mapeo de coordenadas internas a visuales:
        // Horizontal superior (Y=0): Hall(0,0), Sala(1,0), Biblioteca(2,0)
        if (generatorPos == new Vector2Int(0, 0)) return new Vector2Int(0, 0); // Hall
        if (generatorPos == new Vector2Int(1, 0)) return new Vector2Int(1, 0); // Sala
        if (generatorPos == new Vector2Int(2, 0)) return new Vector2Int(2, 0); // Biblioteca
        
        // Horizontal inferior (Y=1 visual): Comedor(0,1), Cocina(0,2), Baño(0,3)
        if (generatorPos == new Vector2Int(0, 1)) return new Vector2Int(0, 1); // Comedor
        if (generatorPos == new Vector2Int(0, 2)) return new Vector2Int(1, 1); // Cocina (centrado)
        if (generatorPos == new Vector2Int(0, 3)) return new Vector2Int(2, 1); // Baño (derecha)
        
        // Vertical (Y>=2 visual, centrado en X=1): Hab.Principal, Sótano, Hab.Niños
        if (generatorPos == new Vector2Int(0, 4)) return new Vector2Int(1, 2); // Hab.Principal
        if (generatorPos == new Vector2Int(0, 5)) return new Vector2Int(1, 3); // Sótano
        if (generatorPos == new Vector2Int(0, 6)) return new Vector2Int(1, 4); // Hab.Niños
        
        // Fallback (no debería llegar aquí)
        return generatorPos;
    }
    
    /// <summary>
    /// Dibuja puertas con posiciones reorganizadas en forma de T
    /// </summary>
    private void DrawDoorsReorganized()
    {
        if (roomGenerator.casa.puertas == null) return;
        
        foreach (var door in roomGenerator.casa.puertas)
        {
            // Convertir ambas posiciones de la puerta a coordenadas visuales
            Vector2Int visualPos1 = GetVisualPosition(door.cuarto1);
            Vector2Int visualPos2 = GetVisualPosition(door.cuarto2);
            
            // Determinar si la puerta es horizontal o vertical
            bool isHorizontal = visualPos1.y == visualPos2.y;
            
            Rect doorRect;
            if (isHorizontal)
            {
                // Puerta horizontal (entre habitaciones en la misma fila)
                int doorX = Mathf.Min(visualPos1.x, visualPos2.x);
                int doorY = visualPos1.y;
                doorRect = new Rect(
                    mapOffsetX + doorX * (cellSize + cellSpacing) + cellSize,
                    mapOffsetY + doorY * (cellSize + cellSpacing) + (cellSize - doorSize) / 2,
                    cellSpacing,
                    doorSize
                );
            }
            else
            {
                // Puerta vertical (entre habitaciones en la misma columna)
                int doorX = visualPos1.x;
                int doorY = Mathf.Min(visualPos1.y, visualPos2.y);
                doorRect = new Rect(
                    mapOffsetX + doorX * (cellSize + cellSpacing) + (cellSize - doorSize) / 2,
                    mapOffsetY + doorY * (cellSize + cellSpacing) + cellSize,
                    doorSize,
                    cellSpacing
                );
            }
            
            GUI.DrawTexture(doorRect, doorTexture);
        }
    }
    
    private void DrawDoors(int minX, int minY)
    {
        if (roomGenerator.casa.puertas == null) return;
        
        foreach (var door in roomGenerator.casa.puertas)
        {
            Vector2Int pos1 = door.cuarto1;
            Vector2Int pos2 = door.cuarto2;
            
            int localX1 = pos1.x - minX;
            int localY1 = pos1.y - minY;
            int localX2 = pos2.x - minX;
            int localY2 = pos2.y - minY;
            
            // Determinar la orientación de la puerta
            int deltaX = pos2.x - pos1.x;
            int deltaY = pos2.y - pos1.y;
            
            Rect doorRect;
            
            if (deltaX != 0) // Puerta horizontal (derecha/izquierda)
            {
                // Puerta entre celdas horizontales
                float x1 = mapOffsetX + localX1 * (cellSize + cellSpacing) + cellSize;
                float y1 = mapOffsetY + localY1 * (cellSize + cellSpacing) + cellSize / 2 - doorSize / 2;
                doorRect = new Rect(x1, y1, cellSpacing, doorSize);
            }
            else if (deltaY != 0) // Puerta vertical (arriba/abajo)
            {
                // Puerta entre celdas verticales
                float x1 = mapOffsetX + localX1 * (cellSize + cellSpacing) + cellSize / 2 - doorSize / 2;
                float y1 = mapOffsetY + localY1 * (cellSize + cellSpacing) + cellSize;
                doorRect = new Rect(x1, y1, doorSize, cellSpacing);
            }
            else
            {
                // Puerta en la misma posición (error de datos)
                continue;
            }
            
            GUI.DrawTexture(doorRect, doorTexture);
        }
    }
    
    private void DrawPlayerPosition()
    {
        if (roomBridge == null || roomGenerator == null || roomGenerator.casa == null || roomGenerator.casa.habitaciones == null) return;
        
        // Usar directamente currentPlayerPosition del bridge
        Vector2Int playerPos = roomBridge.currentPlayerPosition;
        
        // Verificar que no sea la posición por defecto
        if (playerPos == Vector2Int.zero)
        {
            // Intentar obtener desde GetCurrentRoom como fallback
            var currentRoom = roomBridge.GetCurrentRoom();
            if (currentRoom == null) return;
            
            // Buscar la Room con ese ID
            Room playerRoom = null;
            foreach (var room in roomGenerator.casa.habitaciones)
            {
                if (("room_" + room.id) == currentRoom.roomId)
                {
                    playerRoom = room;
                    break;
                }
            }
            
            if (playerRoom == null) return;
            playerPos = playerRoom.posicion;
        }
        
        // NUEVO: Usar posición visual en forma de T
        Vector2Int visualPos = GetVisualPosition(playerPos);
        
        // Log si cambió de posición
        if (playerPos != lastPlayerPosition)
        {
            var currentRoom = roomBridge.GetCurrentRoom();
            string roomName = currentRoom != null ? currentRoom.roomName : "Desconocida";
            Debug.Log($"[MapVisualizer] 🎮 Jugador movido → ({playerPos.x}, {playerPos.y}) - {roomName}");
            lastPlayerPosition = playerPos;
        }
        
        // Dibujar texto "YOU" centrado en la celda del jugador
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = playerTextColor; // Amarillo
        
        Rect labelRect = new Rect(
            mapOffsetX + visualPos.x * (cellSize + cellSpacing),
            mapOffsetY + visualPos.y * (cellSize + cellSpacing) + cellSize / 2 - 8,
            cellSize,
            16
        );
        
        GUI.Label(labelRect, "YOU", style);
    }
    
    private void DrawCurrentRoomInfo()
    {
        if (roomBridge == null) return;
        
        var currentRoom = roomBridge.GetCurrentRoom();
        if (currentRoom == null) return;
        
        int infoX = 10;
        int infoY = 40;
        int infoWidth = 300;
        int infoHeight = 80;
        
        GUIStyle bgStyle = new GUIStyle(GUI.skin.box);
        bgStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f));
        
        GUI.Box(new Rect(infoX, infoY, infoWidth, infoHeight), "", bgStyle);
        
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 14;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = Color.yellow;
        
        GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
        normalStyle.fontSize = 12;
        normalStyle.normal.textColor = Color.white;
        
        Vector2Int pos = roomBridge.currentPlayerPosition;
        
        GUI.Label(new Rect(infoX + 10, infoY + 10, infoWidth - 20, 20), 
            $"HABITACIÓN ACTUAL: {currentRoom.roomName}", textStyle);
        GUI.Label(new Rect(infoX + 10, infoY + 35, infoWidth - 20, 20), 
            $"Posición: ({pos.x}, {pos.y})", normalStyle);
        GUI.Label(new Rect(infoX + 10, infoY + 55, infoWidth - 20, 20), 
            $"Puertas: {currentRoom.doors.Count}", normalStyle);
    }
    
    private void DrawLegend()
    {
        int legendX = 10;
        int legendY = Screen.height - 150;
        int legendWidth = 180;
        int legendHeight = 140;
        
        // Fondo de la leyenda
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f));
        GUI.Box(new Rect(legendX, legendY, legendWidth, legendHeight), "LEYENDA", boxStyle);
        
        int itemY = legendY + 30;
        int itemHeight = 22;
        
        // Usar cuadrado blanco para el jugador
        Texture2D whiteSquare = MakeTexture(2, 2, Color.white);
        DrawLegendItem(legendX + 10, itemY, whiteSquare, "Jugador (YOU)");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, roomTexture, "Habitación");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, doorTexture, "Puerta");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, initialRoomTexture, "Inicio (Verde)");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, emptyTexture, "Vacío");
    }
    
    private void DrawLegendItem(int x, int y, Texture2D texture, string label)
    {
        GUI.DrawTexture(new Rect(x, y, 15, 15), texture);
        GUI.Label(new Rect(x + 20, y, 120, 15), label);
    }
    
    private void DrawCellBorder(Rect rect)
    {
        // Borde simple usando GUI.Box
        Color oldColor = GUI.color;
        GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        GUI.Box(rect, "");
        GUI.color = oldColor;
    }
    
    private Texture2D GetTextureForCell(int cellValue)
    {
        switch (cellValue)
        {
            case 0: return emptyTexture;
            case 1: return roomTexture;
            case 2: return initialRoomTexture;
            default: return emptyTexture;
        }
    }
}
