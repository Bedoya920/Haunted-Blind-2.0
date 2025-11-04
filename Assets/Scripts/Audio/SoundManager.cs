using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Audio
{
    /// <summary>
    /// Gestiona todo el audio del juego: música, efectos, screamers
    /// Singleton con sistema de fade, pools de AudioSource, y control de volumen por categoría
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager instance;
        public static SoundManager Instance => instance;
        
        [Header("Referencias")]
        [SerializeField] private SoundLibrary soundLibrary;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource screamerSource;
        
        [Header("Volumen por Categoría")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float effectsVolume = 0.8f;
        [SerializeField] [Range(0f, 1f)] private float screamersVolume = 1f;
        
        [Header("Configuración de Fade")]
        [SerializeField] private float defaultFadeTime = 2f;
        [SerializeField] private float screamerFadeOutTime = 1f;
        [SerializeField] private float screamerFadeInTime = 1.5f;
        
        [Header("Estado")]
        [SerializeField] private bool isMuted = false;
        [SerializeField] private bool enableDebugLogs = true;
        
        // Estado interno
        private Coroutine currentMusicFadeCoroutine;
        private Coroutine currentScreamerCoroutine;
        private Coroutine ambientEffectsCoroutine;
        private string currentMusicId = "";
        private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
        private const float minTimeBetweenSameSounds = 0.1f;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            LoadSoundLibrary();
        }
        
        /// <summary>
        /// Inicializa los AudioSource si no existen
        /// </summary>
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = false;
                ambientSource.playOnAwake = false;
            }
            
            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
                effectsSource.loop = false;
                effectsSource.playOnAwake = false;
            }
            
            if (screamerSource == null)
            {
                screamerSource = gameObject.AddComponent<AudioSource>();
                screamerSource.loop = false;
                screamerSource.playOnAwake = false;
            }
            
            UpdateAllVolumes();
            LogDebug("[SoundManager] AudioSources inicializados");
        }
        
        /// <summary>
        /// Carga el SoundLibrary desde Resources
        /// </summary>
        private void LoadSoundLibrary()
        {
            if (soundLibrary == null)
            {
                soundLibrary = Resources.Load<SoundLibrary>("Audio/SoundLibrary");
                
                if (soundLibrary == null)
                {
                    Debug.LogWarning("[SoundManager] ⚠️ SoundLibrary.asset no encontrado en Resources/Audio/. Creando uno vacío en memoria.");
                    soundLibrary = ScriptableObject.CreateInstance<SoundLibrary>();
                }
                else
                {
                    LogDebug($"[SoundManager] ✅ SoundLibrary cargado con {soundLibrary.sounds.Count} sonidos");
                }
            }
        }
        
        #region Background Music
        
        /// <summary>
        /// Reproduce música de fondo con fade in
        /// </summary>
        public void PlayBackgroundMusic(string musicId, bool loop = true, float fadeTime = -1f)
        {
            if (fadeTime < 0) fadeTime = defaultFadeTime;
            
            // Si ya está sonando esta música, no hacer nada
            if (currentMusicId == musicId && musicSource.isPlaying)
            {
                LogDebug($"[SoundManager] Música '{musicId}' ya está sonando");
                return;
            }
            
            AudioClip clip = GetClip(musicId);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] ⚠️ Música '{musicId}' no encontrada");
                return;
            }
            
            LogDebug($"[SoundManager] 🎵 Reproduciendo música: {musicId} (fade: {fadeTime}s)");
            
            // Cancelar fade anterior si existe
            if (currentMusicFadeCoroutine != null)
            {
                StopCoroutine(currentMusicFadeCoroutine);
                currentMusicFadeCoroutine = null;
            }
            
            // Fade out música anterior si existe
            if (musicSource.isPlaying)
            {
                currentMusicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip, loop, fadeTime));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = 0f;
                musicSource.Play();
                currentMusicFadeCoroutine = StartCoroutine(FadeInCoroutine(musicSource, musicVolume * masterVolume, fadeTime));
            }
            
            currentMusicId = musicId;
        }
        
        /// <summary>
        /// Crossfade entre dos pistas musicales
        /// </summary>
        private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop, float fadeTime)
        {
            float halfFadeTime = fadeTime / 2f;
            
            // Fade out música actual
            yield return StartCoroutine(FadeOutCoroutine(musicSource, halfFadeTime, null));
            
            // Cambiar clip y fade in
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.volume = 0f;
            musicSource.Play();
            
            yield return StartCoroutine(FadeInCoroutine(musicSource, musicVolume * masterVolume, halfFadeTime));
            
            // Limpiar referencia al terminar
            currentMusicFadeCoroutine = null;
        }
        
        /// <summary>
        /// Detiene la música de fondo con fade out
        /// </summary>
        public void StopBackgroundMusic(float fadeTime = -1f)
        {
            if (fadeTime < 0) fadeTime = defaultFadeTime;
            
            // Cancelar fade anterior
            if (currentMusicFadeCoroutine != null)
            {
                StopCoroutine(currentMusicFadeCoroutine);
                currentMusicFadeCoroutine = null;
            }
            
            if (musicSource.isPlaying)
            {
                LogDebug($"[SoundManager] 🛑 Deteniendo música (fade: {fadeTime}s)");
                currentMusicFadeCoroutine = StartCoroutine(FadeOutCoroutine(musicSource, fadeTime, () => {
                    musicSource.Stop();
                    currentMusicId = "";
                    currentMusicFadeCoroutine = null;
                }));
            }
        }
        
        #endregion
        
        #region Ambient Effects
        
        /// <summary>
        /// Inicia la reproducción de efectos ambientales aleatorios
        /// </summary>
        public void StartAmbientEffects(string[] effectIds, float delayMin = 5f, float delayMax = 15f)
        {
            if (ambientEffectsCoroutine != null)
            {
                StopCoroutine(ambientEffectsCoroutine);
            }
            
            ambientEffectsCoroutine = StartCoroutine(PlayRandomAmbientEffects(effectIds, delayMin, delayMax));
            LogDebug($"[SoundManager] 🌊 Efectos ambientales iniciados ({effectIds.Length} sonidos)");
        }
        
        /// <summary>
        /// Detiene los efectos ambientales
        /// </summary>
        public void StopAmbientEffects()
        {
            if (ambientEffectsCoroutine != null)
            {
                StopCoroutine(ambientEffectsCoroutine);
                ambientEffectsCoroutine = null;
                ambientSource.Stop();
                LogDebug("[SoundManager] 🛑 Efectos ambientales detenidos");
            }
        }
        
        /// <summary>
        /// Coroutine que reproduce efectos ambientales aleatorios
        /// </summary>
        private IEnumerator PlayRandomAmbientEffects(string[] effectIds, float delayMin, float delayMax)
        {
            while (true)
            {
                float delay = Random.Range(delayMin, delayMax);
                yield return new WaitForSeconds(delay);
                
                if (effectIds.Length > 0)
                {
                    string randomEffectId = effectIds[Random.Range(0, effectIds.Length)];
                    PlayAmbientEffect(randomEffectId);
                }
            }
        }
        
        /// <summary>
        /// Reproduce un efecto ambiental específico
        /// </summary>
        public void PlayAmbientEffect(string effectId)
        {
            AudioClip clip = GetClip(effectId);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] ⚠️ Efecto ambiental '{effectId}' no encontrado");
                return;
            }
            
            SoundEntry entry = GetSoundEntry(effectId);
            float volume = entry != null ? entry.defaultVolume : 1f;
            
            ambientSource.PlayOneShot(clip, volume * effectsVolume * masterVolume);
            LogDebug($"[SoundManager] 🌫️ Efecto ambiental: {effectId}");
        }
        
        #endregion
        
        #region Screamers
        
        /// <summary>
        /// Reproduce un screamer con fade out de música, sonido, y fade in de música
        /// </summary>
        public void PlayScreamer(string screamerId, System.Action onComplete = null)
        {
            AudioClip clip = GetClip(screamerId);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] ⚠️ Screamer '{screamerId}' no encontrado");
                onComplete?.Invoke();
                return;
            }
            
            LogDebug($"[SoundManager] 👻 SCREAMER: {screamerId}");
            
            // Cancelar screamer anterior si existe
            if (currentScreamerCoroutine != null)
            {
                StopCoroutine(currentScreamerCoroutine);
            }
            
            currentScreamerCoroutine = StartCoroutine(PlayScreamerSequence(clip, onComplete));
        }
        
        /// <summary>
        /// Secuencia completa de screamer: fade out → sonido → fade in
        /// </summary>
        private IEnumerator PlayScreamerSequence(AudioClip screamerClip, System.Action onComplete)
        {
            // 1. Fade out música de fondo
            if (musicSource.isPlaying)
            {
                yield return StartCoroutine(FadeOutCoroutine(musicSource, screamerFadeOutTime, null));
            }
            
            // 2. Reproducir screamer
            screamerSource.PlayOneShot(screamerClip, screamersVolume * masterVolume);
            
            // 3. Esperar a que termine el screamer
            yield return new WaitForSeconds(screamerClip.length);
            
            // 4. Callback (narración de texto)
            onComplete?.Invoke();
            
            // 5. Pequeña pausa antes de volver la música
            yield return new WaitForSeconds(0.5f);
            
            // 6. Fade in música de fondo (usar volumen objetivo, no el anterior)
            if (!musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.Play();
            }
            
            if (musicSource.isPlaying)
            {
                float targetVolume = musicVolume * masterVolume;
                yield return StartCoroutine(FadeInCoroutine(musicSource, targetVolume, screamerFadeInTime));
            }
            
            // Limpiar referencia
            currentScreamerCoroutine = null;
        }
        
        #endregion
        
        #region One-Shot Effects
        
        /// <summary>
        /// Reproduce un efecto de sonido de una sola vez
        /// Con cooldown para evitar superposición
        /// </summary>
        public void PlayOneShot(string soundId, float volumeScale = 1f)
        {
            // Verificar cooldown
            if (lastPlayTime.ContainsKey(soundId) && 
                Time.time - lastPlayTime[soundId] < minTimeBetweenSameSounds)
            {
                LogDebug($"[SoundManager] ⏸️ Cooldown activo para: {soundId}");
                return;
            }
            
            AudioClip clip = GetClip(soundId);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] ⚠️ Sonido '{soundId}' no encontrado");
                return;
            }
            
            SoundEntry entry = GetSoundEntry(soundId);
            float volume = entry != null ? entry.defaultVolume : 1f;
            
            effectsSource.PlayOneShot(clip, volume * volumeScale * effectsVolume * masterVolume);
            lastPlayTime[soundId] = Time.time;
            
            LogDebug($"[SoundManager] 🔊 OneShot: {soundId} (vol: {volumeScale:F2})");
        }
        
        #endregion
        
        #region Fade System
        
        /// <summary>
        /// Fade out de un AudioSource
        /// </summary>
        public IEnumerator FadeOutCoroutine(AudioSource source, float duration, System.Action onComplete)
        {
            float startVolume = source.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            
            source.volume = 0f;
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Fade in de un AudioSource
        /// </summary>
        public IEnumerator FadeInCoroutine(AudioSource source, float targetVolume, float duration)
        {
            float elapsed = 0f;
            source.volume = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        #endregion
        
        #region Volume Control
        
        /// <summary>
        /// Ajusta el volumen maestro
        /// </summary>
        public void AdjustMasterVolume(float delta)
        {
            masterVolume = Mathf.Clamp01(masterVolume + delta);
            UpdateAllVolumes();
            LogDebug($"[SoundManager] 🔊 Volumen maestro: {masterVolume:F2} ({Mathf.RoundToInt(masterVolume * 100)}%)");
        }
        
        /// <summary>
        /// Establece el volumen maestro
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }
        
        /// <summary>
        /// Obtiene el volumen maestro actual
        /// </summary>
        public float GetMasterVolume()
        {
            return masterVolume;
        }
        
        /// <summary>
        /// Establece el volumen de una categoría específica
        /// </summary>
        public void SetVolume(AudioCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            switch (category)
            {
                case AudioCategory.Music:
                    musicVolume = volume;
                    break;
                case AudioCategory.Effects:
                    effectsVolume = volume;
                    break;
                case AudioCategory.Screamers:
                    screamersVolume = volume;
                    break;
            }
            
            UpdateAllVolumes();
            LogDebug($"[SoundManager] 🔊 {category}: {volume:F2}");
        }
        
        /// <summary>
        /// Silencia o activa todo el audio
        /// </summary>
        public void SetMute(bool mute)
        {
            if (isMuted == mute) return;
            
            isMuted = mute;
            
            // Aplicar/remover mute directamente en los AudioSources
            // No modificamos musicVolume/effectsVolume/screamersVolume
            // para preservar los ajustes del usuario
            
            if (musicSource != null && musicSource.isPlaying)
            {
                if (mute)
                {
                    musicSource.volume = 0f;
                }
                else
                {
                    UpdateAllVolumes(); // Restaura con los valores actuales
                }
            }
            
            LogDebug(mute ? "[SoundManager] 🔇 Audio silenciado" : "[SoundManager] 🔊 Audio activado");
        }
        
        /// <summary>
        /// Actualiza los volúmenes de todos los AudioSource
        /// </summary>
        private void UpdateAllVolumes()
        {
            // Solo actualizar volumen de música si no hay fade en progreso
            if (musicSource != null && musicSource.isPlaying && currentMusicFadeCoroutine == null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
            
            // effectsSource y screamerSource usan volumeScale en PlayOneShot
            // No necesitan actualización constante
            
            LogDebug($"[SoundManager] Volúmenes actualizados: Music={musicVolume:F2}, Effects={effectsVolume:F2}, Screamers={screamersVolume:F2}, Master={masterVolume:F2}");
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Obtiene un AudioClip del SoundLibrary
        /// </summary>
        private AudioClip GetClip(string soundId)
        {
            if (soundLibrary == null) return null;
            
            SoundEntry entry = soundLibrary.sounds.Find(s => s.soundId == soundId);
            return entry?.clip;
        }
        
        /// <summary>
        /// Obtiene una entrada completa del SoundLibrary
        /// </summary>
        private SoundEntry GetSoundEntry(string soundId)
        {
            if (soundLibrary == null) return null;
            return soundLibrary.sounds.Find(s => s.soundId == soundId);
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log(message);
            }
        }
        
        #endregion
        
        #region Public Queries
        
        /// <summary>
        /// Verifica si hay música sonando actualmente
        /// </summary>
        public bool IsMusicPlaying()
        {
            return musicSource != null && musicSource.isPlaying;
        }
        
        /// <summary>
        /// Obtiene el ID de la música actual
        /// </summary>
        public string GetCurrentMusicId()
        {
            return currentMusicId;
        }
        
        #endregion
    }
}

