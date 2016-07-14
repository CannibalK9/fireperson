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
		public bool IgnoreCurrentPlatform { get; set; }
		public GameObject Pivot { get; private set; }
		public Vector3 CurrentAcceleration { get; private set; }
		public Vector3 PreviousPivotPoint { get; set; }
		public ColliderPoint CharacterPoint { get; private set; }
		public ColliderPoint TargetPoint { get; private set; }

		public MovementState()
		{
			Pivot = new GameObject();
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
			if (CurrentAcceleration.x < 0)
				deltaMovement.x = 0;
			LeftCollision = true;
		}

		public void OnRightCollision(ref Vector3 deltaMovement)
		{
			if (CurrentAcceleration.x > 0)
				deltaMovement.x = 0;
			RightCollision = true;
		}

		public void SetPivot(Collider2D pivotCollider, ColliderPoint targetPoint, ColliderPoint characterPoint)
		{
			PivotCollider = pivotCollider;
			Pivot.transform.parent = pivotCollider.transform.parent;
			CharacterPoint = characterPoint;
			TargetPoint = targetPoint;
			Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
			PreviousPivotPoint = Pivot.transform.position;
		}

		public void UpdatePivotToTarget()
		{
			if (PivotCollider != null)
			{
				PreviousPivotPoint = Pivot.transform.position;
				Pivot.transform.position = PivotCollider.GetPoint(TargetPoint);
			}
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
