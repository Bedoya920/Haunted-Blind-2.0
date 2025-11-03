using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceSystem.Core.Interfaces;
using VoiceSystem.Core.Data;

namespace VoiceSystem.AI
{
    /// <summary>
    /// Basic AI assistant using rule-based responses (without LLM for demo)
    /// </summary>
    public class BasicAIAssistant : MonoBehaviour, IAIAssistant
    {
        [Header("Configuration")]
        public AIPromptTemplates promptTemplates;
        public VoiceCommandLibrary commandLibrary;
        
        // Events
        public event Action<string> OnResponseGenerated;
        public event Action<string> OnCommandExtracted;
        public event Action<string> OnError;
        
        // State
        public bool IsInitialized { get; private set; }
        public bool IsProcessing { get; private set; }
        
        // Components
        private ResponseParser responseParser;
        private ConversationHistory conversationHistory;
        
        // Response rules
        private Dictionary<string, string> responseRules;
        
        private void Awake()
        {
            responseParser = new ResponseParser();
            conversationHistory = new ConversationHistory();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            try
            {
                Debug.Log("[AI] Initializing Basic AI Assistant...");
                
                // Load response rules
                LoadResponseRules();
                
                IsInitialized = true;
                Debug.Log("[AI] Initialization complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AI] Initialization failed: {e.Message}");
                OnError?.Invoke($"Initialization failed: {e.Message}");
            }
        }
        
