using UnityEngine;
using UnityEditor;
using VoiceSystem.Core;
using VoiceSystem.GameIntegration;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Setup automático para integración con RoomGenerator3000
    /// </summary>
    public static class SetupRoomGeneratorIntegration
    {
        [MenuItem("Tools/VoiceSystem/Setup Room Generator Integration")]
        public static void SetupIntegration()
        {
            Debug.Log("=== CONFIGURANDO INTEGRACIÓN CON ROOM GENERATOR ===");
            
            try
            {
                // 1. Buscar RoomGenerator3000
                var generator = Object.FindFirstObjectByType<RoomGenerator3000>();
                if (generator == null)
                {
                    EditorUtility.DisplayDialog(
                        "Error", 
                        "No se encontró RoomGenerator3000 en la escena.\n\n" +
                        "Primero crea el generador de habitaciones o abre la escena que lo contiene.", 
                        "OK"
                    );
                    return;
                }
                
                Debug.Log($"✅ RoomGenerator3000 encontrado en: {generator.gameObject.name}");
                
                // 2. Asegurar RoomInventoryManager existe
                var inventoryManager = Object.FindFirstObjectByType<RoomInventoryManager>();
                if (inventoryManager == null)
                {
                    var invManagerGO = new GameObject("RoomInventoryManager");
                    inventoryManager = invManagerGO.AddComponent<RoomInventoryManager>();
                    Debug.Log("✅ RoomInventoryManager creado");
                }
                else
                {
                    Debug.Log("✅ RoomInventoryManager ya existe");
                }
                
                // 3. Crear Bridge si no existe
                var bridge = Object.FindFirstObjectByType<RoomSystemBridge>();
                if (bridge == null)
                {
                    var bridgeGO = new GameObject("RoomSystemBridge");
                    bridge = bridgeGO.AddComponent<RoomSystemBridge>();
                    Debug.Log("✅ RoomSystemBridge creado");
                }
                else
                {
                    Debug.Log("✅ RoomSystemBridge ya existe");
                }
                
                // 4. Asignar referencias en Bridge
                var bridgeSO = new SerializedObject(bridge);
                bridgeSO.FindProperty("roomGenerator").objectReferenceValue = generator;
                bridgeSO.FindProperty("inventoryManager").objectReferenceValue = inventoryManager;
                
                // Buscar y asignar FatigueSystem
                var fatigueSystem = Object.FindFirstObjectByType<FatigueSystem>();
                if (fatigueSystem != null)
                {
                    bridgeSO.FindProperty("fatigueSystem").objectReferenceValue = fatigueSystem;
                    Debug.Log("✅ FatigueSystem conectado");
                }
                else
                {
                    Debug.LogWarning("⚠️ FatigueSystem no encontrado");
                }
                
                // Buscar y asignar GameTimer
                var gameTimer = Object.FindFirstObjectByType<GameTimer>();
                if (gameTimer != null)
                {
                    bridgeSO.FindProperty("gameTimer").objectReferenceValue = gameTimer;
                    Debug.Log("✅ GameTimer conectado");
                }
                else
                {
                    Debug.LogWarning("⚠️ GameTimer no encontrado");
                }
                
                bridgeSO.ApplyModifiedProperties();
                Debug.Log("✅ Referencias del Bridge asignadas (incluyendo RoomInventoryManager)");
                
                // 5. Conectar con GameContextProvider
                var contextProvider = Object.FindFirstObjectByType<GameContextProvider>();
                if (contextProvider != null)
                {
                    var providerSO = new SerializedObject(contextProvider);
                    providerSO.FindProperty("roomBridge").objectReferenceValue = bridge;
                    providerSO.FindProperty("useRealRoomGenerator").boolValue = true;
                    providerSO.ApplyModifiedProperties();
                    Debug.Log("✅ GameContextProvider conectado al Bridge");
                    Debug.Log("✅ useRealRoomGenerator = TRUE");
                }
                else
                {
                    Debug.LogWarning("⚠️ GameContextProvider no encontrado - ejecuta 'Setup Windows Voice System' primero");
                }
                
                // 6. Asegurar que existan los Singletons del juego
                Debug.Log("=== VERIFICANDO SINGLETONS DEL JUEGO ===");
                
                var gameTimerSingleton = GameTimer.Instance;
                var fatigueSystemSingleton = FatigueSystem.Instance;
                var consumablesManagerSingleton = ConsumiblesManager.Instance;
                
                Debug.Log($"✅ GameTimer Singleton: {(gameTimerSingleton != null ? "OK" : "FALLO")}");
                Debug.Log($"✅ FatigueSystem Singleton: {(fatigueSystemSingleton != null ? "OK" : "FALLO")}");
                Debug.Log($"✅ ConsumiblesManager Singleton: {(consumablesManagerSingleton != null ? "OK" : "FALLO")}");
                Debug.Log($"✅ RoomInventoryManager Singleton: {(inventoryManager != null ? "OK" : "FALLO")}");
                
                // 7. Crear EventTriggerSystem si no existe
                Debug.Log("=== CONFIGURANDO SISTEMA DE EVENTOS ===");
                
                var eventTrigger = Object.FindFirstObjectByType<VoiceSystem.GameIntegration.EventTriggerSystem>();
                if (eventTrigger == null)
                {
                    var triggerGO = new GameObject("EventTriggerSystem");
                    eventTrigger = triggerGO.AddComponent<VoiceSystem.GameIntegration.EventTriggerSystem>();
                    Debug.Log("✅ EventTriggerSystem creado");
                }
                else
                {
                    Debug.Log("✅ EventTriggerSystem ya existe");
                }
                
                // 8. Asegurar EventManager Singleton
                var eventManager = EventManager.Instance;
                Debug.Log($"✅ EventManager Singleton: {(eventManager != null ? "OK" : "FALLO")}");
                
                // 9. Asegurar ActionConfirmationManager Singleton
                var confirmationManager = VoiceSystem.GameIntegration.ActionConfirmationManager.Instance;
                Debug.Log($"✅ ActionConfirmationManager Singleton: {(confirmationManager != null ? "OK" : "FALLO")}");
                
                Debug.Log("=== INTEGRACIÓN COMPLETADA ===");
                
                EditorUtility.DisplayDialog(
                    "Integración Completa", 
                    "✅ RoomSystemBridge configurado\n" +
                    "✅ RoomInventoryManager configurado\n" +
                    "✅ EventTriggerSystem configurado\n" +
                    "✅ Referencias asignadas correctamente\n" +
                    "✅ GameContextProvider conectado\n\n" +
                    "El sistema de voz ahora usará:\n" +
                    "• Habitaciones reales del generador procedural\n" +
                    "• Sistema de items con búsqueda de ocultos\n" +
                    "• Consumibles automáticos\n" +
                    "• Eventos narrados automáticamente\n\n" +
                    "Singletons optimizados:\n" +
                    "✅ GameTimer\n" +
                    "✅ FatigueSystem\n" +
                    "✅ ConsumiblesManager\n" +
                    "✅ RoomInventoryManager\n" +
                    "✅ EventManager\n" +
                    "✅ ActionConfirmationManager\n\n" +
                    "Triggers automáticos:\n" +
                    "• Eventos cada intervalo del GameTimer\n" +
                    "• Eventos al cambiar de habitación\n" +
                    "• Eventos específicos por roomId\n\n" +
                    "Confirmaciones de acciones:\n" +
                    "• Comer: Confirma qué comiste y cuánto recuperaste\n" +
                    "• Tomar: Confirma qué tomaste y tipo de item\n" +
                    "• Buscar: Confirma qué encontraste o si no hay nada\n" +
                    "• Puertas: Confirma si usaste puerta o está bloqueada\n\n" +
                    "IMPORTANTE: Asegúrate de ejecutar el generador (Iniciar) antes de usar el sistema de voz.", 
                    "Perfecto"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Setup] Error durante configuración: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Error", 
                    $"Error durante la configuración:\n\n{e.Message}", 
                    "OK"
                );
            }
        }
        
        [MenuItem("Tools/VoiceSystem/Disconnect Room Generator")]
        public static void DisconnectRoomGenerator()
        {
            Debug.Log("=== DESCONECTANDO ROOM GENERATOR ===");
            
            var contextProvider = Object.FindFirstObjectByType<GameContextProvider>();
            if (contextProvider != null)
            {
                var providerSO = new SerializedObject(contextProvider);
                providerSO.FindProperty("useRealRoomGenerator").boolValue = false;
                providerSO.ApplyModifiedProperties();
                Debug.Log("✅ Volviendo a modo demo");
                
                EditorUtility.DisplayDialog(
                    "Desconexión Completa", 
                    "El sistema de voz ahora usará datos demo en lugar del generador.\n\n" +
                    "Puedes reconectar con 'Setup Room Generator Integration'.", 
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning("⚠️ GameContextProvider no encontrado");
            }
        }
    }
}

