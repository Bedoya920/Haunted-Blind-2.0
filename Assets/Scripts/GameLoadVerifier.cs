using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoiceSystem.Core.Data;
using VoiceSystem.GameIntegration;

/// <summary>
/// Verifica que el juego se cargue correctamente al inicio
/// Valida que todos los sistemas estén funcionando
/// </summary>
public class GameLoadVerifier : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool hablarResultado = true;
    [SerializeField] private float delayVerificacion = 3f;
    
    void Start()
    {
        StartCoroutine(VerifyGameLoad());
    }
    
    IEnumerator VerifyGameLoad()
    {
        Debug.Log("[GameLoadVerifier] Esperando inicialización...");
        yield return new WaitForSeconds(delayVerificacion);
        
        Debug.Log("========== VERIFICANDO CARGA DEL JUEGO ==========");
        
        bool allGood = true;
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Check 1: PlayerData
        var playerData = Resources.Load<PlayerData>("Data/PlayerData");
        if (playerData == null)
        {
            errors.Add("❌ PlayerData no cargado desde Resources/Data/");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ PlayerData cargado - Salud: {playerData.CurrentHealth}, Inventario: {playerData.Inventory.Count} items");
        }
        
        // Check 2: House generated
        var generator = FindFirstObjectByType<RoomGenerator3000>();
        if (generator == null || generator.casa == null || generator.casa.habitaciones.Count == 0)
        {
            errors.Add("❌ Casa no generada");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ Casa generada: {generator.casa.habitaciones.Count} habitaciones, {generator.casa.puertas.Count} puertas");
        }
        
        // Check 3: RoomSystemBridge
        var roomBridge = RoomSystemBridge.Instance;
        if (roomBridge == null)
        {
            errors.Add("❌ RoomSystemBridge no inicializado");
            allGood = false;
        }
        else
        {
            var currentRoom = roomBridge.GetCurrentRoom();
            if (currentRoom != null)
            {
                Debug.Log($"✅ RoomSystemBridge funcionando - Habitación actual: {currentRoom.roomName} (ID: {currentRoom.roomId})");
                Debug.Log($"   Puertas en habitación: {currentRoom.doors?.Count ?? 0}");
            }
            else
            {
                warnings.Add("⚠️ RoomSystemBridge no tiene habitación actual");
            }
        }
        
        // Check 4: Story events loaded
        var eventManager = EventManager.Instance;
        if (eventManager == null)
        {
            errors.Add("❌ EventManager no inicializado");
            allGood = false;
        }
        else if (eventManager.StoryEvents == null || eventManager.StoryEvents.Length == 0)
        {
            errors.Add("❌ Story events no cargados");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ EventManager inicializado:");
            Debug.Log($"   Main Events: {eventManager.MainEvents?.Length ?? 0}");
            Debug.Log($"   Random Events: {eventManager.RandomEvents?.Length ?? 0}");
            Debug.Log($"   Story Events: {eventManager.StoryEvents.Length}");
            Debug.Log($"   Screamers: {eventManager.Screamers?.Length ?? 0}");
        }
        
        // Check 5: Event mapping
        var mapeo = FindFirstObjectByType<MapeoHabitacionesGDD>();
        if (mapeo == null)
        {
            warnings.Add("⚠️ MapeoHabitacionesGDD no encontrado - Los eventos podrían no dispararse correctamente");
        }
        else
        {
            Debug.Log("✅ MapeoHabitacionesGDD presente");
        }
        
        // Check 6: PlayerStateManager
        var playerState = PlayerStateManager.Instance;
        if (playerState == null)
        {
            errors.Add("❌ PlayerStateManager no inicializado");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ PlayerStateManager inicializado:");
            Debug.Log($"   {playerState.GetStateSummary()}");
        }
        
        // Check 7: CommandValidator
        var validator = CommandValidator.Instance;
        if (validator == null)
        {
            errors.Add("❌ CommandValidator no inicializado");
            allGood = false;
        }
        else
        {
            var validCommands = validator.GetValidCommandsNow();
            Debug.Log($"✅ CommandValidator inicializado - Comandos válidos ahora: {validCommands.Count}");
            Debug.Log($"   Comandos: {string.Join(", ", validCommands)}");
        }
        
        // Check 8: FatigueSystem
        var fatigueSystem = FatigueSystem.Instance;
        if (fatigueSystem == null)
        {
            errors.Add("❌ FatigueSystem no inicializado");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ FatigueSystem inicializado - Vidas: {fatigueSystem.PlayerLives?.currentLives ?? 0}, Fatiga: {fatigueSystem.NivelFatiga}");
        }
        
        // Check 9: VoiceSystem
        var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
        if (voiceSystem == null)
        {
            errors.Add("❌ VoiceSystemManager no inicializado");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ VoiceSystemManager inicializado - Escuchando: {voiceSystem.IsListening}");
        }
        
        // Print warnings
        if (warnings.Count > 0)
        {
            Debug.LogWarning("========== ⚠️ ADVERTENCIAS ==========");
            foreach (var warning in warnings)
            {
                Debug.LogWarning(warning);
            }
        }
        
        // Final report
        if (allGood)
        {
            Debug.Log("========== ✅ JUEGO CARGADO CORRECTAMENTE ==========");
            Debug.Log("Todos los sistemas están funcionando");
            Debug.Log("El juego está listo para jugar");
            Debug.Log("==================================================");
            
            if (hablarResultado && voiceSystem != null && voiceSystem.textToSpeech != null)
            {
                voiceSystem.textToSpeech.Speak("Todos los sistemas cargados correctamente. El juego está listo");
            }
        }
        else
        {
            Debug.LogError("========== ❌ ERRORES AL CARGAR JUEGO ==========");
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }
            Debug.LogError("El juego NO está en estado funcional");
            Debug.LogError("===============================================");
            
            if (hablarResultado && voiceSystem != null && voiceSystem.textToSpeech != null)
            {
                voiceSystem.textToSpeech.Speak($"Error al cargar el juego. {errors.Count} problemas detectados. Revisa la consola");
            }
        }
    }
}

