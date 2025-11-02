using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLivesData", menuName = "Game/Player Lives Data")]
public class PlayerLivesData : ScriptableObject
{
    [Header("Configuración de vidas")]
    [Tooltip("Número total de vidas del jugador al iniciar el juego.")]
    public int totalLives = 3;

    [Header("Estado actual (no se guarda entre partidas)")]
    [HideInInspector] public int currentLives;

    // Reinicia las vidas al valor inicial
    public void ResetLives()
    {
        currentLives = totalLives;
    }

    // Resta una vida (retorna true si el jugador sigue vivo)
    public bool LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
        }
        return currentLives > 0;
    }

    // Añade una vida
    public void GainLife()
    {
        currentLives++;
    }
}
