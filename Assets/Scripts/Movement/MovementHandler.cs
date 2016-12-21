#define DEBUG_CC2D_RAYS
using Assets.Scripts.Helpers;
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
		private float _angle;
		private RaycastHit2D _lipHit;
		private bool _leftHit = false;
		private bool _rightHit = false;
		private Transform _currentPlatform;
		private bool _reset = false;

		public MovementHandler(IMotor motor)
		{
			_motor = motor;
		}

		public bool MoveLinearly(float speed, bool applyRotation = false, bool forceOffEdge = false)
		{
			if (_motor.MovementState.PivotCollider == null)
				return false;

			IgnorePlatforms(true);
			_motor.MovementState.UpdatePivotToTarget(forceOffEdge);

			float distance;
			Vector2 characterPoint = _motor.Collider.GetPoint(_motor.MovementState.CharacterPoint);
			Vector3 movement = MoveTowardsPivot(out distance, speed, characterPoint);
			_motor.Transform.Translate(movement, Space.World);

			if (applyRotation)
			{
				float angle = _motor.MovementState.GetCornerAngle();
				float rotation = Mathf.Lerp(angle, 0, distance);
				if (_angle != angle)
					_motor.Transform.rotation = Quaternion.Euler(0,0, rotation);

				_angle = angle;
			}
			else
				_motor.Transform.rotation = Quaternion.Euler(0, 0, 0);

			return true;
		}

		public bool IsCollidingWithNonPivot(bool lowerHalf)
		{
			float collisionFudge = 0.2f;
			Bounds bounds = _motor.Collider.bounds;
			float startY = lowerHalf ? bounds.center.y : bounds.max.y;
			float distance = lowerHalf ? bounds.extents.y : bounds.size.y;
			RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, startY - collisionFudge), new Vector2(bounds.size.x - (collisionFudge * 2), 0.001f), 0, Vector2.down, distance - (collisionFudge * 2), Layers.Platforms);

			return ClimbCollision.IsCollisionInvalid(hits, _motor.MovementState.Pivot.transform);
		}

		public void SetMovementCollisions(Vector3 deltaMovement, bool jumping)
		{
			BoxCastSetup(deltaMovement);
			Bounds bounds = _motor.Collider.bounds;

			List<RaycastHit2D> downHits = Physics2D.BoxCastAll(new Vector2(bounds.center.x, bounds.min.y + 0.5f), new Vector2(bounds.size.x, 0.001f), 0, Vector2.down, 1f, Layers.Platforms).ToList();

			if (jumping)
				RemoveCurrentPlatform(downHits);
			else
				_currentPlatform = null;

			_downHit = GetDownwardsHit(downHits);

			if (_downHit)
				_motor.MovementState.OnGroundHit();

			bool offGround = jumping || _downHit == false;

			_leftHit = _motor.MovementState.CurrentAcceleration.x > 0 ? false : DirectionCast(bounds, DirectionTravelling.Left, offGround);
			_rightHit = _motor.MovementState.CurrentAcceleration.x < 0 ? false : DirectionCast(bounds, DirectionTravelling.Right, offGround);
		}

		private void RemoveCurrentPlatform(List<RaycastHit2D> hits)
		{
			if (_currentPlatform == null)
				_currentPlatform = _motor.MovementState.Pivot.transform.parent;

			RaycastHit2D hitToRemove = hits.SingleOrDefault(h => h.collider.transform == _currentPlatform);
			hits.Remove(hitToRemove);
		}

		public void BoxCastMove()
		{
			if (Time.timeSinceLevelLoad < 1)
				return;

			Bounds bounds = _motor.Collider.bounds;

			if (_downHit)
			{
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

				_motor.MovementState.TrappedBetweenSlopes = _motor.MovementState.IsOnSlope
					&& ((_leftHit && _downHit.normal.x < 0)
						|| (_rightHit && _downHit.normal.x > 0));

				bool directionHit = _leftHit || _rightHit;
				if ((_reset && directionHit == false) || _motor.MovementState.Pivot.transform.parent == null || hitLocation != _previousColliderPoint || _downHit.collider != _motor.MovementState.PivotCollider || Vector2.Distance(_motor.MovementState.Pivot.transform.localPosition, _motor.MovementState.Pivot.transform.parent.InverseTransformPoint(pivotPosition)) > 0.05f)//  _downHit.collider != _motor.MovementState.PivotCollider || leftHit || rightHit || hitLocation != _previousColliderPoint || Vector2.Distance(_motor.MovementState.Pivot.transform.position, bounds.center) > 10f)
				{
					_motor.MovementState.SetPivotPoint(_downHit.collider, pivotPosition, _downHit.normal);
				}              
				_previousColliderPoint = hitLocation;
				_reset = directionHit;

                if (_motor.MovementState.TrappedBetweenSlopes == false && directionHit == false)
                {
					if (_lipHit)
					{
						_motor.MovementState.SetPivotPoint(_lipHit.collider, _lipHit.point, _lipHit.normal);
						_motor.Transform.position = _motor.MovementState.Pivot.transform.position + _motor.Transform.position - offset;
					}
					else
					{
						_motor.MovementState.Normal = _downHit.normal;
						_motor.MovementState.Pivot.transform.Translate(MoveAlongSurface(), Space.World);
						_motor.Rigidbody.MovePosition(_motor.MovementState.Pivot.transform.position + _motor.Transform.position - offset);
					}
                }
			}
			else
			{
				float rotation = _motor.Transform.rotation.eulerAngles.z;

				if (rotation != 0)
				{
					if (rotation < 180)
						_motor.Transform.Rotate(Vector3.forward, -rotation / 2);
					else
						_motor.Transform.Rotate(Vector3.forward, (360 - rotation) / 2);
				}

				_motor.MovementState.PivotCollider = null;
				_motor.Transform.Translate(_motor.MovementState.CurrentAcceleration, Space.World);
			}
		}

        private void BoxCastSetup(Vector3 deltaMovement)
        {
            _angle = 0;
			_lipHit = new RaycastHit2D();
			_leftHit = false;
			_rightHit = false;

            if (Mathf.Abs(deltaMovement.x) < 0.05f)
                deltaMovement.x = 0;

            IgnorePlatforms(false);
            _motor.MovementState.Reset(deltaMovement);
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

		private bool DirectionCast(Bounds bounds, DirectionTravelling direction, bool offGround)
		{
			Vector2 dir = direction == DirectionTravelling.Left ? Vector2.left : Vector2.right;
			RaycastHit2D hit = GetDirectionHit(bounds, dir);
			
			if (hit)
			{
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Layers.Ice))
					_motor.MovementState.ApproachingSnow = true;

				float lipHeight = bounds.min.y + ConstantVariables.MaxLipHeight;
				if (offGround || hit.point.y > lipHeight)
				{
					if (direction == DirectionTravelling.Left)
						_motor.MovementState.OnLeftCollision();
					else
						_motor.MovementState.OnRightCollision();
					return hit;
				}

				if (direction == DirectionTravelling.Left)
				{
					//special player crouch check/set
					RaycastHit2D lipHit = Physics2D.Raycast(new Vector2(bounds.min.x - 0.3f, bounds.max.y), Vector2.down, bounds.size.y, Layers.Platforms);
					if (ActualSidewaysCollision(lipHit, lipHeight))
						_motor.MovementState.OnLeftCollision();
					else
					{
						hit = new RaycastHit2D();
						_lipHit = lipHit;
					}
				}
				else
				{
					RaycastHit2D lipHit = Physics2D.Raycast(new Vector2(bounds.max.x + 0.3f, bounds.max.y), Vector2.down, bounds.size.y, Layers.Platforms);
					if (ActualSidewaysCollision(lipHit, lipHeight))
						_motor.MovementState.OnRightCollision();
					else
					{
						hit = new RaycastHit2D();
						_lipHit = lipHit;
					}
				}
			}
			if (offGround)
				return false;
			else
				return hit || AtEdge(bounds, direction);
		}

        private bool AtEdge(Bounds bounds, DirectionTravelling direction)
        {
			if (_motor.MovementState.IsOnSlope || _downHit.transform.gameObject.layer == LayerMask.NameToLayer(Layers.Ice))
				return false;

            float xOrigin = direction == DirectionTravelling.Right
                ? bounds.max.x + 0.1f
                : bounds.min.x - 0.1f;
            var edgeRay = new Vector2(xOrigin, bounds.min.y);

            Debug.DrawRay(edgeRay, _motor.MovementState.GetSurfaceDownDirection(), Color.blue);
            RaycastHit2D hit = Physics2D.Raycast(edgeRay, _motor.MovementState.GetSurfaceDownDirection(), 1.5f, Layers.Platforms);
            bool atEdge = hit == false ? true : Vector2.Angle(Vector2.up, hit.normal) > _motor.SlopeLimit;

            if (atEdge)
            {
                edgeRay += new Vector2(direction == DirectionTravelling.Right ? 1 : -1, ConstantVariables.MaxLipHeight);
                RaycastHit2D hit2 = Physics2D.Raycast(edgeRay, Vector2.down, 1.5f + ConstantVariables.MaxLipHeight, Layers.Platforms);
                atEdge = hit2 == false ? true : Vector2.Angle(Vector2.up, hit2.normal) > _motor.SlopeLimit;
            }

            if (atEdge)
            {
                if (direction == DirectionTravelling.Right)
                    _motor.MovementState.OnRightEdge();
                else
                    _motor.MovementState.OnLeftEdge();
            }

            return atEdge;
        }

        private bool ActualSidewaysCollision(RaycastHit2D lipCast, float lipHeight)
		{
			return lipCast && (lipCast.collider == _downHit.collider || lipCast.point.y > lipHeight || Vector2.Angle(lipCast.normal, Vector2.up) > _motor.SlopeLimit);
		}

		private RaycastHit2D GetDirectionHit(Bounds bounds, Vector2 direction)
		{
			RaycastHit2D[] cast = Physics2D.BoxCastAll(bounds.center, new Vector2(0.01f, bounds.size.y), 0, direction, bounds.extents.x + 0.1f, Layers.Platforms);

			foreach (RaycastHit2D hit in cast.Where(hit => hit.collider != _downHit.collider && hit.transform != _currentPlatform && Vector2.Angle(hit.normal, Vector2.up) > _motor.SlopeLimit))
			{
				return hit;
			}
			return new RaycastHit2D();
		}

		private RaycastHit2D GetDownwardsHit(List<RaycastHit2D> hits)
		{
			var validHit = new RaycastHit2D();
			const float maxAngle = 90f;
			float hitAngle = maxAngle;

			if (_motor.MovementState.CurrentAcceleration.x < 0)
				hits.OrderBy(h => h.point.x);
			else
				hits.OrderByDescending(h => h.point.x);

			foreach (RaycastHit2D hit in hits)
			{
				hitAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Layers.Ice) && hitAngle <= 90)
				{
					_motor.MovementState.ApproachingSnow = true;
					_motor.MovementState.IsGrounded = true;
					return hit;
				}

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
			bool isGoingUpSteepSlope = direction.y > 0 && Vector2.Angle(direction, moveRight ? Vector2.right : Vector2.left) > _motor.SlopeLimit + 10;

			return isGoingUpSteepSlope
				? Vector3.zero
				: direction.normalized * speed;
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
