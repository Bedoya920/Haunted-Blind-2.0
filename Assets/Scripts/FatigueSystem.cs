using UnityEngine;

public class FatigueSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerLivesData playerLives;
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private ConsumiblesManager consumiblesManager;

    [Header("Configuración de Fatiga")]
    [Tooltip("Cuánta fatiga se necesita para perder una vida.")]
    [SerializeField] private int fatigaPorVida = 5;

    private int nivelFatigaActual = 0;
    private float tiempoUltimoLog = 0f;

    private void Start()
    {
        if (playerLives != null) playerLives.ResetLives();

        if (gameTimer == null)
        {
            Debug.LogError("Falta asignar el GameTimer en el inspector.");
            return;
        }

        gameTimer.OnTimerEnd += AlTerminarElTiempo;

        Debug.Log($"[INICIO] Vidas: {playerLives.currentLives}, Tiempo total: {gameTimer.GetTotalTime()} segundos.");
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
        Debug.Log("El sistema de fatiga detecta que el tiempo terminó.");
    }
}






