using UnityEngine;

namespace VoiceSystem.Synthesis
{
    /// <summary>
    /// Configuration for Text-to-Speech system
    /// </summary>
    [CreateAssetMenu(fileName = "TTSConfig", menuName = "VoiceSystem/TTS Config")]
    public class TTSConfig : ScriptableObject
    {
        [Header("Voice Settings")]
        [Tooltip("Language code for Unity TTS")]
        public string language = "es-ES";
        
        [Tooltip("Speech rate (0.1 to 3.0)")]
        [Range(0.1f, 3.0f)]
        public float speechRate = 1.0f;
        
        [Tooltip("Pitch (0.1 to 2.0)")]
        [Range(0.1f, 2.0f)]
        public float pitch = 1.0f;
        
        [Header("Queue Settings")]
        [Tooltip("Maximum number of messages in queue")]
        public int maxQueueSize = 10;
        
        [Tooltip("Whether to interrupt lower priority messages")]
        public bool interruptLowerPriority = true;
        
        [Tooltip("Default priority for messages")]
        public VoiceSystem.Core.Interfaces.TTSPriority defaultPriority = VoiceSystem.Core.Interfaces.TTSPriority.Normal;
        
        [Header("Audio Settings")]
        [Tooltip("Audio source to use for playback")]
        public AudioSource audioSource;
        
        [Tooltip("Enable debug logging")]
        public bool debugMode = true;
        
        [Header("Performance")]
        [Tooltip("Enable caching of synthesized audio")]
        public bool enableCaching = true;
        
        [Tooltip("Maximum cache size in MB")]
        public int maxCacheSizeMB = 50;
        
        /// <summary>
        /// Get available languages for Unity TTS
        /// </summary>
        public string[] GetAvailableLanguages()
        {
            return new string[]
            {
                "es-ES", // Spanish (Spain)
                "es-MX", // Spanish (Mexico)
                "en-US", // English (US)
                "en-GB", // English (UK)
                "fr-FR", // French
                "de-DE", // German
                "it-IT", // Italian
                "pt-BR"  // Portuguese (Brazil)
            };
        }
        
        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(language) && 
                   speechRate >= 0.1f && speechRate <= 3.0f &&
                   pitch >= 0.1f && pitch <= 2.0f &&
                   maxQueueSize > 0;
        }
    }
}