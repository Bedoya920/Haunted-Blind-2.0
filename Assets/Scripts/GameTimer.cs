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
    public event Action<int> OnHourPassed; // Evento cuando pasa una hora del juego

    private bool isRunning = false;
    private int lastHourAnnounced = 6; // Inicia a las 6 PM (18:00)

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
        
        // Verificar si pasó una hora del juego
        int currentHour = GetCurrentHour();
        if (currentHour != lastHourAnnounced)
        {
            lastHourAnnounced = currentHour;
            OnHourPassed?.Invoke(currentHour);
            Debug.Log($"[GameTimer] 🕐 Hora actual: {GetCurrentTimeString()}");
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
    /// Obtiene la hora actual del juego (6 PM a 6 AM = 12 horas)
    /// </summary>
    public int GetCurrentHour()
    {
        // El juego va de 6 PM (18:00) a 6 AM (6:00) = 12 horas
        // Cada hora real del juego = totalTime / 12
        float elapsedTime = totalTime - timeRemaining;
        float hourDuration = totalTime / 12f;
        int hoursPassed = Mathf.FloorToInt(elapsedTime / hourDuration);
        
        // Calcular hora actual (18, 19, 20, 21, 22, 23, 0, 1, 2, 3, 4, 5, 6)
        int currentHour = (18 + hoursPassed) % 24;
        
        return currentHour;
    }
    
    /// <summary>
    /// Obtiene la hora en formato legible (ej: "7:00 PM", "2:00 AM")
    /// </summary>
    public string GetCurrentTimeString()
    {
        int hour = GetCurrentHour();
        string period = hour >= 12 ? "PM" : "AM";
        int displayHour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour);
        
        return $"{displayHour}:00 {period}";
    }
    
    /// <summary>
    /// Verifica si es las 2 AM (hora del evento del niño)
    /// </summary>
    public bool IsItTwoAM()
    {
        return GetCurrentHour() == 2;
    }
    
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
