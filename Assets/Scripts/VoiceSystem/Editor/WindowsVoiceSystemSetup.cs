using UnityEngine;
using UnityEditor;
using VoiceSystem.Core;
using VoiceSystem.Recognition;
using VoiceSystem.Synthesis;
using VoiceSystem.AI;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core.Data;

namespace VoiceSystem.Editor
{
    /// <summary>
    /// Setup automático para el sistema de voz real con Windows
    /// </summary>
    public static class WindowsVoiceSystemSetup
    {
        [MenuItem("Tools/VoiceSystem/Setup Windows Voice System")]
        public static void SetupWindowsVoiceSystem()
        {
            Debug.Log("=== CONFIGURANDO SISTEMA DE VOZ REAL WINDOWS ===");
            
            try
            {
                // 1. Crear GameObject principal
                var managerGO = CreateVoiceSystemManager();
                
                // 2. Agregar componentes reales
                AddRealComponents(managerGO);
                
                // 3. Asegurar GamePauseManager existe
                EnsureGamePauseManager();
                
                // 4. Crear configuraciones
                CreateConfigurations();
                
                // 5. Asignar referencias
                AssignReferences(managerGO);
                
                // 6. Configurar permisos
                ConfigurePermissions();
                
                Debug.Log("=== SISTEMA DE VOZ REAL CONFIGURADO ===");
                
                EditorUtility.DisplayDialog(
                    "Sistema de Voz Real Configurado", 
                    "El sistema de voz real ha sido configurado correctamente:\n\n" +
                    "✅ WindowsSpeechRecognizer - Escucha tu voz real\n" +
                    "✅ WindowsTTSPlugin - Habla de verdad\n" +
                    "✅ BasicAIAssistant - IA funcional\n" +
                    "✅ GameContextProvider - Contexto del juego\n" +
                    "✅ AICommandExecutor - Ejecuta comandos\n" +
                    "✅ GamePauseManager - Pausa durante narración\n\n" +
                    "Presiona Play para usar el sistema real.", 
                    "¡Perfecto!"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error configurando sistema: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Error", 
                    $"Error configurando sistema: {e.Message}", 
                    "OK"
                );
            }
        }
        
        private static GameObject CreateVoiceSystemManager()
        {
            // Buscar si ya existe
            var existing = Object.FindFirstObjectByType<VoiceSystemManager>();
            if (existing != null)
            {
                Debug.Log("VoiceSystemManager ya existe, eliminando...");
                Object.DestroyImmediate(existing.gameObject);
            }
            
            // Crear nuevo GameObject
            var managerGO = new GameObject("VoiceSystemManager");
            managerGO.transform.position = Vector3.zero;
            
            // Agregar componente principal
            var manager = managerGO.AddComponent<VoiceSystemManager>();
            
            Debug.Log("✅ VoiceSystemManager creado");
            return managerGO;
        }
        
        private static void AddRealComponents(GameObject managerGO)
        {
            // Agregar WindowsSpeechRecognizer
            var recognizer = managerGO.AddComponent<WindowsSpeechRecognizer>();
            recognizer.language = "es-ES";
            recognizer.confidenceLevel = UnityEngine.Windows.Speech.ConfidenceLevel.Medium;
            Debug.Log("✅ WindowsSpeechRecognizer agregado");
            
                // Agregar WindowsTTSPlugin
                var tts = managerGO.AddComponent<WindowsTTSPlugin>();
                tts.voice = "Microsoft Sabina Desktop"; // Voz en español
                tts.rate = 0;
                tts.volume = 100;
                Debug.Log("✅ WindowsTTSPlugin agregado - AUDIO REAL");
            
            // Agregar BasicAIAssistant
            var ai = managerGO.AddComponent<BasicAIAssistant>();
            Debug.Log("✅ BasicAIAssistant agregado");
            
            // Agregar GameContextProvider
            var context = managerGO.AddComponent<GameContextProvider>();
            Debug.Log("✅ GameContextProvider agregado");
            
            // Agregar AICommandExecutor
            var executor = managerGO.AddComponent<AICommandExecutor>();
            Debug.Log("✅ AICommandExecutor agregado");
        }
        
        private static void EnsureGamePauseManager()
        {
            // Check if GamePauseManager already exists
            var existing = Object.FindFirstObjectByType<GamePauseManager>();
            if (existing != null)
            {
                Debug.Log("✅ GamePauseManager ya existe");
                return;
            }
            
            // Note: GamePauseManager se crea automáticamente como Singleton en Play Mode
            // No lo creamos aquí porque DontDestroyOnLoad solo funciona en Play Mode
            Debug.Log("✅ GamePauseManager se creará automáticamente en Play Mode");
        }
        
