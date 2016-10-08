#define DEBUG_CC2D_RAYS
using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using System.Collections.Generic;
using System.Linq;
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

		public bool MoveLinearly(float speed, bool applyRotation = false)
		{
			if (_motor.MovementState.PivotCollider == null)
				return false;

			IgnorePlatforms(true);
			_motor.MovementState.UpdatePivotToTarget();

			float distance;
			Vector2 characterPoint = _motor.Collider.GetPoint(_motor.MovementState.CharacterPoint);
			Vector3 movement = MoveTowardsPivot(out distance, speed, characterPoint);
			_motor.Transform.Translate(movement, Space.World);

			if (applyRotation)
			{
				float fullRotation;
				
				if (_motor.MovementState.PivotCollider.IsUpright())
					fullRotation= OrientationHelper.GetRotationConsideringOrientation(_motor.MovementState.Pivot.transform.parent);
				else
				{
					bool left = OrientationHelper.GetOrientation(_motor.MovementState.Pivot.transform.parent.rotation.eulerAngles.z) == Orientation.Flat
						? _motor.MovementState.Pivot.transform.position.x < _motor.MovementState.Pivot.transform.parent.position.x
						: _motor.MovementState.Pivot.transform.position.x > _motor.MovementState.Pivot.transform.parent.position.x;
					
					Collider2D cornerCollider = left
						? _motor.MovementState.Pivot.transform.parent.GetComponent<ClimbableEdges>().LeftException
						: _motor.MovementState.Pivot.transform.parent.GetComponent<ClimbableEdges>().RightException;

					fullRotation = OrientationHelper.GetCornerRotationConsideringOrientation(cornerCollider.transform);
				}
				float rotation = Mathf.Lerp(fullRotation, 0, distance);
				_motor.Transform.rotation = Quaternion.Euler(0,0, rotation);
			}
			else
				_motor.Transform.rotation = Quaternion.Euler(0, 0, 0);

			return true;
		}

		public bool IsCollidingWithNonPivot()
		{
			Bounds bounds = _motor.Collider.bounds;
			RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, bounds.max.y), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, bounds.size.y, Layers.Platforms);

			return hits.Any(hit =>
				hit.collider.transform != _motor.MovementState.Pivot.transform.parent
				&& hit.collider != _motor.MovementState.Pivot.transform.parent.GetComponent<ClimbableEdges>().LeftException
				&& hit.collider != _motor.MovementState.Pivot.transform.parent.GetComponent<ClimbableEdges>().RightException);
		}

		public void BoxCastMove(Vector3 deltaMovement, bool isKinematic)
		{
			if (Time.timeSinceLevelLoad < 1)
				return;

			if (Mathf.Abs(deltaMovement.x) < 0.05f)
				deltaMovement.x = 0;

			IgnorePlatforms(isKinematic);
            _motor.MovementState.Reset(deltaMovement);
			Bounds bounds = _motor.Collider.bounds;

			if (isKinematic == false)
			{
				RaycastHit2D[] downHits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, bounds.min.y + 0.5f), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, 1f, Layers.Platforms);
				_downHit = GetDownwardsHit(downHits);
			}

			if (isKinematic == false && _downHit)
			{
				_motor.MovementState.Normal = _downHit.normal;
				deltaMovement.y = 0;

				RaycastHit2D leftHit = DirectionCast(bounds, DirectionTravelling.Left);
				RaycastHit2D rightHit = DirectionCast(bounds, DirectionTravelling.Right);

				ColliderPoint hitLocation;

				float n = _downHit.normal.x;

				if (Mathf.Abs(n) < 0.02f)
					hitLocation = ColliderPoint.BottomFace;
				else if (n > 0)
					hitLocation = ColliderPoint.BottomLeft;
				else
					hitLocation = ColliderPoint.BottomRight;

                _motor.MovementState.CharacterPoint = hitLocation;
				Vector3 offset = _motor.Collider.GetPoint(hitLocation);

				float x = hitLocation == ColliderPoint.BottomFace ? bounds.center.x : _downHit.point.x;
				var pivotPosition = new Vector3(x, _downHit.point.y);

				float rotation = _motor.Transform.rotation.eulerAngles.z;

				if (rotation != 0)
				{
					if (rotation < 180)
						_motor.Transform.RotateAround(pivotPosition, Vector3.forward, -rotation / 2);
					else
						_motor.Transform.RotateAround(pivotPosition, Vector3.forward, (360 - rotation) / 2);
				}

				if (_downHit.collider != _motor.MovementState.PivotCollider || leftHit || rightHit || hitLocation != _previousColliderPoint)
				{
					SetPivotPoint(_downHit.collider, pivotPosition);
					_previousColliderPoint = hitLocation;
				}

                _motor.MovementState.TrappedBetweenSlopes = _motor.MovementState.IsOnSlope
                    && ((leftHit && _downHit.normal.x < 0)
                        || (rightHit && _downHit.normal.x > 0));

                if (_motor.MovementState.TrappedBetweenSlopes == false)
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

		public void IgnorePlatforms(bool ignore)
		{
			_motor.Rigidbody.isKinematic = ignore;
			int layer = _motor.Transform.gameObject.layer;
			Physics2D.IgnoreLayerCollision(layer, LayerMask.NameToLayer(Layers.OutdoorMetal), ignore);
			Physics2D.IgnoreLayerCollision(layer, LayerMask.NameToLayer(Layers.OutdoorWood), ignore);
			Physics2D.IgnoreLayerCollision(layer, LayerMask.NameToLayer(Layers.IndoorMetal), ignore);
			Physics2D.IgnoreLayerCollision(layer, LayerMask.NameToLayer(Layers.IndoorWood), ignore);
		}

		private RaycastHit2D DirectionCast(Bounds bounds, DirectionTravelling direction)
		{
			RaycastHit2D hit = GetDirectionHit(bounds, _motor.MovementState.GetSurfaceDirection(direction));
			
			if (hit)
			{
				float lipHeight = bounds.min.y + ConstantVariables.MaxLipHeight;
				if (hit.point.y > lipHeight)
				{
					if (direction == DirectionTravelling.Left)
						_motor.MovementState.OnLeftCollision();
					else
						_motor.MovementState.OnRightCollision();
					return hit;
				}

				Vector2 dir = direction == DirectionTravelling.Left ? Vector2.left : Vector2.right;

				if (direction == DirectionTravelling.Left)
				{
					RaycastHit2D lipCast = Physics2D.Raycast(new Vector2(bounds.min.x, lipHeight), dir, 2, Layers.Platforms);
					if (ActualSidewaysCollision(lipCast))
						_motor.MovementState.OnLeftCollision();
					else
						hit = new RaycastHit2D();
				}
				else
				{
					RaycastHit2D lipCast = Physics2D.Raycast(new Vector2(bounds.max.x, lipHeight), dir, 2, Layers.Platforms);
					if (ActualSidewaysCollision(lipCast))
						_motor.MovementState.OnRightCollision();
					else
						hit = new RaycastHit2D();
				}
			}
			return hit;
		}

		private bool ActualSidewaysCollision(RaycastHit2D lipCast)
		{
			return lipCast && (lipCast.collider == _downHit.collider || Vector2.Angle(lipCast.normal, Vector2.up) > _motor.SlopeLimit);
		}

		private RaycastHit2D GetDirectionHit(Bounds bounds, Vector2 direction)
		{
			RaycastHit2D[] cast = Physics2D.BoxCastAll(bounds.center, new Vector2(0.001f, bounds.size.y), 0, direction, bounds.extents.x + 0.05f, Layers.Platforms);

			foreach (RaycastHit2D hit in cast.Where(hit => hit.collider != _downHit.collider && Vector2.Angle(hit.normal, Vector2.up) > _motor.SlopeLimit))
			{
				return hit;
			}
			return new RaycastHit2D();
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
				{
					hitAngle = maxAngle;
					continue;
				}

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

			moveRight = _motor.MovementState.CurrentAcceleration.x > 0;
			speed = Mathf.Abs(_motor.MovementState.CurrentAcceleration.x);

			Vector3 direction = _motor.MovementState.GetSurfaceDirection(moveRight ? DirectionTravelling.Right : DirectionTravelling.Left);
			return direction.normalized * speed;
		}

		private void SetPivotPoint(Collider2D col, Vector3 point)
		{
			_motor.MovementState.Pivot.transform.position = point;
			_motor.MovementState.PivotCollider = col;
			_motor.MovementState.Pivot.transform.parent = _motor.MovementState.PivotCollider.transform;
			DrawRay(point, _downHit.normal, Color.yellow);
		}

		private Vector3 MoveTowardsPivot(out float distance, float speed, Vector3 offset)
		{
			Vector3 pivotPosition = _motor.MovementState.Pivot.transform.position + _motor.Transform.position - offset;
			distance = Vector2.Distance(_motor.Transform.position, pivotPosition);
			Vector3 newPosition = Vector3.MoveTowards(_motor.Transform.position, pivotPosition, speed);
			return newPosition - _motor.Transform.position;
		}

		[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
		void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			Debug.DrawRay(start, dir, color);
		}
	}
}
