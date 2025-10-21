using System;
using UnityEngine;
using VoiceSystem.Core.Interfaces;
using VoiceSystem.Core.Data;
using VoiceSystem.Recognition;
using VoiceSystem.Synthesis;
using VoiceSystem.AI;
using VoiceSystem.GameIntegration;

namespace VoiceSystem.Core
{
    /// <summary>
    /// Central manager that coordinates all voice system components
    /// </summary>
    public class VoiceSystemManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private WindowsSpeechRecognizer speechRecognizerComponent;
        [SerializeField] private WindowsTTSPlugin textToSpeechComponent;
        public BasicAIAssistant aiAssistant;
        public GameContextProvider contextProvider;
        public AICommandExecutor commandExecutor;
        
        // Interfaces (asignadas automáticamente)
        public ISpeechRecognizer speechRecognizer { get; private set; }
        public ITextToSpeech textToSpeech { get; private set; }
        
        [Header("Configuration")]
        public TTSConfig ttsConfig;
        public VoiceCommandLibrary commandLibrary;
        public AIPromptTemplates promptTemplates;
        
        [Header("Debug")]
        public bool enableDebugLogs = true;
        public bool autoStartListening = false; // Disabled by default to prevent conflicts
        
        // Events
        public event Action<string> OnPlayerSpoke;
        public event Action<string> OnAIResponded;
        public event Action<string> OnCommandExecuted;
        public event Action<string> OnError;
        
        // State
        public bool IsInitialized { get; private set; }
        public bool IsListening { get; private set; }
        