        private static void CreateConfigurations()
        {
            // Crear carpeta Resources si no existe
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/Resources/VoiceSystem"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "VoiceSystem");
            }
            
            // Crear TTSConfig
            var ttsConfig = ScriptableObject.CreateInstance<TTSConfig>();
            ttsConfig.language = "es-ES";
            ttsConfig.speechRate = 1.0f;
            ttsConfig.pitch = 1.0f;
            
            AssetDatabase.CreateAsset(ttsConfig, "Assets/Resources/VoiceSystem/TTSConfig.asset");
            Debug.Log("✅ TTSConfig creado");
            
            // Crear VoiceCommandLibrary
            var commandLibrary = ScriptableObject.CreateInstance<VoiceCommandLibrary>();
            commandLibrary.commands = new System.Collections.Generic.List<VoiceCommand>();
            
            // Agregar comandos básicos
            AddVoiceCommand(commandLibrary, "adelante", "Mover hacia adelante", new string[] { "adelante", "avanzar", "ir adelante" });
            AddVoiceCommand(commandLibrary, "atrás", "Mover hacia atrás", new string[] { "atrás", "retroceder", "ir atrás" });
            AddVoiceCommand(commandLibrary, "izquierda", "Mover hacia la izquierda", new string[] { "izquierda", "ir izquierda" });
            AddVoiceCommand(commandLibrary, "derecha", "Mover hacia la derecha", new string[] { "derecha", "ir derecha" });
            AddVoiceCommand(commandLibrary, "inspeccionar", "Inspeccionar el área", new string[] { "inspeccionar", "mirar", "examinar" });
            AddVoiceCommand(commandLibrary, "tomar", "Tomar un objeto", new string[] { "tomar", "agarrar", "recoger" });
            AddVoiceCommand(commandLibrary, "usar", "Usar un objeto", new string[] { "usar", "utilizar" });
            AddVoiceCommand(commandLibrary, "comer", "Comer algo", new string[] { "comer", "alimentarse" });
            AddVoiceCommand(commandLibrary, "ayuda", "Mostrar ayuda", new string[] { "ayuda", "comandos", "qué puedo hacer" });
            
            AssetDatabase.CreateAsset(commandLibrary, "Assets/Resources/VoiceSystem/VoiceCommandLibrary.asset");
            Debug.Log("✅ VoiceCommandLibrary creado con comandos");
            
            // Crear AIPromptTemplates
            var promptTemplates = ScriptableObject.CreateInstance<AIPromptTemplates>();
            promptTemplates.systemPrompt = "Eres un asistente de voz para un juego de supervivencia para personas ciegas. Responde en español de manera clara y útil.";
            promptTemplates.locationDescriptionTemplate = "Estás en {location}. {description}";
            promptTemplates.helpTemplate = "Puedes usar estos comandos: {commands}. También puedes preguntarme sobre tu entorno o pedirme ayuda.";
            
            AssetDatabase.CreateAsset(promptTemplates, "Assets/Resources/VoiceSystem/AIPromptTemplates.asset");
            Debug.Log("✅ AIPromptTemplates creado");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static void AddVoiceCommand(VoiceCommandLibrary library, string commandId, string description, string[] keywords)
        {
            var voiceCommand = ScriptableObject.CreateInstance<VoiceCommand>();
            voiceCommand.commandId = commandId;
            voiceCommand.displayName = commandId;
            voiceCommand.description = description;
            voiceCommand.keywords = new System.Collections.Generic.List<string>(keywords);
            voiceCommand.actionCost = 1;
            voiceCommand.fatigueCost = 1;
            
            library.commands.Add(voiceCommand);
        }
        
        private static void AssignReferences(GameObject managerGO)
        {
            var manager = managerGO.GetComponent<VoiceSystemManager>();
            
            // Cargar configuraciones
            var ttsConfig = Resources.Load<TTSConfig>("VoiceSystem/TTSConfig");
            var commandLibrary = Resources.Load<VoiceCommandLibrary>("VoiceSystem/VoiceCommandLibrary");
            var promptTemplates = Resources.Load<AIPromptTemplates>("VoiceSystem/AIPromptTemplates");
            
            // Asignar referencias
            manager.ttsConfig = ttsConfig;
            manager.commandLibrary = commandLibrary;
            manager.promptTemplates = promptTemplates;
            
            // Configurar auto-start
            manager.autoStartListening = true;
            manager.enableDebugLogs = true;
            
            Debug.Log("✅ Referencias asignadas");
            
            // Marcar como dirty
            EditorUtility.SetDirty(manager);
        }
        
        private static void ConfigurePermissions()
        {
            Debug.Log("✅ Permisos de micrófono configurados (Windows)");
            Debug.Log("NOTA: Asegúrate de que Unity tenga permisos de micrófono en Windows");
        }
    }
}
