using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Represents a voice command that can be recognized and executed
    /// </summary>
    [CreateAssetMenu(fileName = "VoiceCommand", menuName = "VoiceSystem/Voice Command")]
    public class VoiceCommand : ScriptableObject
    {
        [Header("Command Info")]
        public string commandId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Recognition")]
        [Tooltip("Keywords that trigger this command (in Spanish)")]
        public List<string> keywords = new List<string>();
        
        [Header("Game Mechanics")]
        [Tooltip("Action cost to execute this command")]
        public int actionCost = 1;
        [Tooltip("Fatigue cost to execute this command")]
        public int fatigueCost = 1;
        [Tooltip("Whether this command requires a target object")]
        public bool requiresTarget = false;
        
        [Header("Audio")]
        [Tooltip("Audio clip to play when command is recognized")]
        public AudioClip audioFeedback;
        [Tooltip("Text to speak when command is executed")]
        public string executionFeedback;
        
        [Header("Context")]
        [Tooltip("Locations where this command is valid (empty = everywhere)")]
        public List<string> validLocations = new List<string>();
        [Tooltip("Required items in inventory")]
        public List<string> requiredItems = new List<string>();
        [Tooltip("Commands that must be executed before this one")]
        public List<string> prerequisites = new List<string>();
        
        /// <summary>
        /// Check if this command is valid in the given context
        /// </summary>
        public bool IsValidInContext(GameContext context)
        {
            // Check location
            if (validLocations.Count > 0 && !validLocations.Contains(context.currentLocation))
                return false;
                
            // Check required items
            foreach (string item in requiredItems)
            {
                if (!context.inventory.Contains(item))
                    return false;
            }
            
            // Check prerequisites
            foreach (string prereq in prerequisites)
            {
                bool found = false;
                foreach (string recentEvent in context.recentEvents)
                {
                    if (recentEvent.Contains(prereq))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
            }
            
            // Check if player has enough actions
            if (!context.CanPerformAction(actionCost))
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Check if the given text matches this command's keywords
        /// </summary>
        public bool MatchesText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
                
            string lowerText = text.ToLower().Trim();
            
            foreach (string keyword in keywords)
            {
                if (lowerText.Contains(keyword.ToLower()))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get a random keyword for this command
        /// </summary>
        public string GetRandomKeyword()
        {
            if (keywords.Count == 0)
                return commandId;
                
            return keywords[Random.Range(0, keywords.Count)];
        }
    }
}