using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VoiceSystem.Core;
using VoiceSystem.Core.Data;
using System.Collections.Generic;

namespace VoiceSystem.UI
{
    /// <summary>
    /// Debug UI for testing the Voice System
    /// </summary>
    public class VoiceSystemDebugUI : MonoBehaviour
    {
        [Header("References")]
        public VoiceSystemManager voiceSystemManager;
        
        [Header("UI Elements")]
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI transcriptionText;
        public TextMeshProUGUI aiResponseText;
        public TextMeshProUGUI contextText;
        public TextMeshProUGUI conversationLogText;
        public Toggle listeningToggle;
        public Button testTTSButton;
        public Button testAIButton;
        public Button clearLogButton;
        public Image microphoneIndicator;
        
        [Header("Settings")]
        public Color activeColor = Color.green;
        public Color inactiveColor = Color.red;
        public int maxLogEntries = 20;
        
        private Queue<string> conversationLog = new Queue<string>();
        
        private void Start()
        {
            if (voiceSystemManager == null)
                voiceSystemManager = FindFirstObjectByType<VoiceSystemManager>();
            
            SetupUICallbacks();
            SetupVoiceSystemCallbacks();
            UpdateUI();
        }
        
        private void SetupUICallbacks()
        {
            if (listeningToggle != null)
            {
                listeningToggle.onValueChanged.AddListener(OnListeningToggleChanged);
            }
            
            if (testTTSButton != null)
            {
                testTTSButton.onClick.AddListener(OnTestTTSClicked);
            }
            
            if (testAIButton != null)
            {
                testAIButton.onClick.AddListener(OnTestAIClicked);
            }
            
            if (clearLogButton != null)
            {
                clearLogButton.onClick.AddListener(OnClearLogClicked);
            }
        }
        
        private void SetupVoiceSystemCallbacks()
        {
            if (voiceSystemManager == null)
                return;
            
            voiceSystemManager.OnPlayerSpoke += OnPlayerSpoke;
            voiceSystemManager.OnAIResponded += OnAIResponded;
            voiceSystemManager.OnCommandExecuted += OnCommandExecuted;
            voiceSystemManager.OnError += OnError;
        }
        
        private void Update()
        {
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (voiceSystemManager == null)
                return;
            
            // Update status
            if (statusText != null)
            {
                statusText.text = $"<b>Estado del Sistema</b>\n" +
                    $"Inicializado: {(voiceSystemManager.IsInitialized ? "✓" : "✗")}\n" +
                    $"Escuchando: {(voiceSystemManager.IsListening ? "✓" : "✗")}";
            }
            
            // Update microphone indicator
            if (microphoneIndicator != null)
            {
                microphoneIndicator.color = voiceSystemManager.IsListening ? activeColor : inactiveColor;
            }
            
            // Update listening toggle
            if (listeningToggle != null && listeningToggle.isOn != voiceSystemManager.IsListening)
            {
                listeningToggle.SetIsOnWithoutNotify(voiceSystemManager.IsListening);
            }
            
            // Update context
            UpdateContextDisplay();
        }
        
        private void UpdateContextDisplay()
        {
            if (contextText == null || voiceSystemManager == null)
                return;
            
            var context = voiceSystemManager.GetCurrentContext();
            if (context == null)
                return;
            
            contextText.text = $"<b>Contexto del Juego</b>\n" +
                $"Ubicación: {context.currentLocation}\n" +
                $"Vida: {context.health}/{context.maxHealth}\n" +
                $"Fatiga: {context.fatigue}\n" +
                $"Acciones: {context.actions}/{context.maxActions}\n" +
                $"Tiempo: {context.gameTime}\n" +
                $"Inventario: {(context.inventory.Count > 0 ? string.Join(", ", context.inventory) : "vacío")}\n" +
                $"Objetos cercanos: {(context.nearbyObjects.Count > 0 ? string.Join(", ", context.nearbyObjects) : "ninguno")}";
        }
        
        private void AddToConversationLog(string entry)
        {
            conversationLog.Enqueue($"[{System.DateTime.Now:HH:mm:ss}] {entry}");
            
            if (conversationLog.Count > maxLogEntries)
            {
                conversationLog.Dequeue();
            }
            
            UpdateConversationLogDisplay();
        }
        
        private void UpdateConversationLogDisplay()
        {
            if (conversationLogText == null)
                return;
            
            conversationLogText.text = "<b>Registro de Conversación</b>\n" + string.Join("\n", conversationLog);
        }
        
        #region Voice System Callbacks
        
        private void OnPlayerSpoke(string text)
        {
            if (transcriptionText != null)
            {
                transcriptionText.text = $"<b>Usuario:</b> {text}";
            }
            AddToConversationLog($"<color=cyan>Usuario:</color> {text}");
        }
        
        private void OnAIResponded(string response)
        {
            if (aiResponseText != null)
            {
                aiResponseText.text = $"<b>IA:</b> {response}";
            }
            AddToConversationLog($"<color=yellow>IA:</color> {response}");
        }
        
        private void OnCommandExecuted(string command)
        {
            AddToConversationLog($"<color=green>Comando ejecutado:</color> {command}");
        }
        
        private void OnError(string error)
        {
            AddToConversationLog($"<color=red>Error:</color> {error}");
        }
        
        #endregion
        
        #region UI Callbacks
        
        private void OnListeningToggleChanged(bool isOn)
        {
            if (voiceSystemManager == null)
                return;
            
            if (isOn)
            {
                voiceSystemManager.StartListening();
                AddToConversationLog("Sistema de voz activado");
            }
            else
            {
                voiceSystemManager.StopListening();
                AddToConversationLog("Sistema de voz desactivado");
            }
        }
        
        private void OnTestTTSClicked()
        {
            if (voiceSystemManager != null)
            {
                voiceSystemManager.SpeakToPlayer("Hola, este es un test del sistema de texto a voz. ¿Me escuchas bien?");
                AddToConversationLog("Test de TTS ejecutado");
            }
        }
        
        private void OnTestAIClicked()
        {
            if (voiceSystemManager != null)
            {
                // Simulate user input
                OnPlayerSpoke("dónde estoy");
                voiceSystemManager.TestAIResponse();
                AddToConversationLog("Test de IA ejecutado");
            }
        }
        
        private void OnClearLogClicked()
        {
            conversationLog.Clear();
            UpdateConversationLogDisplay();
            
            if (transcriptionText != null)
                transcriptionText.text = "";
            
            if (aiResponseText != null)
                aiResponseText.text = "";
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (voiceSystemManager != null)
            {
                voiceSystemManager.OnPlayerSpoke -= OnPlayerSpoke;
                voiceSystemManager.OnAIResponded -= OnAIResponded;
                voiceSystemManager.OnCommandExecuted -= OnCommandExecuted;
                voiceSystemManager.OnError -= OnError;
            }
        }
    }
}