#define DEBUG_CC2D_RAYS
using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementHandler
	{
		public bool IsFacingRight = true;

		private readonly IController _controller;
		private RaycastHit2D _raycastHit;
		private CharacterRaycastOrigins _raycastOrigins;
		private Vector3 _previousBuildingPosition;
        private Transform _buildingTransform;
        public Transform BuildingTransform
        {
            get { return _buildingTransform; }
            set
            {
                _buildingTransform = value;
                _previousBuildingPosition = _buildingTransform.position;
                _controller.RaycastHitsThisFrame.Clear();
            }
        }

        private const float _kSkinWidthFloatFudgeFactor = 0.001f;
		private bool _isGoingUpSlope;

		public MovementHandler(IController controller)
		{
			_controller = controller;
		}

        public void MoveWithBuilding()
        {
            if (_controller.RaycastHitsThisFrame.Count > 0 && BuildingTransform != _controller.RaycastHitsThisFrame[0].transform)
                BuildingTransform = _controller.RaycastHitsThisFrame[0].transform;

            if (BuildingTransform != null)
            {
                _controller.Transform.position += new Vector3(
                    BuildingTransform.position.x - _previousBuildingPosition.x,
                    BuildingTransform.position.y - _previousBuildingPosition.y);

                _previousBuildingPosition = _buildingTransform.position;
            }
        }

		public void Move(Vector3 deltaMovement)
		{
			_controller.CollisionState.WasGroundedLastFrame = _controller.CollisionState.Below;
			_controller.CollisionState.Reset();
			_controller.RaycastHitsThisFrame.Clear();
			_isGoingUpSlope = false;

			PrimeRaycastOrigins();

			if (deltaMovement.y < 0f && _controller.CollisionState.WasGroundedLastFrame)
				HandleVerticalSlope(ref deltaMovement);

			if (deltaMovement.x != 0f)
				MoveHorizontally(ref deltaMovement);

			if (deltaMovement.y != 0f)
				MoveVertically(ref deltaMovement);

			_controller.Transform.Translate(deltaMovement, Space.World);

			if (Time.deltaTime > 0f)
				_controller.Velocity = deltaMovement / Time.deltaTime;

			if (!_controller.CollisionState.WasGroundedLastFrame && _controller.CollisionState.Below)
				_controller.CollisionState.BecameGroundedThisFrame = true;

			if (_isGoingUpSlope)
				_controller.Velocity = new Vector3(_controller.Velocity.x, 0, _controller.Velocity.z);
		}

		/// <summary>
		/// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
		/// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
		/// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
		/// actually moving the player
		/// </summary>
		private void MoveHorizontally(ref Vector3 deltaMovement)
		{
			IsFacingRight = deltaMovement.x > 0;
			var rayDistance = Mathf.Abs(deltaMovement.x) + _controller.SkinWidth;
			var rayDirection = IsFacingRight ? Vector2.right : -Vector2.right;
			var initialRayOrigin = IsFacingRight ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;

			for (var i = 0; i < _controller.TotalHorizontalRays; i++)
			{
				var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _controller.VerticalDistanceBetweenRays);

				DrawRay(ray, rayDirection * rayDistance, Color.red);

				_raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, _controller.PlatformMask);

				if (_raycastHit)
				{
					// the bottom ray can hit a slope but no other ray can so we have special handling for these cases
					if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(_raycastHit.normal, Vector2.up)))
					{
						_controller.RaycastHitsThisFrame.Add(_raycastHit);
						break;
					}

					// set our new deltaMovement and recalculate the rayDistance taking it into account
					deltaMovement.x = _raycastHit.point.x - ray.x;
					rayDistance = Mathf.Abs(deltaMovement.x);

					// remember to remove the skinWidth from our deltaMovement
					if (IsFacingRight)
					{
						deltaMovement.x -= _controller.SkinWidth;
						_controller.CollisionState.Right = true;
					}
					else
					{
						deltaMovement.x += _controller.SkinWidth;
						_controller.CollisionState.Left = true;
					}

					_controller.RaycastHitsThisFrame.Add(_raycastHit);

					// we add a small fudge factor for the float operations here. if our rayDistance is smaller
					// than the width + fudge bail out because we have a direct impact
					if (rayDistance < _controller.SkinWidth + _kSkinWidthFloatFudgeFactor)
					{
						float x = IsFacingRight
							? _controller.SkinWidth + _kSkinWidthFloatFudgeFactor
							: -_controller.SkinWidth + _kSkinWidthFloatFudgeFactor;

						Vector2 origin = IsFacingRight
							? _controller.BoxCollider.bounds.max
							: _raycastOrigins.TopLeft;

						float colliderHeight = _controller.BoxCollider.bounds.size.y;

						float xMove = IsFacingRight
							? 0.3f
							: -0.3f;

						RaycastHit2D hit = Physics2D.Raycast(origin + new Vector2(x, 0), Vector2.down, colliderHeight, _controller.PlatformMask);

						if (hit && colliderHeight - hit.distance < 1f)
						{
							_controller.Transform.position += new Vector3(xMove, hit.point.y - _controller.BoxCollider.bounds.min.y);
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// handles adjusting deltaMovement if we are going up a slope.
		/// </summary>
		/// <returns><c>true</c>, if horizontal slope was handled, <c>false</c> otherwise.</returns>
		/// <param name="deltaMovement">Delta movement.</param>
		/// <param name="angle">Angle.</param>
		private bool HandleHorizontalSlope(ref Vector3 deltaMovement, float angle)
		{
			// disregard 90 degree angles (walls)
			if (Mathf.RoundToInt(angle) == 90)
				return false;

			// if we can walk on slopes and our angle is small enough we need to move up
			if (angle < _controller.SlopeLimit)
			{
				// we only need to adjust the deltaMovement if we are not jumping
				// TODO: this uses a magic number which isn't ideal! The alternative is to have the user pass in if there is a jump this frame
				const float jumpingThreshold = 0.07f;

				if (deltaMovement.y < jumpingThreshold)
				{
					// apply the slopeModifier to slow our movement up the slope
					var slopeModifier = _controller.SlopeSpeedMultiplier.Evaluate(angle);
					deltaMovement.x *= slopeModifier;

					// we dont set collisions on the sides for this since a slope is not technically a side collision.
					// smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
					// to our new x location using our good friend Pythagoras
					deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
					var isGoingRight = deltaMovement.x > 0;

					// safety check. we fire a ray in the direction of movement just in case the diagonal we calculated above ends up
					// going through a wall. if the ray hits, we back off the horizontal movement to stay in bounds.
					var ray = isGoingRight ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;
					RaycastHit2D raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, _controller.PlatformMask);

					if (raycastHit)
					{
						// we crossed an edge when using Pythagoras calculation, so we set the actual delta movement to the ray hit location
						deltaMovement = (Vector3)raycastHit.point - ray;
						if (isGoingRight)
							deltaMovement.x -= _controller.SkinWidth;
						else
							deltaMovement.x += _controller.SkinWidth;
					}

					_isGoingUpSlope = true;
					_controller.CollisionState.Below = true;
				}
			}
			else // too steep. get out of here
			{
				deltaMovement.x = 0;
			}

			return true;
		}

		private void MoveVertically(ref Vector3 deltaMovement)
		{
			var isGoingUp = deltaMovement.y > 0;
			var rayDistance = Mathf.Abs(deltaMovement.y) + _controller.SkinWidth;
			var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
			var initialRayOrigin = isGoingUp ? _raycastOrigins.TopLeft : _raycastOrigins.BottomLeft;

			// apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
			initialRayOrigin.x += deltaMovement.x;

			// if we are moving up, we should ignore the layers in oneWayPlatformMask
			var mask = _controller.PlatformMask;


			for (var i = 0; i < _controller.TotalVerticalRays; i++)
			{
				var ray = new Vector2(initialRayOrigin.x + i * _controller.HorizontalDistanceBetweenRays, initialRayOrigin.y);

				DrawRay(ray, rayDirection * rayDistance, Color.red);
				_raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);
				if (_raycastHit)
				{
					// set our new deltaMovement and recalculate the rayDistance taking it into account
					deltaMovement.y = _raycastHit.point.y - ray.y;
					rayDistance = Mathf.Abs(deltaMovement.y);

					// remember to remove the skinWidth from our deltaMovement
					if (isGoingUp)
					{
						deltaMovement.y -= _controller.SkinWidth;
						_controller.CollisionState.Above = true;
					}
					else
					{
						deltaMovement.y += _controller.SkinWidth;
						_controller.CollisionState.Below = true;
					}

					_controller.RaycastHitsThisFrame.Add(_raycastHit);

					// this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
					// where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
					if (!isGoingUp && deltaMovement.y > 0.00001f)
						_isGoingUpSlope = true;

					// we add a small fudge factor for the float operations here. if our rayDistance is smaller
					// than the width + fudge bail out because we have a direct impact
					if (rayDistance < _controller.SkinWidth + _kSkinWidthFloatFudgeFactor)
						break;
				}
			}
		}

		/// <summary>
		/// checks the center point under the BoxCollider2D for a slope. If it finds one then the deltaMovement is adjusted so that
		/// the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.
		/// </summary>
		/// <param name="deltaMovement">Delta movement.</param>
		private void HandleVerticalSlope(ref Vector3 deltaMovement)
		{
			// slope check from the center of our collider
			var centerOfCollider = (_raycastOrigins.BottomLeft.x + _raycastOrigins.BottomRight.x) * 0.5f;
			var rayDirection = -Vector2.up;

			// the ray distance is based on our slopeLimit
			float slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);
			var slopeCheckRayDistance = slopeLimitTangent * (_raycastOrigins.BottomRight.x - centerOfCollider);

			var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.BottomLeft.y);
			DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
			_raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, _controller.PlatformMask);
			if (_raycastHit)
			{
				// bail out if we have no slope
				var angle = Vector2.Angle(_raycastHit.normal, Vector2.up);
				if (angle == 0)
					return;

				// we are moving down the slope if our normal and movement direction are in the same x direction
				var isMovingDownSlope = Mathf.Sign(_raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);
				if (isMovingDownSlope)
				{
					// going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
					var slopeModifier = _controller.SlopeSpeedMultiplier.Evaluate(-angle);
					// we add the extra downward movement here to ensure we "stick" to the surface below
					deltaMovement.y += _raycastHit.point.y - slopeRay.y - _controller.SkinWidth;
					deltaMovement.x *= slopeModifier;
					_controller.CollisionState.MovingDownSlope = true;
					_controller.CollisionState.SlopeAngle = angle;
				}
			}
		}
		[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
		void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			Debug.DrawRay(start, dir, color);
		}
		/// <summary>
		/// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
		/// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
		/// </summary>
		/// <param name="futurePosition">Future position.</param>
		/// <param name="deltaMovement">Delta movement.</param>
		private void PrimeRaycastOrigins()
		{
			// our raycasts need to be fired from the bounds inset by the skinWidth
			var modifiedBounds = _controller.BoxCollider.bounds;
			modifiedBounds.Expand(-2f * _controller.SkinWidth);

			_raycastOrigins = new CharacterRaycastOrigins
			{
				TopLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y),
				BottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y),
				BottomLeft = modifiedBounds.min
			};
		}
	}
}
