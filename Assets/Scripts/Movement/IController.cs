using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Movement
{
    public interface IController
    {
        CollisionState CollisionState { get; set; }
        List<RaycastHit2D> RaycastHitsThisFrame { get; set; }
        BoxCollider2D BoxCollider { get; set; }
        float SkinWidth { get; set; }
        Transform Transform { get; set; }
        Vector3 Velocity { get; set; }
        int TotalHorizontalRays { get;}
        int TotalVerticalRays { get;}
        float VerticalDistanceBetweenRays { get; set; }
        float HorizontalDistanceBetweenRays { get; set; }
        AnimationCurve SlopeSpeedMultiplier { get; }
        float SlopeLimit { get; }
        LayerMask PlatformMask { get; }
    }
}
