using UnityEngine;

namespace VoiceSystem.Recognition
{
    /// <summary>
    /// Handles audio preprocessing for speech recognition
    /// </summary>
    public class AudioProcessor
    {
        private float[] audioBuffer;
        private int bufferSize;
        private float silenceThreshold;
        private float[] normalizedBuffer;
        
        public AudioProcessor(int sampleRate, float chunkDuration, float silenceThreshold)
        {
            this.bufferSize = Mathf.RoundToInt(sampleRate * chunkDuration);
            this.silenceThreshold = silenceThreshold;
            this.audioBuffer = new float[bufferSize];
            this.normalizedBuffer = new float[bufferSize];
        }
        
        /// <summary>
        /// Process audio data for recognition
        /// </summary>
        public float[] ProcessAudio(float[] inputAudio)
        {
            if (inputAudio == null || inputAudio.Length == 0)
                return null;
                
            // Normalize audio
            NormalizeAudio(inputAudio, normalizedBuffer);
            
            // Apply noise reduction (simple high-pass filter)
            ApplyNoiseReduction(normalizedBuffer);
            
            return normalizedBuffer;
        }
        
        /// <summary>
        /// Check if audio contains speech activity
        /// </summary>
        public bool HasSpeechActivity(float[] audio)
        {
            if (audio == null || audio.Length == 0)
                return false;
                
            float rms = CalculateRMS(audio);
            return rms > silenceThreshold;
        }
        
        /// <summary>
        /// Normalize audio to [-1, 1] range
        /// </summary>
        private void NormalizeAudio(float[] input, float[] output)
        {
            float maxValue = 0f;
            
            // Find maximum value
            for (int i = 0; i < input.Length && i < output.Length; i++)
            {
                float absValue = Mathf.Abs(input[i]);
                if (absValue > maxValue)
                    maxValue = absValue;
            }
            
            // Normalize
            if (maxValue > 0f)
            {
                float normalizationFactor = 1f / maxValue;
                for (int i = 0; i < input.Length && i < output.Length; i++)
                {
                    output[i] = input[i] * normalizationFactor;
                }
            }
        }
        
        /// <summary>
        /// Apply simple noise reduction
        /// </summary>
        private void ApplyNoiseReduction(float[] audio)
        {
            // Simple high-pass filter to remove low-frequency noise
            float alpha = 0.95f;
            float prevSample = 0f;
            
            for (int i = 0; i < audio.Length; i++)
            {
                float currentSample = audio[i];
                audio[i] = alpha * (audio[i] - prevSample);
                prevSample = currentSample;
            }
        }
        
        /// <summary>
        /// Calculate RMS (Root Mean Square) of audio
        /// </summary>
        private float CalculateRMS(float[] audio)
        {
            float sum = 0f;
            for (int i = 0; i < audio.Length; i++)
            {
                sum += audio[i] * audio[i];
            }
            return Mathf.Sqrt(sum / audio.Length);
        }
        
        /// <summary>
        /// Convert audio to the format expected by Whisper (16-bit PCM)
        /// </summary>
        public byte[] ConvertToPCM16(float[] audio)
        {
            byte[] pcmData = new byte[audio.Length * 2]; // 16-bit = 2 bytes per sample
            
            for (int i = 0; i < audio.Length; i++)
            {
                // Clamp to [-1, 1] and convert to 16-bit integer
                short sample = (short)(Mathf.Clamp(audio[i], -1f, 1f) * 32767f);
                
                // Convert to little-endian bytes
                pcmData[i * 2] = (byte)(sample & 0xFF);
                pcmData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
            }
            
            return pcmData;
        }
    }
}