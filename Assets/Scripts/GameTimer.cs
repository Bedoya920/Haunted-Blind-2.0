using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TimerData timerSettings;

    private float totalTime;
    private float interval;
    private float timeRemaining;
    private float nextTriggerTime;

    public event Action OnIntervalReached;
    public event Action OnTimerEnd;

    private bool isRunning = false;

    void Start()
    {
        // Carga valores iniciales desde el ScriptableObject
        totalTime = timerSettings.totalDuration;
        interval = timerSettings.interval;

        ResetTimer();
        StartTimer(totalTime, interval);
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
}


