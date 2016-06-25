using UnityEngine;

namespace Assets.Scripts.Movement
{
	public interface IController
	{
		BoxCollider2D BoxCollider { get; set; }
		Transform Transform { get; set; }
		Vector3 Velocity { get; set; }
		AnimationCurve SlopeSpeedMultiplier { get; }
		float SlopeLimit { get; }
		MovementState MovementState { get; }
    }
}
