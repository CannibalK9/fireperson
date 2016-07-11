using UnityEngine;

namespace Assets.Scripts.Movement
{
	public interface IMotor
	{
		Collider2D Collider { get; set; }
		Rigidbody2D Rigidbody { get; set; }
		Transform Transform { get; set; }
		MovementState MovementState { get; set; }
		float SlopeLimit { get; }
    }
}
