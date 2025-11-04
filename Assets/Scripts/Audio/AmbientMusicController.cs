using UnityEngine;
using System.Collections;

namespace Audio
{
    /// <summary>
    /// Controla la música ambiental y efectos aleatorios basándose en el estado del juego
    /// Se suscribe a eventos del GameTimer y PlayerStateManager
    /// </summary>
    public class AmbientMusicController : MonoBehaviour
    {
        private static AmbientMusicController instance;
        public static AmbientMusicController Instance => instance;
        
        [Header("IDs de Música")]
        [SerializeField] private string pre2AMMusicId = "ambient_pre2am";
        [SerializeField] private string post2AMMusicId = "ambient_post2am";
        [SerializeField] private string childEventMusicId = "child_event";
        
        [Header("Efectos Ambientales Pre-2 AM")]
        [SerializeField] private string[] preEventAmbientEffects = new string[]
        {
            "creak_01", "creak_02", "creak_03"
        };
        
        [Header("Efectos Ambientales Post-2 AM")]
        [SerializeField] private string[] postEventAmbientEffects = new string[]
        {
            "creak_01", "creak_02", "creak_03",
            "whisper_01", "whisper_02", "whisper_03"
        };
        
        [Header("Configuración de Efectos Aleatorios")]
        [SerializeField] private float effectDelayMin = 5f;
        [SerializeField] private float effectDelayMax = 15f;
        
        [Header("Referencias")]
        private SoundManager soundManager;
        private GameTimer gameTimer;
        private PlayerStateManager playerState;
        
        [Header("Estado")]
        [SerializeField] private MusicPhase currentPhase = MusicPhase.Pre2AM;
        [SerializeField] private bool enableDebugLogs = true;
        
        private enum MusicPhase
        {
            Pre2AM,         // Antes de las 2 AM
            Post2AM,        // Después de las 2 AM
            ChildEvent      // Durante evento del niño
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Obtener referencias
            soundManager = SoundManager.Instance;
            gameTimer = GameTimer.Instance;
            playerState = PlayerStateManager.Instance;
            
            if (soundManager == null)
            {
                Debug.LogError("[AmbientMusic] ❌ SoundManager no encontrado!");
                enabled = false; // Deshabilitar componente
                return;
            }
            
            // Suscribirse a eventos
            if (gameTimer != null)
            {
                gameTimer.OnHourPassed += OnHourChanged;
                LogDebug("[AmbientMusic] ✅ Suscrito a GameTimer.OnHourPassed");
            }
            
            // Iniciar música pre-2 AM
            StartCoroutine(InitialMusicDelay());
        }
        
        private void OnDestroy()
        {
            if (gameTimer != null)
            {
                gameTimer.OnHourPassed -= OnHourChanged;
            }
        }
        
        /// <summary>
        /// Pequeño delay antes de iniciar la música para evitar conflictos con narración inicial
        /// </summary>
        private IEnumerator InitialMusicDelay()
        {
            yield return new WaitForSeconds(3f);
            
            // Verificar que soundManager siga disponible
            if (soundManager == null)
            {
                Debug.LogError("[AmbientMusic] ❌ SoundManager no disponible en InitialMusicDelay");
                yield break;
            }
            
            LogDebug("[AmbientMusic] 🎵 Iniciando música ambiente pre-2 AM");
            StartPhase(MusicPhase.Pre2AM);
        }
        
        /// <summary>
        /// Llamado cuando pasa una hora del juego
        /// </summary>
        private void OnHourChanged(int currentHour)
        {
            LogDebug($"[AmbientMusic] 🕐 Hora cambiada a: {currentHour}");
            
            // Si es las 2 AM y no hemos cambiado de fase, hacerlo
            if (currentHour == 2 && currentPhase == MusicPhase.Pre2AM)
            {
                // Verificar si el evento del niño ya ocurrió
                if (playerState != null && playerState.HasSeenEvent("child_event_triggered"))
                {
                    StartPhase(MusicPhase.Post2AM);
                }
            }
            
            // Verificar si el evento del niño se activó (independiente de la hora)
            CheckForChildEvent();
        }
        
