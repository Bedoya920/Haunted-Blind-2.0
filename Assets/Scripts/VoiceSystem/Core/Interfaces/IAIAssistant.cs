using System;
using VoiceSystem.Core.Data;

namespace VoiceSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for AI assistant systems
    /// </summary>
    public interface IAIAssistant
    {
        /// <summary>
        /// Event fired when AI generates a response
        /// </summary>
        event Action<string> OnResponseGenerated;
        
        /// <summary>
        /// Event fired when AI extracts a command
        /// </summary>
        event Action<string> OnCommandExtracted;
        
        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// Initialize the AI assistant
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Process user input and generate response
        /// </summary>
        void ProcessInput(string userInput, GameContext context);
        
        /// <summary>
        /// Check if the AI is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Check if the AI is currently processing
        /// </summary>
        bool IsProcessing { get; }
        
        /// <summary>
        /// Set the system prompt/template
        /// </summary>
        void SetSystemPrompt(string prompt);
        
        /// <summary>
        /// Add conversation history entry
        /// </summary>
        void AddToHistory(string userInput, string aiResponse);
        
        /// <summary>
        /// Clear conversation history
        /// </summary>
        void ClearHistory();
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        void Dispose();
    }
}