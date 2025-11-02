using System;

namespace VoiceSystem.Core.Interfaces
{
    /// <summary>
    /// Priority levels for TTS messages
    /// </summary>
    public enum TTSPriority
    {
        Background,  // Ambient narration
        Normal,      // AI responses, descriptions
        Urgent       // Dangers, screamers, critical info
    }
    
    /// <summary>
    /// Interface for text-to-speech systems
    /// </summary>
    public interface ITextToSpeech
    {
        /// <summary>
        /// Event fired when speech starts
        /// </summary>
        event Action<string> OnSpeechStarted;
        
        /// <summary>
        /// Event fired when speech completes
        /// </summary>
        event Action<string> OnSpeechCompleted;
        
        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;
        
        /// <summary>
        /// Initialize the TTS system
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Speak the given text
        /// </summary>
        void Speak(string text, TTSPriority priority = TTSPriority.Normal);
        
        /// <summary>
        /// Stop current speech
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Check if currently speaking
        /// </summary>
        bool IsSpeaking { get; }
        
        /// <summary>
        /// Set the volume (0.0 to 1.0)
        /// </summary>
        void SetVolume(float volume);
        
        /// <summary>
        /// Set the speech rate (-10 to 10)
        /// </summary>
        void SetRate(int rate);
        
        /// <summary>
        /// Set the voice to use
        /// </summary>
        void SetVoice(string voiceName);
        
        /// <summary>
        /// Get available voices
        /// </summary>
        string[] GetAvailableVoices();
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        void Dispose();
    }
}