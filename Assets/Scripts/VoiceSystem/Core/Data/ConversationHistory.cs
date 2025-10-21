using System.Collections.Generic;
using System.Linq;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Manages conversation history for AI context
    /// </summary>
    [System.Serializable]
    public class ConversationHistory
    {
        [System.Serializable]
        public class ConversationEntry
        {
            public string userInput;
            public string aiResponse;
            public string timestamp;
            
            public ConversationEntry(string user, string ai)
            {
                userInput = user;
                aiResponse = ai;
                timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            }
        }
        
        private List<ConversationEntry> history = new List<ConversationEntry>();
        private int maxEntries = 10;
        
        /// <summary>
        /// Add a new conversation entry
        /// </summary>
        public void AddEntry(string userInput, string aiResponse)
        {
            history.Add(new ConversationEntry(userInput, aiResponse));
            
            // Keep only the last maxEntries
            if (history.Count > maxEntries)
            {
                history.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Get the conversation history as formatted string
        /// </summary>
        public string GetHistoryString()
        {
            if (history.Count == 0)
                return "No hay historial de conversación.";
                
            return string.Join("\n", history.Select(entry => 
                $"[{entry.timestamp}] Usuario: {entry.userInput}\n" +
                $"[{entry.timestamp}] IA: {entry.aiResponse}"));
        }
        
        /// <summary>
        /// Get the last N entries
        /// </summary>
        public List<ConversationEntry> GetLastEntries(int count)
        {
            return history.TakeLast(count).ToList();
        }
        
        /// <summary>
        /// Clear the conversation history
        /// </summary>
        public void Clear()
        {
            history.Clear();
        }
        
        /// <summary>
        /// Get the number of entries
        /// </summary>
        public int Count => history.Count;
        
        /// <summary>
        /// Get all entries
        /// </summary>
        public List<ConversationEntry> GetAllEntries()
        {
            return new List<ConversationEntry>(history);
        }
    }
}