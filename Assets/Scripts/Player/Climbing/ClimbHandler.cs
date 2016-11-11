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
		private Collider2D _climbCollider;
		private Transform _climbParent;
		private int _climbLayer;
		private readonly AnimationScript _anim;
		private readonly int _rightClimbLayer;
		private readonly int _leftClimbLayer;
		private ColliderPoint _target;
		private ColliderPoint _player;
		private bool _retryCheckAbove = true;
		private bool _shouldHang;
		private bool _sameEdge;
		private Collider2D _exception;

		public Climb CurrentClimb { get; set; }
		public List<Climb> NextClimbs { get; set; }
		public DirectionFacing ClimbSide { get; set; }
		public float DistanceToEdge { get; private set; }

		public ClimbHandler(PlayerMotor motor)
		{
			_motor = motor;
			_anim = _motor.Anim;
			_climbCollider = null;
			_rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
			_leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);
			NextClimbs = new List<Climb>();
		}

		public ClimbingState GetClimbingState(bool recalculate)
		{
			if (CurrentClimb == Climb.End || recalculate == false)
			{
				if (_motor.ClimbingState == null)
					return new ClimbingState(CurrentClimb, _climbCollider, ConstantVariables.DefaultMovementSpeed, ColliderPoint.Centre, ColliderPoint.Centre, false);
				_motor.ClimbingState.Climb = CurrentClimb;
				_motor.ClimbingState.Recalculate = false;
				return _motor.ClimbingState;
			}

			float climbingSpeed = ConstantVariables.DefaultMovementSpeed;

			switch (CurrentClimb)
			{
				case Climb.Up:
				case Climb.Flip:
					Hanging();
					break;
				case Climb.Mantle:
					Mantle();
					break;
				case Climb.Down:
					OffEdge();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
				case Climb.AcrossLeft:
				case Climb.AcrossRight:
					if (_shouldHang)
						Hanging();
					else
						OffEdge();
					climbingSpeed = ConstantVariables.AcrossSpeed;
					break;
				case Climb.MoveToEdge:
					OffEdge();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
			}
			 return new ClimbingState(CurrentClimb, _climbCollider, climbingSpeed, _target, _player, true);
		}

		public void CancelClimb()
		{
			if (CurrentClimb != Climb.MoveToEdge)
			{
				CurrentClimb = Climb.None;
				_climbCollider = null;
			}
		}

		private void Hanging()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.TopLeft;
			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.TopRight;
			}
		}

		private void Mantle()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.LeftFace;
			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.RightFace;
			}
		}

		private void OffEdge()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.BottomLeft;

			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.BottomRight;
			}
		}

		public bool CheckLedgeAbove(DirectionFacing direction, out Climb climb, bool retryCheck = true)
		{
			climb = Climb.End;

			float checkWidth = _retryCheckAbove ? 4f : 1f;
			const float checkHeight = 4f;

            float xValue = direction == DirectionFacing.Left ? _motor.Collider.bounds.center.x - (checkWidth / 2) : _motor.Collider.bounds.center.x + (checkWidth / 2);
            float yValue = _motor.Collider.bounds.min.y + ConstantVariables.MaxLipHeight;

            float actualHeight = _motor.Collider.bounds.size.y + checkHeight - ConstantVariables.MaxLipHeight;

			var origin = new Vector3(xValue, yValue);
			var size = new Vector2(checkWidth, 0.01f);

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, actualHeight, GetClimbMask());
			hits = RemoveInvalidColliders(hits);

			var hit = hits.FirstOrDefault(h => EdgeValidator.CanJumpToHang(h.collider, _motor.Collider.bounds));

			if (hit)
			{
				if (ShouldStraightClimb(hit.collider))
				{
					if (CurrentClimb == Climb.None)
						CurrentClimb = Climb.Up;

					climb = Climb.Up;
				}
				else
				{
					if (CurrentClimb == Climb.None)
						CurrentClimb = Climb.Flip;

					climb = Climb.Flip;
				}
			}

			if (climb == Climb.End)
			{
				hit = hits.FirstOrDefault(h => h.point.y < _motor.Collider.bounds.max.y && EdgeValidator.CanMantle(h.collider, _motor.CrouchedCollider.bounds));

				if (hit)
				{
					if (CurrentClimb == Climb.None)
						CurrentClimb = Climb.Mantle;

					climb = Climb.Mantle;
				}
			}

			if (climb != Climb.End)
				SetClimbingParameters(hit.collider);
			else if (climb == Climb.End && _retryCheckAbove && retryCheck)
			{
				_retryCheckAbove = false;
				CheckLedgeAbove(direction == DirectionFacing.Left ? DirectionFacing.Right : DirectionFacing.Left, out climb);
			}
			_retryCheckAbove = true;
			return climb != Climb.End;
		}

        private bool CheckLedgeWhileHanging()
        {
            const float checkWidth = 3f;
            const float checkHeight = 3f;

			int layer = _climbCollider.gameObject.layer;

			float xValue = layer == _leftClimbLayer ? _motor.Collider.bounds.max.x - (checkWidth / 2) : _motor.Collider.bounds.min.x + (checkWidth / 2);
            float yValue = _motor.Collider.bounds.max.y + 1;

            var origin = new Vector3(xValue, yValue);
            var size = new Vector2(checkWidth, 0.01f);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, checkHeight, GetClimbMask());
			hits = RemoveInvalidColliders(hits);

			var hit = hits.FirstOrDefault(h => EdgeValidator.CanJumpToHang(h.collider, _motor.Collider.bounds));

            if (hit)
            {
				SetClimbingParameters(hit.collider);
				_anim.SetBool("inverted", ClimbSide == _motor.GetDirectionFacing());
				_sameEdge = false;
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

			if (down == false
				&& _motor.MovementState.PivotCollider != null
				&& _motor.MovementState.PivotCollider.gameObject.layer == LayerMask.NameToLayer(Layers.Ice))
			{
				if (JumpInPlace(direction, intendedClimbingState, canDrop, out animation))
					return true;
			}

			const float checkWidth = 2f;

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

            if (edge != null && EdgeValidator.CanJumpToOrFromEdge(edge, _motor.Collider.bounds))
			{
                canDrop = edge.CanClimbDown();
                var distance = Vector2.Distance(_motor.GetGroundPivotPosition(), edge.transform.position);
                nearEdge = distance < checkWidth;

                Bounds projectedBounds = new Bounds(
                    new Vector3(
                        direction == DirectionFacing.Right
                            ? edge.transform.position.x + _motor.Collider.bounds.extents.x
                            : edge.transform.position.x - _motor.Collider.bounds.extents.x,
                        edge.transform.position.y + _motor.Collider.bounds.extents.y),
                    _motor.Collider.bounds.size);

                if (nearEdge && ((down && EdgeValidator.CanClimbUpOrDown(edge, _motor.CrouchedCollider.bounds)) || CheckLedgeAcross(direction, projectedBounds) || canDrop))
                {
                    CurrentClimb = intendedClimbingState;
                    SetClimbingParameters(edge);
                    DistanceToEdge = distance;
                    if (down)
                    {
                        animation = EdgeValidator.CanHang(edge, _motor.Collider.bounds) == false
                            ? Animations.HopDown
                            : _motor.Anim.GetBool(PlayerAnimBool.Moving)
                                ? Animations.RollDown
                                : Animations.ClimbDown;
                    }
                    else
                    {
                        NextClimbs.Add(
                            direction == DirectionFacing.Left
                            ? Climb.AcrossLeft
                            : Climb.AcrossRight);
                        animation = Animations.MoveToEdge;
                    }

                    return true;
                }
            }

            bool downhill = _motor.MovementState.NormalDirection == direction && Vector2.Angle(Vector2.up, _motor.MovementState.Normal) > 20f;

            if (downhill && down == false)
            {
				_exception = edge;
				if (JumpInPlace(direction, intendedClimbingState, canDrop, out animation))
					return true;
            }

            animation = "";
            return false;
        }

		private bool JumpInPlace(DirectionFacing direction, Climb intendedClimbingState, bool canDrop, out string animation)
		{
			if (CheckLedgeAcross(direction))
			{
				_climbParent = null;
				CurrentClimb = intendedClimbingState;
				_climbCollider = _motor.MovementState.PivotCollider;
				NextClimbs.Add(
					direction == DirectionFacing.Left
					? Climb.AcrossLeft
					: Climb.AcrossRight);
				_motor.Anim.SwitchClimbingState();
				_motor.Anim.ResetAcrossTrigger();
				animation = Animations.DiveAcross;
				_exception = null;
				return true;
			}
			else if (canDrop)
			{
				CurrentClimb = intendedClimbingState;
				_climbCollider = _motor.MovementState.PivotCollider;
				_motor.MovementState.JumpInPlace();
				_motor.Anim.SwitchClimbingState();
				animation = Animations.DiveAcross;
				_exception = null;
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

			float checkLength = 6f;
			const float maxHeightAbove = 1f;
			const float maxHeightBelow = 2f;
            const float spaceInFront = 2f;

			float checkDepth = bounds.size.y + maxHeightAbove + maxHeightBelow;

            float x;
            if (_climbCollider == null || _climbCollider.CanClimbDown())
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

			RaycastHit2D hit = hits.FirstOrDefault(h => h.point.y < bounds.center.y && EdgeValidator.CanJumpToOrFromEdge(h.collider, _motor.CrouchedCollider.bounds));

			if (hit)
			{
				if (projectedBounds != null)
					return hit;

				Transform previousClimbParent = _climbParent;
				_shouldHang = hit.collider.transform.parent == previousClimbParent;
			}
			else
			{
				hit = hits.FirstOrDefault(h => EdgeValidator.CanJumpToHang(h.collider, _motor.Collider.bounds));

				if (hit)
				{
					if (projectedBounds != null)
						return hit;
					_shouldHang = true;
				}
			}

			if (hit)
			{
				SetClimbingParameters(hit.collider);
				_anim.SetBool("shouldHang", _shouldHang);
				_anim.SetBool("inverted", ClimbSide == direction);
			}
			return hit;
		}

		public bool CheckGrab(DirectionFacing direction = DirectionFacing.None, bool holdingUp = false)
		{
			float checkLength = ConstantVariables.GrabDistance;

			var origin = new Vector2(_motor.Collider.bounds.center.x, _motor.Collider.bounds.max.y - 0.5f);

			var size = new Vector2(_motor.Collider.bounds.size.x + checkLength * 2, 0.01f);

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.down, 0.1f, GetClimbMask());

			int layer = 0;
			if (direction == DirectionFacing.Left)
				layer = _rightClimbLayer;
			else if (direction == DirectionFacing.Right)
				layer = _leftClimbLayer;

			RaycastHit2D hit = hits.FirstOrDefault(h => (holdingUp || (h.collider.CanClimbDown() == false || h.collider.gameObject.layer == layer)) && EdgeValidator.CanJumpToHang(h.collider, _motor.Collider.bounds));

			if (hit)
			{
				SetClimbingParameters(hit.collider);
				_shouldHang = true;
				CurrentClimb = hit.collider.gameObject.layer == _leftClimbLayer ? Climb.AcrossRight : Climb.AcrossLeft;
				_motor.ClimbingState = GetClimbingState(true);
				_motor.Anim.SetAcrossTrigger();
			}
			return hit;
		}

		private RaycastHit2D[] RemoveInvalidColliders(RaycastHit2D[] hits)
		{
			var returnedHits = _climbCollider == null
				? hits
				: hits.Where(h => h.collider != _climbCollider && h.collider != _exception).ToArray();
			return returnedHits;
		}

		public bool CheckReattach()
		{
			BoxCollider2D[] edges = _climbParent.GetComponentsInChildren<BoxCollider2D>();
			foreach (BoxCollider2D edge in edges)
			{
				if (_climbLayer == edge.gameObject.layer && Vector2.Distance(edge.bounds.center, _motor.Collider.GetTopFace()) < 2)
				{
					SetClimbingParameters(edge);
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

		private void SetClimbingParameters(Collider2D col)
		{
			if (_climbCollider != null)
				_anim.SetBool("onCorner", _climbCollider.IsUpright());

			_climbCollider = col;
			_climbParent = col.transform.parent;
			_climbLayer = col.gameObject.layer;
			ClimbSide = GetClimbingSide(col);
			_anim.SetBool("falling", true);
			_anim.SetBool("corner", col.IsUpright());
			_anim.SetBool("upright", col.IsUpright() && col.IsCorner() == false);
		}

		private DirectionFacing GetClimbingSide(Collider2D col)
		{
			return col.gameObject.layer == _rightClimbLayer
				? DirectionFacing.Right
				: DirectionFacing.Left;
		}

		private bool ShouldStraightClimb(Collider2D col)
		{
			return col.gameObject.layer == _leftClimbLayer
				? _motor.transform.position.x < col.bounds.center.x
				: _motor.transform.position.x > col.bounds.center.x;
		}

		public ClimbingState SwitchClimbingState(DirectionFacing direction)
		{
			if (_climbCollider == null)
				return new ClimbingState(Climb.None, null, 1, ColliderPoint.BottomFace, ColliderPoint.BottomFace, false);

			_anim.SetBool("onCorner", _climbCollider.IsUpright());

			bool recalculate = true;

			var nextClimb = CurrentClimb == Climb.End || CurrentClimb == Climb.Jump ? Climb.None : Climb.End;

			if (CurrentClimb == Climb.AcrossLeft || CurrentClimb == Climb.AcrossRight)
			{
				CurrentClimb = _shouldHang ? Climb.Down : Climb.Up;
			}

			DirectionFacing directionFacing = _motor.GetDirectionFacing();
			if (direction == DirectionFacing.None)
				direction = directionFacing;

			bool forward = direction == directionFacing || (_climbCollider.IsUpright() && CurrentClimb == Climb.Down);
			_anim.SetBool(PlayerAnimBool.Forward, forward);

			switch (CurrentClimb)
			{
				case Climb.Mantle:
				case Climb.Flip:
				case Climb.Up:
                    if (NextClimbs.Contains(Climb.Down))
                    {
						recalculate = false;
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
                        MovePivotAlongSurface();

					if (nextClimb == Climb.Jump && _climbCollider.CanClimbDown() == false)
					{
						nextClimb = Climb.End;
                        MovePivotAlongSurface();
                    }
                    break;
				case Climb.Down:
					_sameEdge = true;
					if (NextClimbs.Contains(Climb.Up) && (EdgeValidator.CanClimbUpOrDown(_climbCollider, _motor.CrouchedCollider.bounds) || CheckLedgeWhileHanging()))
					{
                        nextClimb = Climb.Up;
                        _motor.Anim.SetBool("sameEdge", _sameEdge);
						if (_sameEdge)
                            recalculate = false;
                    }
                    else if (NextClimbs.Contains(Climb.AcrossLeft) && (_climbCollider.IsUpright() == false || ClimbSide == DirectionFacing.Left))
						if (forward)
							nextClimb = CheckLedgeAcross(DirectionFacing.Left)
								? Climb.AcrossLeft
								: Climb.Jump;
						else
						{
							nextClimb = Climb.AcrossLeft;
							recalculate = false;
						}
					else if (NextClimbs.Contains(Climb.AcrossRight) && (_climbCollider.IsUpright() == false || ClimbSide == DirectionFacing.Right))
						if (forward)
							nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
						else
						{
							nextClimb = Climb.AcrossRight;
							recalculate = false;
						}
					else if (NextClimbs.Contains(Climb.Down))
						nextClimb = Climb.Down;

					if ((nextClimb == Climb.Jump || nextClimb == Climb.Down) && _climbCollider.CanClimbDown() == false)
						nextClimb = Climb.End;

                    if (nextClimb == Climb.Jump && NextClimbs.Contains(Climb.Up) && (NextClimbs.Contains(Climb.AcrossLeft) || NextClimbs.Contains(Climb.AcrossRight)))
                        nextClimb = Climb.End;

                    if (nextClimb == Climb.Down)
						_motor.MovementState.IsGrounded = false;

					break;
				case Climb.MoveToEdge:
					if (NextClimbs.Contains(Climb.AcrossLeft) && CheckLedgeAcross(DirectionFacing.Left))
						nextClimb = Climb.AcrossLeft;
					else if (NextClimbs.Contains(Climb.AcrossRight) && CheckLedgeAcross(DirectionFacing.Right))
						nextClimb = Climb.AcrossRight;
					else if (NextClimbs.Contains(Climb.Down))
						nextClimb = Climb.Down;
					else if (_climbCollider.CanClimbDown())
						nextClimb = Climb.Jump;
					else if (_motor.MovementState.WasOnSlope == false)
						MovePivotAlongSurface();
					else
						_motor.MovementState.IsOnSlope = true;
					break;
			}

			_motor.MovementState.WasOnSlope = false;
			CurrentClimb = nextClimb;
			NextClimbs.Clear();

			if (CurrentClimb == Climb.End || CurrentClimb == Climb.None || CurrentClimb == Climb.Jump)
				recalculate = false;

			return GetClimbingState(recalculate);
		}

        private bool CanVault()
        {
            return _climbCollider.IsUpright() && _climbCollider.IsCorner() == false;
        }

        private void MovePivotAlongSurface()
        {
            _motor.MovementState.MovePivotAlongSurface(ClimbSide == DirectionFacing.Right ? DirectionTravelling.Left : DirectionTravelling.Right, _motor.Collider.bounds.extents.x);
        }
	}
}