        // Singleton
        public static VoiceSystemManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            try
            {
                LogDebug("Initializing Voice System Manager...");
                
                // Initialize components
                InitializeComponents();
                
                // Set up event handlers
                SetupEventHandlers();
                
                // Mark as initialized BEFORE starting to listen
                IsInitialized = true;
                LogDebug("Voice System Manager marked as initialized");
                
                // Start listening if auto-start is enabled
                if (autoStartListening)
                {
                    // Delay the start to ensure everything is ready
                    Invoke(nameof(DelayedStartListening), 0.5f);
                }
                
                LogDebug("Voice System Manager initialized successfully");
            }
            catch (Exception e)
            {
                LogError($"Initialization failed: {e.Message}");
                OnError?.Invoke($"Initialization failed: {e.Message}");
                // Even if there's an error, mark as initialized to prevent infinite loops
                IsInitialized = true;
            }
        }
        
        private void DelayedStartListening()
        {
            if (IsInitialized && autoStartListening)
            {
                StartListening();
            }
        }
        
        private void InitializeComponents()
        {
            // Initialize speech recognizer
            if (speechRecognizerComponent == null)
                speechRecognizerComponent = GetComponent<WindowsSpeechRecognizer>();
            
            if (speechRecognizerComponent != null)
            {
                speechRecognizer = speechRecognizerComponent;
                speechRecognizer.Initialize();
                LogDebug("Windows Speech recognizer initialized");
            }
            else
            {
                LogError("WindowsSpeechRecognizer component not found");
            }
            
            // Initialize text-to-speech
            if (textToSpeechComponent == null)
                textToSpeechComponent = GetComponent<WindowsTTSPlugin>();
            
            if (textToSpeechComponent != null)
            {
                textToSpeech = textToSpeechComponent;
                textToSpeech.Initialize();
                LogDebug("Windows TTS Plugin initialized - REAL AUDIO");
            }
            else
            {
                LogError("WindowsTTSPlugin component not found");
            }
            
            // Initialize AI assistant
            if (aiAssistant == null)
                aiAssistant = GetComponent<BasicAIAssistant>();
            
            if (aiAssistant != null)
            {
                aiAssistant.promptTemplates = promptTemplates;
                aiAssistant.commandLibrary = commandLibrary;
                aiAssistant.Initialize();
            }
            
            // Initialize context provider
            if (contextProvider == null)
                contextProvider = GetComponent<GameContextProvider>();
            
            // Initialize command executor
            if (commandExecutor == null)
                commandExecutor = GetComponent<AICommandExecutor>();
            
            if (commandExecutor != null)
            {
                commandExecutor.contextProvider = contextProvider;
            }
        }
        
        private void SetupEventHandlers()
        {
            // Speech recognition events
            if (speechRecognizer != null)
            {
                speechRecognizer.OnSpeechRecognized += OnSpeechRecognized;
                speechRecognizer.OnError += OnSpeechError;
                speechRecognizer.OnStatusChanged += OnSpeechStatusChanged;
            }
            
            // AI assistant events
            if (aiAssistant != null)
            {
                aiAssistant.OnResponseGenerated += OnAIResponseGenerated;
                aiAssistant.OnCommandExtracted += OnAICommandExtracted;
                aiAssistant.OnError += OnAIError;
            }
            
            // Text-to-speech events
            if (textToSpeech != null)
            {
                textToSpeech.OnSpeechStarted += OnTTSStarted;
                textToSpeech.OnSpeechCompleted += OnTTSCompleted;
                textToSpeech.OnError += OnTTSError;
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Start listening for voice input
        /// </summary>
        public void StartListening()
        {
            if (!IsInitialized)
            {
                LogError("Voice system not initialized");
                return;
            }
            
            if (speechRecognizer != null)
            {
                speechRecognizer.StartListening();
                IsListening = true;
                LogDebug("Started listening for voice input");
            }
            else
            {
                LogError("Speech recognizer not available");
            }
        }
        
        /// <summary>
        /// Stop listening for voice input
        /// </summary>
        public void StopListening()
        {
            if (speechRecognizer != null)
            {
                speechRecognizer.StopListening();
                IsListening = false;
                LogDebug("Stopped listening for voice input");
            }
        }
        
        /// <summary>
        /// Speak text to the player
        /// </summary>
        public void SpeakToPlayer(string text, VoiceSystem.Core.Interfaces.TTSPriority priority = VoiceSystem.Core.Interfaces.TTSPriority.Normal)
        {
            if (textToSpeech != null)
            {
                textToSpeech.Speak(text, priority);
                LogDebug($"Speaking: {text}");
            }
        }
        
        /// <summary>
        /// Update game context
        /// </summary>
        public void UpdateGameContext(GameContext context)
        {
            if (contextProvider != null)
            {
                contextProvider.UpdateContext(context);
                LogDebug($"Game context updated: {context.currentLocation}");
            }
        }
        
        /// <summary>
        /// Get current game context
        /// </summary>
        public GameContext GetCurrentContext()
        {
            if (contextProvider != null)
            {
                return contextProvider.GetCurrentContext();
            }
            return null;
        }
        
        /// <summary>
        /// Register a context provider
        /// </summary>
        public void RegisterContextProvider(IGameContextProvider provider)
        {
            if (provider is GameContextProvider gameProvider)
            {
                contextProvider = gameProvider;
                LogDebug("Context provider registered");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSpeechRecognized(string text)
        {
            LogDebug($"Speech recognized: {text}");
            OnPlayerSpoke?.Invoke(text);
            
            // Process with AI
            if (aiAssistant != null && contextProvider != null)
            {
                var context = contextProvider.GetCurrentContext();
                aiAssistant.ProcessInput(text, context);
            }
        }
        
        private void OnSpeechError(string error)
        {
            LogError($"Speech recognition error: {error}");
            OnError?.Invoke($"Speech error: {error}");
        }
        
        private void OnSpeechStatusChanged(bool isListening)
        {
            IsListening = isListening;
            LogDebug($"Speech recognition status: {(isListening ? "Listening" : "Stopped")}");
        }
        
        private void OnAIResponseGenerated(string response)
        {
            LogDebug($"AI response: {response}");
            OnAIResponded?.Invoke(response);
            
            // Speak the response
            SpeakToPlayer(response, VoiceSystem.Core.Interfaces.TTSPriority.Normal);
        }
        
        private void OnAICommandExtracted(string command)
        {
            LogDebug($"AI extracted command: {command}");
            OnCommandExecuted?.Invoke(command);
            
            // Execute the command
            if (commandExecutor != null)
            {
                commandExecutor.ExecuteCommand(command);
            }
        }
        
        private void OnAIError(string error)
        {
            LogError($"AI error: {error}");
            OnError?.Invoke($"AI error: {error}");
        }
        
        private void OnTTSStarted(string text)
        {
            LogDebug($"TTS started: {text}");
        }
        
        private void OnTTSCompleted(string text)
        {
            LogDebug($"TTS completed: {text}");
        }
        
        private void OnTTSError(string error)
        {
            LogError($"TTS error: {error}");
            OnError?.Invoke($"TTS error: {error}");
        }
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("Test Speech Recognition")]
        public void TestSpeechRecognition()
        {
            if (speechRecognizer != null)
            {
                // Simulate speech recognition
                OnSpeechRecognized("inspeccionar");
            }
        }
        
        [ContextMenu("Test TTS")]
        public void TestTTS()
        {
            SpeakToPlayer("Hola, soy tu asistente de voz. ¿En qué puedo ayudarte?", VoiceSystem.Core.Interfaces.TTSPriority.Normal);
        }
        
        [ContextMenu("Test AI Response")]
        public void TestAIResponse()
        {
            if (aiAssistant != null && contextProvider != null)
            {
                var context = contextProvider.GetCurrentContext();
                aiAssistant.ProcessInput("dónde estoy", context);
            }
        }
        
        [ContextMenu("Show System Status")]
        public void ShowSystemStatus()
        {
            LogDebug("=== Voice System Status ===");
            LogDebug($"Initialized: {IsInitialized}");
            LogDebug($"Listening: {IsListening}");
            LogDebug($"Speech Recognizer: {(speechRecognizer != null ? "OK" : "NULL")}");
            LogDebug($"Text-to-Speech: {(textToSpeech != null ? "OK" : "NULL")}");
            LogDebug($"AI Assistant: {(aiAssistant != null ? "OK" : "NULL")}");
            LogDebug($"Context Provider: {(contextProvider != null ? "OK" : "NULL")}");
            LogDebug($"Command Executor: {(commandExecutor != null ? "OK" : "NULL")}");
            
            if (contextProvider != null)
            {
                var context = contextProvider.GetCurrentContext();
                LogDebug($"Current Location: {context.currentLocation}");
                LogDebug($"Health: {context.health}/{context.maxHealth}");
                LogDebug($"Actions: {context.actions}/{context.maxActions}");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[VoiceSystem] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[VoiceSystem] {message}");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Cleanup
            if (speechRecognizer != null)
                speechRecognizer.Dispose();
            
            if (textToSpeech != null)
                textToSpeech.Dispose();
            
            if (aiAssistant != null)
                aiAssistant.Dispose();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Don't stop/start on pause to keep listening continuously
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // Don't stop/start on focus to keep listening continuously
        }
    }
}