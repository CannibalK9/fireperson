#define DEBUG_CC2D_RAYS
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
			if (speed > 0)
			{
				MoveTowardsPivot(ref deltaMovement, speed, _motor.Collider.GetPoint(_motor.MovementState.CharacterPoint));
				Debug.Log("towards");
			}
			else
			{
				MoveWithPivotPoint(ref deltaMovement, _motor.MovementState.PivotCollider);
				Debug.Log("with");
			}
			_motor.Transform.Translate(deltaMovement, Space.World);
			return true;
		}

		public bool IsCollidingWithNonPivot()
		{
			Bounds bounds = _motor.Collider.bounds;
			RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, bounds.max.y), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, bounds.size.y, Layers.Platforms);

			return hits.Any(hit => hit.collider.transform != _motor.MovementState.Pivot.transform.parent);
		}

		public void BoxCastMove(Vector3 deltaMovement, bool isKinetic)
		{
			if (Time.timeSinceLevelLoad < 1)
				return;

			_motor.Rigidbody.isKinematic = isKinetic;
			_motor.MovementState.Reset(deltaMovement);
			Bounds bounds = _motor.Collider.bounds;

			float extraWidth = _downHit ? 0.2f : 0;

			RaycastHit2D[] downHits = Physics2D.BoxCastAll(new Vector2(bounds.center.x + deltaMovement.x, bounds.min.y + 1.2f), new Vector2(bounds.size.x + extraWidth, 0.001f), 0, Vector2.down, 1.4f, Layers.Platforms);
			_downHit = GetDownwardsHit(downHits);

			if (_downHit)
			{
				DownCast(ref deltaMovement);
			}

			var leftHit = new RaycastHit2D();
			var rightHit = new RaycastHit2D();

			if (_motor.MovementState.IsOnSlope == false)
			{
				if (_downHit)
				{
					leftHit = GetDirectionHit(bounds, deltaMovement, GetSurfaceDirection(DirectionTravelling.Left));
					rightHit = GetDirectionHit(bounds, deltaMovement, GetSurfaceDirection(DirectionTravelling.Right));

					if (leftHit)
						_motor.MovementState.OnLeftCollision(ref deltaMovement);
					else if (rightHit)
						_motor.MovementState.OnRightCollision(ref deltaMovement);
					else
						MoveWithPivotPoint(ref deltaMovement, _downHit.collider);
				}
			}

			if (_downHit)
			{
				SetPivotPoint(
					_downHit.collider,
					_downHit.normal == Vector2.up
						? _motor.Collider.GetBottomCenter()
						: _downHit.point);

				bool moveRight = _motor.MovementState.CurrentAcceleration.x > 0;
				if ((leftHit == false && rightHit == false) || (leftHit && moveRight) || (rightHit && moveRight == false))
					MoveAlongSurface(ref deltaMovement);
			}
			_motor.Transform.Translate(deltaMovement, Space.World);
		}

		private RaycastHit2D GetDirectionHit(Bounds bounds, Vector3 deltaMovement, Vector2 direction)
		{
			RaycastHit2D[] cast = Physics2D.BoxCastAll(bounds.center + deltaMovement, new Vector2(0.001f, bounds.size.y), 0, direction, bounds.extents.x + 0.1f, Layers.Platforms);

			foreach (RaycastHit2D hit in cast.Where(hit => hit.collider != _downHit.collider && Vector2.Angle(hit.normal, Vector2.up) > _motor.SlopeLimit))
			{
				return hit;
			}
			return new RaycastHit2D();
		}

		private void DownCast(ref Vector3 deltaMovement)
		{
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

		private void MoveAlongSurface(ref Vector3 deltaMovement)
		{
			bool moveRight;
			float speed;

			if (_motor.MovementState.IsOnSlope)
			{
				moveRight = _downHit.normal.x > 0;
				speed = 0.2f + Mathf.Abs(_motor.MovementState.CurrentAcceleration.y);
				deltaMovement.x = 0;
			}
			else
			{
				moveRight = _motor.MovementState.CurrentAcceleration.x > 0;
				speed = Mathf.Abs(_motor.MovementState.CurrentAcceleration.x);
			}

			Vector3 direction = GetSurfaceDirection(moveRight ? DirectionTravelling.Right : DirectionTravelling.Left);
			TranslatePivot(ref deltaMovement, direction, speed);
		}

		public Vector3 GetSurfaceDirection(DirectionTravelling direction)
		{
			return direction == DirectionTravelling.Right
				? Quaternion.Euler(0, 0, -90) * _downHit.normal
				: Quaternion.Euler(0, 0, 90) * _downHit.normal;
		}

		private static void TranslatePivot(ref Vector3 deltaMovement, Vector3 direction, float speed)
		{
			deltaMovement += direction.normalized * speed;
		}

		private void SetPivotPoint(Collider2D col, Vector2 point)
		{
			_motor.MovementState.PreviousPivotPoint = point;
			_motor.MovementState.Pivot.transform.position = point;
			if (_motor.MovementState.PivotCollider != col)
			{
				_motor.MovementState.PivotCollider = col;
				_motor.MovementState.Pivot.transform.parent = _motor.MovementState.PivotCollider.transform;
			}
			DrawRay(point, _downHit.normal, Color.yellow);
		}

		private void MoveWithPivotPoint(ref Vector3 deltaMovement, Collider2D col)
		{
			if (col == _motor.MovementState.PivotCollider)
			{
				deltaMovement += _motor.MovementState.Pivot.transform.position - _motor.MovementState.PreviousPivotPoint;
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
