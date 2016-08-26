using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private Collider2D _climbCollider;
		private readonly Collider2D _playerCollider;
		private readonly AnimationScript _anim;
		private readonly int _rightClimbLayer;
		private readonly int _leftClimbLayer;
		private ColliderPoint _target;
		private ColliderPoint _player;
		bool _retryCheckAbove = true;

		public Climb CurrentClimb { get; set; }
		public List<Climb> NextClimbs { get; set; }
		public DirectionFacing ClimbSide { get; set; }
		public float DistanceToEdge { get; private set; }

		public ClimbHandler(PlayerMotor motor)
		{
			_motor = motor;
			_anim = _motor.Anim;
			_climbCollider = null;
			_playerCollider = _motor.Collider;
			_rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
			_leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);
			NextClimbs = new List<Climb>();
		}

		public ClimbingState GetClimbingState(bool recalculate)
		{
			if (CurrentClimb == Climb.End || recalculate == false)
			{
				_motor.ClimbingState.Climb = CurrentClimb;
				_motor.ClimbingState.Recalculate = false;
				return _motor.ClimbingState;
			}

			float climbingSpeed = ConstantVariables.DefaultMovementSpeed;

			switch (CurrentClimb)
			{
				case Climb.Up:
					Hanging();
					break;
				case Climb.Flip:
					Hanging();
					break;
				case Climb.Mantle:
					Mantle();
					break;
				case Climb.Down:
					AtEdge();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
				case Climb.AcrossLeft:
				case Climb.AcrossRight:
					Hanging();
					climbingSpeed = ConstantVariables.AcrossSpeed;
					break;
				case Climb.SwingLeft:
				case Climb.SwingRight:
					OffEdge();
					climbingSpeed = ConstantVariables.SwingSpeed;
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

		private void Underside()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.TopRight;
			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.TopLeft;
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

		private void AtEdge()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.BottomRight;

			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.BottomLeft;
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

		private bool IsEdgeUnblocked(RaycastHit2D originalHit)
		{
			Vector3 origin = _playerCollider.bounds.center;

			Vector2 direction = originalHit.collider.bounds.center - origin;

			RaycastHit2D hit = Physics2D.Raycast(origin, direction, 10f, Layers.Platforms);
			Debug.DrawRay(origin, direction, Color.red);

			if (hit == false || Vector2.Distance(originalHit.point, hit.point) < 1)
			{
				return Physics2D.Raycast(
					new Vector2(originalHit.collider.bounds.center.x, originalHit.collider.bounds.max.y + 0.01f),
					Vector2.up,
					_playerCollider.bounds.size.y,
					Layers.Platforms) == false;
			}

			Debug.Log("Edge blocked");
			return false;

		}

		public bool CheckLedgeAbove(DirectionFacing direction, out Climb climb)
		{
			climb = Climb.None;

			const float checkWidth = 4f;
			const float checkHeight = 6f;

			float actualHeight = _playerCollider.bounds.size.y + checkHeight - 1;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x,
			   _playerCollider.bounds.min.y + 1 + actualHeight / 2);

			var size = new Vector2(0.01f, actualHeight);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkWidth, GetClimbMask());

			var hit = GetValidHit(hits);

			if (hit)
			{
				bool belowHeadHeight = hit.point.y < _playerCollider.bounds.max.y;

				if (belowHeadHeight && CurrentClimb == Climb.None && ShouldStraightClimb())
				{
					CurrentClimb = climb = Climb.Mantle;
				}
				else if (belowHeadHeight)
				{ }
				else if (ShouldStraightClimb())
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

			if (climb == Climb.None && _retryCheckAbove)
			{
				_retryCheckAbove = false;
				CheckLedgeAbove(direction == DirectionFacing.Left ? DirectionFacing.Right : DirectionFacing.Left, out climb);
			}
			_retryCheckAbove = true;
			return climb != Climb.None;
		}

		public bool CheckLedgeBelow(Climb intendedClimbingState, DirectionFacing direction)
		{
			const float checkWidth = 3f;
			const float checkDepth = 0.1f;

			Vector2 origin = _motor.GetGroundPivotPosition();

			Vector2 castDirection = _motor.GetSurfaceDirection(direction == DirectionFacing.Left ? DirectionTravelling.Left : DirectionTravelling.Right);

			float offset = castDirection.x > 0
				? _playerCollider.bounds.center.x - origin.x
				: origin.x - _playerCollider.bounds.center.x;

			origin += offset * castDirection;
			origin -= new Vector2(0, checkDepth);

			RaycastHit2D hit = Physics2D.Raycast(origin, castDirection, checkWidth, GetClimbMask());
			Debug.DrawRay(origin, castDirection, Color.black);
			if (hit && IsEdgeUnblocked(hit))
			{
				DirectionFacing dir = hit.collider.gameObject.layer == _rightClimbLayer
					? DirectionFacing.Right
					: DirectionFacing.Left;

				if (dir != direction || intendedClimbingState == Climb.Down && hit.collider.CanClimbDown() == false
					|| intendedClimbingState == Climb.MoveToEdge && hit.collider.CanCross() == false)
					return false;
				CurrentClimb = intendedClimbingState;
				SetClimbingParameters(hit);
				DistanceToEdge = hit.distance;
				return true;
			}
			return false;
		}

		public bool CheckLedgeAcross(DirectionFacing direction)
		{
			NextClimbs.Clear();

			const float checkLength = 7f;
			const float checkDepth = 4f;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x,
			   _playerCollider.bounds.min.y);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());

			return GetValidHit(hits);
		}

		public bool CheckLedgeSwing(DirectionFacing direction, bool invertSwingDirection)
		{
			bool swing = direction != _motor.GetDirectionFacing();
			if (invertSwingDirection)
				swing = !swing;
			_anim.SetBool("swing", swing);

			const float checkLength = 5f;
			const float checkDepth = 5f;

			float xOffset = direction == DirectionFacing.Left ? -1 : 1;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x + xOffset,
			   _playerCollider.bounds.min.y - checkDepth/2 + 1);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());

			return GetValidHit(hits);
		}

		private RaycastHit2D GetValidHit(RaycastHit2D[] hits)
		{
			var hit = new RaycastHit2D();

			RaycastHit2D[] checkHits = _climbCollider == null
				? hits
				: hits.Where(h => h.transform.parent != _climbCollider.transform.parent).ToArray();

			foreach (RaycastHit2D h in checkHits)
			{
				if (IsEdgeUnblocked(h))
				{
					hit = h;
					SetClimbingParameters(hit);
					return hit;
				}
			}
			return hit;
		}

		private LayerMask GetClimbMask()
		{
			return 1 << _rightClimbLayer | 1 << _leftClimbLayer;
		}

		private void SetClimbingParameters(RaycastHit2D hit)
		{			
			_climbCollider = hit.collider;
			SetClimbingSide();
			_anim.SetBool("falling", true);
		}

		private void SetClimbingSide()
		{
			if (_climbCollider.IsUpright())
				ClimbSide = _motor.transform.position.x < _climbCollider.transform.position.x
					? DirectionFacing.Left
					: DirectionFacing.Right;
			else
				ClimbSide = _climbCollider.gameObject.layer == _rightClimbLayer
					? DirectionFacing.Right
					: DirectionFacing.Left;
		}

		private bool ShouldStraightClimb()
		{
			return ClimbSide == DirectionFacing.Left
				? _motor.transform.position.x < _climbCollider.transform.position.x
				: _motor.transform.position.x > _climbCollider.transform.position.x;
		}

		public ClimbingState SwitchClimbingState(DirectionFacing direction, bool invertSwingDirection)
		{
			bool recalculate = true;

			if (NextClimbs.Count == 0)
				_climbCollider = null;

			var nextClimb = CurrentClimb == Climb.End || CurrentClimb == Climb.Jump ? Climb.None : Climb.End;
			if (direction == DirectionFacing.None)
				direction = _motor.GetDirectionFacing();

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
					else if (NextClimbs.Contains(Climb.AcrossLeft) && (ClimbSide == DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Left)
							? Climb.AcrossLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.AcrossRight) && ClimbSide == DirectionFacing.Right)
						nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
					else
						_motor.MoveHorizontally();
					break;
				case Climb.Down:
					if (NextClimbs.Contains(Climb.Up))
					{
						recalculate = false;
						nextClimb = Climb.Up;
					}
					else if (NextClimbs.Contains(Climb.AcrossLeft) && (_climbCollider.IsCorner() == false || DirectionFacing.Left == direction))
						nextClimb = CheckLedgeSwing(DirectionFacing.Left, invertSwingDirection)
							? Climb.SwingLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.AcrossRight) && (_climbCollider.IsCorner() == false || DirectionFacing.Right == direction))
						nextClimb = CheckLedgeSwing(DirectionFacing.Right, invertSwingDirection)
							? Climb.SwingRight
							: Climb.Jump;
					break;
				case Climb.AcrossLeft:
				case Climb.SwingLeft:
					if (NextClimbs.Contains(Climb.AcrossRight) && (ClimbSide == DirectionFacing.Right || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Up) && CheckLedgeAbove(direction, out nextClimb))
					{ }
					else if (NextClimbs.Contains(Climb.Down) && _climbCollider.CanClimbDown())
					{
						recalculate = false;
						nextClimb = Climb.Down;
					}
					else
						_motor.MoveHorizontally();
					break;
				case Climb.AcrossRight:
				case Climb.SwingRight:
					if (NextClimbs.Contains(Climb.AcrossLeft) && (ClimbSide == DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Left)
							? Climb.AcrossLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Up) && CheckLedgeAbove(direction, out nextClimb))
					{ }
					else if (NextClimbs.Contains(Climb.Down) && _climbCollider.CanClimbDown())
					{
						recalculate = false;
						nextClimb = Climb.Down;
					}
					else
						_motor.MoveHorizontally();
					break;
				case Climb.MoveToEdge:
					if (NextClimbs.Contains(Climb.AcrossLeft) && CheckLedgeAcross(DirectionFacing.Left))
						nextClimb = Climb.AcrossLeft;
					else if (NextClimbs.Contains(Climb.AcrossRight) && CheckLedgeAcross(DirectionFacing.Right))
						nextClimb = Climb.AcrossRight;
					else if (_climbCollider.CanClimbDown())
						nextClimb = Climb.Jump;
					else
						_motor.MoveHorizontally();
					break;
			}

			CurrentClimb = nextClimb;
			NextClimbs.Clear();

			return GetClimbingState(recalculate);
		}

		public bool CanClimbDown()
		{
			return _climbCollider.CanClimbDown();
		}
	}
}
