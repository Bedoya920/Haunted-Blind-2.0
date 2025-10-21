using UnityEngine;

namespace VoiceSystem.Recognition
{
    /// <summary>
    /// Configuration for Whisper model in Sentis
    /// </summary>
    [CreateAssetMenu(fileName = "WhisperModelConfig", menuName = "VoiceSystem/Whisper Model Config")]
    public class WhisperModelConfig : ScriptableObject
    {
        [Header("Model Settings")]
        [Tooltip("Path to the .sentis model file")]
        public string modelPath = "AI/Models/Whisper/whisper-tiny-es.sentis";
        
        [Tooltip("Size of the Whisper model")]
        public WhisperModelSize modelSize = WhisperModelSize.Tiny;
        
        [Tooltip("Language for recognition")]
        public string language = "es";
        
        [Header("Audio Processing")]
        [Tooltip("Threshold for silence detection")]
        [Range(0f, 1f)]
        public float silenceThreshold = 0.01f;
        
        [Tooltip("Duration of audio chunks in seconds")]
        [Range(0.5f, 5f)]
        public float chunkDuration = 2f;
        
        [Tooltip("Sample rate for audio processing")]
        public int sampleRate = 16000;
        
        [Tooltip("Number of audio channels")]
        public int channels = 1;
        
        [Header("Recognition Settings")]
        [Tooltip("Minimum confidence for recognition")]
        [Range(0f, 1f)]
        public float minConfidence = 0.3f;
        
        [Tooltip("Maximum length of recognized text")]
        public int maxTextLength = 200;
        
        [Tooltip("Enable debug logging")]
        public bool debugMode = true;
        
        [Header("Performance")]
        [Tooltip("Use GPU acceleration if available")]
        public bool useGPU = true;
        
        [Tooltip("Number of threads for CPU processing")]
        [Range(1, 8)]
        public int cpuThreads = 4;
        
        public enum WhisperModelSize
        {
            Tiny,    // ~40MB, fastest
            Base,    // ~150MB, balanced
            Small,   // ~500MB, better quality
            Medium,  // ~1.5GB, high quality
            Large    // ~3GB, best quality
        }
        
        /// <summary>
        /// Get the expected model size in MB
        /// </summary>
        public int GetModelSizeMB()
        {
            switch (modelSize)
            {
                case WhisperModelSize.Tiny: return 40;
                case WhisperModelSize.Base: return 150;
                case WhisperModelSize.Small: return 500;
                case WhisperModelSize.Medium: return 1500;
                case WhisperModelSize.Large: return 3000;
                default: return 40;
            }
        }
        
        /// <summary>
        /// Get the expected processing time in seconds
        /// </summary>
        public float GetExpectedProcessingTime()
        {
            switch (modelSize)
            {
                case WhisperModelSize.Tiny: return 2f;
                case WhisperModelSize.Base: return 3f;
                case WhisperModelSize.Small: return 5f;
                case WhisperModelSize.Medium: return 8f;
                case WhisperModelSize.Large: return 12f;
                default: return 2f;
            }
        }
    }
}