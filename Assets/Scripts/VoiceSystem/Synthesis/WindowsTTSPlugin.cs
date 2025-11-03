using System;
using System.Runtime.InteropServices;
using UnityEngine;
using VoiceSystem.Core.Interfaces;
using VoiceSystem.GameIntegration;

namespace VoiceSystem.Synthesis
{
    /// <summary>
    /// Windows Text-to-Speech usando SAPI directamente via P/Invoke
    /// </summary>
    public class WindowsTTSPlugin : MonoBehaviour, ITextToSpeech
    {
        // Events
        public event Action<string> OnSpeechStarted;
        public event Action<string> OnSpeechCompleted;
        public event Action<string> OnError;
        
        // State
        public bool IsSpeaking { get; private set; }
        
        [Header("Configuration")]
        public string voice = "Microsoft Sabina Desktop"; // Voz en español
        public int rate = 0; // -10 to 10
        public int volume = 100; // 0 to 100
        
        [Header("Game Pause")]
        [SerializeField] private bool pauseGameDuringSpeech = true;
        [Tooltip("Reference to GamePauseManager (auto-assigned if null)")]
        [SerializeField] private GamePauseManager gamePauseManager;
        
        [Header("Speech Recognition Pause")]
        [SerializeField] private bool pauseRecognitionDuringSpeech = true;
        [Tooltip("Reference to WindowsSpeechRecognizer (auto-assigned if null)")]
        [SerializeField] private Recognition.WindowsSpeechRecognizer speechRecognizer;
        
        // P/Invoke declarations for Windows SAPI
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);
        
        private const uint SND_ASYNC = 0x0001;
        private const uint SND_MEMORY = 0x0004;
        
        private void Awake()
        {
            // Ensure main thread dispatcher exists
            UnityMainThreadDispatcher.Instance();
            
            // Get or create GamePauseManager if needed
            if (pauseGameDuringSpeech && gamePauseManager == null)
            {
                gamePauseManager = GamePauseManager.Instance;
            }
            
            // Get or create WindowsSpeechRecognizer if needed
            if (pauseRecognitionDuringSpeech && speechRecognizer == null)
            {
                speechRecognizer = FindFirstObjectByType<Recognition.WindowsSpeechRecognizer>();
            }
        }
        
        public void Initialize()
        {
            Debug.Log("[WindowsTTS] Plugin initialized - READY FOR REAL AUDIO");
            
            // Ensure GamePauseManager is available if pause is enabled
            if (pauseGameDuringSpeech && gamePauseManager == null)
            {
                gamePauseManager = GamePauseManager.Instance;
                Debug.Log("[WindowsTTS] GamePauseManager reference assigned");
            }
        }
        
        public void Speak(string text, TTSPriority priority = TTSPriority.Normal)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("[WindowsTTS] Empty text, skipping");
                return;
            }
            
            // Si ya está hablando, añadir a la cola
            if (IsSpeaking)
            {
                var queue = NarrationQueue.Instance;
                if (queue != null)
                {
                    Debug.Log($"[WindowsTTS] Ocupado → Añadiendo a cola: {text.Substring(0, Mathf.Min(50, text.Length))}...");
                    queue.Enqueue(text, priority);
                    return;
                }
                else
                {
                    Debug.LogWarning($"[WindowsTTS] Ya está hablando y no hay cola, ignorando: {text}");
                    return;
                }
            }
            
            // Debug.Log($"[WindowsTTS] Speaking: {text}"); // Comentado - demasiado verbose
            
            IsSpeaking = true;
            
            // Pause game if enabled
            if (pauseGameDuringSpeech && gamePauseManager != null)
            {
                gamePauseManager.PauseGame();
            }
            
            // NUEVO: Pausar reconocimiento de voz durante la narración
            if (pauseRecognitionDuringSpeech && speechRecognizer != null)
            {
                speechRecognizer.PauseRecognition();
            }
            
            OnSpeechStarted?.Invoke(text);
            
            // Usar PowerShell para sintetizar voz
            SpeakWithPowerShell(text);
        }
        
        private void SpeakWithPowerShell(string text)
        {
            try
            {
                // Escapar comillas en el texto
                string escapedText = text.Replace("\"", "`\"");
                
                // Crear comando PowerShell
                string command = $@"
Add-Type -AssemblyName System.Speech;
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer;
$synth.Volume = {volume};
$synth.Rate = {rate};
$synth.Speak('{escapedText}');
$synth.Dispose();
";
                
                // Ejecutar PowerShell
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                
                process.Start();
                
                // Esperar a que termine en un thread separado
                System.Threading.Tasks.Task.Run(() =>
                {
                    process.WaitForExit();
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        IsSpeaking = false;
                        
                        // Resume game if it was paused
                        if (pauseGameDuringSpeech && gamePauseManager != null)
                        {
                            gamePauseManager.ResumeGame();
                        }
                        
                        // NUEVO: Reanudar reconocimiento de voz
                        if (pauseRecognitionDuringSpeech && speechRecognizer != null)
                        {
                            speechRecognizer.ResumeRecognition();
                        }
                        
                        OnSpeechCompleted?.Invoke(text);
                        // Debug.Log($"[WindowsTTS] Speech completed: {text}"); // Comentado - spam
                    });
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[WindowsTTS] Error: {e.Message}");
                OnError?.Invoke(e.Message);
                IsSpeaking = false;
                
                // Resume game on error
                if (pauseGameDuringSpeech && gamePauseManager != null && gamePauseManager.IsGamePaused)
                {
                    gamePauseManager.ResumeGame();
                }
                
                // NUEVO: Reanudar reconocimiento en caso de error
                if (pauseRecognitionDuringSpeech && speechRecognizer != null)
                {
                    speechRecognizer.ResumeRecognition();
                }
            }
        }
        
        public void Stop()
        {
            // Matar procesos de PowerShell
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName("powershell");
                foreach (var process in processes)
                {
                    process.Kill();
                }
                IsSpeaking = false;
                
                // Resume game if it was paused
                if (pauseGameDuringSpeech && gamePauseManager != null && gamePauseManager.IsGamePaused)
                {
                    gamePauseManager.ResumeGame();
                }
                
                // NUEVO: Reanudar reconocimiento al detener
                if (pauseRecognitionDuringSpeech && speechRecognizer != null)
                {
                    speechRecognizer.ResumeRecognition();
                }
                
                Debug.Log("[WindowsTTS] Speech stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WindowsTTS] Error stopping: {e.Message}");
            }
        }
        
        public void SetVolume(float volume)
        {
            this.volume = (int)(volume * 100);
            Debug.Log($"[WindowsTTS] Volume set to {this.volume}");
        }
        
        public void SetRate(int rate)
        {
            this.rate = rate;
            Debug.Log($"[WindowsTTS] Rate set to {rate}");
        }
        
        public void SetVoice(string voiceName)
        {
            this.voice = voiceName;
            Debug.Log($"[WindowsTTS] Voice set to {voiceName}");
        }
        
        public string[] GetAvailableVoices()
        {
            return new string[]
            {
                "Microsoft Sabina Desktop", // Español (España)
                "Microsoft Helena Desktop", // Español (España)
                "Microsoft David Desktop", // Inglés (US)
                "Microsoft Zira Desktop"    // Inglés (US)
            };
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
    
    // Helper para ejecutar código en el hilo principal de Unity
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly System.Collections.Generic.Queue<Action> _executionQueue = new System.Collections.Generic.Queue<Action>();
        
        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                var go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
        
        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
        
        private void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}
