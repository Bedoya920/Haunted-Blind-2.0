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
                {"información", "Estás en {location}. {location_description}. Tienes {health} de vida y {actions} acciones disponibles. Tu inventario contiene: {inventory}. Comandos disponibles: adelante, atrás, izquierda, derecha, inspeccionar, tomar, usar, comer, dar, leer, ayuda, información."},
                {"info", "Ubicación: {location}. Vida: {health}/5. Acciones: {actions}. Inventario: {inventory}. Di 'ayuda' para ver los comandos."},
                {"estado", "Tu estado: Vida {health}/5, Acciones {actions}/{max_actions}, Fatiga {fatigue}. Estás en {location}."},
                {"situación", "Estás en {location}. {location_description}. Tienes {health} de vida, {actions} acciones, y llevas: {inventory}."},
                
                // Inventory
                {"inventario", "Tienes estos objetos: {inventory}. Tu vida es {health}/5 y tienes {actions} acciones restantes."},
                {"qué tengo", "Tienes: {inventory}. También tienes {actions} acciones disponibles."},
                
                // Movement commands
                {"adelante", "Avanzas hacia adelante. [CMD:adelante]"},
                {"atrás", "Retrocedes. [CMD:atrás]"},
                {"izquierda", "Te mueves hacia la izquierda. [CMD:izquierda]"},
                {"derecha", "Te mueves hacia la derecha. [CMD:derecha]"},
                {"subir", "Subes las escaleras. [CMD:subir]"},
                {"bajar", "Bajas las escaleras. [CMD:bajar]"},
                
                // Interaction commands
                {"inspeccionar", "Inspeccionas tu entorno. [CMD:inspeccionar]"},
                {"tomar", "Intentas tomar algo. [CMD:tomar]"},
                {"usar", "Usas un objeto. [CMD:usar]"},
                {"comer", "Comes algo para recuperar energía. [CMD:comer]"},
                {"leer", "Lees algo. [CMD:leer]"},
                {"dar", "Das algo. [CMD:dar]"},
                
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
                    
                    // Fire events
                    OnResponseGenerated?.Invoke(parsedResponse.cleanText);
                    
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
            
            // Replace placeholders
            response = response.Replace("{location}", context.currentLocation);
            response = response.Replace("{location_description}", GetLocationDescription(context.currentLocation));
            response = response.Replace("{inventory}", GetInventoryText(context.inventory));
            response = response.Replace("{health}", context.health.ToString());
            response = response.Replace("{max_health}", context.maxHealth.ToString());
            response = response.Replace("{actions}", context.actions.ToString());
            response = response.Replace("{max_actions}", context.maxActions.ToString());
            response = response.Replace("{fatigue}", context.fatigue.ToString());
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
            
            // Default response
            return "No estoy seguro de lo que quieres hacer. Puedes preguntarme por ayuda o usar comandos como 'inspeccionar', 'adelante', 'atrás', 'información', etc.";
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