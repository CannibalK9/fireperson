using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private readonly AnimationScript _anim;
		private readonly int _rightClimbLayer;
		private readonly int _leftClimbLayer;
		private bool _retryCheckAbove = true;
		private Collider2D _exception;

		public List<Climb> NextClimbs { get; set; }
		public DirectionFacing ClimbSide { get { return CurrentClimbingState.ClimbSide != DirectionFacing.None ? CurrentClimbingState.ClimbSide : NextClimbingState.ClimbSide; } }
		public ClimbingState CurrentClimbingState { get; set; }
		public ClimbingState NextClimbingState { get; set; }

		public ClimbHandler(PlayerMotor motor)
		{
			_motor = motor;
			_anim = _motor.Anim;
			_rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
			_leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);
			NextClimbs = new List<Climb>();
			CurrentClimbingState = new ClimbingState();
		}

		public void CancelClimb()
		{
			CurrentClimbingState = new ClimbingState();
		}

		public bool CheckLedgeAbove(DirectionFacing direction, out Climb outputClimb, bool retryCheck = true)
		{
			Bounds bounds = _motor.Collider.bounds;
			outputClimb = Climb.None;
			Climb currentClimb = Climb.None;

			float checkWidth = _retryCheckAbove ? 4f : 1f;
			const float checkHeight = 4f;

            float xValue = direction == DirectionFacing.Left ? bounds.center.x - (checkWidth / 2) : bounds.center.x + (checkWidth / 2);
            float yValue = bounds.min.y + ConstantVariables.MaxLipHeight;

            float actualHeight = bounds.size.y + checkHeight - ConstantVariables.MaxLipHeight;

			var origin = new Vector3(xValue, yValue);
			var size = new Vector2(checkWidth, 0.01f);

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, actualHeight, GetClimbMask());
			hits = RemoveInvalidColliders(hits);

			var hit = hits.FirstOrDefault(h => h.point.y > bounds.max.y && EdgeValidator.CanJumpToHang(h.collider, bounds.center, _motor.StandingCollider));

			if (hit)
			{
				if (ShouldStraightClimb(hit.collider))
				{
					if (currentClimb == Climb.None)
						currentClimb = Climb.Up;

					outputClimb = Climb.Up;
				}
				else
				{
					if (currentClimb == Climb.None)
						currentClimb = Climb.Flip;

					outputClimb = Climb.Flip;
				}
			}

			if (outputClimb == Climb.None)
			{
				int layer = direction == DirectionFacing.Left ? _rightClimbLayer : _leftClimbLayer;
				hit = hits.FirstOrDefault(h => h.collider.gameObject.layer == layer && h.point.y <= bounds.max.y && EdgeValidator.CanMantle(h.collider, bounds.center, _motor.CrouchedCollider));

				if (hit)
				{
					if (currentClimb == Climb.None)
						currentClimb = Climb.Mantle;

					outputClimb = Climb.Mantle;
				}
			}

			if (outputClimb != Climb.None)
			{
				CurrentClimbingState = GetStaticClimbingState();
				NextClimbingState = GetClimbingState(currentClimb, hit.collider);
			}
			else if (outputClimb == Climb.None && _retryCheckAbove && retryCheck)
			{
				_retryCheckAbove = false;
				CheckLedgeAbove(direction == DirectionFacing.Left ? DirectionFacing.Right : DirectionFacing.Left, out outputClimb);
			}
			_retryCheckAbove = true;
			return outputClimb != Climb.None;
		}

        private bool CheckLedgeWhileHanging()
        {
			Bounds bounds = _motor.Collider.bounds;

			const float checkWidth = 3f;
            const float checkHeight = 3f;

			int layer = CurrentClimbingState.PivotCollider.gameObject.layer;

			float xValue = layer == _leftClimbLayer ? bounds.max.x - (checkWidth / 2) : bounds.min.x + (checkWidth / 2);
            float yValue = bounds.max.y + 1;

            var origin = new Vector3(xValue, yValue);
            var size = new Vector2(checkWidth, 0.01f);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, checkHeight, GetClimbMask());
			hits = RemoveInvalidColliders(hits);

			var hit = hits.FirstOrDefault(h => EdgeValidator.CanJumpToHang(h.collider, bounds.center, _motor.StandingCollider));

            if (hit)
            {
				NextClimbingState = GetClimbingState(CurrentClimbingState.Climb, hit.collider);
				_anim.SetBool("inverted", ClimbSide == _motor.GetDirectionFacing());
            }
            return hit;
        }

		public bool CheckLedgeBelow(Climb intendedClimbingState, DirectionFacing direction, out string animation)
		{
			//check the edge in the direction travelling. If it's near enough, move to it and jump unless it's dropless.
			//else, if the slope is declining, set the current spot as the edge and jump, unless there's a dropless edge and we're only jumping

			bool nearEdge = false;
			bool canDrop = true;
			bool down = intendedClimbingState == Climb.Down;

			if (intendedClimbingState == Climb.Jump)
				intendedClimbingState = direction == DirectionFacing.Left ? Climb.AcrossLeft : Climb.AcrossRight;

			if (down == false
				&& _motor.MovementState.PivotCollider != null
				&& _motor.MovementState.PivotCollider.gameObject.layer == LayerMask.NameToLayer(Layers.Ice))
			{
				if (JumpInPlace(direction, canDrop, out animation))
					return true;
			}

			const float checkWidth = 4f;

            int layer = direction == DirectionFacing.Right ? _rightClimbLayer : _leftClimbLayer;
			BoxCollider2D edge = null;
			try
			{
				edge = _motor.MovementState.PivotCollider.gameObject.GetComponentsInChildren<BoxCollider2D>().SingleOrDefault(c => c.gameObject.layer == layer);

				if (edge == null)
				{
					var climbableEdges = _motor.MovementState.PivotCollider.gameObject.GetComponent<ClimbableEdges>();

					if (climbableEdges != null)
					{
						Orientation orientation = OrientationHelper.GetOrientation(_motor.MovementState.PivotCollider.transform);

						Collider2D excluded = null;
						if (orientation == Orientation.Flat)
						{
							if (direction == DirectionFacing.Left && climbableEdges.IsLeftCorner && climbableEdges.IsLeftCornerInverted == false)
								excluded = climbableEdges.LeftException;
							else if (direction == DirectionFacing.Right && climbableEdges.IsRightCorner && climbableEdges.IsRightCornerInverted == false)
								excluded = climbableEdges.RightException;
						}
						else if (orientation == Orientation.UpsideDown)
						{
							if (direction == DirectionFacing.Left && climbableEdges.IsRightCorner && climbableEdges.IsRightCornerInverted)
								excluded = climbableEdges.RightException;
							else if (direction == DirectionFacing.Right && climbableEdges.IsLeftCorner && climbableEdges.IsLeftCornerInverted)
								excluded = climbableEdges.LeftException;
						}

						if (excluded != null)
						{
							edge = excluded.gameObject.GetComponentsInChildren<BoxCollider2D>().SingleOrDefault(c => c.gameObject.layer == layer);
						}
					}
				}
			}
			catch (Exception)
			{ }

			Bounds bounds = _motor.Collider.bounds;
			bool facingEdge = (_motor.MovementState.LeftEdge && direction == DirectionFacing.Left)
				|| (_motor.MovementState.RightEdge && direction == DirectionFacing.Right);

			if (edge != null && (facingEdge || EdgeValidator.CanJumpToOrFromEdge(edge, bounds.center, _motor.CrouchedCollider)))
			{
                canDrop = edge.CanClimbDown();
                var distance = Vector2.Distance(_motor.GetGroundPivotPosition(), edge.transform.position);
                nearEdge = distance < checkWidth;

                Bounds projectedBounds = new Bounds(
                    new Vector3(
                        direction == DirectionFacing.Right
                            ? edge.transform.position.x + bounds.extents.x
                            : edge.transform.position.x - bounds.extents.x,
                        edge.transform.position.y + bounds.extents.y),
					bounds.size);

                if (nearEdge && ((down && EdgeValidator.CanClimbUpOrDown(edge, _motor.CrouchedCollider)) || (down == false && (CheckLedgeAcross(direction, projectedBounds) || canDrop))))
                {
                    CurrentClimbingState = GetClimbingState(intendedClimbingState, edge);
                    if (down)
                    {
                        animation = EdgeValidator.CanHang(edge, _motor.StandingCollider) == false
                            ? Animations.HopDown
                            : _motor.Anim.GetBool(PlayerAnimBool.Moving)
                                ? Animations.RollDown
                                : Animations.ClimbDown;
                    }
                    else
                    {
                        animation = Animations.DiveAcross;
                    }

                    return true;
                }
            }

			if (edge == null && down && facingEdge)
			{
				CurrentClimbingState = GetStaticClimbingState();
				animation = Animations.HopDown;
				_exception = null;
				return true;
			}

			bool downhill = _motor.MovementState.NormalDirection == direction && Vector2.Angle(Vector2.up, _motor.MovementState.Normal) > 20f;

            if (downhill && down == false)
            {
				_exception = edge;
				if (JumpInPlace(direction, canDrop, out animation))
					return true;
            }

            animation = "";
            return false;
        }

		private bool JumpInPlace(DirectionFacing direction, bool canDrop, out string animation)
		{
			if (CheckLedgeAcross(direction) || canDrop)
			{
				animation = Animations.DiveAcross;
				_exception = null;
				CurrentClimbingState = GetStaticClimbingState();
				return true;
			}

            animation = "";
			return false;
		}

		public bool CheckLedgeAcross(DirectionFacing direction, Bounds? projectedBounds = null)
		{
			NextClimbs.Clear();
			Bounds bounds = projectedBounds != null
				? (Bounds) projectedBounds
				: _motor.Collider.bounds;

			float maxNonHangDistance = 6f;
			float checkLength = 7f;
			const float maxHeightAbove = 1f;
			const float maxHeightBelow = 2f;
            const float spaceInFront = 2f;

			float checkDepth = bounds.size.y + maxHeightAbove + maxHeightBelow;

            float x;
            if (CurrentClimbingState.PivotCollider == null || CurrentClimbingState.CanClimbDown)
            {
                x = direction == DirectionFacing.Left ? bounds.min.x - spaceInFront : bounds.max.x + spaceInFront;
                checkLength -= spaceInFront;
            }
            else
                x = direction == DirectionFacing.Left ? bounds.min.x : bounds.max.x;

			float y = bounds.center.y - maxHeightBelow + maxHeightAbove;

			var origin = new Vector2(x, y);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());
			hits = RemoveInvalidColliders(hits);

			RaycastHit2D hit = hits.FirstOrDefault(h => h.point.y < bounds.center.y && Mathf.Abs(h.point.y - bounds.center.y) < maxNonHangDistance && EdgeValidator.CanJumpToOrFromEdge(h.collider, bounds.center, _motor.CrouchedCollider));

			bool shouldHang = false;
			Climb directionalAcross = direction == DirectionFacing.Left ? Climb.AcrossLeft : Climb.AcrossRight;

			if (hit)
			{
				shouldHang = CurrentClimbingState.PivotCollider != null && hit.collider.transform.parent == CurrentClimbingState.PivotCollider.transform.parent;
				if (shouldHang && EdgeValidator.CanJumpToHang(hit.collider, bounds.center, _motor.StandingCollider) == false)
					hit = new RaycastHit2D();
			}
			else
			{
				hit = hits.FirstOrDefault(h => EdgeValidator.CanJumpToHang(h.collider, bounds.center, _motor.StandingCollider));

				if (hit)
				{
					shouldHang = true;
				}
			}

			if (hit)
			{
				NextClimbingState = GetClimbingState(directionalAcross, hit.collider, shouldHang);
				_anim.SetBool("shouldHang", shouldHang);
				_anim.SetBool("inverted", ClimbSide == direction);
			}
			return hit;
		}

		public bool CheckGrab(DirectionFacing direction = DirectionFacing.None, bool holdingUp = false)
		{
			Bounds bounds = _motor.Collider.bounds;
			float checkLength = ConstantVariables.GrabDistance;

			var origin = new Vector2(bounds.center.x, bounds.max.y - 0.5f);
			float distance = bounds.size.x + checkLength * 2;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, new Vector2(distance, 0.1f), 0, Vector2.down, 0.2f, GetClimbMask());

			if (hits.Any())
				Debug.Log("grab");

			int layer = 0;
			if (direction == DirectionFacing.Left)
				layer = _rightClimbLayer;
			else if (direction == DirectionFacing.Right)
				layer = _leftClimbLayer;

			RaycastHit2D hit = hits.FirstOrDefault(h => (holdingUp || (h.collider.CanClimbDown() == false || h.collider.gameObject.layer == layer)) && EdgeValidator.CanJumpToHang(h.collider, bounds.center, _motor.StandingCollider));

			if (hit)
			{
				CurrentClimbingState = GetClimbingState(
					hit.collider.gameObject.layer == _leftClimbLayer ? Climb.AcrossRight : Climb.AcrossLeft,
					hit.collider,
					true);
				_motor.Anim.SetAcrossTrigger();
			}
			return hit;
		}

		private RaycastHit2D[] RemoveInvalidColliders(RaycastHit2D[] hits)
		{
			var returnedHits = CurrentClimbingState.PivotCollider == null
				? hits
				: hits.Where(h => h.collider != CurrentClimbingState.PivotCollider && h.collider != _exception).ToArray();
			return returnedHits;
		}

		public bool CheckReattach()
		{
			if (_motor.MovementState.PivotCollider == null)
				return false;

			BoxCollider2D[] edges = _motor.MovementState.PivotCollider.transform.parent.GetComponentsInChildren<BoxCollider2D>();
			foreach (BoxCollider2D edge in edges)
			{
				if (_motor.MovementState.PivotCollider.gameObject.layer == edge.gameObject.layer && Vector2.Distance(edge.bounds.center, _motor.Collider.GetTopFace()) < 2)
				{
					CurrentClimbingState = GetClimbingState(CurrentClimbingState.Climb, edge);
					_motor.MovementState.SetPivotCollider(edge);
					return true;
				}
			}
			return false;
		}

		private LayerMask GetClimbMask()
		{
			return 1 << _rightClimbLayer | 1 << _leftClimbLayer;
		}

		private ClimbingState GetClimbingState(Climb climb, Collider2D col, bool shouldHang = false)
		{
			if (CurrentClimbingState.PivotCollider != null)
				_anim.SetBool(PlayerAnimBool.OnCorner, CurrentClimbingState.IsUpright);

			return ClimbingState.GetClimbingState(climb, col, _motor.Collider, shouldHang);
		}

		private ClimbingState GetStaticClimbingState()
		{
			return ClimbingState.GetStaticClimbingState(CurrentClimbingState);
		}

		private bool ShouldStraightClimb(Collider2D col)
		{
			return col.gameObject.layer == _leftClimbLayer
				? _motor.transform.position.x < col.bounds.center.x + 0.5f
				: _motor.transform.position.x > col.bounds.center.x - 0.5f;
		}

		public Climb SwitchClimbingState(DirectionFacing direction, bool hanging = false)
		{
			NextClimbingState = new ClimbingState();
			if (CurrentClimbingState.PivotCollider == null)
				return Climb.None;

			_anim.SetBool("onCorner", CurrentClimbingState.IsUpright);

			var nextClimb = Climb.None;
			Climb currentClimb = CurrentClimbingState.Climb;
			Collider2D climbCollider = CurrentClimbingState.PivotCollider;

			if (currentClimb == Climb.AcrossLeft || currentClimb == Climb.AcrossRight)
			{
				currentClimb = CurrentClimbingState.PlayerPosition == ColliderPoint.TopLeft || CurrentClimbingState.PlayerPosition == ColliderPoint.TopRight ? Climb.Down : Climb.Up;
			}

			DirectionFacing directionFacing = _motor.GetDirectionFacing();
			if (direction == DirectionFacing.None)
				direction = directionFacing;

			bool forward = direction == directionFacing;
			_anim.SetBool(PlayerAnimBool.Forward, forward);

			switch (currentClimb)
			{
				case Climb.Mantle:
				case Climb.Flip:
				case Climb.Up:
					if (NextClimbs.Contains(Climb.Down) && CanVault() == false)
					{
						nextClimb = Climb.Down;
					}
					else if (NextClimbs.Contains(Climb.Up) && CheckLedgeAbove(direction, out nextClimb))
					{ }
					else if (NextClimbs.Contains(Climb.AcrossLeft) && ClimbSide == DirectionFacing.Left)
						nextClimb = CheckLedgeAcross(DirectionFacing.Left)
							? Climb.AcrossLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.AcrossRight) && ClimbSide == DirectionFacing.Right)
						nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
					else
					{
						_anim.SetBool(PlayerAnimBool.Moving, NextClimbs.Contains(Climb.AcrossLeft) || NextClimbs.Contains(Climb.AcrossRight));
					}

					if (nextClimb == Climb.Jump && climbCollider.CanClimbDown() == false)
					{
						nextClimb = Climb.None;
                    }
                    break;
				case Climb.Down:
				case Climb.Hang:
					if (NextClimbs.Contains(Climb.Up) && (EdgeValidator.CanClimbUpOrDown(climbCollider, _motor.CrouchedCollider) || CheckLedgeWhileHanging()))
					{
						nextClimb = Climb.Up;
					}
					else if (NextClimbs.Contains(Climb.AcrossLeft) && (CurrentClimbingState.IsUpright == false || ClimbSide == DirectionFacing.Left))
						nextClimb = hanging && directionFacing == DirectionFacing.Right
							? Climb.AcrossLeft
							: CheckLedgeAcross(DirectionFacing.Left)
								? Climb.AcrossLeft
								: Climb.Jump;
					else if (NextClimbs.Contains(Climb.AcrossRight) && (CurrentClimbingState.IsUpright == false || ClimbSide == DirectionFacing.Right))
						nextClimb = hanging && directionFacing == DirectionFacing.Left
							? Climb.AcrossRight
							: CheckLedgeAcross(DirectionFacing.Right)
								? Climb.AcrossRight
								: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Down))
						nextClimb = Climb.Down;
					else
						nextClimb = Climb.Hang;

					if ((nextClimb == Climb.Jump || nextClimb == Climb.Down) && CurrentClimbingState.CanClimbDown == false)
						nextClimb = Climb.Down;

                    if (nextClimb == Climb.Jump && NextClimbs.Contains(Climb.Up) && (NextClimbs.Contains(Climb.AcrossLeft) || NextClimbs.Contains(Climb.AcrossRight)))
                        nextClimb = Climb.None;

                    if (nextClimb == Climb.Down)
						_motor.MovementState.IsGrounded = false;

					break;
			}

			if (NextClimbingState.Climb == Climb.None)
				CurrentClimbingState.Climb = nextClimb;

			_motor.MovementState.WasOnSlope = false;
			NextClimbs.Clear();
			_motor.Anim.SetBool("sameEdge", NextClimbingState.Climb == Climb.None && nextClimb != Climb.None);

			return nextClimb;
		}

        private bool CanVault()
        {
            return CurrentClimbingState.IsUpright && CurrentClimbingState.IsCorner == false;
        }

        private void MovePivotAlongSurface()
        {
            _motor.MovementState.MovePivotAlongSurface(ClimbSide == DirectionFacing.Right ? DirectionTravelling.Left : DirectionTravelling.Right, _motor.Collider.bounds.extents.x);
        }
	}
}
