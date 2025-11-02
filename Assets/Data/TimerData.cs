using UnityEngine;

[CreateAssetMenu(fileName = "TimerData", menuName = "Game/Timer Data")]
public class TimerData : ScriptableObject
{
    [Header("Configuración del tiempo")]
    [Tooltip("Duración total de la partida en segundos.")]
    public float totalDuration = 300f;

    [Tooltip("Cada cuántos segundos ocurre un evento o actualización.")]
    public float interval = 30f;

    [Header("Estado actual (no se guarda entre partidas)")]
    [HideInInspector] public float currentTime;
    [HideInInspector] public bool isRunning = false;

    public void ResetTimer()
    {
        currentTime = totalDuration;
        isRunning = true;
    }
}

