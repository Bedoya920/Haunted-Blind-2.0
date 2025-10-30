using UnityEngine;
using VoiceSystem.Core.Data;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Executes commands extracted by the AI system
    /// </summary>
    public class AICommandExecutor : MonoBehaviour
    {
        [Header("References")]
        public GameContextProvider contextProvider;
        
        [Header("Debug")]
        public bool enableDebugLogs = true;
        
        private void Awake()
        {
            if (contextProvider == null)
                contextProvider = GetComponent<GameContextProvider>();
        }
        
        /// <summary>
        /// Execute a command extracted by AI
        /// </summary>
        public void ExecuteCommand(string commandId)
        {
            if (string.IsNullOrEmpty(commandId))
            {
                LogDebug("Command ID is null or empty");
                return;
            }
            
            LogDebug($"Executing command: {commandId}");
            
            // Validate command
            if (!IsValidCommand(commandId))
            {
                LogDebug($"Invalid command: {commandId}");
                return;
            }
            
            // Execute command through context provider
            contextProvider.ExecuteCommand(commandId);
            
            LogDebug($"Command executed successfully: {commandId}");
        }
        
        /// <summary>
        /// Check if command is valid in current context
        /// </summary>
        public bool IsValidCommand(string commandId)
        {
            if (contextProvider == null)
                return false;
                
            return contextProvider.IsCommandValid(commandId);
        }
        
        /// <summary>
        /// Get available commands in current context
        /// </summary>
        public string[] GetAvailableCommands()
        {
            if (contextProvider == null)
                return new string[0];
                
            var context = contextProvider.GetCurrentContext();
            return context.availableCommands.ToArray();
        }
        
        /// <summary>
        /// Get command execution cost
        /// </summary>
        public int GetCommandCost(string commandId)
        {
            // Default cost for most commands
            return 1;
        }
        
        /// <summary>
        /// Check if player can afford command
        /// </summary>
        public bool CanAffordCommand(string commandId)
        {
            if (contextProvider == null)
                return false;
                
            var context = contextProvider.GetCurrentContext();
            int cost = GetCommandCost(commandId);
            
            return context.CanPerformAction(cost);
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AICommandExecutor] {message}");
            }
        }
        
        // Demo methods for testing
        [ContextMenu("Test Execute Command - adelante")]
        public void TestExecuteAdelante()
        {
            ExecuteCommand("adelante");
        }
        
        [ContextMenu("Test Execute Command - inspeccionar")]
        public void TestExecuteInspeccionar()
        {
            ExecuteCommand("inspeccionar");
        }
        
        [ContextMenu("Test Execute Command - comer")]
        public void TestExecuteComer()
        {
            ExecuteCommand("comer");
        }
        
        [ContextMenu("Show Available Commands")]
        public void ShowAvailableCommands()
        {
            var commands = GetAvailableCommands();
            LogDebug($"Available commands: {string.Join(", ", commands)}");
        }
    }
}