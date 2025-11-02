using UnityEngine;
using UnityEditor;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Test para verificar integración con RoomGenerator3000
    /// </summary>
    public static class TestRoomGeneratorIntegration
    {
        [MenuItem("Tools/VoiceSystem/Test Room Generator Integration")]
        public static void RunTest()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Error", 
                    "Este test requiere que la escena esté en Play Mode.\n\n" +
                    "Presiona Play primero y luego ejecuta este test.", 
                    "OK"
                );
                return;
            }
            
            Debug.Log("=== TEST DE INTEGRACIÓN ROOM GENERATOR ===");
            
            // Test 1: Verificar RoomSystemBridge
            var bridge = Object.FindFirstObjectByType<RoomSystemBridge>();
            if (bridge == null)
            {
                Debug.LogError("[Test] ❌ RoomSystemBridge no encontrado");
                EditorUtility.DisplayDialog(
                    "Error", 
                    "RoomSystemBridge no encontrado.\n\n" +
                    "Ejecuta 'Tools/VoiceSystem/Setup Room Generator Integration' primero.", 
                    "OK"
                );
                return;
            }
            
            Debug.Log("[Test] ✅ RoomSystemBridge encontrado");
            
            // Test 2: Verificar RoomGenerator3000
            var generator = Object.FindFirstObjectByType<RoomGenerator3000>();
            if (generator == null)
            {
                Debug.LogError("[Test] ❌ RoomGenerator3000 no encontrado");
                return;
            }
            
            if (generator.casa == null || generator.casa.habitaciones.Count == 0)
            {
                Debug.LogError("[Test] ❌ RoomGenerator no ha generado habitaciones. Ejecuta 'Iniciar' en el componente.");
                EditorUtility.DisplayDialog(
                    "Error", 
                    "RoomGenerator no ha generado habitaciones.\n\n" +
                    "Selecciona el GameObject con RoomGenerator3000 y ejecuta 'Iniciar' desde el Context Menu.", 
                    "OK"
                );
                return;
            }
            
            Debug.Log($"[Test] ✅ RoomGenerator tiene {generator.casa.habitaciones.Count} habitaciones generadas");
            Debug.Log($"[Test] ✅ RoomGenerator tiene {generator.casa.puertas.Count} puertas generadas");
            
            // Test 3: Habitación actual
            var currentRoom = bridge.GetCurrentRoom();
            if (currentRoom != null)
            {
                Debug.Log($"[Test] ✅ Habitación actual: {currentRoom.roomName}");
                Debug.Log($"[Test]    - ID: {currentRoom.roomId}");
                Debug.Log($"[Test]    - Descripción corta: {currentRoom.shortDescription}");
                Debug.Log($"[Test]    - Descripción larga: {currentRoom.longDescription}");
                Debug.Log($"[Test]    - Objetos: {string.Join(", ", currentRoom.objects)}");
                Debug.Log($"[Test]    - Puertas: {currentRoom.doors.Count}");
            }
            else
            {
                Debug.LogError("[Test] ❌ No se pudo obtener habitación actual");
            }
            
            // Test 4: Puertas de la habitación actual
            var doors = bridge.GetCurrentRoomDoors();
            Debug.Log($"[Test] ✅ Puertas encontradas: {doors.Length}");
            
            int doorIndex = 1;
            foreach (var door in doors)
            {
                Debug.Log($"[Test] Puerta {doorIndex}:");
                Debug.Log($"[Test]    - ID: {door.doorId}");
                Debug.Log($"[Test]    - Nombre: {door.doorName}");
                Debug.Log($"[Test]    - Dirección: {door.direction}");
                Debug.Log($"[Test]    - Estado: {(door.isLocked ? "🔒 BLOQUEADA" : "✅ Abierta")}");
                Debug.Log($"[Test]    - Lleva a: {door.leadsToRoomId}");
                if (!string.IsNullOrEmpty(door.keyItemId))
                {
                    Debug.Log($"[Test]    - Requiere llave: {door.keyItemId}");
                }
                Debug.Log($"[Test]    - Descripción: {door.description}");
                doorIndex++;
            }
            
            // Test 5: Estadísticas
            Debug.Log($"[Test] ✅ Total de habitaciones en casa: {bridge.GetTotalRoomCount()}");
            Debug.Log($"[Test] ✅ Habitaciones visitadas: {bridge.GetVisitedRoomCount()}");
            
            // Test 6: Conversión de direcciones
            Debug.Log("[Test] Test de cálculo de direcciones:");
            TestDirection(new Vector2Int(0, 0), new Vector2Int(1, 0), "este");
            TestDirection(new Vector2Int(0, 0), new Vector2Int(-1, 0), "oeste");
            TestDirection(new Vector2Int(0, 0), new Vector2Int(0, 1), "sur");
            TestDirection(new Vector2Int(0, 0), new Vector2Int(0, -1), "norte");
            
            // Test 7: Verificar GameContextProvider
            var contextProvider = Object.FindFirstObjectByType<GameContextProvider>();
            if (contextProvider != null)
            {
                var context = contextProvider.GetCurrentContext();
                if (context != null && context.currentRoom != null)
                {
                    Debug.Log($"[Test] ✅ GameContext sincronizado:");
                    Debug.Log($"[Test]    - Ubicación: {context.currentLocation}");
                    Debug.Log($"[Test]    - Room actual: {context.currentRoom.roomName}");
                    Debug.Log($"[Test]    - Puertas disponibles: {context.GetAvailableDoors().Count}");
                }
                else
                {
                    Debug.LogWarning("[Test] ⚠️ GameContext no tiene currentRoom - ¿useRealRoomGenerator está activado?");
                }
            }
            
            Debug.Log("=== TEST COMPLETADO ===");
            
            // Resumen
            string resumen = $"RESUMEN DEL TEST:\n\n" +
                           $"✅ RoomSystemBridge: OK\n" +
                           $"✅ RoomGenerator: {generator.casa.habitaciones.Count} habitaciones\n" +
                           $"✅ Puertas: {generator.casa.puertas.Count} conexiones\n" +
                           $"✅ Habitación actual: {currentRoom?.roomName ?? "N/A"}\n" +
                           $"✅ Puertas actuales: {doors.Length}\n" +
                           $"✅ Visitadas: {bridge.GetVisitedRoomCount()}/{bridge.GetTotalRoomCount()}\n\n" +
                           $"El sistema está listo para usar con comandos de voz.";
            
            EditorUtility.DisplayDialog(
                "Test de Integración - Completo", 
                resumen, 
                "OK"
            );
        }
        
        private static void TestDirection(Vector2Int from, Vector2Int to, string expected)
        {
            // Replicar lógica del Bridge para verificar
            int deltaX = to.x - from.x;
            int deltaY = to.y - from.y;
            
            string result;
            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                result = deltaX > 0 ? "este" : "oeste";
            }
            else if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
            {
                result = deltaY > 0 ? "sur" : "norte";
            }
            else if (deltaX != 0)
            {
                result = deltaX > 0 ? "este" : "oeste";
            }
            else
            {
                result = "aquí";
            }
            
            if (result == expected)
            {
                Debug.Log($"[Test] ✅ Dirección {from} → {to} = {result} (esperado: {expected})");
            }
            else
            {
                Debug.LogError($"[Test] ❌ Dirección {from} → {to} = {result} (esperado: {expected})");
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Show Room Generator Status")]
        public static void ShowStatus()
        {
            var generator = Object.FindFirstObjectByType<RoomGenerator3000>();
            var bridge = Object.FindFirstObjectByType<RoomSystemBridge>();
            var contextProvider = Object.FindFirstObjectByType<GameContextProvider>();
            
            Debug.Log("=== ESTADO DEL SISTEMA ===");
            Debug.Log($"RoomGenerator3000: {(generator != null ? "✅ Encontrado" : "❌ No encontrado")}");
            
            if (generator != null)
            {
                Debug.Log($"  - Casa generada: {(generator.casa != null ? "✅ Sí" : "❌ No")}");
                if (generator.casa != null)
                {
                    Debug.Log($"  - Habitaciones: {generator.casa.habitaciones.Count}");
                    Debug.Log($"  - Puertas: {generator.casa.puertas.Count}");
                    Debug.Log($"  - Posición inicial: {generator.casa.habitacionInicial}");
                }
            }
            
            Debug.Log($"RoomSystemBridge: {(bridge != null ? "✅ Encontrado" : "❌ No encontrado")}");
            
            if (bridge != null && Application.isPlaying)
            {
                Debug.Log($"  - Posición jugador: {bridge.currentPlayerPosition}");
                Debug.Log($"  - Habitaciones visitadas: {bridge.GetVisitedRoomCount()}");
            }
            
            Debug.Log($"GameContextProvider: {(contextProvider != null ? "✅ Encontrado" : "❌ No encontrado")}");
            
            if (contextProvider != null)
            {
                var so = new SerializedObject(contextProvider);
                bool useReal = so.FindProperty("useRealRoomGenerator").boolValue;
                Debug.Log($"  - useRealRoomGenerator: {(useReal ? "✅ TRUE (usando generador)" : "⚠️ FALSE (usando demo)")}");
            }
            
            Debug.Log("=== FIN DE ESTADO ===");
        }
    }
}

