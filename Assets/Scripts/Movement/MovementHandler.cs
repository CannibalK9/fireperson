#define DEBUG_CC2D_RAYS
using System;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player.PL;
using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementHandler
	{
		private readonly IMotor _motor;
		private RaycastHit2D _downHit;
		private Collider2D _previousCollider;
		private Vector3 _previousPivotPoint;
		private bool _pivotPointOnBase;

		public MovementHandler(IMotor motor)
		{
			_motor = motor;
		}

			//more casts to cover col faces

		public void MoveLinearly(float speed, Vector2 controllerPosition)
		{
			var deltaMovement = new Vector3();
			MoveWithPivotPoint(ref deltaMovement);
			MoveTowardsPivot(ref deltaMovement, speed, controllerPosition);
			_motor.Transform.Translate(deltaMovement, Space.World);
		}

		public void Rotate(Transform t, ColliderPoint playerCorner)
		{
			var deltaMovement = new Vector3();
			MoveWithPivotPoint(ref deltaMovement);
			if (_motor.MovementState.GroundPivot != null)
				t.RotateAround(_motor.MovementState.GroundPivot.transform.position, Vector3.forward, 12);
			else
				t.Rotate(0, 0, 12);
			_motor.Transform.Translate(deltaMovement, Space.World);
		}

		//if ignoreing the current collider, this should apply to all casts and only be turned off when hitting the ground with a different collider

		public void BoxCastMove(Vector3 deltaMovement)
		{
			if (Time.timeSinceLevelLoad < 1)
				return;

			_motor.MovementState.Reset(deltaMovement);

			Bounds bounds = _motor.BoxCollider.bounds;

			if (_motor is PilotedLightController == false)
			{
				RaycastHit2D[] downHits = Physics2D.BoxCastAll(new Vector3(bounds.center.x, bounds.min.y + 1.2f), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, 1.4f, Layers.Platforms);
				_downHit = GetDownwardsHit(downHits);
			}

			int count = 0;
			while (true)
			{
				count++;
				RaycastHit2D anyHit = Physics2D.BoxCast(new Vector3(bounds.center.x, bounds.min.y) + deltaMovement, new Vector2(bounds.size.x, 0.001f), 0, Vector2.up, bounds.size.y, Layers.Platforms);

				if (anyHit == false)
					break;

				Vector2 upRight = Vector2.right + Vector2.up;
				Vector2 upLeft = Vector2.left + Vector2.up;
				Vector2 downRight = Vector2.down + Vector2.right;
				Vector2 downLeft = Vector2.down + Vector2.left;

				RaycastHit2D upRightHit = GetCollisionHit(new Vector3(bounds.max.x - 0.2f, bounds.max.y - 0.2f) + deltaMovement, upRight);
				if (upRightHit)
				{
					_motor.MovementState.OnRightCollision(ref deltaMovement);
					MoveInDirection(downLeft, upRightHit, ref deltaMovement);
				}
				RaycastHit2D upLeftHit = GetCollisionHit(new Vector3(bounds.min.x + 0.2f, bounds.max.y - 0.2f) + deltaMovement, upLeft);
				if (upLeftHit)
				{
					_motor.MovementState.OnLeftCollision(ref deltaMovement);
					MoveInDirection(downRight, upLeftHit, ref deltaMovement);
				}
				RaycastHit2D downLeftHit = GetCollisionHit(new Vector3(bounds.min.x + 0.2f, bounds.min.y + 0.2f) + deltaMovement, downLeft);
				if (downLeftHit && downLeftHit.collider != _downHit.collider)
				{
					_motor.MovementState.OnLeftCollision(ref deltaMovement);
					MoveInDirection(upRight, downLeftHit, ref deltaMovement);
				}
				RaycastHit2D downRightHit = GetCollisionHit(new Vector3(bounds.max.x - 0.2f, bounds.min.y + 0.2f) + deltaMovement, downRight);
				if (downRightHit && downRightHit.collider != _downHit.collider)
				{
					_motor.MovementState.OnRightCollision(ref deltaMovement);
					MoveInDirection(upLeft, downRightHit, ref deltaMovement);
				}
				if (count > 10)
					break;
			}

			if (_downHit)
			{
				DownCast(ref deltaMovement);
				MoveWithPivotPoint(ref deltaMovement);
				_motor.MovementState.PivotCollider = null;

				MoveAlongSurface(ref deltaMovement);
			}
			_motor.Transform.Translate(deltaMovement, Space.World);
		}

		private RaycastHit2D GetCollisionHit(Vector2 origin, Vector2 size)
		{
			DrawRay(origin, size, Color.grey);
			return Physics2D.Raycast(origin, size, Mathf.Sqrt(0.2f * 0.2f * 2) + 0.05f, Layers.Platforms);
		}

		private void DownCast(ref Vector3 deltaMovement)
		{
			deltaMovement.y = 1.2f - _downHit.distance;
			if (_downHit.normal == Vector2.up)
				SetPivotPoint(_downHit.collider, _motor.BoxCollider.GetBottomCenter(), true);
			else
				SetPivotPoint(_downHit.collider, _downHit.point, false);
		}

		private RaycastHit2D GetDownwardsHit(RaycastHit2D[] hits)
		{
			var validHit = new RaycastHit2D();
			float angle = 90f;

			foreach (RaycastHit2D hit in hits)
			{
				float hitAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (hitAngle < angle)
				{
					validHit = hit;
					angle = hitAngle;
				}
			}

			if (angle < _motor.SlopeLimit)
				_motor.MovementState.IsGrounded = true;
			else if (angle < 90)
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
			}
			else
			{
				moveRight = _motor.MovementState.CurrentAcceleration.x > 0;
				speed = Mathf.Abs(_motor.MovementState.CurrentAcceleration.x);

			}
			Vector3 direction = moveRight
				? Quaternion.Euler(0, 0, -90) * _downHit.normal
				: Quaternion.Euler(0, 0, 90) * _downHit.normal;

			_motor.MovementState.GroundPivot.transform.position += direction * speed;
			deltaMovement += _motor.MovementState.GroundPivot.transform.position - _previousPivotPoint;
		}

		private void MoveInDirection(Vector2 castDirection, RaycastHit2D hit, ref Vector3 deltaMovement)
		{
			if (hit.distance < 0.2f)
			{
				float distance = Math.Abs(hit.fraction) < 0.01f
					? hit.distance
					: (hit.distance / hit.fraction) - hit.distance;
				Vector3 movement = distance * castDirection;

				deltaMovement += movement;
				if (hit.normal.y < 0)
					DrawRay(hit.point, hit.normal, Color.yellow);
				else if (hit.normal.y > 0)
					DrawRay(hit.point, hit.normal, Color.cyan);
				else if (hit.normal.x > 0)
					DrawRay(hit.point, hit.normal, Color.red);
				else if (hit.normal.x < 0)
					DrawRay(hit.point, hit.normal, Color.green);
			}
		}

		private void SetPivotPoint(Collider2D col, Vector2 point, bool movingToBase)
		{
			_motor.MovementState.PivotCollider = col;
			if (_pivotPointOnBase != movingToBase || _motor.MovementState.PivotCollider != _previousCollider)
			{
				_motor.MovementState.GroundPivot.transform.position = point;
				_motor.MovementState.GroundPivot.transform.parent = _motor.MovementState.PivotCollider.transform;
				_pivotPointOnBase = movingToBase;
				_previousCollider = null;
			}
			DrawRay(point, _downHit.normal, Color.yellow);
		}

		private void MoveWithPivotPoint(ref Vector3 deltaMovement)
		{
			if (_motor.MovementState.PivotCollider != null)
			{
				if (_previousCollider == _motor.MovementState.PivotCollider)
				{
					deltaMovement += _motor.MovementState.GroundPivot.transform.position - _previousPivotPoint;
				}
				_previousCollider = _motor.MovementState.PivotCollider;
				_previousPivotPoint = _motor.MovementState.GroundPivot.transform.position;
			}
		}

		private void MoveTowardsPivot(ref Vector3 deltaMovement, float speed, Vector3 offset)
		{
			Vector3 pivotPosition = _motor.MovementState.GroundPivot.transform.position + _motor.Transform.position - offset;
			Vector3 newPosition = Vector3.MoveTowards(_motor.Transform.position, pivotPosition, speed);
			deltaMovement += newPosition - _motor.Transform.position;
		}

		[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
		void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			Debug.DrawRay(start, dir, color);
		}
	}
}
