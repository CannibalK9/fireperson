using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
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
		public Transform CornerCollider { get; set; }
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
			if (PivotCollider.IsUpright())
				CornerCollider = Pivot.transform.parent;
			else
			{
				bool left = OrientationHelper.GetOrientation(Pivot.transform.parent.rotation.eulerAngles.z) == Orientation.Flat
						? Pivot.transform.position.x < Pivot.transform.parent.position.x
						: Pivot.transform.position.x > Pivot.transform.parent.position.x;

				CornerCollider = left
					? Pivot.transform.parent.GetComponent<ClimbableEdges>().LeftException.transform
					: Pivot.transform.parent.GetComponent<ClimbableEdges>().RightException.transform;
			}

			Vector3 vAcross = CornerCollider.up;
			Vector3 vDown = CornerCollider.right;

			if (vDown.y > 0)
				vDown = -vDown;

			if ((vAcross.x > 0 && targetPoint == ColliderPoint.TopLeft) || (vAcross.x < 0 && targetPoint == ColliderPoint.TopRight))
				vAcross = -vAcross;

			return PivotCollider.GetPoint(targetPoint) + (vAcross.normalized * _colliderDimensions.x) + (vDown.normalized * _colliderDimensions.y);
		}

		public float GetCornerAngle()
		{
			float angle = Mathf.Abs(Vector2.Angle(Vector2.up, CornerCollider.right));

			if (angle > 180)
				angle -= 180;

			Vector3 vDown = CornerCollider.right;

			if (vDown.y > 0)
				vDown = -vDown;

			return vDown.x < 0
				? -angle
				: angle;
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
