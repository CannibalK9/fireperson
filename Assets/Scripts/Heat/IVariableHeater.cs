using UnityEngine;

namespace Assets.Scripts.Heat
{
    public interface IVariableHeater
    {
        BoxCollider2D BoxCollider { get; set; }
        float HeatRayDistance { get; }
    }
}
