using UnityEngine;

public class ConsumiblesManager : MonoBehaviour
{
    [Header("Referencia al ScriptableObject del consumible")]
    [SerializeField] private ConsumibleData consumibleData;

    /// <summary>
    /// Devuelve la cantidad actual de consumibles.
    /// </summary>
    public int GetCantidadConsumibles()
    {
        return consumibleData.cantidadConsumibles;
    }

    /// <summary>
    /// Aumenta el número de consumibles.
    /// </summary>
    public void SumarConsumible(int cantidad = 1)
    {
        consumibleData.cantidadConsumibles += cantidad;
        Debug.Log($"Se añadieron {cantidad} consumible(s). Total: {consumibleData.cantidadConsumibles}");
    }

    /// <summary>
    /// Resta consumibles (no baja de 0).
    /// </summary>
    public void RestarConsumible(int cantidad = 1)
    {
        consumibleData.cantidadConsumibles = Mathf.Max(0, consumibleData.cantidadConsumibles - cantidad);
        Debug.Log($"Se usó {cantidad} consumible. Restan: {consumibleData.cantidadConsumibles}");
    }

    /// <summary>
    /// Devuelve el ScriptableObject asociado, para acceder a sus propiedades.
    /// </summary>
    public ConsumibleData GetConsumibleData()
    {
        return consumibleData;
    }
}

