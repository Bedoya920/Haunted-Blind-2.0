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
    [SerializeField] private int cellSize = 40;
    [SerializeField] private int mapOffsetX = 10;
    [SerializeField] private int mapOffsetY = 10;
    
    [Header("Colores")]
    [SerializeField] private Color emptyColor = Color.black;
    [SerializeField] private Color roomColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color goalColor = Color.green;
    [SerializeField] private Color doorColor = Color.cyan;
    [SerializeField] private Color playerColor = Color.white;
    
    private Texture2D emptyTexture;
    private Texture2D roomTexture;
    private Texture2D goalTexture;
    private Texture2D doorTexture;
    private Texture2D playerTexture;
    
    private Vector2Int lastPlayerPosition = new Vector2Int(-999, -999);
    
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
        goalTexture = MakeTexture(2, 2, goalColor);
        doorTexture = MakeTexture(2, 2, doorColor);
        playerTexture = MakeTexture(2, 2, playerColor);
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
        
        // Calcular bounds del mapa
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        
        foreach (var room in roomGenerator.casa.habitaciones)
        {
            if (room.posicion.x < minX) minX = room.posicion.x;
            if (room.posicion.y < minY) minY = room.posicion.y;
            if (room.posicion.x > maxX) maxX = room.posicion.x;
            if (room.posicion.y > maxY) maxY = room.posicion.y;
        }
        
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        
        // Dibujar grid vacío
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Rect cellRect = new Rect(
                    mapOffsetX + x * cellSize,
                    mapOffsetY + y * cellSize,
                    cellSize,
                    cellSize
                );
                
                GUI.DrawTexture(cellRect, emptyTexture);
                DrawCellBorder(cellRect);
            }
        }
        
        // Dibujar habitaciones
        foreach (var room in roomGenerator.casa.habitaciones)
        {
            int localX = room.posicion.x - minX;
            int localY = room.posicion.y - minY;
            
            Rect roomRect = new Rect(
                mapOffsetX + localX * cellSize,
                mapOffsetY + localY * cellSize,
                cellSize,
                cellSize
            );
            
            GUI.DrawTexture(roomRect, roomTexture);
            DrawCellBorder(roomRect);
        }
        
        // Dibujar puertas
        DrawDoors(minX, minY);
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
                // Dibujar puerta en el centro del borde entre las dos habitaciones
                float centerX = mapOffsetX + (localX1 + localX2) * cellSize / 2f + cellSize / 2f;
                float centerY = mapOffsetY + localY1 * cellSize + cellSize / 2f;
                doorRect = new Rect(centerX - 3, centerY - cellSize / 4, 6, cellSize / 2);
            }
            else if (deltaY != 0) // Puerta vertical (arriba/abajo)
            {
                // Dibujar puerta en el centro del borde entre las dos habitaciones
                float centerX = mapOffsetX + localX1 * cellSize + cellSize / 2f;
                float centerY = mapOffsetY + (localY1 + localY2) * cellSize / 2f + cellSize / 2f;
                doorRect = new Rect(centerX - cellSize / 4, centerY - 3, cellSize / 2, 6);
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
        
        // Calcular offsets del mapa
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var room in roomGenerator.casa.habitaciones)
        {
            if (room.posicion.x < minX) minX = room.posicion.x;
            if (room.posicion.y < minY) minY = room.posicion.y;
        }
        
        int localX = playerPos.x - minX;
        int localY = playerPos.y - minY;
        
        // Dibujar icono del jugador (más grande y visible)
        Rect playerRect = new Rect(
            mapOffsetX + localX * cellSize + cellSize / 4,
            mapOffsetY + localY * cellSize + cellSize / 4,
            cellSize / 2,
            cellSize / 2
        );
        
        GUI.DrawTexture(playerRect, playerTexture);
        
        // Log si cambió de posición
        if (playerPos != lastPlayerPosition)
        {
            var currentRoom = roomBridge.GetCurrentRoom();
            string roomName = currentRoom != null ? currentRoom.roomName : "Desconocida";
            Debug.Log($"[MapVisualizer] 🎮 Jugador movido → ({playerPos.x}, {playerPos.y}) - {roomName}");
            lastPlayerPosition = playerPos;
        }
        
        // Etiqueta del jugador (más visible)
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.yellow;
        
        // Fondo negro para el texto
        GUIStyle bgStyle = new GUIStyle(GUI.skin.box);
        bgStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
        
        Rect labelRect = new Rect(
            mapOffsetX + localX * cellSize,
            mapOffsetY + localY * cellSize - 18,
            cellSize,
            18
        );
        
        GUI.Box(labelRect, "", bgStyle);
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
        int legendWidth = 150;
        int legendHeight = 140;
        
        // Fondo de la leyenda
        GUI.Box(new Rect(legendX, legendY, legendWidth, legendHeight), "LEYENDA");
        
        int itemY = legendY + 25;
        int itemHeight = 20;
        
        DrawLegendItem(legendX + 10, itemY, playerTexture, "Jugador (YOU)");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, roomTexture, "Habitación");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, doorTexture, "Puerta");
        itemY += itemHeight;
        
        DrawLegendItem(legendX + 10, itemY, goalTexture, "Meta");
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
            case 2: return goalTexture;
            default: return emptyTexture;
        }
    }
}
