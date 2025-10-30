using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using VoiceSystem.Core.Interfaces;

namespace VoiceSystem.Recognition
{
    /// <summary>
    /// REAL Speech recognition using Windows Speech Recognition - NO SIMULATION
    /// </summary>
    public class WindowsSpeechRecognizer : MonoBehaviour, ISpeechRecognizer
    {
        [Header("Configuration")]
        public string language = "es-ES";
        public ConfidenceLevel confidenceLevel = ConfidenceLevel.Medium;
        
        // Events
        public event Action<string> OnSpeechRecognized;
        public event Action<string> OnError;
        public event Action<bool> OnListeningStatusChanged;
        public event Action<bool> OnStatusChanged;
        
        // State
        public bool IsListening { get; private set; }
        public bool IsInitialized { get; private set; }
        
        // Windows Speech Recognition
        private DictationRecognizer dictationRecognizer;
        
        private void Awake()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            try
            {
                Debug.Log("[WindowsSpeech] Initializing REAL Windows Speech Recognition...");
                
                // Create DictationRecognizer for continuous speech recognition
                dictationRecognizer = new DictationRecognizer(confidenceLevel);
                
                // Set up event handlers
                dictationRecognizer.DictationResult += OnDictationResult;
                dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
                dictationRecognizer.DictationComplete += OnDictationComplete;
                dictationRecognizer.DictationError += OnDictationError;
                
                IsInitialized = true;
                Debug.Log("[WindowsSpeech] REAL Windows Speech Recognition initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WindowsSpeech] Initialization failed: {e.Message}");
                OnError?.Invoke($"Initialization failed: {e.Message}");
            }
        }
        
        public void StartListening()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[WindowsSpeech] Not initialized");
                return;
            }
            
            if (IsListening)
            {
                Debug.LogWarning("[WindowsSpeech] Already listening");
                return;
            }
            
            try
            {
                Debug.Log("[WindowsSpeech] Starting REAL speech recognition...");
                
                // Ensure we stop any existing session first
                if (dictationRecognizer != null)
                {
                    try
                    {
                        dictationRecognizer.Stop();
                    }
                    catch (Exception stopEx)
                    {
                        Debug.LogWarning($"[WindowsSpeech] Error stopping previous session: {stopEx.Message}");
                    }
                }
                
                // Small delay to ensure previous session is fully stopped
                System.Threading.Thread.Sleep(100);
                
                dictationRecognizer.Start();
                IsListening = true;
                OnListeningStatusChanged?.Invoke(true);
                OnStatusChanged?.Invoke(true);
                Debug.Log("[WindowsSpeech] REAL speech recognition started - listening to your voice");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WindowsSpeech] Failed to start listening: {e.Message}");
                OnError?.Invoke($"Failed to start listening: {e.Message}");
                
                // Try to recover by reinitializing
                if (e.Message.Contains("another dictation recognition session"))
                {
                    Debug.Log("[WindowsSpeech] Attempting to recover from session conflict...");
                    Invoke(nameof(RecoverFromSessionConflict), 1.0f);
                }
            }
        }
        
        private void RecoverFromSessionConflict()
        {
            Debug.Log("[WindowsSpeech] Recovering from session conflict...");
            try
            {
                if (dictationRecognizer != null)
                {
                    dictationRecognizer.Dispose();
                }
                
                // Reinitialize
                Initialize();
                
                // Try starting again
                if (IsInitialized)
                {
                    StartListening();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WindowsSpeech] Recovery failed: {e.Message}");
            }
        }
        
        public void StopListening()
        {
            if (!IsListening)
            {
                Debug.LogWarning("[WindowsSpeech] Not listening");
                return;
            }
            
            try
            {
                Debug.Log("[WindowsSpeech] Stopping speech recognition...");
                dictationRecognizer.Stop();
                IsListening = false;
                OnListeningStatusChanged?.Invoke(false);
                OnStatusChanged?.Invoke(false);
                Debug.Log("[WindowsSpeech] Speech recognition stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WindowsSpeech] Failed to stop listening: {e.Message}");
                OnError?.Invoke($"Failed to stop listening: {e.Message}");
            }
        }
        
        private void OnDictationResult(string text, ConfidenceLevel confidence)
        {
            Debug.Log($"[WindowsSpeech] REAL recognition result: '{text}' (confidence: {confidence})");
            OnSpeechRecognized?.Invoke(text);
        }
        
        private void OnDictationHypothesis(string text)
        {
            Debug.Log($"[WindowsSpeech] Recognition hypothesis: '{text}'");
        }
        
        private void OnDictationComplete(DictationCompletionCause cause)
        {
            Debug.Log($"[WindowsSpeech] Dictation completed: {cause}");
            
            // Always restart listening, regardless of cause
            if (IsListening)
            {
                Debug.Log($"[WindowsSpeech] Restarting listening after {cause}...");
                IsListening = false; // Reset state
                
                // Small delay before restarting
                Invoke(nameof(RestartListening), 0.5f);
            }
        }
        
        private void RestartListening()
        {
            if (IsInitialized && !IsListening)
            {
                Debug.Log("[WindowsSpeech] Restarting dictation...");
                StartListening();
            }
        }
        
        private void OnDictationError(string error, int hresult)
        {
            Debug.LogError($"[WindowsSpeech] Dictation error: {error} (HRESULT: {hresult})");
            OnError?.Invoke($"Dictation error: {error}");
        }
        
        private void OnDestroy()
        {
            if (dictationRecognizer != null)
            {
                if (IsListening)
                {
                    dictationRecognizer.Stop();
                }
                
                dictationRecognizer.DictationResult -= OnDictationResult;
                dictationRecognizer.DictationHypothesis -= OnDictationHypothesis;
                dictationRecognizer.DictationComplete -= OnDictationComplete;
                dictationRecognizer.DictationError -= OnDictationError;
                
                dictationRecognizer.Dispose();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Keep listening even when paused
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // Keep listening even when focus changes
        }
        
        // Interface methods
        public void AddKeywords(List<string> keywords)
        {
            Debug.Log($"[WindowsSpeech] AddKeywords called with {keywords.Count} keywords");
            // DictationRecognizer doesn't use keywords, but we log for compatibility
        }
        
        public void RemoveKeywords(List<string> keywords)
        {
            Debug.Log($"[WindowsSpeech] RemoveKeywords called with {keywords.Count} keywords");
            // DictationRecognizer doesn't use keywords, but we log for compatibility
        }
        
        public void SetLanguage(string language)
        {
            Debug.Log($"[WindowsSpeech] SetLanguage called: {language}");
            this.language = language;
            // Note: DictationRecognizer language is set at creation time
        }
        
        public void Dispose()
        {
            Debug.Log("[WindowsSpeech] Dispose called");
            OnDestroy();
        }
    }
}
