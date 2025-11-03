using UnityEngine;
using System.Collections.Generic;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Gestiona eventos de confirmación de acciones del jugador.
    /// Carga desde action_confirmations.json y proporciona métodos helper.
    /// </summary>
    public class ActionConfirmationManager : MonoBehaviour
    {
        // Singleton
        private static ActionConfirmationManager _instance;
        public static ActionConfirmationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ActionConfirmationManager");
                    _instance = go.AddComponent<ActionConfirmationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // JSON filename si se necesita en el futuro
        private const string CONFIRMATION_JSON_FILENAME = "action_confirmations";

        private Dictionary<string, string> confirmationTemplates;
        private EventManager eventManager;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            eventManager = EventManager.Instance;
        }

        private void Initialize()
        {
            confirmationTemplates = new Dictionary<string, string>
            {
                // Templates de confirmación
                {"eat_item", "Comiste {item_name}. Te sientes un poco mejor."},
                {"take_item", "Tomaste {item_name}. Lo guardas en tu inventario."},
                {"search_success", "Buscaste cuidadosamente. Encontraste {items_found}."},
                {"search_empty", "Buscaste cuidadosamente pero no encontraste nada nuevo."},
                {"inspect_item", "Inspeccionas {item_name}. {item_description}"},
                {"use_door", "Atravesaste la puerta hacia {direction}."},
                {"door_locked", "La puerta está bloqueada. {reason}"},
                {"use_key", "Usaste {key_name} y abriste la puerta."},
                {"item_not_found", "No ves {item_name} aquí."},
                {"no_consumables", "No tienes comida para consumir."},
                {"health_full", "Ya tienes la vida completa. No necesitas comer ahora."},
                {"take_key", "Tomaste {key_name}. Podrías usarla para abrir puertas bloqueadas."},
                {"read_diary", "Lees el diario. {diary_content}"},
                {"fatigue_increased", "Te sientes más cansado. Tu nivel de fatiga ha aumentado."},
                {"life_lost", "Has perdido una vida. Te quedan {remaining_lives} vidas."}
            };

            Debug.Log($"[ActionConfirmation] Singleton inicializado con {confirmationTemplates.Count} templates");
        }

        #region Public API

        /// <summary>
        /// Confirmar que el jugador comió un item
        /// </summary>
        public void ConfirmEat(string itemName, int healthRestored, int fatigueReduced)
        {
            string message = confirmationTemplates["eat_item"].Replace("{item_name}", itemName);
            
            if (healthRestored > 0)
            {
                message += $" Recuperaste {healthRestored} de vida.";
            }
            
            if (fatigueReduced > 0)
            {
                message += $" Tu fatiga se redujo en {fatigueReduced}.";
            }
            
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar que el jugador tomó un item
        /// </summary>
        public void ConfirmTakeItem(string itemName, VoiceSystem.Core.Data.ItemType itemType)
        {
            string template = itemType == VoiceSystem.Core.Data.ItemType.Key ? "take_key" : "take_item";
            string message = confirmationTemplates[template].Replace("{item_name}", itemName)
                                                             .Replace("{key_name}", itemName);
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar búsqueda exitosa
        /// </summary>
        public void ConfirmSearch(List<string> itemsFound)
        {
            if (itemsFound == null || itemsFound.Count == 0)
            {
                NarrateConfirmation(confirmationTemplates["search_empty"]);
            }
            else
            {
                string itemsList = string.Join(", ", itemsFound);
                string message = confirmationTemplates["search_success"].Replace("{items_found}", itemsList);
                NarrateConfirmation(message);
            }
        }

        /// <summary>
        /// Confirmar inspección de item
        /// </summary>
        public void ConfirmInspect(string itemName, string description)
        {
            string message = confirmationTemplates["inspect_item"]
                .Replace("{item_name}", itemName)
                .Replace("{item_description}", description);
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar uso de puerta
        /// </summary>
        public void ConfirmUseDoor(string direction)
        {
            string message = confirmationTemplates["use_door"].Replace("{direction}", direction);
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar puerta bloqueada
        /// </summary>
        public void ConfirmDoorLocked(string reason)
        {
            string message = confirmationTemplates["door_locked"].Replace("{reason}", reason);
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar uso de llave
        /// </summary>
        public void ConfirmUseKey(string keyName)
        {
            string message = confirmationTemplates["use_key"].Replace("{key_name}", keyName);
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar item no encontrado
        /// </summary>
        public void ConfirmItemNotFound(string itemName)
        {
            string message = confirmationTemplates["item_not_found"].Replace("{item_name}", itemName);
            NarrateConfirmation(message);
        }

        /// <summary>
        /// Confirmar sin consumibles
        /// </summary>
        public void ConfirmNoConsumables()
        {
            NarrateConfirmation(confirmationTemplates["no_consumables"]);
        }

        /// <summary>
        /// Confirmar vida completa
        /// </summary>
        public void ConfirmHealthFull()
        {
            NarrateConfirmation(confirmationTemplates["health_full"]);
        }

        /// <summary>
        /// Confirmar fatiga aumentada
        /// </summary>
        public void ConfirmFatigueIncreased()
        {
            NarrateConfirmation(confirmationTemplates["fatigue_increased"]);
        }

        /// <summary>
        /// Confirmar pérdida de vida
        /// </summary>
        public void ConfirmLifeLost(int remainingLives)
        {
            string message = confirmationTemplates["life_lost"].Replace("{remaining_lives}", remainingLives.ToString());
            NarrateConfirmation(message);
        }
        
        /// <summary>
        /// Confirmar screamer (evento que asusta y reduce vida)
        /// </summary>
        public void ConfirmScreamer(string screamerDescription)
        {
            // Los screamers se narran directamente a través de su audioTxt
            // Este método puede usarse para audio adicional o efectos de sonido
            Debug.Log($"[ActionConfirmation] Screamer activado: {screamerDescription}");
        }
        
        /// <summary>
        /// Confirmar movimiento direccional
        /// </summary>
        public void ConfirmMove(string direction, string newRoomName)
        {
            string message = $"Te mueves {direction} hacia {newRoomName}";
            NarrateConfirmation(message);
        }
        
        /// <summary>
        /// Confirmar evento de historia activado
        /// </summary>
        public void ConfirmStoryEvent(string eventName)
        {
            Debug.Log($"[ActionConfirmation] Evento de historia: {eventName}");
            // Los eventos de historia se narran a través de EventManager
        }

        #endregion

        private void NarrateConfirmation(string message)
        {
            var voiceSystem = VoiceSystem.Core.VoiceSystemManager.Instance;
            if (voiceSystem != null && voiceSystem.textToSpeech != null)
            {
                voiceSystem.textToSpeech.Speak(
                    message,
                    VoiceSystem.Core.Interfaces.TTSPriority.Normal // Confirmaciones tienen prioridad normal
                );
                
                // Debug.Log($"[ActionConfirmation] {message}"); // Comentado - redundante con TTS
            }
        }
    }
}

