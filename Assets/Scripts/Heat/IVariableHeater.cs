using UnityEngine;

namespace Assets.Scripts.Heat
{
    public interface IVariableHeater
    {
        Collider2D Collider { get; set; }
        float HeatRayDistance { get; }
        float HeatIntensity { get; }
    }
}
