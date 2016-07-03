using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementState
	{
		public bool IsGrounded { get; set; }
		public bool IsOnSlope { get; set; }
		public Collider2D PivotCollider { get; set; }
		public bool MovementOverridden { get; set; }
		public bool LeftCollision { get; private set; }
		public bool RightCollision { get; private set; }
		public bool IgnoreCurrentPlatform { get; set; }
		public GameObject GroundPivot { get; private set; }
		public Vector3 CurrentAcceleration { get; private set; }

		public MovementState()
		{
			GroundPivot = new GameObject();
		}

		public void Reset(Vector3 currentAcceleration)
		{
			IsGrounded = false;
			LeftCollision = false;
			RightCollision = false;
			IsOnSlope = false;
			CurrentAcceleration = currentAcceleration;
		}

		public void OnLeftCollision(ref Vector3 deltaMovement)
		{
			LeftCollision = true;
			if (CurrentAcceleration.x < 0)
				deltaMovement.x -= CurrentAcceleration.x;
		}

		public void OnRightCollision(ref Vector3 deltaMovement)
		{
			RightCollision = true;
			if (CurrentAcceleration.x > 0)
				deltaMovement.x -= CurrentAcceleration.x;
		}

		public void SetPivot(Vector2 position, Collider2D col)
		{
			PivotCollider = col;
			GroundPivot.transform.position = position;
			GroundPivot.transform.parent = col.transform;
		}

		public void UnsetPivot()
		{
			PivotCollider = null;
		}

		public void StopMoving()
		{
			MovementOverridden = true;
		}
	}
}
