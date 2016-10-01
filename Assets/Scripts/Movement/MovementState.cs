using Assets.Scripts.Helpers;
using Assets.Scripts.Player;
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
		public GameObject Pivot { get; private set; }
		public Vector3 CurrentAcceleration { get; set; }
		public ColliderPoint CharacterPoint { get; set; }
		public ColliderPoint PreviousCharacterPoint { get; set; }
		public ColliderPoint TargetPoint { get; private set; }
        public bool TrappedBetweenSlopes { get; set; }
		public Vector3 Normal { get; set; }
		public bool WasOnSlope { get; set; }
		public DirectionFacing NormalDirection
		{
			get
			{
				if (Normal.x == 0)
					return DirectionFacing.None;
				else
					return Normal.x > 0 ? DirectionFacing.Right : DirectionFacing.Left;
			}
		}
		private bool _updatePivot;
		private Vector2 _colliderDimensions;

		public MovementState()
		{
			Pivot = new GameObject();
			Pivot.name = "Pivot";
		}

		public MovementState(Vector2 colliderDimensions) : this()
		{
			_colliderDimensions = colliderDimensions;
		}

		public Vector3 GetSurfaceDirection(DirectionTravelling direction)
		{
			return direction == DirectionTravelling.Right
				? Quaternion.Euler(0, 0, -90) * Normal
				: Quaternion.Euler(0, 0, 90) * Normal;
		}

		public Vector3 GetSurfaceDownDirection()
		{
			return Quaternion.Euler(0, 0, 180) * Normal;
		}

        public void MovePivotAlongSurface(DirectionTravelling direction, float distance)
        {
            Orientation o = OrientationHelper.GetOrientation(GetPivotParentRotation());
            Vector3 v = OrientationHelper.GetSurfaceVectorTowardsRight(o, Pivot.transform.parent);

            if (direction == DirectionTravelling.Left)
                v = -v;

			Vector3 movement = v.normalized * distance;

			_updatePivot = false;
			Pivot.transform.Translate(movement, Space.World);
        }

		public float GetPivotParentRotation()
		{
			return Pivot.transform.parent.rotation.eulerAngles.z;
		}

		public void Reset(Vector3 currentAcceleration)
		{
			IsGrounded = false;
			LeftCollision = false;
			RightCollision = false;
			IsOnSlope = false;
			CurrentAcceleration = currentAcceleration;
            TrappedBetweenSlopes = false;
		}

		public void OnLeftCollision()
		{
			LeftCollision = true;
		}

		public void OnRightCollision()
		{
			RightCollision = true;
		}

		public void SetPivotCollider(Collider2D pivotCollider)
		{
			PivotCollider = pivotCollider;
		}

		public void SetPivot(Collider2D pivotCollider, ColliderPoint targetPoint, ColliderPoint characterPoint)
		{
			PivotCollider = pivotCollider;
			Pivot.transform.parent = pivotCollider.transform.parent;

			if (pivotCollider.IsCorner() && (characterPoint == ColliderPoint.TopLeft || characterPoint == ColliderPoint.TopRight))
			{
				CharacterPoint = ColliderPoint.Centre;
				PreviousCharacterPoint = characterPoint;
				TargetPoint = targetPoint;
				Pivot.transform.position = GetPivotPositionWhenCorner(targetPoint);
				_updatePivot = false;
			}
			else
			{
				CharacterPoint = characterPoint;
				TargetPoint = targetPoint;
				Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
				_updatePivot = true;
			}
		}

		private Vector3 GetPivotPositionWhenCorner(ColliderPoint targetPoint)
		{
			Orientation o = OrientationHelper.GetOrientation(GetPivotParentRotation());
			Vector3 vDown = OrientationHelper.GetDownwardVector(o, Pivot.transform.parent);
			Vector3 vAcross = OrientationHelper.GetSurfaceVectorTowardsRight(o, Pivot.transform.parent);

			if (targetPoint == ColliderPoint.TopLeft)
				vAcross = -vAcross;

			return PivotCollider.GetPoint(targetPoint) + (vAcross.normalized * _colliderDimensions.x) + (vDown.normalized * _colliderDimensions.y);
		}

		public void UpdatePivotToTarget()
		{
			if (PivotCollider != null && _updatePivot)
			{
				Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
			}
		}

		public void JumpInPlace()
		{
			_updatePivot = false;
		}

		public void UnsetPivot()
		{
			PivotCollider = null;
		}
	}
}
