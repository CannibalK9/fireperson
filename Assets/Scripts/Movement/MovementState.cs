using Assets.Scripts.Helpers;
using Assets.Scripts.Player.Climbing;
using System.Collections.Generic;
using System.Linq;
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
        public bool LeftEdge { get; private set; }
        public bool RightEdge { get; private set; }
		public bool ApproachingSnow { get; set; }
		public bool OnSnow { get; set; }
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
		public bool UpdatePivot { get; set; }
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
            Vector3 v = OrientationHelper.GetSurfaceVectorTowardsRight(Pivot.transform);

            if (direction == DirectionTravelling.Left)
                v = -v;

			Vector3 movement = v.normalized * distance;

			UpdatePivot = false;
			Pivot.transform.Translate(movement, Space.World);
        }

		public void MovePivotDown()
		{
			Vector3 v = OrientationHelper.GetDownwardVector(Pivot.transform);

			Vector3 movement = v.normalized * 0.5f;

			UpdatePivot = false;
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
			LeftEdge = false;
			RightEdge = false;
			ApproachingSnow = false;
			OnSnow = false;
			IsOnSlope = false;
			CurrentAcceleration = currentAcceleration;
            TrappedBetweenSlopes = false;
		}

		public void OnGroundHit()
		{
			CurrentAcceleration = new Vector2(CurrentAcceleration.x, 0);
		}

		public void OnLeftCollision()
		{
			if (CurrentAcceleration.x < 0)
				CurrentAcceleration = new Vector2(0, CurrentAcceleration.y);
			LeftCollision = true;
		}

		public void OnRightCollision()
		{
			if (CurrentAcceleration.x > 0)
				CurrentAcceleration = new Vector2(0, CurrentAcceleration.y);
			RightCollision = true;
		}

        public void OnLeftEdge()
        {
            if (IsOnSlope == false && CurrentAcceleration.x < 0)
                CurrentAcceleration = new Vector2(0, CurrentAcceleration.y);
            LeftEdge = true;
        }

        public void OnRightEdge()
        {
			if (IsOnSlope == false && CurrentAcceleration.x > 0)
                CurrentAcceleration = new Vector2(0, CurrentAcceleration.y);
            RightEdge = true;
        }

        public void SetPivotCollider(Collider2D pivotCollider)
		{
			PivotCollider = pivotCollider;
		}

		public void SetPivotCollider(Collider2D pivotCollider, ColliderPoint targetPoint, ColliderPoint characterPoint)
		{
			PivotCollider = pivotCollider;
			Pivot.transform.parent = pivotCollider.transform.parent;

			if (pivotCollider.IsUpright() && (characterPoint == ColliderPoint.TopLeft || characterPoint == ColliderPoint.TopRight))
			{
				CharacterPoint = ColliderPoint.Centre;
				PreviousCharacterPoint = characterPoint;
				TargetPoint = targetPoint;
				Pivot.transform.position = GetPivotPositionWhenCorner(targetPoint);
				UpdatePivot = false;
			}
			else
			{
				CharacterPoint = characterPoint;
				TargetPoint = targetPoint;
				Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
				UpdatePivot = true;
			}
		}

		public void SetPivotPoint(Collider2D col, Vector3 point, Vector2 normal)
		{
			if (Pivot == null)
			{
				Pivot = new GameObject();
				Pivot.name = "Pivot";
			}

			Pivot.transform.position = point;
			PivotCollider = col;
			Pivot.transform.parent = PivotCollider.transform;
			Normal = normal;
		}

		public void SetNewPivot(ClimbingState climbingState)
		{
			SetPivotCollider(climbingState.PivotCollider, climbingState.PivotPosition, climbingState.PlayerPosition);
		}

		private Vector3 GetPivotPositionWhenCorner(ColliderPoint targetPoint)
		{
			CornerCollider = PivotCollider.transform;

			Vector3 vAcross = targetPoint == ColliderPoint.TopRight ? CornerCollider.right : -CornerCollider.right;
			Vector3 vDown = -CornerCollider.up;

			return PivotCollider.GetPoint(targetPoint) + (vAcross.normalized * _colliderDimensions.x) + (vDown.normalized * _colliderDimensions.y);
		}

		public float GetCornerAngle()
		{
			return CornerCollider.rotation.eulerAngles.z;
		}

		public void UpdatePivotToTarget(bool forceOffEdge = false)
		{
			if (PivotCollider != null && UpdatePivot)
			{
				Vector2 position = PivotCollider.GetPoint(TargetPoint);
				Pivot.transform.position = position;

				if (CharacterPoint == ColliderPoint.BottomLeft || CharacterPoint == ColliderPoint.BottomRight)
				{
					float edgeAmount = forceOffEdge ? -0.2f : 0.2f;
					MovePivotAlongSurface(CharacterPoint == ColliderPoint.BottomLeft ? DirectionTravelling.Left : DirectionTravelling.Right, edgeAmount);
				}
			}
		}

		public void JumpInPlace()
		{
			UpdatePivot = false;
		}

		public void UnsetPivot()
		{
			PivotCollider = null;
		}

		public bool IsUpright()
		{
			if (Pivot.transform.parent == null)
				return false;

			IEnumerable<BoxCollider2D> edges = Pivot.transform.parent.GetComponentsInChildren<BoxCollider2D>().Where(e => e.transform != Pivot.transform.parent);
			if (edges.Count() == 2)
			{
				foreach (var edge in edges)
				{
					if (edge.IsUpright() == false)
						return false;
				}
				return true;
			}
			return false;
		}
	}
}