        /// <summary>
        /// Verifica si el evento del niño se activó y cambia la música
        /// </summary>
        public void CheckForChildEvent()
        {
            if (playerState == null) return;
            
            // Si el evento del niño se activó y aún no estamos en esa fase
            if (playerState.HasSeenEvent("child_event_triggered") && currentPhase != MusicPhase.Post2AM)
            {
                LogDebug("[AmbientMusic] 👶 Evento del niño detectado - Cambiando a música post-2 AM");
                StartPhase(MusicPhase.Post2AM);
            }
        }
        
        /// <summary>
        /// Inicia una fase musical específica
        /// </summary>
        private void StartPhase(MusicPhase phase)
        {
            if (currentPhase == phase) return;
            
            currentPhase = phase;
            
            switch (phase)
            {
                case MusicPhase.Pre2AM:
                    soundManager.PlayBackgroundMusic(pre2AMMusicId, loop: true, fadeTime: 2f);
                    soundManager.StartAmbientEffects(preEventAmbientEffects, effectDelayMin, effectDelayMax);
                    LogDebug("[AmbientMusic] 🎵 Fase: Pre-2 AM");
                    break;
                    
                case MusicPhase.Post2AM:
                    soundManager.PlayBackgroundMusic(post2AMMusicId, loop: true, fadeTime: 3f);
                    soundManager.StartAmbientEffects(postEventAmbientEffects, effectDelayMin * 0.7f, effectDelayMax * 0.7f);
                    LogDebug("[AmbientMusic] 🎵 Fase: Post-2 AM (más intenso)");
                    break;
                    
                case MusicPhase.ChildEvent:
                    soundManager.PlayBackgroundMusic(childEventMusicId, loop: true, fadeTime: 2f);
                    soundManager.StopAmbientEffects();
                    LogDebug("[AmbientMusic] 🎵 Fase: Evento del Niño");
                    break;
            }
        }
        
        /// <summary>
        /// Cambia temporalmente a la música del evento del niño (llamado por BasementDoorDialogue)
        /// </summary>
        public void StartChildEventMusic()
        {
            LogDebug("[AmbientMusic] 👶 Iniciando música del evento del niño");
            StartPhase(MusicPhase.ChildEvent);
        }
        
        /// <summary>
        /// Regresa a la música post-evento después del diálogo del niño
        /// </summary>
        public void EndChildEventMusic()
        {
            LogDebug("[AmbientMusic] 👶 Finalizando música del evento del niño");
            StartPhase(MusicPhase.Post2AM);
        }
        
        /// <summary>
        /// Fuerza un cambio de fase (para debugging o eventos especiales)
        /// </summary>
        public void ForcePhase(string phaseName)
        {
            switch (phaseName.ToLower())
            {
                case "pre2am":
                    StartPhase(MusicPhase.Pre2AM);
                    break;
                case "post2am":
                    StartPhase(MusicPhase.Post2AM);
                    break;
                case "childevent":
                    StartPhase(MusicPhase.ChildEvent);
                    break;
                default:
                    Debug.LogWarning($"[AmbientMusic] ⚠️ Fase desconocida: {phaseName}");
                    break;
            }
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
        
        #region Public Queries
        
        /// <summary>
        /// Obtiene la fase musical actual
        /// </summary>
        public string GetCurrentPhase()
        {
            return currentPhase.ToString();
        }
        
        /// <summary>
        /// Verifica si estamos en fase post-2 AM
        /// </summary>
        public bool IsPost2AM()
        {
            return currentPhase == MusicPhase.Post2AM || currentPhase == MusicPhase.ChildEvent;
        }
        
        #endregion
    }
}

