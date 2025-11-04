using UnityEngine;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core;
using System.Collections;

/// <summary>
/// Maneja el diálogo del niño en la puerta del sótano
/// </summary>
public class BasementDoorDialogue : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string basementRoomId = "room_8"; // ID del sótano
    [SerializeField] private float dialogueTimeout = 10f; // Tiempo de espera para respuesta
    
    [Header("Estado")]
    [SerializeField] private bool dialogueTriggered = false;
    [SerializeField] private bool waitingForResponse = false;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private RoomSystemBridge roomBridge;
    private PlayerStateManager playerState;
    private VoiceSystemManager voiceSystem;
    private Coroutine dialogueCoroutine;
    
    private void Start()
    {
        roomBridge = RoomSystemBridge.Instance;
        playerState = PlayerStateManager.Instance;
        voiceSystem = VoiceSystemManager.Instance;
        
        if (roomBridge != null)
        {
            roomBridge.OnRoomChanged += OnRoomChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (roomBridge != null)
        {
            roomBridge.OnRoomChanged -= OnRoomChanged;
        }
    }
    
    private void OnRoomChanged(VoiceSystem.Core.Data.RoomData newRoom)
    {
        if (newRoom == null) return;
        if (dialogueTriggered) return; // Ya se disparó
        
        // Verificar si está en el sótano
        if (newRoom.roomId == basementRoomId)
        {
            CheckTriggerDialogue();
        }
    }
    
    /// <summary>
    /// Verifica si debe disparar el diálogo del niño
    /// </summary>
    private void CheckTriggerDialogue()
    {
        if (playerState == null) return;
        
        // Requisitos: Tener el oso y haber activado el evento del niño
        bool hasBear = playerState.HasItem("toy_bear");
        bool eventTriggered = playerState.HasSeenEvent("child_event_triggered");
        
        if (hasBear && eventTriggered)
        {
            LogDebug("[BasementDoor] ✅ Requisitos cumplidos: oso + evento. Iniciando diálogo...");
            StartDialogue();
        }
        else
        {
            LogDebug($"[BasementDoor] Requisitos no cumplidos - Oso: {hasBear}, Evento: {eventTriggered}");
        }
    }
    
    /// <summary>
    /// Inicia el diálogo del niño
    /// </summary>
    private void StartDialogue()
    {
        if (dialogueTriggered) return;
        dialogueTriggered = true;
        
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }
        
        dialogueCoroutine = StartCoroutine(DialogueSequence());
    }
    
    /// <summary>
    /// Secuencia del diálogo del niño con pausas
    /// </summary>
    private IEnumerator DialogueSequence()
    {
        LogDebug("[BasementDoor] 🚪 Iniciando secuencia de diálogo del niño...");
        
        if (voiceSystem?.textToSpeech == null)
        {
            Debug.LogError("[BasementDoor] VoiceSystem no disponible!");
            yield break;
        }
        
        // PAUSAR el GameTimer durante el diálogo para evitar que las campanadas interrumpan
        var gameTimer = GameTimer.Instance;
        bool timerWasRunning = false;
        if (gameTimer != null)
        {
            timerWasRunning = gameTimer.IsRunning(); // FIX: IsRunning es un método, no propiedad
            if (timerWasRunning)
            {
                gameTimer.PauseTimer();
                LogDebug("[BasementDoor] ⏸️ Timer pausado durante el diálogo");
            }
        }
        
        // Narración inicial
        string intro = "El aire se espesa apenas te acercas a la puerta. La madera cruje, no por el peso del tiempo, sino porque respira. " +
                      "Desde el otro lado, un murmullo atraviesa las rendijas, mezclado con el eco del tic tac que parece provenir de dentro de las paredes. " +
                      "La temperatura desciende. Algo pequeño, casi un sollozo, se distingue entre los ruidos.";
        
        voiceSystem.textToSpeech.Speak(intro, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
        yield return new WaitForSeconds(15f);
        
        // Diálogo del niño (línea por línea con pausas)
        string[] childDialogue = new string[]
        {
            "El reloj no se detuvo cuando yo dormí.",
            "Mamá dijo que si alguien me hablaba, podría abrir los ojos.",
            "Pero, no quiero hacerlo solo.",
            "¿Tienes mi oso? Él no teme a la oscuridad. Si lo sostienes, puedo seguir el hilo de su calor.",
            "Dime Despertar, y cuando lo haga, llévale el regalo a mamá."
        };
        
        foreach (string line in childDialogue)
        {
            voiceSystem.textToSpeech.Speak(line, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
            yield return new WaitForSeconds(5f);
        }
        
        // Esperar respuesta del jugador
        LogDebug("[BasementDoor] ⏳ Esperando respuesta del jugador (Despertar o silencio)...");
        waitingForResponse = true;
        
        float startTime = Time.time;
        bool playerResponded = false;
        
        while (Time.time - startTime < dialogueTimeout)
        {
            // Verificar si el jugador dijo "Despertar"
            // Esto se maneja en GameContextProvider
            
            yield return null;
        }
        
        // Si llegó aquí, el jugador guardó silencio (correcto)
        if (!playerResponded)
        {
            OnPlayerSilence();
        }
    }
    
    /// <summary>
    /// Llamado cuando el jugador guarda silencio (respuesta correcta)
    /// </summary>
    public void OnPlayerSilence()
    {
        waitingForResponse = false;
        
        LogDebug("[BasementDoor] ✅ Jugador guardó silencio - RESPUESTA CORRECTA");
        
        // Activar flags del evento del niño
        if (playerState != null)
        {
            playerState.SetEventFlag("lotus_flower_activated");
            playerState.SetEventFlag("child_dialogue_complete"); // CRÍTICO: Para activar eventos post-niño
            LogDebug("[BasementDoor] 🌸 Flor de loto activada");
            LogDebug("[BasementDoor] ✅ Flag 'child_dialogue_complete' marcado");
        }
        
        // Narrar confirmación
        if (voiceSystem?.textToSpeech != null)
        {
            string confirmation = "El silencio se profundiza. Un suspiro leve atraviesa la puerta, y el murmullo cesa. " +
                                "Algo ha cambiado en la casa. El aire huele ahora a flores frescas, a tierra húmeda y vida nueva.";
            
            voiceSystem.textToSpeech.Speak(confirmation, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
        }
        
        // RESUMIR el GameTimer después del diálogo
        var gameTimer = GameTimer.Instance;
        if (gameTimer != null && !gameTimer.IsRunning()) // FIX: IsRunning es un método
        {
            gameTimer.ResumeTimer();
            LogDebug("[BasementDoor] ▶️ Timer reanudado después del diálogo");
        }
    }
    
    /// <summary>
    /// Llamado cuando el jugador dice "Despertar" (respuesta incorrecta)
    /// </summary>
    public void OnPlayerSaidDespertar()
    {
        waitingForResponse = false;
        
        LogDebug("[BasementDoor] ❌ Jugador dijo 'Despertar' - FINAL MALO");
        
        // Narrar final malo
        if (voiceSystem?.textToSpeech != null)
        {
            string badEnding = "La puerta del sótano se abre de golpe. Un aire helado te golpea. " +
                             "La voz del niño se vuelve aguda, distorsionada: Ya no estás solo. " +
                             "Escuchas pasos subiendo las escaleras. Muchos pasos. Pasos pequeños. " +
                             "Y entonces, el silencio. El reloj se detiene. El tiempo se acaba. " +
                             "Has perdido.";
            
            voiceSystem.textToSpeech.Speak(badEnding, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
        }
        
        // Disparar muerte del jugador
        StartCoroutine(TriggerBadEnding());
    }
    
    private IEnumerator TriggerBadEnding()
    {
        yield return new WaitForSeconds(10f); // Esperar a que termine la narración principal
        
        // Narrar mensaje de muerte
        if (voiceSystem?.textToSpeech != null)
        {
            voiceSystem.textToSpeech.Speak("Has muerto. El juego se reiniciará.", VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
        }
        
        // Matar al jugador
        var fatigueSystem = FatigueSystem.Instance;
        if (fatigueSystem != null)
        {
            fatigueSystem.PlayerLives.currentLives = 0; // Muerte instantánea
            LogDebug("[BasementDoor] 💀 Final malo ejecutado - Vidas: 0");
        }
        else
        {
            Debug.LogError("[BasementDoor] FatigueSystem no encontrado");
        }
        
        // Reiniciar la escena después de 3 segundos
        yield return new WaitForSeconds(3f);
        LogDebug("[BasementDoor] 🔄 Reiniciando juego...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

