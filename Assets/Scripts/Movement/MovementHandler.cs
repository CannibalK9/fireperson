﻿#define DEBUG_CC2D_RAYS
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementHandler
	{
		private readonly IMotor _motor;
		private RaycastHit2D _downHit;
		private ColliderPoint _previousColliderPoint;

		public MovementHandler(IMotor motor)
		{
			_motor = motor;
		}

		public bool MoveLinearly(float speed)
		{
			if (_motor.MovementState.PivotCollider == null)
				return false;

			_motor.Rigidbody.isKinematic = true;
			var deltaMovement = new Vector3();
			_motor.MovementState.UpdatePivotToTarget();
			MoveTowardsPivot(ref deltaMovement, speed, _motor.Collider.GetPoint(_motor.MovementState.CharacterPoint));
			_motor.Transform.Translate(deltaMovement, Space.World);
			return true;
		}

		public bool IsCollidingWithNonPivot()
		{
			Bounds bounds = _motor.Collider.bounds;
			RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, bounds.max.y), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, bounds.size.y, Layers.Platforms);

			return hits.Any(hit => hit.collider.transform != _motor.MovementState.Pivot.transform.parent);
		}

		public void BoxCastMove(Vector3 deltaMovement, bool isKinematic)
		{
			if (Time.timeSinceLevelLoad < 1)
				return;

			if (Mathf.Abs(deltaMovement.x) < 0.05f)
				deltaMovement.x = 0;

			_motor.Rigidbody.isKinematic = isKinematic;
			_motor.MovementState.Reset(deltaMovement);
			Bounds bounds = _motor.Collider.bounds;

			RaycastHit2D[] downHits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, bounds.min.y + 0.5f), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, 1f, Layers.Platforms);
			_downHit = GetDownwardsHit(downHits);
			var leftHit = new RaycastHit2D();
			var rightHit = new RaycastHit2D();

			if (_downHit)
			{
				deltaMovement.y = 0;

				leftHit = GetDirectionHit(bounds, deltaMovement, GetSurfaceDirection(DirectionTravelling.Left));
				rightHit = GetDirectionHit(bounds, deltaMovement, GetSurfaceDirection(DirectionTravelling.Right));

				if (leftHit)
				{
					RaycastHit2D lipCast = Physics2D.Raycast(new Vector2(bounds.min.x, bounds.min.y + 1), Vector2.left, 2, Layers.Platforms);
					if (lipCast && (lipCast.collider == _downHit.collider || Vector2.Angle(lipCast.normal, Vector2.up) > _motor.SlopeLimit))
						_motor.MovementState.OnLeftCollision();
					else
						leftHit = new RaycastHit2D();
				}
				if (rightHit)
				{
					RaycastHit2D lipCast = Physics2D.Raycast(new Vector2(bounds.max.x, bounds.min.y + 1), Vector2.right, 2, Layers.Platforms);
					if (lipCast && (lipCast.collider == _downHit.collider || Vector2.Angle(lipCast.normal, Vector2.up) > _motor.SlopeLimit))
						_motor.MovementState.OnRightCollision();
					else
						rightHit = new RaycastHit2D();
				}
				ColliderPoint hitLocation;

				float n = _downHit.normal.x;

				if (Mathf.Abs(n) < 0.02f)
					hitLocation = ColliderPoint.BottomFace;
				else if (n > 0)
					hitLocation = ColliderPoint.BottomLeft;
				else
					hitLocation = ColliderPoint.BottomRight;

				Vector3 offset = _motor.Collider.GetPoint(hitLocation);

				float x = hitLocation == ColliderPoint.BottomFace ? bounds.center.x : _downHit.point.x;
				Vector3 pivotPosition = new Vector3(x, _downHit.point.y);

				if (_downHit.collider != _motor.MovementState.PivotCollider || leftHit || rightHit || hitLocation != _previousColliderPoint)
				{
					SetPivotPoint(_downHit.collider, pivotPosition);
					_previousColliderPoint = hitLocation;
					Debug.Log("pivot switched");
				}

				bool moveRight = _motor.MovementState.CurrentAcceleration.x > 0;
				if ((_motor.MovementState.IsOnSlope && ((leftHit && _downHit.normal.x < 0) || (rightHit && _downHit.normal.x > 0))) == false)
				{
					_motor.MovementState.Pivot.transform.Translate(MoveAlongSurface(), Space.World);

					pivotPosition = _motor.MovementState.Pivot.transform.position + _motor.Transform.position - offset;
					_motor.Transform.position = Vector3.MoveTowards(_motor.Transform.position, pivotPosition, 100f);
					DrawRay(_motor.MovementState.Pivot.transform.position, _downHit.normal, Color.yellow);
				}
			}
			else
			{
				_motor.MovementState.PivotCollider = null;
				_motor.Transform.Translate(deltaMovement, Space.World);
			}
		}

		private RaycastHit2D GetDirectionHit(Bounds bounds, Vector3 deltaMovement, Vector2 direction)
		{
			RaycastHit2D[] cast = Physics2D.BoxCastAll(bounds.center, new Vector2(0.001f, bounds.size.y), 0, direction, bounds.extents.x + 0.05f, Layers.Platforms);

			foreach (RaycastHit2D hit in cast.Where(hit => hit.collider != _downHit.collider && Vector2.Angle(hit.normal, Vector2.up) > _motor.SlopeLimit))
			{
				return hit;
			}
			return new RaycastHit2D();
		}

		private void DownCast(ref Vector3 deltaMovement)
		{
			if (Mathf.Abs(_downHit.distance - 1.2f) < 0.02f)
				deltaMovement.y = 0;
			else
				deltaMovement.y = 1.2f - _downHit.distance;
		}

		private RaycastHit2D GetDownwardsHit(IEnumerable<RaycastHit2D> hits)
		{
			var validHit = new RaycastHit2D();
			const float maxAngle = 90f;
			float hitAngle = maxAngle;

			foreach (RaycastHit2D hit in hits)
			{
				hitAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (_downHit && _downHit.collider != hit.collider && hitAngle >= _motor.SlopeLimit)
					continue;

				if (hitAngle < maxAngle)
				{
					validHit = hit;
					break;
				}
			}

			if (hitAngle < _motor.SlopeLimit)
				_motor.MovementState.IsGrounded = true;
			else if (hitAngle < maxAngle)
				_motor.MovementState.IsOnSlope = true;

			return validHit;
		}

		private Vector3 MoveAlongSurface()
		{
			bool moveRight;
			float speed;

			if (_motor.MovementState.IsOnSlope)
			{
				moveRight = _downHit.normal.x > 0;
				speed = 0.2f + Mathf.Abs(_motor.MovementState.CurrentAcceleration.y);
			}
			else
			{
				moveRight = _motor.MovementState.CurrentAcceleration.x > 0;
				speed = Mathf.Abs(_motor.MovementState.CurrentAcceleration.x);
			}

			Vector3 direction = GetSurfaceDirection(moveRight ? DirectionTravelling.Right : DirectionTravelling.Left);
			return direction.normalized * speed;
		}

		public Vector3 GetSurfaceDirection(DirectionTravelling direction)
		{
			return direction == DirectionTravelling.Right
				? Quaternion.Euler(0, 0, -90) * _downHit.normal
				: Quaternion.Euler(0, 0, 90) * _downHit.normal;
		}

		private void SetPivotPoint(Collider2D col, Vector3 point)
		{
			_motor.MovementState.Pivot.transform.position = point;
			_motor.MovementState.PivotCollider = col;
			_motor.MovementState.Pivot.transform.parent = _motor.MovementState.PivotCollider.transform;
			DrawRay(point, _downHit.normal, Color.yellow);
		}

		private void MoveWithPivotPoint(Collider2D col)
		{
			if (col == _motor.MovementState.PivotCollider)
			{
				_motor.Transform.Translate(_motor.MovementState.Pivot.transform.position - _motor.MovementState.PreviousPivotPoint);
			}
		}

		private void MoveTowardsPivot(ref Vector3 deltaMovement, float speed, Vector3 offset)
		{
			Vector3 pivotPosition = _motor.MovementState.Pivot.transform.position + _motor.Transform.position - offset;
			Vector3 newPosition = Vector3.MoveTowards(_motor.Transform.position, pivotPosition, speed);
			Vector3 movement= newPosition - _motor.Transform.position;
			deltaMovement += movement;
		}

		[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
		void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			Debug.DrawRay(start, dir, color);
		}
	}
}
