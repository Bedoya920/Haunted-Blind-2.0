using UnityEngine;

public class FatigueSystem : MonoBehaviour
{
    // Singleton
    private static FatigueSystem _instance;
    public static FatigueSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("FatigueSystem");
                _instance = go.AddComponent<FatigueSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [Header("Referencias")]
    [SerializeField] private PlayerLivesData playerLives;
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private ConsumiblesManager consumiblesManager;

    [Header("Configuración de Fatiga")]
    [Tooltip("Cuánta fatiga se necesita para perder una vida.")]
    [SerializeField] private int fatigaPorVida = 5;

    private int nivelFatigaActual = 0;
    private float tiempoUltimoLog = 0f;
    
    // Propiedades públicas para acceso externo
    public int NivelFatiga => nivelFatigaActual;
    public int FatigaPorVida => fatigaPorVida;
    public PlayerLivesData PlayerLives => playerLives;

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

    private void Start()
    {
        if (playerLives != null) playerLives.ResetLives();

        // Obtener GameTimer como singleton
        if (gameTimer == null)
        {
            gameTimer = GameTimer.Instance;
        }

        if (gameTimer != null)
        {
            gameTimer.OnTimerEnd += AlTerminarElTiempo;
        }

        Debug.Log($"[FatigueSystem] Singleton inicializado - Vidas: {playerLives?.currentLives ?? 0}, Tiempo: {gameTimer?.GetTotalTime() ?? 0}s");
    }

    private void Update()
    {
        if (gameTimer == null) return;

        if (gameTimer.IsRunning())
        {
            // Mostrar tiempo restante una vez por segundo
            if (Time.time - tiempoUltimoLog >= 1f)
            {
                Debug.Log($"Tiempo restante: {Mathf.CeilToInt(gameTimer.GetRemainingTime())} segundos");
                tiempoUltimoLog = Time.time;
            }

            // Aumentar fatiga con Espacio
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (playerLives.currentLives <= 0)
                {
                    Debug.Log("No puedes ganar fatiga, ya no tienes vidas.");
                    return;
                }

                nivelFatigaActual++;
                Debug.Log($"Fatiga actual: {nivelFatigaActual}");

                if (nivelFatigaActual >= fatigaPorVida)
                {
                    nivelFatigaActual = 0;
                    playerLives.LoseLife();

                    if (playerLives.currentLives > 0)
                        Debug.Log($"Has acumulado demasiada fatiga. Pierdes una vida. Vidas restantes: {playerLives.currentLives}");
                    else
                        Debug.Log("Has perdido todas tus vidas. Fin del juego.");
                }
            }

            // Usar consumible con F
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (playerLives.currentLives <= 0)
                {
                    Debug.Log("No puedes usar consumibles, ya no tienes vidas.");
                    return;
                }

                if (consumiblesManager == null)
                {
                    Debug.Log("No hay ConsumiblesManager asignado en el inspector.");
                    return;
                }

                int cantidadActual = consumiblesManager.GetCantidadConsumibles();
                if (cantidadActual <= 0)
                {
                    Debug.Log("No tienes consumibles disponibles.");
                    return;
                }

                ConsumibleData consumible = consumiblesManager.GetConsumibleData();

                if (playerLives.currentLives >= playerLives.totalLives)
                {
                    Debug.Log("Tus vidas ya están al máximo. No puedes usar el consumible.");
                    return;
                }

                int vidasARecuperar = Mathf.Min(consumible.vidasQueDevuelve, playerLives.totalLives - playerLives.currentLives);
                playerLives.currentLives += vidasARecuperar;

                // Restar el consumible del inventario
                consumiblesManager.RestarConsumible(1);

                Debug.Log($"Has usado {consumible.nombreConsumible}. Recuperas {vidasARecuperar} vida(s). " +
                          $"Vidas totales: {playerLives.currentLives}. Consumibles restantes: {consumiblesManager.GetCantidadConsumibles()}");
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Debug.Log("No puedes ganar fatiga, el tiempo ya terminó.");

            if (Input.GetKeyDown(KeyCode.F))
                Debug.Log("No puedes usar consumibles, el tiempo ya terminó.");
        }
    }

    private void AlTerminarElTiempo()
    {
        Debug.Log("[FatigueSystem] El tiempo terminó.");
    }
    
    #region Public API
    
    /// <summary>
    /// Aumentar fatiga (API pública para sistema de acciones)
    /// </summary>
    public void AddFatigue(int amount = 1)
    {
        if (playerLives == null || playerLives.currentLives <= 0)
        {
            return;
        }
        
        nivelFatigaActual += amount;
        
        if (nivelFatigaActual >= fatigaPorVida)
        {
            nivelFatigaActual = 0;
            playerLives.LoseLife();
            
            if (playerLives.currentLives > 0)
            {
                Debug.Log($"[FatigueSystem] Fatiga acumulada. Vida perdida. Vidas: {playerLives.currentLives}");
            }
            else
            {
                Debug.Log("[FatigueSystem] Has perdido todas tus vidas. Fin del juego.");
            }
        }
    }
    
    /// <summary>
    /// Usar consumible desde RoomInventoryManager (integración con sistema de voz)
    /// </summary>
    public bool TryUseConsumableFromInventory()
    {
        if (playerLives == null || playerLives.currentLives >= playerLives.totalLives)
        {
            return false;
        }
        
        // Intentar sistema rico de RoomInventoryManager primero
        var inventoryManager = VoiceSystem.GameIntegration.RoomInventoryManager.Instance;
        if (inventoryManager != null)
        {
            var contextProvider = FindObjectOfType<VoiceSystem.GameIntegration.GameContextProvider>();
            if (contextProvider != null)
            {
                var context = contextProvider.GetCurrentContext();
                
                // Buscar primer consumible en inventario
                foreach (string itemId in context.inventory)
                {
                    var item = inventoryManager.FindItemInAllRooms(itemId);
                    if (item != null && item.IsConsumable())
                    {
                        // Consumir
                        context.inventory.Remove(itemId);
                        playerLives.currentLives = Mathf.Min(
                            playerLives.totalLives, 
                            playerLives.currentLives + item.healthRestore
                        );
                        
                        nivelFatigaActual = Mathf.Max(0, nivelFatigaActual - item.fatigueReduction);
                        
                        Debug.Log($"[FatigueSystem] Consumible usado: {item.itemName} (+{item.healthRestore} vida, -{item.fatigueReduction} fatiga)");
                        return true;
                    }
                }
            }
        }
        
        // Fallback: sistema de Valuv con ConsumiblesManager
        if (consumiblesManager != null && consumiblesManager.GetCantidadConsumibles() > 0)
        {
            var consumible = consumiblesManager.GetConsumibleData();
            int vidasARecuperar = Mathf.Min(
                consumible.vidasQueDevuelve, 
                playerLives.totalLives - playerLives.currentLives
            );
            
            playerLives.currentLives += vidasARecuperar;
            consumiblesManager.RestarConsumible(1);
            
            Debug.Log($"[FatigueSystem] Consumible genérico usado (+{vidasARecuperar} vidas). Restantes: {consumiblesManager.GetCantidadConsumibles()}");
            return true;
        }
        
        return false;
    }
    
    #endregion
}


