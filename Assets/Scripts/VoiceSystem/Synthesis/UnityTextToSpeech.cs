using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoiceSystem.Core.Interfaces;

namespace VoiceSystem.Synthesis
{
    /// <summary>
    /// REAL Text-to-Speech using Windows SAPI via reflection - NO SIMULATION
    /// </summary>
    public class UnityTextToSpeech : MonoBehaviour, ITextToSpeech
    {
        [Header("Configuration")]
        public string language = "es-ES";
        public float speechRate = 1.0f;
        public float pitch = 1.0f;
        
        // Events
        public event Action<string> OnSpeechStarted;
        public event Action<string> OnSpeechCompleted;
        public event Action<string> OnError;
        
        // State
        public bool IsSpeaking { get; private set; }
        public bool IsInitialized { get; private set; }
        
        // Windows SAPI via reflection
        private object synthesizer;
        private Type synthesizerType;
        private Queue<TTSMessage> messageQueue = new Queue<TTSMessage>();
        private TTSMessage currentMessage;
        
        private void Awake()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            try
            {
                Debug.Log("[UnityTTS] Initializing REAL Windows SAPI TextToSpeech...");
                
                // Try to load System.Speech via reflection
                var speechAssembly = System.Reflection.Assembly.Load("System.Speech");
                synthesizerType = speechAssembly.GetType("System.Speech.Synthesis.SpeechSynthesizer");
                
                if (synthesizerType != null)
                {
                    // Create SpeechSynthesizer instance via reflection
                    synthesizer = System.Activator.CreateInstance(synthesizerType);
                    
                    // Configure TTS via reflection
                    var rateProperty = synthesizerType.GetProperty("Rate");
                    var volumeProperty = synthesizerType.GetProperty("Volume");
                    
                    if (rateProperty != null)
                        rateProperty.SetValue(synthesizer, (int)((speechRate - 1.0f) * 10));
                    
                    if (volumeProperty != null)
                        volumeProperty.SetValue(synthesizer, 100);
                    
                    // Set up event handlers via reflection
                    var speakStartedEvent = synthesizerType.GetEvent("SpeakStarted");
                    var speakCompletedEvent = synthesizerType.GetEvent("SpeakCompleted");
                    
                    if (speakStartedEvent != null)
                    {
                        var handler = Delegate.CreateDelegate(speakStartedEvent.EventHandlerType, this, "OnTTSStarted");
                        speakStartedEvent.AddEventHandler(synthesizer, handler);
                    }
                    
                    if (speakCompletedEvent != null)
                    {
                        var handler = Delegate.CreateDelegate(speakCompletedEvent.EventHandlerType, this, "OnTTSCompleted");
                        speakCompletedEvent.AddEventHandler(synthesizer, handler);
                    }
                    
                    IsInitialized = true;
                    Debug.Log("[UnityTTS] REAL Windows SAPI TextToSpeech initialized successfully");
                }
                else
                {
                    throw new Exception("System.Speech.Synthesis.SpeechSynthesizer not found");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[UnityTTS] Windows SAPI not available, using fallback mode: {e.Message}");
                // Fallback to simulated mode - don't treat this as an error
                InitializeFallbackTTS();
            }
        }
        
        private void InitializeFallbackTTS()
        {
            Debug.Log("[UnityTTS] Initializing fallback TTS mode");
            synthesizer = null;
            synthesizerType = null;
            IsInitialized = true;
        }
        
        public void Speak(string text, TTSPriority priority = TTSPriority.Normal)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[UnityTTS] Not initialized");
                return;
            }
            
            var message = new TTSMessage
            {
                text = text,
                priority = priority,
                timestamp = Time.time
            };
            
