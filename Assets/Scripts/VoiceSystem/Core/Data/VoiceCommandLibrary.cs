using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Library containing all available voice commands
    /// </summary>
    [CreateAssetMenu(fileName = "VoiceCommandLibrary", menuName = "VoiceSystem/Voice Command Library")]
    public class VoiceCommandLibrary : ScriptableObject
    {
        [Header("Commands")]
        public List<VoiceCommand> commands = new List<VoiceCommand>();
        
        /// <summary>
        /// Find a command by its ID
        /// </summary>
        public VoiceCommand GetCommandById(string commandId)
        {
            return commands.FirstOrDefault(cmd => cmd.commandId == commandId);
        }
        
        /// <summary>
        /// Find commands that match the given text
        /// </summary>
        public List<VoiceCommand> FindMatchingCommands(string text)
        {
            return commands.Where(cmd => cmd.MatchesText(text)).ToList();
        }
        
        /// <summary>
        /// Get commands valid in the given context
        /// </summary>
        public List<VoiceCommand> GetValidCommands(GameContext context)
        {
            return commands.Where(cmd => cmd.IsValidInContext(context)).ToList();
        }
        
        /// <summary>
        /// Get all command keywords for recognition
        /// </summary>
        public List<string> GetAllKeywords()
        {
            List<string> allKeywords = new List<string>();
            foreach (VoiceCommand cmd in commands)
            {
                allKeywords.AddRange(cmd.keywords);
            }
            return allKeywords;
        }
        
        /// <summary>
        /// Get commands by category
        /// </summary>
        public List<VoiceCommand> GetCommandsByCategory(string category)
        {
            return commands.Where(cmd => cmd.commandId.StartsWith(category)).ToList();
        }
        
        /// <summary>
        /// Get movement commands
        /// </summary>
        public List<VoiceCommand> GetMovementCommands()
        {
            return GetCommandsByCategory("movement");
        }
        
        /// <summary>
        /// Get interaction commands
        /// </summary>
        public List<VoiceCommand> GetInteractionCommands()
        {
            return GetCommandsByCategory("interaction");
        }
        
        /// <summary>
        /// Get system commands
        /// </summary>
        public List<VoiceCommand> GetSystemCommands()
        {
            return GetCommandsByCategory("system");
        }
    }
}