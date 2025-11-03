using UnityEngine;
using VoiceSystem.GameIntegration;
using VoiceSystem.Core;
using System.Collections;

/// <summary>
/// Maneja la condición de victoria del juego
/// </summary>
public class WinConditionManager : MonoBehaviour
{
    private static WinConditionManager instance;
    public static WinConditionManager Instance => instance;
    
    [Header("Configuración")]
    [SerializeField] private string livingRoomId = "room_2"; // Sala Principal
    
    [Header("Estado")]
    [SerializeField] private bool victoryAchieved = false;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private PlayerStateManager playerState;
    private RoomSystemBridge roomBridge;
    private VoiceSystemManager voiceSystem;
    
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
        playerState = PlayerStateManager.Instance;
        roomBridge = RoomSystemBridge.Instance;
        voiceSystem = VoiceSystemManager.Instance;
    }
    
    /// <summary>
    /// Verifica si el jugador puede ganar (tiene flor y está en Sala)
    /// </summary>
    public bool CanWin()
    {
        if (victoryAchieved) return false;
        if (playerState == null || roomBridge == null) return false;
        
        // Verificar que tenga la flor viva
        bool hasFlower = playerState.HasItem("lotus_flower_alive");
        
        // Verificar que esté en la Sala Principal
        var currentRoom = roomBridge.GetCurrentRoom();
        bool inLivingRoom = currentRoom != null && currentRoom.roomId == livingRoomId;
        
        LogDebug($"[WinCondition] CanWin check - Flor: {hasFlower}, En Sala: {inLivingRoom}");
        
        return hasFlower && inLivingRoom;
    }
    
    /// <summary>
    /// Intenta ganar el juego (llamado cuando dice "Dar" + "Renacer")
    /// </summary>
    public void AttemptVictory()
    {
        if (!CanWin())
        {
            LogDebug("[WinCondition] ❌ Condiciones de victoria no cumplidas");
            
            // Dar feedback al jugador
            if (voiceSystem?.textToSpeech != null)
            {
                string feedback = "";
                
                if (!playerState.HasItem("lotus_flower_alive"))
                {
                    feedback = "No tienes la flor de loto viva. Debes encontrarla primero.";
                }
                else
                {
                    feedback = "Debes estar frente al retrato familiar en la Sala Principal.";
                }
                
                voiceSystem.textToSpeech.Speak(feedback, VoiceSystem.Core.Interfaces.TTSPriority.Normal);
            }
            
            return;
        }
        
        // VICTORIA
        TriggerVictory();
    }
    
    /// <summary>
    /// Dispara la secuencia de victoria
    /// </summary>
    private void TriggerVictory()
    {
        victoryAchieved = true;
        LogDebug("[WinCondition] 🎉 VICTORIA LOGRADA!");
        
        StartCoroutine(VictorySequence());
    }
    
    /// <summary>
    /// Secuencia de narración de victoria
    /// </summary>
    private IEnumerator VictorySequence()
    {
        if (voiceSystem?.textToSpeech == null) yield break;
        
        // Narración de victoria
        string[] victoryNarration = new string[]
        {
            "Te acercas al retrato familiar, sosteniendo la flor de loto viva entre tus manos.",
            "Los pétalos brillan con una luz suave, palpitante, como un corazón que renace.",
            "Extiendes la flor hacia la madre del retrato, y susurras: Renacer.",
            "Por un instante, el tiempo se detiene por completo. El tic tac del reloj cesa. El aire se vuelve inmóvil.",
            "Entonces, las flores de loto en las manos de la madre comienzan a florecer de nuevo, vivas, blancas, puras.",
            "La línea oscura que goteaba desaparece. El niño que había salido del cuadro vuelve a aparecer, sosteniendo su oso, sonriendo.",
            "Una voz femenina, suave, llena de alivio, resuena en el aire: Gracias.",
            "La casa exhala. Las sombras retroceden. El peso del aire se disuelve.",
            "Has devuelto la paz a esta familia. Has roto la maldición. Has ganado."
        };
        
        foreach (string line in victoryNarration)
        {
            voiceSystem.textToSpeech.Speak(line, VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
            yield return new WaitForSeconds(6f);
        }
        
        yield return new WaitForSeconds(3f);
        
        // Final del juego
        LogDebug("[WinCondition] ✅ Narración de victoria completada");
        
        // Aquí podrías cargar una escena de créditos, mostrar pantalla de victoria, etc.
        voiceSystem.textToSpeech.Speak("Fin del juego. Has escapado de la casa embrujada.", VoiceSystem.Core.Interfaces.TTSPriority.Urgent);
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