        private void LoadResponseRules()
        {
            responseRules = new Dictionary<string, string>
            {
                // Greetings
                {"hola", "Hola. Soy tu asistente en esta casa embrujada. ¿En qué puedo ayudarte?"},
                {"buenos días", "Buenos días. Aunque aquí siempre es de noche... ¿Qué necesitas?"},
                {"buenas tardes", "Buenas tardes. El tiempo parece detenerse en este lugar. ¿Cómo puedo ayudarte?"},
                {"buenas noches", "Buenas noches. La oscuridad es tu compañera aquí. ¿Qué quieres hacer?"},
                
                // Location questions
                {"dónde estoy", "Estás en la {location}. {location_description}"},
                {"dónde me encuentro", "Te encuentras en la {location}. {location_description}"},
                {"qué lugar es este", "Este es {location}. {location_description}"},
                
                // Help requests
                {"ayuda", "Puedes usar estos comandos: {available_commands}. También puedes preguntarme sobre tu entorno."},
                {"qué puedo hacer", "Puedes: {available_commands}. También puedes explorar e inspeccionar objetos."},
                {"cómo salir", "Para escapar, necesitas encontrar la flor de loto. Explora las habitaciones y busca pistas."},
                
                // Information command - Complete situation report
                {"información", "Estás en {location}. {location_description}. Tienes {health} de vida y {actions} acciones disponibles. Tu inventario contiene: {inventory}. Movimientos disponibles: {available_movement_directions}. Otras acciones: inspeccionar, tomar, usar, comer, dar, leer, ayuda."},
                {"info", "Ubicación: {location}. Vida: {health}/5. Acciones: {actions}. Inventario: {inventory}. Puedes moverte: {available_movement_directions}. Di 'ayuda' para ver más comandos."},
                {"estado", "Tu estado: Vida {health}/5, Acciones {actions}/{max_actions}, Fatiga {fatigue}. Estás en {location}."},
                {"situación", "Estás en {location}. {location_description}. Tienes {health} de vida, {actions} acciones, y llevas: {inventory}."},
                
                // Inventory
                {"inventario", "Tienes estos objetos: {inventory}. Tu vida es {health}/5 y tienes {actions} acciones restantes."},
                {"qué tengo", "Tienes: {inventory}. También tienes {actions} acciones disponibles."},
                
                // Door queries
                {"qué puertas hay", "Puedes moverte hacia: {available_movement_directions}. Puertas: {available_doors}."},
                {"cuántas puertas hay", "Hay {door_count} puertas. Puedes ir: {available_movement_directions}."},
                {"qué puertas", "Direcciones disponibles: {available_movement_directions}. Puertas: {available_doors}."},
                {"puertas", "Puedes moverte: {available_movement_directions}."},
                {"inspeccionar puerta", "Examinas la puerta. {door_description}"},
                {"ver puerta", "{door_description}"},
                {"está bloqueada", "{door_lock_status}"},
                {"puerta bloqueada", "{door_lock_status}"},
                
                // Movement commands - Solo extraer comando, sin respuesta de texto
                {"adelante", "[CMD:arriba]"},
                {"arriba", "[CMD:arriba]"},
                {"atrás", "[CMD:abajo]"},
                {"atras", "[CMD:abajo]"},
                {"abajo", "[CMD:abajo]"},
                {"izquierda", "[CMD:izquierda]"},
                {"derecha", "[CMD:derecha]"},
                {"frente", "[CMD:arriba]"},
                {"subir", "[CMD:subir]"},
                {"bajar", "[CMD:bajar]"},
                
                // Interaction commands
                {"inspeccionar", "Inspeccionas tu entorno. [CMD:inspeccionar]"},
                {"tomar", "Intentas tomar algo. [CMD:tomar]"},
                {"usar", "Usas un objeto. [CMD:usar]"},
                {"comer", "Comes algo para recuperar energía. [CMD:comer]"},
                {"leer", "Lees algo. [CMD:leer]"},
                {"dar", "Das algo. [CMD:dar]"},
                
                // Item actions
                {"buscar", "Buscas cuidadosamente en la habitación. [CMD:buscar]"},
                
                // System commands
                {"renacer", "Intentas renacer. [CMD:renacer]"},
                {"despertar", "Intentas despertar. [CMD:despertar]"},
                
                // Fear responses
                {"tengo miedo", "Es normal tener miedo aquí. Respira profundo. Tienes {actions} acciones para escapar. ¿Qué quieres hacer?"},
                {"estoy asustado", "La casa puede ser intimidante, pero debes mantener la calma. Usa tus acciones sabiamente."},
                {"qué es ese ruido", "Los sonidos de esta casa pueden ser inquietantes. Mantén la calma y explora con cuidado."},
                
                // Time questions
                {"qué hora es", "El reloj marca {gameTime}. Tienes {timeRemaining} horas para escapar."},
                {"cuánto tiempo queda", "Te quedan {timeRemaining} horas para escapar de esta casa."},
                
                // Health questions
                {"cómo estoy", "Tu vida es {health}/5. Tu fatiga es {fatigue}. Tienes {actions} acciones restantes."},
                {"estoy herido", "Tu vida es {health}/5. Si necesitas recuperar vida, busca comida en las habitaciones."},
            };
        }
        
        public void ProcessInput(string userInput, GameContext context)
        {
            if (!IsInitialized)
            {
                OnError?.Invoke("AI Assistant not initialized");
                return;
            }
            
            if (IsProcessing)
            {
                Debug.LogWarning("[AI] Already processing input");
                return;
            }
            
            StartCoroutine(ProcessInputAsync(userInput, context));
        }
        
