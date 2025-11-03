using UnityEngine;
using VoiceSystem.Recognition;

/// <summary>
/// Script de emergencia para forzar reanudación del reconocimiento de voz
/// </summary>
public class ForceResumeRecognition : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float checkInterval = 1f; // Verificar cada 1 segundo
    [SerializeField] private float maxPausedTime = 5f; // Máximo 5 segundos pausado antes de forzar reanudación
    
    private WindowsSpeechRecognizer recognizer;
    private float lastCheckTime = 0f;
    private float pausedSince = 0f;
    private bool wasPaused = false;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Start()
    {
        recognizer = FindFirstObjectByType<WindowsSpeechRecognizer>();
        if (recognizer == null)
        {
            Debug.LogError("[ForceResume] WindowsSpeechRecognizer no encontrado!");
            enabled = false;
        }
    }
    
    private void Update()
    {
        if (recognizer == null) return;
        
        if (Time.time - lastCheckTime < checkInterval)
            return;
        
        lastCheckTime = Time.time;
        
        // Verificar si el reconocimiento está pausado
        bool isPaused = recognizer.IsPaused;
        
        if (isPaused)
        {
            if (!wasPaused)
            {
                // Acaba de pausarse
                pausedSince = Time.time;
                wasPaused = true;
                LogDebug("[ForceResume] Reconocimiento pausado detectado");
            }
            else
            {
                // Ha estado pausado por un tiempo
                float pausedDuration = Time.time - pausedSince;
                
                if (pausedDuration > maxPausedTime)
                {
                    LogDebug($"[ForceResume] ⚠️ Reconocimiento pausado por {pausedDuration:F1}s - FORZANDO REANUDACIÓN");
                    recognizer.ResumeRecognition();
                    wasPaused = false;
                }
            }
        }
        else
        {
            // No está pausado
            wasPaused = false;
        }
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

