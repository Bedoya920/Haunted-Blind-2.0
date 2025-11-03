using UnityEngine;
using VoiceSystem.Core;

/// <summary>
/// Sistema que anuncia las campanadas cada hora y controla accesos basados en tiempo
/// </summary>
public class HourlyBellSystem : MonoBehaviour
{
    private static HourlyBellSystem instance;
    public static HourlyBellSystem Instance => instance;
    
    [Header("Referencias")]
    private GameTimer gameTimer;
    private VoiceSystemManager voiceSystem;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        gameTimer = GameTimer.Instance;
        voiceSystem = VoiceSystemManager.Instance;
        
        if (gameTimer != null)
        {
            gameTimer.OnHourPassed += OnHourChanged;
            LogDebug("[HourlyBell] Suscrito a OnHourPassed");
        }
        else
        {
            Debug.LogError("[HourlyBell] GameTimer no encontrado!");
        }
    }
    
    private void OnDestroy()
    {
        if (gameTimer != null)
        {
            gameTimer.OnHourPassed -= OnHourChanged;
        }
    }
    
    /// <summary>
    /// Llamado cuando pasa una hora del juego
    /// </summary>
    private void OnHourChanged(int currentHour)
    {
        LogDebug($"[HourlyBell] 🕐 Hora cambiada a: {GetHourString(currentHour)}");
        
        // Determinar número de campanadas
        int bellCount = GetBellCount(currentHour);
        
        // Anunciar con campanadas
        AnnounceBells(bellCount, currentHour);
        
        // Si es las 2 AM, desbloquear puerta del sótano
        if (currentHour == 2)
        {
            UnlockBasementDoor();
        }
    }
    
    /// <summary>
    /// Anuncia las campanadas con voz
    /// </summary>
    private void AnnounceBells(int count, int hour)
    {
        if (voiceSystem?.textToSpeech == null) return;
        
        string hourStr = GetHourString(hour);
        string bellNarration = "";
        
        if (count == 1)
        {
            bellNarration = $"Una campanada resuena en la casa. Es {hourStr}.";
        }
        else if (count == 2)
        {
            bellNarration = $"Dos campanadas resuenan en la casa. Son las {hourStr}. El reloj del hall marca eternamente esta hora.";
        }
        else
        {
            bellNarration = $"{count} campanadas resuenan en la casa. Son las {hourStr}.";
        }
        
        voiceSystem.textToSpeech.Speak(bellNarration, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
        LogDebug($"[HourlyBell] 🔔 Campanadas anunciadas: {count} ({hourStr})");
    }
    
    /// <summary>
    /// Obtiene el número de campanadas para la hora
    /// </summary>
    private int GetBellCount(int hour)
    {
        if (hour == 0) return 12; // Medianoche
        if (hour > 12) return hour - 12; // PM
        return hour; // AM
    }
    
    /// <summary>
    /// Convierte hora numérica a string legible
    /// </summary>
    private string GetHourString(int hour)
    {
        string period = hour >= 12 ? "PM" : "AM";
        int displayHour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour);
        return $"{displayHour}:00 {period}";
    }
    
    /// <summary>
    /// Desbloquea la puerta del sótano a las 2 AM
    /// </summary>
    private void UnlockBasementDoor()
    {
        LogDebug("[HourlyBell] 🔓 Son las 2 AM - Desbloqueando puerta del sótano");
        
        // Buscar y desbloquear la puerta del sótano
        var roomGenerator = FindFirstObjectByType<RoomGenerator3000>();
        if (roomGenerator?.casa?.puertas != null)
        {
            // La puerta al sótano conecta Hab. Niños (0,5) con Sótano (0,6)
            var basementDoor = roomGenerator.casa.puertas.Find(d => 
                (d.cuarto1.x == 0 && d.cuarto1.y == 5 && d.cuarto2.x == 0 && d.cuarto2.y == 6) ||
                (d.cuarto1.x == 0 && d.cuarto1.y == 6 && d.cuarto2.x == 0 && d.cuarto2.y == 5)
            );
            
            if (basementDoor != null)
            {
                basementDoor.abierta = true;
                LogDebug($"[HourlyBell] ✅ Puerta del sótano (ID:{basementDoor.id}) desbloqueada");
                
                // Narrar
                if (voiceSystem?.textToSpeech != null)
                {
                    voiceSystem.textToSpeech.Speak("Escuchas un clic en algún lugar de la casa. Una puerta se ha desbloqueado.", VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
                }
            }
            else
            {
                Debug.LogWarning("[HourlyBell] No se encontró la puerta del sótano");
            }
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

