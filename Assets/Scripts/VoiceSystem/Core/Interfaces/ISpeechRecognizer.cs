using System;
using System.Collections.Generic;

namespace VoiceSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for speech recognition systems
    /// </summary>
    public interface ISpeechRecognizer
    {
        /// <summary>
        /// Event fired when speech is recognized
        /// </summary>
        event Action<string> OnSpeechRecognized;
        
        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// Event fired when recognition status changes
        /// </summary>
        event Action<bool> OnStatusChanged;
        
        /// <summary>
        /// Initialize the speech recognizer
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Start listening for speech
        /// </summary>
        void StartListening();
        
        /// <summary>
        /// Stop listening for speech
        /// </summary>
        void StopListening();
        
        /// <summary>
        /// Check if the recognizer is currently listening
        /// </summary>
        bool IsListening { get; }
        
        /// <summary>
        /// Check if the recognizer is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Add keywords to recognize (for keyword-based systems)
        /// </summary>
        void AddKeywords(List<string> keywords);
        
        /// <summary>
        /// Remove keywords from recognition
        /// </summary>
        void RemoveKeywords(List<string> keywords);
        
        /// <summary>
        /// Set the language for recognition
        /// </summary>
        void SetLanguage(string language);
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        void Dispose();
    }
}