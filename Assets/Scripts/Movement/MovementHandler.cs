#define DEBUG_CC2D_RAYS
using System;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementHandler
	{
		private readonly IController _controller;
		private RaycastHit2D _downHit;
		private Collider2D _previousCollider;
		private Vector3 _previousPivotPoint;
		private bool _pivotPointOnBase;

		public MovementHandler(IController controller)
		{
			_controller = controller;
		}

			//more casts to cover col faces

		public void MoveLinearly(float speed, Vector2 controllerPosition)
		{
			Vector3 deltaMovement = new Vector3();
			MoveWithPivotPoint(ref deltaMovement);
			MoveTowardsPivot(ref deltaMovement, speed, controllerPosition);
			_controller.Transform.Translate(deltaMovement, Space.World);
		}

		public void BoxCastMove(Vector3 deltaMovement)
		{
				_controller.MovementState.Reset(deltaMovement);

				Bounds bounds = _controller.BoxCollider.bounds;

				if (_controller is PilotedLightController == false)
				{
					RaycastHit2D[] downHits = Physics2D.BoxCastAll(new Vector3(bounds.center.x, bounds.min.y + 1.2f), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, 1.4f, Layers.Platforms);

					if (downHits.Length > 0)
					{
						DownCast(downHits, ref deltaMovement);
						MoveWithPivotPoint(ref deltaMovement);
						_controller.MovementState.PivotCollider = null;

						MoveAlongSurface(ref deltaMovement);
					}
				}

			if (_controller.MovementState.IsOnSlope == false)
			{
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
						_controller.MovementState.OnRightCollision(ref deltaMovement);
						MoveInDirection(downLeft, upRightHit, ref deltaMovement);
					}
					RaycastHit2D upLeftHit = GetCollisionHit(new Vector3(bounds.min.x + 0.2f, bounds.max.y - 0.2f) + deltaMovement, upLeft);
					if (upLeftHit)
					{
						_controller.MovementState.OnLeftCollision(ref deltaMovement);
						MoveInDirection(downRight, upLeftHit, ref deltaMovement);
					}

					RaycastHit2D downLeftHit = GetCollisionHit(new Vector3(bounds.min.x + 0.2f, bounds.min.y + 0.2f) + deltaMovement, downLeft);
					if (downLeftHit && downLeftHit.collider != _downHit.collider)
					{
						_controller.MovementState.OnLeftCollision(ref deltaMovement);
						MoveInDirection(upRight, downLeftHit, ref deltaMovement);
					}
					RaycastHit2D downRightHit = GetCollisionHit(new Vector3(bounds.max.x - 0.2f, bounds.min.y + 0.2f) + deltaMovement, downRight);
					if (downRightHit && downRightHit.collider != _downHit.collider)
					{
						_controller.MovementState.OnRightCollision(ref deltaMovement);
						MoveInDirection(upLeft, downRightHit, ref deltaMovement);
					}
					if (count > 10)
						break;
				}
				Debug.logger.Log(count);
			}
			_controller.Transform.Translate(deltaMovement, Space.World);
		}

		private RaycastHit2D GetCollisionHit(Vector2 origin, Vector2 size)
		{
			DrawRay(origin, size, Color.grey);
			return Physics2D.Raycast(origin, size, Mathf.Sqrt(0.2f * 0.2f * 2), Layers.Platforms);
		}

		private void DownCast(RaycastHit2D[] downHits, ref Vector3 deltaMovement)
		{
			_downHit = GetDownwardsHit(downHits);

			if (_downHit)
			{
				deltaMovement.y = 1.2f - _downHit.distance;
				if (_downHit.normal == Vector2.up)
					SetPivotPoint(_downHit.collider, _controller.BoxCollider.GetBottomCenter(), true);
				else
					SetPivotPoint(_downHit.collider, _downHit.point, false);
			}
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

			if (angle < _controller.SlopeLimit)
				_controller.MovementState.IsGrounded = true;
			else if (angle < 90)
				_controller.MovementState.IsOnSlope = true;

			return validHit;
		}

		private void MoveAlongSurface(ref Vector3 deltaMovement)
		{
			bool moveRight;
			float speed;

			if (_controller.MovementState.IsOnSlope)
			{
				moveRight = _downHit.normal.x > 0;
				speed = 0.2f + Mathf.Abs(_controller.MovementState.CurrentAcceleration.y);
			}
			else
			{
				moveRight = _controller.MovementState.CurrentAcceleration.x > 0;
				speed = Mathf.Abs(_controller.MovementState.CurrentAcceleration.x);

			}
			Vector3 direction = moveRight
				? Quaternion.Euler(0, 0, -90) * _downHit.normal
				: Quaternion.Euler(0, 0, 90) * _downHit.normal;

			_controller.MovementState.GroundPivot.transform.position += direction * speed;
			deltaMovement += _controller.MovementState.GroundPivot.transform.position - _previousPivotPoint;
		}

		private void MoveInDirection(Vector2 castDirection, RaycastHit2D hit, ref Vector3 deltaMovement)
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

		private void SetPivotPoint(Collider2D col, Vector2 point, bool movingToBase)
		{
			_controller.MovementState.PivotCollider = col;
			if (_pivotPointOnBase != movingToBase || _controller.MovementState.PivotCollider != _previousCollider)
			{
				_controller.MovementState.GroundPivot.transform.position = point;
				_controller.MovementState.GroundPivot.transform.parent = _controller.MovementState.PivotCollider.transform;
				_pivotPointOnBase = movingToBase;
				_previousCollider = null;
			}
			DrawRay(point, _downHit.normal, Color.yellow);
		}

		private void MoveWithPivotPoint(ref Vector3 deltaMovement)
		{
			if (_controller.MovementState.PivotCollider != null)
			{
				if (_previousCollider == _controller.MovementState.PivotCollider)
				{
					deltaMovement += _controller.MovementState.GroundPivot.transform.position - _previousPivotPoint;

				}
				_previousCollider = _controller.MovementState.PivotCollider;
				_previousPivotPoint = _controller.MovementState.GroundPivot.transform.position;
			}
		}

		private void MoveTowardsPivot(ref Vector3 deltaMovement, float speed, Vector3 offset)
		{
			Vector3 pivotPosition = _controller.MovementState.GroundPivot.transform.position + _controller.Transform.position - offset;
			Vector3 newPosition = Vector3.MoveTowards(_controller.Transform.position, pivotPosition, speed);
			deltaMovement += newPosition - _controller.Transform.position;
		}

		[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
		void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			Debug.DrawRay(start, dir, color);
		}
	}
}
