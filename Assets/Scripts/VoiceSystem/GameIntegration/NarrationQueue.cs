using System.Collections.Generic;
using UnityEngine;
using VoiceSystem.Core.Interfaces;
using VoiceSystem.Synthesis;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Sistema de cola de narraciones que gestiona múltiples mensajes TTS
    /// Evita que se pierdan narraciones cuando el TTS está ocupado
    /// </summary>
    public class NarrationQueue : MonoBehaviour
    {
        private static NarrationQueue _instance;
        public static NarrationQueue Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NarrationQueue>();
                    if (_instance == null)
                    {
                        var go = new GameObject("NarrationQueue");
                        _instance = go.AddComponent<NarrationQueue>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [System.Serializable]
        public class NarrationItem
        {
            public string text;
            public TTSPriority priority;
            public float timestamp;

            public NarrationItem(string text, TTSPriority priority)
            {
                this.text = text;
                this.priority = priority;
                this.timestamp = Time.time;
            }
        }

        private Queue<NarrationItem> narrationQueue = new Queue<NarrationItem>();
        private bool isNarrating = false;
        private WindowsTTSPlugin ttsPlugin;
        private GamePauseManager pauseManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false; // Deshabilitado por defecto
        [SerializeField] private int queueCount = 0;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LogDebug("[NarrationQueue] Singleton inicializado");
        }

        private void Start()
        {
            // Buscar WindowsTTSPlugin
            ttsPlugin = FindFirstObjectByType<WindowsTTSPlugin>();
            if (ttsPlugin == null)
            {
                Debug.LogError("[NarrationQueue] WindowsTTSPlugin no encontrado!");
            }
            else
            {
                LogDebug("[NarrationQueue] WindowsTTSPlugin conectado");
            }

            // Buscar GamePauseManager
            pauseManager = GamePauseManager.Instance;
            if (pauseManager == null)
            {
                Debug.LogWarning("[NarrationQueue] GamePauseManager no encontrado!");
            }
            else
            {
                LogDebug("[NarrationQueue] GamePauseManager conectado");
            }
        }

        private void Update()
        {
            queueCount = narrationQueue.Count;

            // Procesar siguiente narración si terminó la anterior
            if (!isNarrating && narrationQueue.Count > 0)
            {
                ProcessNextNarration();
            }
        }

        /// <summary>
        /// Añade una narración a la cola
        /// </summary>
        public void Enqueue(string text, TTSPriority priority = TTSPriority.Normal)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("[NarrationQueue] Texto vacío ignorado");
                return;
            }

            var item = new NarrationItem(text, priority);
            narrationQueue.Enqueue(item);
            
            LogDebug($"[NarrationQueue] ➕ Añadido ({priority}): '{TruncateText(text)}' | Cola: {narrationQueue.Count}");

            // Si no está narrando, procesar inmediatamente
            if (!isNarrating)
            {
                ProcessNextNarration();
            }
        }

        private void ProcessNextNarration()
        {
            if (ttsPlugin == null)
            {
                Debug.LogError("[NarrationQueue] TTS Plugin no disponible!");
                return;
            }

            if (narrationQueue.Count == 0)
            {
                return;
            }

            // Verificar que el TTS esté realmente disponible
            if (ttsPlugin.IsSpeaking)
            {
                LogDebug("[NarrationQueue] TTS aún hablando, esperando...");
                return;
            }

            var item = narrationQueue.Dequeue();
            isNarrating = true;

            LogDebug($"[NarrationQueue] ▶️ Narrando ({item.priority}): '{TruncateText(item.text)}' | Quedan: {narrationQueue.Count}");

            // Iniciar narración
            StartCoroutine(NarrateWithCallback(item));
        }

        private System.Collections.IEnumerator NarrateWithCallback(NarrationItem item)
        {
            // Esperar a que TTS esté completamente libre (usando unscaledDeltaTime para funcionar durante pausas)
            while (ttsPlugin != null && ttsPlugin.IsSpeaking)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // Pequeño delay para asegurar que completó
            yield return new WaitForSecondsRealtime(0.1f);

            // Hablar (WindowsTTSPlugin.Speak manejará el pauseManager automáticamente)
            if (ttsPlugin != null)
            {
                ttsPlugin.Speak(item.text, item.priority);
            }

            // Esperar a que inicie (usando realtime para evitar bloqueos)
            yield return new WaitForSecondsRealtime(0.3f);
            
            // Esperar a que termine
            while (ttsPlugin != null && ttsPlugin.IsSpeaking)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // Delay adicional antes de continuar
            yield return new WaitForSecondsRealtime(0.2f);

            // Marcar como finalizado
            isNarrating = false;
            LogDebug($"[NarrationQueue] ✅ Narración completada | Quedan: {narrationQueue.Count}");

            // Procesar siguiente en el próximo frame
            if (narrationQueue.Count > 0)
            {
                // No llamar directamente, dejar que Update lo haga
                LogDebug("[NarrationQueue] Preparando siguiente narración...");
            }
        }

        /// <summary>
        /// Limpia toda la cola de narraciones
        /// </summary>
        public void Clear()
        {
            narrationQueue.Clear();
            isNarrating = false;
            LogDebug("[NarrationQueue] Cola limpiada");
        }

        /// <summary>
        /// Obtiene el número de narraciones pendientes
        /// </summary>
        public int PendingCount => narrationQueue.Count;

        public bool IsNarrating => isNarrating || (ttsPlugin != null && ttsPlugin.IsSpeaking);

        private string TruncateText(string text, int maxLength = 50)
        {
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength) + "...";
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}

