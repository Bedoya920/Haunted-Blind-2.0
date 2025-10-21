using System.Collections.Generic;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Templates for AI prompts and responses
    /// </summary>
    [CreateAssetMenu(fileName = "AIPromptTemplates", menuName = "VoiceSystem/AI Prompt Templates")]
    public class AIPromptTemplates : ScriptableObject
    {
        [Header("System Prompts")]
        [TextArea(5, 10)]
        public string systemPrompt = @"Eres un asistente para un jugador ciego en un juego de horror llamado 'Haunted Blind'.
Tu objetivo es guiar, describir y ayudar al jugador de forma inmersiva.

REGLAS IMPORTANTES:
- Siempre describe el entorno con audio-descripción detallada
- Da hints sutiles, NO spoilers directos
- Menciona opciones disponibles en cada situación
- Alerta de peligros de forma atmosférica
- Si el jugador pide un comando específico, extráelo con [CMD:nombre]
- Mantén el tono misterioso y atmosférico del horror
- Responde SIEMPRE en español
- Sé empático con el miedo del jugador pero mantén la tensión

FORMATO DE RESPUESTA:
- Descripción del entorno/acción
- Opciones disponibles
- Comando extraído si aplica: [CMD:nombre]";

        [Header("Response Templates")]
        [TextArea(3, 5)]
        public string locationDescriptionTemplate = "Estás en {location}. {description}";
        
        [TextArea(3, 5)]
        public string actionFeedbackTemplate = "{actionDescription}. {result}";
        
        [TextArea(3, 5)]
        public string helpTemplate = "Puedes usar estos comandos: {commands}. También puedes preguntarme sobre tu entorno o pedirme ayuda.";
        
        [TextArea(3, 5)]
        public string errorTemplate = "No puedo hacer eso ahora. {reason}. ¿Qué más te gustaría intentar?";
        
        [Header("Contextual Responses")]
        public List<ContextualResponse> contextualResponses = new List<ContextualResponse>();
        
        [System.Serializable]
        public class ContextualResponse
        {
            public string trigger; // Keyword or phrase that triggers this response
            public string response; // Response template
            public string command; // Optional command to extract
        }
        
        /// <summary>
        /// Get the system prompt with context
        /// </summary>
        public string GetSystemPrompt(GameContext context)
        {
            return systemPrompt + $"\n\nCONTEXTO ACTUAL:\n{context.GetContextSummary()}";
        }
        
        /// <summary>
        /// Get a contextual response for the given input
        /// </summary>
        public string GetContextualResponse(string userInput, GameContext context)
        {
            string lowerInput = userInput.ToLower();
            
            foreach (var response in contextualResponses)
            {
                if (lowerInput.Contains(response.trigger.ToLower()))
                {
                    return response.response;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Format a location description
        /// </summary>
        public string FormatLocationDescription(string location, string description)
        {
            return locationDescriptionTemplate
                .Replace("{location}", location)
                .Replace("{description}", description);
        }
        
        /// <summary>
        /// Format an action feedback
        /// </summary>
        public string FormatActionFeedback(string actionDescription, string result)
        {
            return actionFeedbackTemplate
                .Replace("{actionDescription}", actionDescription)
                .Replace("{result}", result);
        }
        
        /// <summary>
        /// Format help text
        /// </summary>
        public string FormatHelp(string commands)
        {
            return helpTemplate.Replace("{commands}", commands);
        }
        
        /// <summary>
        /// Format error message
        /// </summary>
        public string FormatError(string reason)
        {
            return errorTemplate.Replace("{reason}", reason);
        }
    }
}