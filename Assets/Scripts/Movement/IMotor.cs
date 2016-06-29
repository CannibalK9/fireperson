using UnityEngine;

namespace Assets.Scripts.Movement
{
	public interface IMotor
	{
		BoxCollider2D BoxCollider { get; set; }
		Transform Transform { get; set; }
		MovementState MovementState { get; set; }
		float SlopeLimit { get; }
    }
}
