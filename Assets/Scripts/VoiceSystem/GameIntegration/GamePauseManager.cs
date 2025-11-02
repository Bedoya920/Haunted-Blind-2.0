using System;
using UnityEngine;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Manages game pause state during narrator speech
    /// Pauses everything: time, enemies, all game systems
    /// </summary>
    public class GamePauseManager : MonoBehaviour
    {
        // Singleton instance
        private static GamePauseManager _instance;
        public static GamePauseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GamePauseManager");
                    _instance = go.AddComponent<GamePauseManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<bool> OnGamePausedChanged;

        // State
        public bool IsGamePaused { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableDebugLogs = true;

        // Stored state for restoration
        private float previousTimeScale = 1f;

        private void Awake()
        {
            // Singleton pattern
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
        }

        /// <summary>
        /// Pause the entire game
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                LogDebug("Game already paused");
                return;
            }

            // Store current time scale
            previousTimeScale = Time.timeScale;

            // Pause everything
            Time.timeScale = 0f;
            IsGamePaused = true;

            LogDebug("Game PAUSED - Narrator speaking");

            // Notify listeners
            OnGamePausedChanged?.Invoke(true);
        }

        /// <summary>
        /// Resume the game to normal state
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                LogDebug("Game already running");
                return;
            }

            // Restore previous time scale
            Time.timeScale = previousTimeScale;
            IsGamePaused = false;

            LogDebug("Game RESUMED - Narrator finished");

            // Notify listeners
            OnGamePausedChanged?.Invoke(false);
        }

        /// <summary>
        /// Force resume with specific time scale
        /// </summary>
        public void ForceResume(float timeScale = 1f)
        {
            Time.timeScale = timeScale;
            IsGamePaused = false;

            LogDebug($"Game FORCE RESUMED with timeScale: {timeScale}");

            OnGamePausedChanged?.Invoke(false);
        }

        /// <summary>
        /// Get current time scale (for debugging)
        /// </summary>
        public float GetCurrentTimeScale()
        {
            return Time.timeScale;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GamePause] {message}");
            }
        }

        // Context menu for testing
        [ContextMenu("Test Pause")]
        private void TestPause()
        {
            PauseGame();
        }

        [ContextMenu("Test Resume")]
        private void TestResume()
        {
            ResumeGame();
        }

        [ContextMenu("Show Status")]
        private void ShowStatus()
        {
            LogDebug($"IsGamePaused: {IsGamePaused}, Time.timeScale: {Time.timeScale}");
        }

        private void OnDestroy()
        {
            // Ensure game is not left paused when destroyed
            if (IsGamePaused)
            {
                Time.timeScale = 1f;
            }
        }
    }
}

