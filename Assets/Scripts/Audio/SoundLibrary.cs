using UnityEngine;
using System.Collections.Generic;

namespace Audio
{
    /// <summary>
    /// Categoría de audio para control de volumen independiente
    /// </summary>
    public enum AudioCategory
    {
        Music,      // Música de fondo
        Effects,    // Efectos de sonido (puertas, pasos, etc.)
        Screamers,  // Sonidos de screamers
        Voice       // Voz TTS (no usado por SoundManager)
    }
    
    /// <summary>
    /// Entrada individual de sonido en la biblioteca
    /// </summary>
    [System.Serializable]
    public class SoundEntry
    {
        [Tooltip("ID único para referenciar este sonido")]
        public string soundId;
        
        [Tooltip("Clip de audio (MP3/OGG/WAV)")]
        public AudioClip clip;
        
        [Tooltip("Categoría para control de volumen")]
        public AudioCategory category = AudioCategory.Effects;
        
        [Tooltip("Volumen por defecto (0-1)")]
        [Range(0f, 1f)]
        public float defaultVolume = 1f;
        
        [Tooltip("Descripción opcional")]
        public string description;
    }
    
    /// <summary>
    /// Biblioteca de sonidos del juego
    /// ScriptableObject que organiza todos los AudioClips con metadata
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library", order = 1)]
    public class SoundLibrary : ScriptableObject
    {
        [Header("Biblioteca de Sonidos")]
        [Tooltip("Todos los sonidos disponibles en el juego")]
        public List<SoundEntry> sounds = new List<SoundEntry>();
        
        [Header("Configuración por Defecto")]
        [Tooltip("Volumen inicial para música")]
        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.6f;
        
        [Tooltip("Volumen inicial para efectos")]
        [Range(0f, 1f)]
        public float defaultEffectsVolume = 0.8f;
        
        [Tooltip("Volumen inicial para screamers")]
        [Range(0f, 1f)]
        public float defaultScreamersVolume = 1f;
        
        #region Helper Methods
        
        /// <summary>
        /// Busca un sonido por su ID
        /// </summary>
        public SoundEntry GetSound(string soundId)
        {
            return sounds.Find(s => s.soundId == soundId);
        }
        
        /// <summary>
        /// Obtiene el AudioClip de un sonido
        /// </summary>
        public AudioClip GetClip(string soundId)
        {
            SoundEntry entry = GetSound(soundId);
            return entry?.clip;
        }
        
        /// <summary>
        /// Verifica si un sonido existe
        /// </summary>
        public bool HasSound(string soundId)
        {
            return sounds.Exists(s => s.soundId == soundId);
        }
        
        /// <summary>
        /// Obtiene todos los sonidos de una categoría
        /// </summary>
        public List<SoundEntry> GetSoundsByCategory(AudioCategory category)
        {
            return sounds.FindAll(s => s.category == category);
        }
        
        #endregion
        
        #region Editor Helper
        
#if UNITY_EDITOR
        /// <summary>
        /// Crea entradas de sonido de ejemplo (solo en editor)
        /// </summary>
        [ContextMenu("Create Sample Entries")]
        private void CreateSampleEntries()
        {
            sounds.Clear();
            
            // Música
            AddSampleSound("ambient_pre2am", AudioCategory.Music, 0.6f, "Ambiente pre-2 AM");
            AddSampleSound("ambient_post2am", AudioCategory.Music, 0.7f, "Ambiente post-2 AM");
            AddSampleSound("child_event", AudioCategory.Music, 0.5f, "Música evento del niño");
            
            // Efectos ambientales
            AddSampleSound("footsteps", AudioCategory.Effects, 0.5f, "Pasos del jugador");
            AddSampleSound("door_open", AudioCategory.Effects, 0.7f, "Puerta abriéndose");
            AddSampleSound("door_close", AudioCategory.Effects, 0.6f, "Puerta cerrándose");
            AddSampleSound("creak_01", AudioCategory.Effects, 0.4f, "Crujido 1");
            AddSampleSound("creak_02", AudioCategory.Effects, 0.4f, "Crujido 2");
            AddSampleSound("creak_03", AudioCategory.Effects, 0.4f, "Crujido 3");
            AddSampleSound("whisper_01", AudioCategory.Effects, 0.3f, "Susurro 1");
            AddSampleSound("whisper_02", AudioCategory.Effects, 0.3f, "Susurro 2");
            AddSampleSound("whisper_03", AudioCategory.Effects, 0.3f, "Susurro 3");
            
            // Screamers
            AddSampleSound("screamer_kitchen", AudioCategory.Screamers, 1f, "Screamer cocina");
            AddSampleSound("screamer_bathroom", AudioCategory.Screamers, 1f, "Screamer baño");
            AddSampleSound("screamer_bedroom", AudioCategory.Screamers, 1f, "Screamer habitación");
            AddSampleSound("screamer_dining", AudioCategory.Screamers, 1f, "Screamer comedor");
            AddSampleSound("screamer_library", AudioCategory.Screamers, 1f, "Screamer biblioteca");
            AddSampleSound("screamer_default", AudioCategory.Screamers, 1f, "Screamer genérico");
            
            // Campanadas
            AddSampleSound("bell_chime", AudioCategory.Effects, 0.8f, "Campanada de reloj");
            
            Debug.Log($"[SoundLibrary] ✅ {sounds.Count} entradas de ejemplo creadas. Asigna los AudioClips manualmente.");
        }
        
        private void AddSampleSound(string id, AudioCategory category, float volume, string desc)
        {
            sounds.Add(new SoundEntry
            {
                soundId = id,
                clip = null, // Se asigna manualmente
                category = category,
                defaultVolume = volume,
                description = desc
            });
        }
#endif
        
        #endregion
    }
}

