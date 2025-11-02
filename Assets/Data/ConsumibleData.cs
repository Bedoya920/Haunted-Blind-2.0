using UnityEngine;

[CreateAssetMenu(fileName = "NuevoConsumible", menuName = "Game/Consumible")]
public class ConsumibleData : ScriptableObject
{
    [Header("Datos del consumible")]
    [Tooltip("Nombre del consumible (por ejemplo: 'Poción de energía').")]
    public string nombreConsumible = "Poción misteriosa";

    [Tooltip("Cantidad de vidas que recupera al usarse.")]
    [Min(1)] public int vidasQueDevuelve = 1;

    [Header("Inventario del jugador")]
    [Tooltip("Número de consumibles que el jugador posee actualmente.")]
    [SerializeField, Min(0)] public int cantidadConsumibles = 1;
}


