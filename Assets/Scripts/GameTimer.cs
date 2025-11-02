using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{
    // Singleton
    private static GameTimer _instance;
    public static GameTimer Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GameTimer");
                _instance = go.AddComponent<GameTimer>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [SerializeField] private TimerData timerSettings;

    private float totalTime;
    private float interval;
    private float timeRemaining;
    private float nextTriggerTime;

    public event Action OnIntervalReached;
    public event Action OnTimerEnd;

    private bool isRunning = false;

    void Awake()
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

    void Start()
    {
        if (timerSettings == null)
        {
            Debug.LogError("[GameTimer] Falta asignar TimerData");
            return;
        }
        
        // Carga valores iniciales desde el ScriptableObject
        totalTime = timerSettings.totalDuration;
        interval = timerSettings.interval;

        ResetTimer();
        StartTimer(totalTime, interval);
        
        Debug.Log($"[GameTimer] Singleton inicializado - Tiempo total: {totalTime}s");
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        // Dispara un evento cuando se alcanza un intervalo
        if (timeRemaining <= totalTime - nextTriggerTime)
        {
            OnIntervalReached?.Invoke();
            nextTriggerTime += interval;
        }

        // Cuando el tiempo se acaba
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            isRunning = false;
            OnTimerEnd?.Invoke();
        }
    }

    public void StartTimer(float customTotalTime, float customInterval)
    {
        totalTime = customTotalTime;
        interval = customInterval;
        ResetTimer();
        isRunning = true;
    }

    public void PauseTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;

    public void ResetTimer()
    {
        timeRemaining = totalTime;
        nextTriggerTime = interval;
    }

    public float GetRemainingTime() => timeRemaining;
    public float GetTotalTime() => totalTime;
    public float GetInterval() => interval;
    public bool IsRunning() => isRunning;
    
    /// <summary>
    /// Métodos compatibles con GamePauseManager
    /// El timer usa Time.deltaTime que respeta Time.timeScale automáticamente
    /// </summary>
    public void PauseTimerForNarration()
    {
        // No hacer nada - Time.deltaTime ya es 0 cuando Time.timeScale = 0
        // El timer se pausa automáticamente
    }
    
    public void ResumeTimerFromNarration()
    {
        // No hacer nada - Time.deltaTime vuelve a normal automáticamente
    }
}
