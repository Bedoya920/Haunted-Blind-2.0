using UnityEngine;

[CreateAssetMenu(fileName = "NuevoConsumible", menuName = "Game/Consumible")]
public class ConsumibleData : ScriptableObject
{
    [Header("Datos del consumible")]
    [Tooltip("Nombre del consumible (por ejemplo: 'Poción de energía').")]
    public string nombreConsumible = "Poción misteriosa";

    [Tooltip("Cantidad de vidas que recupera al usarse.")]
    [Min(1)] public int vidasQueDevuelve = 1;
}

