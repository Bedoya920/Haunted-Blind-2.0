using VoiceSystem.Core.Data;

namespace VoiceSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for providing game context to the AI system
    /// </summary>
    public interface IGameContextProvider
    {
        /// <summary>
        /// Get the current game context
        /// </summary>
        GameContext GetCurrentContext();
        
        /// <summary>
        /// Update the game context
        /// </summary>
        void UpdateContext(GameContext context);
        
        /// <summary>
        /// Check if a command is valid in the current context
        /// </summary>
        bool IsCommandValid(string commandId);
        
        /// <summary>
        /// Execute a command in the game
        /// </summary>
        void ExecuteCommand(string commandId);
    }
}