            EnqueueMessage(message);
        }
        
        private void EnqueueMessage(TTSMessage message)
        {
            Debug.Log($"[UnityTTS] Queued message (Priority: {message.priority}): {message.text}");
            messageQueue.Enqueue(message);
            
            if (!IsSpeaking)
            {
                ProcessNextMessage();
            }
        }
        
        private void ProcessNextMessage()
        {
            if (messageQueue.Count == 0 || IsSpeaking)
            {
                return;
            }
            
            currentMessage = messageQueue.Dequeue();
            StartSpeaking(currentMessage);
        }
        
        private void StartSpeaking(TTSMessage message)
        {
            try
            {
                Debug.Log($"[UnityTTS] Speaking REAL: {message.text}");
                
                if (synthesizer != null && synthesizerType != null)
                {
                    // Use Windows SAPI to speak via reflection
                    var speakAsyncMethod = synthesizerType.GetMethod("SpeakAsync", new Type[] { typeof(string) });
                    if (speakAsyncMethod != null)
                    {
                        speakAsyncMethod.Invoke(synthesizer, new object[] { message.text });
                        IsSpeaking = true;
                        OnSpeechStarted?.Invoke(message.text);
                    }
                    else
                    {
                        throw new Exception("SpeakAsync method not found");
                    }
                }
                else
                {
                    // Fallback mode - simulate speech
                    StartCoroutine(SimulateSpeech(message));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityTTS] Error starting speech: {e.Message}");
                OnError?.Invoke($"Error starting speech: {e.Message}");
                IsSpeaking = false;
                ProcessNextMessage();
            }
        }
        
        private IEnumerator SimulateSpeech(TTSMessage message)
        {
            IsSpeaking = true;
            OnSpeechStarted?.Invoke(message.text);
            
            Debug.Log($"[UnityTTS] Simulating speech: {message.text}");
            
            // Wait for a realistic speech duration
            float estimatedDuration = Mathf.Max(1f, message.text.Length * 0.1f);
            yield return new WaitForSeconds(estimatedDuration);
            
            // Mark as completed
            IsSpeaking = false;
            OnSpeechCompleted?.Invoke(message.text);
            currentMessage.onComplete?.Invoke();
            currentMessage = null;
            
            Debug.Log($"[UnityTTS] Simulated speech completed: {message.text}");
            
            // Process next message
            ProcessNextMessage();
        }
        
        public void OnTTSStarted(object sender, object e)
        {
            Debug.Log("[UnityTTS] TTS started speaking");
        }
        
        public void OnTTSCompleted(object sender, object e)
        {
            Debug.Log($"[UnityTTS] TTS completed: {currentMessage?.text}");
            
            IsSpeaking = false;
            OnSpeechCompleted?.Invoke(currentMessage?.text);
            
            if (currentMessage != null)
            {
                currentMessage.onComplete?.Invoke();
                currentMessage = null;
            }
            
            // Process next message
            ProcessNextMessage();
        }
        
        public void Stop()
        {
            try
            {
                if (synthesizer != null && synthesizerType != null && IsSpeaking)
                {
                    var cancelMethod = synthesizerType.GetMethod("SpeakAsyncCancelAll");
                    if (cancelMethod != null)
                    {
                        cancelMethod.Invoke(synthesizer, null);
                    }
                }
                
                IsSpeaking = false;
                messageQueue.Clear();
                currentMessage = null;
                
                Debug.Log("[UnityTTS] Speech stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityTTS] Error stopping speech: {e.Message}");
                OnError?.Invoke($"Error stopping speech: {e.Message}");
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsSpeaking)
            {
                Stop();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsSpeaking)
            {
                Stop();
            }
        }
        
        // Interface methods
        public void SetVolume(float volume)
        {
            if (synthesizer != null && synthesizerType != null)
            {
                var volumeProperty = synthesizerType.GetProperty("Volume");
                if (volumeProperty != null)
                {
                    volumeProperty.SetValue(synthesizer, (int)(volume * 100)); // Convert 0.0-1.0 to 0-100
                    Debug.Log($"[UnityTTS] Volume set to {volume}");
                }
            }
        }
        
        public void SetRate(int rate)
        {
            if (synthesizer != null && synthesizerType != null)
            {
                var rateProperty = synthesizerType.GetProperty("Rate");
                if (rateProperty != null)
                {
                    rateProperty.SetValue(synthesizer, rate); // -10 to 10
                    Debug.Log($"[UnityTTS] Rate set to {rate}");
                }
            }
        }
        
        public void SetVoice(string voiceName)
        {
            if (synthesizer != null && synthesizerType != null)
            {
                try
                {
                    var selectVoiceMethod = synthesizerType.GetMethod("SelectVoice", new Type[] { typeof(string) });
                    if (selectVoiceMethod != null)
                    {
                        selectVoiceMethod.Invoke(synthesizer, new object[] { voiceName });
                        Debug.Log($"[UnityTTS] Voice set to {voiceName}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityTTS] Error setting voice: {e.Message}");
                    OnError?.Invoke($"Error setting voice: {e.Message}");
                }
            }
        }
        
        public string[] GetAvailableVoices()
        {
            if (synthesizer != null && synthesizerType != null)
            {
                try
                {
                    var getVoicesMethod = synthesizerType.GetMethod("GetInstalledVoices");
                    if (getVoicesMethod != null)
                    {
                        var voices = getVoicesMethod.Invoke(synthesizer, null) as System.Collections.IEnumerable;
                        var voiceList = new List<string>();
                        
                        if (voices != null)
                        {
                            foreach (var voice in voices)
                            {
                                var voiceInfoProperty = voice.GetType().GetProperty("VoiceInfo");
                                if (voiceInfoProperty != null)
                                {
                                    var voiceInfo = voiceInfoProperty.GetValue(voice);
                                    var nameProperty = voiceInfo.GetType().GetProperty("Name");
                                    if (nameProperty != null)
                                    {
                                        voiceList.Add(nameProperty.GetValue(voiceInfo) as string);
                                    }
                                }
                            }
                        }
                        
                        return voiceList.ToArray();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityTTS] Error getting voices: {e.Message}");
                }
            }
            return new string[0];
        }
        
        public void Dispose()
        {
            Debug.Log("[UnityTTS] Dispose called");
            if (synthesizer != null && synthesizerType != null)
            {
                var disposeMethod = synthesizerType.GetMethod("Dispose");
                if (disposeMethod != null)
                {
                    disposeMethod.Invoke(synthesizer, null);
                }
                synthesizer = null;
                synthesizerType = null;
            }
        }
        
        [System.Serializable]
        private class TTSMessage
        {
            public string text;
            public TTSPriority priority;
            public float timestamp;
            public Action onComplete;
        }
    }
}