        private System.Collections.IEnumerator ProcessInputAsync(string userInput, GameContext context)
        {
            IsProcessing = true;
            
            try
            {
                Debug.Log($"[AI] Processing input: {userInput}");
                
                // Verify dependencies
                if (context == null)
                {
                    Debug.LogWarning("[AI] Context is null, using default");
                    context = new GameContext();
                }
                
                // Generate response
                string response = GenerateResponse(userInput, context);
                
                // Add to conversation history
                if (conversationHistory != null)
                {
                    conversationHistory.AddEntry(userInput, response);
                }
                
                // Parse response for commands
                if (responseParser != null)
                {
                    var parsedResponse = responseParser.ParseResponse(response);
                    
                    // Fire events - Solo si hay texto que hablar
                    if (!string.IsNullOrWhiteSpace(parsedResponse.cleanText))
                    {
                        OnResponseGenerated?.Invoke(parsedResponse.cleanText);
                    }
                    
                    foreach (string command in parsedResponse.extractedCommands)
                    {
                        OnCommandExtracted?.Invoke(command);
                    }
                }
                else
                {
                    // Just fire the response without parsing
                    OnResponseGenerated?.Invoke(response);
                }
                
                Debug.Log($"[AI] Response: {response}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AI] Error processing input: {e.Message}");
                Debug.LogError($"[AI] Stack trace: {e.StackTrace}");
                OnError?.Invoke($"Processing error: {e.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
            
            yield return null;
        }
        
        private string GenerateResponse(string userInput, GameContext context)
        {
            string lowerInput = userInput.ToLower().Trim();
            
            // Check for exact matches first
            if (responseRules.ContainsKey(lowerInput))
            {
                return FormatResponse(responseRules[lowerInput], context);
            }
            
            // Check for partial matches
            foreach (var rule in responseRules)
            {
                if (lowerInput.Contains(rule.Key))
                {
                    return FormatResponse(rule.Value, context);
                }
            }
            
            // Check for contextual responses
            if (promptTemplates != null)
            {
                string contextualResponse = promptTemplates.GetContextualResponse(userInput, context);
                if (!string.IsNullOrEmpty(contextualResponse))
                {
                    return FormatResponse(contextualResponse, context);
                }
            }
            
            // Default response
            return GenerateDefaultResponse(userInput, context);
        }
        
        private string FormatResponse(string template, GameContext context)
        {
            string response = template;
            
            // Replace placeholders - Legacy location
            response = response.Replace("{location}", context.currentLocation);
            response = response.Replace("{location_description}", GetLocationDescription(context.currentLocation));
            
            // Room system placeholders
            response = response.Replace("{room_name}", context.currentRoom?.roomName ?? context.currentLocation);
            response = response.Replace("{room_description}", context.currentRoom?.shortDescription ?? GetLocationDescription(context.currentLocation));
            response = response.Replace("{room_long_description}", context.currentRoom?.longDescription ?? GetLocationDescription(context.currentLocation));
            response = response.Replace("{available_doors}", GetDoorsText(context.GetAvailableDoors()));
            response = response.Replace("{door_count}", context.GetAvailableDoors().Count.ToString());
            response = response.Replace("{available_movement_directions}", GetMovementDirectionsText(context.GetAvailableDoors()));
            
            // Player state placeholders - usar PlayerStateManager para datos REALES
            var playerState = PlayerStateManager.Instance;
            if (playerState != null)
            {
                response = response.Replace("{inventory}", GetInventoryText(playerState.Inventory));
                response = response.Replace("{health}", playerState.Health.ToString());
                response = response.Replace("{fatigue}", playerState.Fatigue.ToString());
            }
            else
            {
                response = response.Replace("{inventory}", GetInventoryText(context.inventory));
                response = response.Replace("{health}", context.health.ToString());
                response = response.Replace("{fatigue}", context.fatigue.ToString());
            }
            
            response = response.Replace("{max_health}", context.maxHealth.ToString());
            response = response.Replace("{actions}", context.fatigue.ToString()); // Fatiga ES el sistema de acciones
            response = response.Replace("{max_actions}", "25"); // Max fatiga antes de muerte
            response = response.Replace("{gameTime}", context.gameTime);
            response = response.Replace("{timeRemaining}", context.timeRemaining.ToString("F1"));
            response = response.Replace("{available_commands}", GetAvailableCommandsText(context));
            
            return response;
        }
        
        private string GetLocationDescription(string location)
        {
            var descriptions = new Dictionary<string, string>
            {
                {"Sala Principal", "Una habitación amplia y polvorienta. Hueles a madera vieja y polvo. Escuchas el eco de tus pasos."},
                {"Biblioteca", "Una habitación llena de libros antiguos. El aire huele a papel viejo y humedad."},
                {"Comedor", "Una mesa larga cubierta de polvo. Los platos y cubiertos están desordenados."},
                {"Cocina", "Un lugar oscuro con utensilios oxidados. El aire es pesado y húmedo."},
                {"Habitación Principal", "Una habitación elegante con una cama y un espejo. Hay una mesa con una flor marchita."},
                {"Habitación de los Niños", "Dos camas pequeñas y juguetes rotos. Una caja musical está en el suelo."},
                {"Baño", "Un baño con un espejo empañado y una bañera. El agua gotea constantemente."},
                {"Sótano", "Una puerta pesada de madera con un cerrojo oxidado. Algo susurra detrás de ella."}
            };
            
            return descriptions.ContainsKey(location) ? descriptions[location] : "Un lugar misterioso y oscuro.";
        }
        
        private string GetInventoryText(List<string> inventory)
        {
            if (inventory.Count == 0)
                return "nada";
            return string.Join(", ", inventory);
        }
        
        private string GetAvailableCommandsText(GameContext context)
        {
            var availableCommands = context.availableCommands.Take(8).ToArray();
            return string.Join(", ", availableCommands);
        }
        
        private string GenerateDefaultResponse(string userInput, GameContext context)
        {
            // Check for item-specific commands first
            string itemResponse = TryProcessItemCommand(userInput, context);
            if (!string.IsNullOrEmpty(itemResponse))
            {
                return itemResponse;
            }
            
            // Check for door-specific queries
            string doorResponse = TryProcessDoorQuery(userInput, context);
            if (!string.IsNullOrEmpty(doorResponse))
            {
                return doorResponse;
            }
            
            // Try to extract a command from natural language
            if (commandLibrary != null)
            {
                var matchingCommands = commandLibrary.FindMatchingCommands(userInput);
                
                if (matchingCommands.Count > 0)
                {
                    var command = matchingCommands[0];
                    return $"Entiendo que quieres {command.displayName.ToLower()}. [CMD:{command.commandId}]";
                }
            }
            
            // Default response con comandos VÁLIDOS
            var validator = VoiceSystem.GameIntegration.CommandValidator.Instance;
            if (validator != null)
            {
                return validator.GetValidCommandsDescription();
            }
            
            // Fallback
            return "No estoy seguro de lo que quieres hacer. Di 'ayuda' para ver comandos disponibles";
        }
        
        /// <summary>
        /// Try to process door-specific queries
        /// </summary>
        private string TryProcessDoorQuery(string userInput, GameContext context)
        {
            string lowerInput = userInput.ToLower().Trim();
            
            // Check for general door queries FIRST
            if (lowerInput.Contains("qué puertas") || lowerInput.Contains("que puertas") || 
                lowerInput.Contains("puertas disponibles") || lowerInput.Contains("puertas hay") ||
                lowerInput.Contains("dónde puedo ir") || lowerInput.Contains("donde puedo ir"))
            {
                // Get REAL doors from RoomSystemBridge
                var roomBridge = VoiceSystem.GameIntegration.RoomSystemBridge.Instance;
                if (roomBridge == null)
                {
                    return "No puedo acceder a la información de puertas";
                }
                
                var currentRoom = roomBridge.GetCurrentRoom();
                if (currentRoom == null || currentRoom.doors == null || currentRoom.doors.Count == 0)
                {
                    return "No hay puertas visibles en esta habitación";
                }
                
                // List REAL doors with directions
                var doorList = new System.Text.StringBuilder("Puertas disponibles: ");
                for (int i = 0; i < currentRoom.doors.Count; i++)
                {
                    var door = currentRoom.doors[i];
                    string relativeDir = TranslateCardinalToRelative(door.direction);
                    
                    doorList.Append($"{relativeDir} hacia {door.doorName}");
                    if (door.isLocked)
                    {
                        doorList.Append(" (bloqueada)");
                    }
                    
                    if (i < currentRoom.doors.Count - 1)
                    {
                        doorList.Append(", ");
                    }
                }
                
                return doorList.ToString();
            }
            
            // Extract direction or door name from input
            string[] directions = { "norte", "sur", "este", "oeste" };
            string foundDirection = null;
            
            foreach (var dir in directions)
            {
                if (lowerInput.Contains(dir))
                {
                    foundDirection = dir;
                    break;
                }
            }
            
            // "inspeccionar puerta [dirección]"
            if (lowerInput.Contains("inspeccionar") && lowerInput.Contains("puerta"))
            {
                if (foundDirection != null)
                {
                    var door = context.GetDoorByDirection(foundDirection);
                    if (door != null)
                    {
                        return $"{door.doorName} ({door.direction}): {door.description}. " +
                               (door.isLocked ? "Está BLOQUEADA." : "Está abierta.");
                    }
                    return $"No hay ninguna puerta hacia el {foundDirection}.";
                }
                
                // No direction specified, list all doors
                var roomBridge = VoiceSystem.GameIntegration.RoomSystemBridge.Instance;
                var currentRoom = roomBridge?.GetCurrentRoom();
                return $"¿Qué puerta quieres inspeccionar? Las puertas disponibles son: {GetDoorsText(currentRoom?.doors)}";
            }
            
            // "abrir puerta [dirección]" or "usar puerta [dirección]"
            if ((lowerInput.Contains("abrir") || lowerInput.Contains("usar")) && lowerInput.Contains("puerta"))
            {
                if (foundDirection != null)
                {
                    var door = context.GetDoorByDirection(foundDirection);
                    if (door != null)
                    {
                        if (context.CanUseDoor(door, out string reason))
                        {
                            // CAMBIO: Usar doorId real en lugar de genérico
                            return $"Abres {door.doorName} y avanzas. [CMD:usar_puerta_{door.doorId}]";
                        }
                        else
                        {
                            return reason;
                        }
                    }
                    return $"No hay ninguna puerta hacia el {foundDirection}.";
                }
                
                return $"¿Qué puerta quieres usar? Las puertas disponibles son: {GetDoorsText(context.GetAvailableDoors())}";
            }
            
            // "está bloqueada la puerta [dirección]"
            if (lowerInput.Contains("bloqueada") && foundDirection != null)
            {
                var door = context.GetDoorByDirection(foundDirection);
                if (door != null)
                {
                    if (door.isLocked)
                    {
                        if (!string.IsNullOrEmpty(door.keyItemId))
                        {
                            return $"Sí, {door.doorName} está bloqueada. Necesitas: {door.keyItemId}.";
                        }
                        return $"Sí, {door.doorName} está bloqueada y no se puede abrir.";
                    }
                    return $"No, {door.doorName} está abierta.";
                }
                return $"No hay ninguna puerta hacia el {foundDirection}.";
            }
            
            return null;
        }
        
        /// <summary>
        /// Traduce dirección del mapa a comando de voz
        /// </summary>
        private string TranslateCardinalToRelative(string cardinal)
        {
            switch (cardinal.ToLower())
            {
                case "arriba": return "arriba";
                case "abajo": return "abajo";
                case "derecha": return "derecha";
                case "izquierda": return "izquierda";
                // Legacy support
                case "norte": return "arriba";
                case "sur": return "abajo";
                case "este": return "derecha";
                case "oeste": return "izquierda";
                default: return cardinal;
            }
        }
        
        /// <summary>
        /// Get formatted text for doors list
        /// </summary>
        private string GetDoorsText(List<DoorData> doors)
        {
            if (doors == null || doors.Count == 0)
            {
                return "no hay puertas visibles";
            }
            
            var doorDescriptions = doors.Select(d => 
                $"{d.doorName} ({d.direction})" + (d.isLocked ? " [bloqueada]" : "")
            );
            
            return string.Join(", ", doorDescriptions);
        }
        
        /// <summary>
        /// Obtiene las direcciones de movimiento disponibles (arriba, abajo, izquierda, derecha)
        /// </summary>
        private string GetMovementDirectionsText(List<DoorData> doors)
        {
            if (doors == null || doors.Count == 0)
            {
                return "ninguna dirección (no hay puertas)";
            }
            
            var directions = doors
                .Where(d => !d.isLocked) // Solo puertas desbloqueadas
                .Select(d => d.direction)
                .Distinct()
                .OrderBy(dir => dir); // Ordenar alfabéticamente
            
            if (!directions.Any())
            {
                return "ninguna dirección (todas las puertas están bloqueadas)";
            }
            
            return string.Join(", ", directions);
        }
        
        /// <summary>
        /// Try to process item-specific commands
        /// </summary>
        private string TryProcessItemCommand(string userInput, GameContext context)
        {
            string lowerInput = userInput.ToLower().Trim();
            
            // "tomar [item]"
            if (lowerInput.StartsWith("tomar "))
            {
                string itemName = lowerInput.Substring(6).Trim();
                
                // Verificar si el item está en la habitación
                if (context.currentRoom != null && context.currentRoom.objects != null)
                {
                    bool found = context.currentRoom.objects.Any(o => o.ToLower().Contains(itemName));
                    
                    if (found)
                    {
                        return $"Tomas {itemName}. [CMD:tomar_{itemName}]";
                    }
                    else
                    {
                        var objectsList = context.currentRoom.objects.Count > 0
                            ? string.Join(", ", context.currentRoom.objects)
                            : "nada visible";
                        return $"No ves ningún {itemName} aquí. Los objetos disponibles son: {objectsList}. Intenta 'buscar' si crees que hay algo oculto.";
                    }
                }
            }
            
            // "inspeccionar [item]" - solo si no es habitación general
            if (lowerInput.StartsWith("inspeccionar ") && !lowerInput.Contains("puerta"))
            {
                string itemName = lowerInput.Substring(13).Trim();
                
                // Evitar conflicto con "inspeccionar" general
                if (!string.IsNullOrEmpty(itemName))
                {
                    return GetItemDescription(itemName, context);
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Obtiene descripción detallada de un item
        /// </summary>
        private string GetItemDescription(string itemName, GameContext context)
        {
            // Buscar en RoomInventoryManager
            var inventoryManager = GameIntegration.RoomInventoryManager.Instance;
            if (inventoryManager != null && context.currentRoom != null)
            {
                var roomInventory = inventoryManager.GetRoomInventory(context.currentRoom.roomId);
                var item = roomInventory.items.Find(i => 
                    i.itemName.ToLower().Contains(itemName.ToLower()) && i.isVisible
                );
                
                if (item != null)
                {
                    return !string.IsNullOrEmpty(item.longDescription)
                        ? item.longDescription
                        : item.shortDescription;
                }
            }
            
            return $"No encuentras información sobre {itemName}. Quizás necesitas buscarlo primero.";
        }
        
        /// <summary>
        /// Helper para formatear lista de objetos
        /// </summary>
        private string GetObjectsText(List<string> objects)
        {
            if (objects == null || objects.Count == 0)
            {
                return "nada visible";
            }
            
            return string.Join(", ", objects);
        }
        
        public void SetSystemPrompt(string prompt)
        {
            // Not used in basic AI
            Debug.Log("[AI] SetSystemPrompt not implemented in BasicAIAssistant");
        }
        
        public void AddToHistory(string userInput, string aiResponse)
        {
            conversationHistory.AddEntry(userInput, aiResponse);
        }
        
        public void ClearHistory()
        {
            conversationHistory.Clear();
        }
        
        public void Dispose()
        {
            IsInitialized = false;
            responseRules?.Clear();
            conversationHistory?.Clear();
            Debug.Log("[AI] Disposed");
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
    }
}