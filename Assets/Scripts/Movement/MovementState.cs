﻿using Assets.Scripts.Helpers;
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
		public Vector3 PreviousPivotPoint { get; set; }
		public ColliderPoint CharacterPoint { get; set; }
		public ColliderPoint TargetPoint { get; private set; }
		public Vector3 Normal { get; set; }
		private bool _updatePivot;

		public MovementState()
		{
			Pivot = new GameObject();
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

			_updatePivot = false;
            Pivot.transform.Translate(v.normalized * distance, Space.World);
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
			CharacterPoint = characterPoint;
			TargetPoint = targetPoint;
			Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
			PreviousPivotPoint = Pivot.transform.position;
			_updatePivot = true;
		}

		public void UpdatePivotToTarget()
		{
			if (PivotCollider != null && _updatePivot)
			{
				PreviousPivotPoint = Pivot.transform.position;
				Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
			}
		}

		public void UnsetPivot()
		{
			PivotCollider = null;
		}
	}
}
