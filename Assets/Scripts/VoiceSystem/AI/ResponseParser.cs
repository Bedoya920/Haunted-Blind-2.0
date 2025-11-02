using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using VoiceSystem.Core.Data;

namespace VoiceSystem.AI
{
    /// <summary>
    /// Parses AI responses to extract commands and clean text
    /// </summary>
    public class ResponseParser
    {
        private static readonly Regex CommandRegex = new Regex(@"\[CMD:(\w+)\]", RegexOptions.IgnoreCase);
        
        /// <summary>
        /// Parse AI response to extract commands and clean text
        /// </summary>
        public ParsedResponse ParseResponse(string aiResponse)
        {
            var parsedResponse = new ParsedResponse();
            
            if (string.IsNullOrEmpty(aiResponse))
            {
                parsedResponse.cleanText = "";
                return parsedResponse;
            }
            
            // Extract commands
            var commandMatches = CommandRegex.Matches(aiResponse);
            foreach (Match match in commandMatches)
            {
                string commandId = match.Groups[1].Value.ToLower();
                parsedResponse.extractedCommands.Add(commandId);
            }
            
            // Remove command markers from text
            parsedResponse.cleanText = CommandRegex.Replace(aiResponse, "").Trim();
            
            // Clean up extra whitespace
            parsedResponse.cleanText = Regex.Replace(parsedResponse.cleanText, @"\s+", " ");
            
            return parsedResponse;
        }
        
        /// <summary>
        /// Check if response contains any commands
        /// </summary>
        public bool HasCommands(string response)
        {
            return CommandRegex.IsMatch(response);
        }
        
        /// <summary>
        /// Extract all commands from response
        /// </summary>
        public List<string> ExtractCommands(string response)
        {
            var commands = new List<string>();
            var matches = CommandRegex.Matches(response);
            
            foreach (Match match in matches)
            {
                commands.Add(match.Groups[1].Value.ToLower());
            }
            
            return commands;
        }
        
        /// <summary>
        /// Validate that extracted commands are valid
        /// </summary>
        public List<string> ValidateCommands(List<string> commands, GameContext context)
        {
            var validCommands = new List<string>();
            
            foreach (string command in commands)
            {
                if (IsValidCommand(command, context))
                {
                    validCommands.Add(command);
                }
                else
                {
                    Debug.LogWarning($"[ResponseParser] Invalid command: {command}");
                }
            }
            
            return validCommands;
        }
        
        private bool IsValidCommand(string commandId, GameContext context)
        {
            // Check if command is in available commands
            if (!context.availableCommands.Contains(commandId))
            {
                return false;
            }
            
            // Additional validation could be added here
            // For example, checking if player has required items, etc.
            
            return true;
        }
    }
    
    /// <summary>
    /// Result of parsing an AI response
    /// </summary>
    [System.Serializable]
    public class ParsedResponse
    {
        public string cleanText = "";
        public List<string> extractedCommands = new List<string>();
        
        public bool HasCommands => extractedCommands.Count > 0;
        
        public override string ToString()
        {
            return $"Text: '{cleanText}', Commands: [{string.Join(", ", extractedCommands)}]";
        }
    }
}