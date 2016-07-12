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

		public Climb CurrentClimb { get; set; }
		public List<Climb> NextClimbs { get; set; }
		public DirectionFacing ClimbSide { get; set; }

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

		public ClimbingState GetClimbingState()
		{
			float climbingSpeed = ConstantVariables.DefaultMovementSpeed;

			switch (CurrentClimb)
			{
				case Climb.Up:
				case Climb.Flip:
				case Climb.Mantle:
					ClimbUp();
					break;
				case Climb.Down:
					ClimbDown();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
				case Climb.AcrossLeft:
				case Climb.AcrossRight:
					Across();
					climbingSpeed = ConstantVariables.AcrossSpeed;
					break;
				case Climb.SwingLeft:
				case Climb.SwingRight:
					Swing();
					climbingSpeed = ConstantVariables.SwingSpeed;
					break;
				case Climb.MoveToEdge:
					MoveToEdge();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
			}
			 return new ClimbingState(CurrentClimb, _climbCollider, climbingSpeed, _target, _player);
		}

		public void CancelClimb()
		{
			CurrentClimb = Climb.None;
			_climbCollider = null;
		}

		private void ClimbUp()
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

		private void ClimbDown()
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

		private void MoveToEdge()
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

		private void Across()
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

		private void Swing()
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

		private bool IsEdgeUnblocked(RaycastHit2D originalHit)
		{
			Vector2 origin = _playerCollider.bounds.center;

			Vector2 direction;
			if (originalHit.collider.IsUpright())
				direction = originalHit.collider.GetTopFace() - origin;
			else
				direction = originalHit.collider.name.Contains("left")
					? originalHit.collider.GetLeftFace() - origin
					: originalHit.collider.GetRightFace() - origin;

			RaycastHit2D hit = Physics2D.Raycast(origin, direction, 10f, Layers.Platforms);
			Debug.DrawRay(origin, direction, Color.red);

			if (hit == false || originalHit.collider.transform.parent == hit.collider.transform)
				return true;

			Debug.Log("Edge blocked");
			return false;

		}

		public Climb CheckLedgeAbove(DirectionFacing direction)
		{
			const float checkWidth = 4f;
			const float checkHeight = 4f;

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
				if (ShouldStraightClimb() == false && hit.collider.IsCorner())
					return Climb.None;

				if (hit.point.y < _playerCollider.bounds.max.y)
				{
					if (CurrentClimb == Climb.None)
					{
						_anim.PlayAnimation(Animations.Mantle);
						CurrentClimb = Climb.Mantle;
					}
					return Climb.Mantle;
				}
				else if (ShouldStraightClimb())
				{
					if (CurrentClimb == Climb.None)
					{
						_anim.PlayAnimation(Animations.ClimbUp);
						CurrentClimb = Climb.Up;
					}
					return Climb.Up;
				}
				else
				{
					if (CurrentClimb == Climb.None)
					{
						_anim.PlayAnimation(Animations.FlipUp);
						CurrentClimb = Climb.Flip;
					}
					return Climb.Flip;
				}
			}
			return Climb.None;
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
				if (intendedClimbingState == Climb.Down && hit.collider.CanClimbDown() == false
					|| intendedClimbingState == Climb.MoveToEdge && hit.collider.CanCross() == false)
					return false;
				CurrentClimb = intendedClimbingState;
				SetClimbingParameters(hit);
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

		public bool CheckLedgeSwing(DirectionFacing direction)
		{
			_anim.SetBool(
				"swing",
				direction != _motor.GetDirectionFacing());

			const float checkLength = 5f;
			const float checkDepth = 5f;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x,
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

		public ClimbingState SwitchClimbingState(DirectionFacing direction)
		{
			if (NextClimbs.Count == 0)
				_climbCollider = null;

			var nextClimb = Climb.None;
			if (direction == DirectionFacing.None)
				direction = _motor.GetDirectionFacing();

			switch (CurrentClimb)
			{
				case Climb.Mantle:
				case Climb.Flip:
				case Climb.Up:
					if (NextClimbs.Contains(Climb.Down))
						nextClimb = Climb.Down;
					else if (NextClimbs.Contains(Climb.Up))
						nextClimb = CheckLedgeAbove(direction);
					else if (NextClimbs.Contains(Climb.AcrossLeft) && (ClimbSide == DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Left)
							? Climb.AcrossLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.AcrossRight) && ClimbSide == DirectionFacing.Right)
						nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
					break;
				case Climb.Down:
					if (NextClimbs.Contains(Climb.Up))
						nextClimb = Climb.Up;
					else if (NextClimbs.Contains(Climb.AcrossLeft)
							&& (_climbCollider.IsCorner() == false
								|| DirectionFacing.Left == direction))
						nextClimb = CheckLedgeSwing(DirectionFacing.Left)
							? Climb.SwingLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.AcrossRight)
							&& (_climbCollider.IsCorner() == false
								|| DirectionFacing.Right == direction))
						nextClimb = CheckLedgeSwing(DirectionFacing.Right)
							? Climb.SwingRight
							: Climb.Jump;
					break;
				case Climb.AcrossLeft:
					if (NextClimbs.Contains(Climb.AcrossRight) && (ClimbSide == DirectionFacing.Right || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Up))
						nextClimb = CheckLedgeAbove(direction);
					else if (NextClimbs.Contains(Climb.Down) && _climbCollider.CanClimbDown())
						nextClimb = Climb.Down;
					break;
				case Climb.AcrossRight:
					if (NextClimbs.Contains(Climb.AcrossLeft) && (ClimbSide == DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Left)
							? Climb.AcrossLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Up))
						nextClimb = CheckLedgeAbove(direction);
					else if (NextClimbs.Contains(Climb.Down) && _climbCollider.CanClimbDown()) 
						nextClimb = Climb.Down;
					break;
				case Climb.SwingLeft:
					if (NextClimbs.Contains(Climb.AcrossRight) && (ClimbSide == DirectionFacing.Right || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Up))
						nextClimb = CheckLedgeAbove(direction);
					else if (NextClimbs.Contains(Climb.Down) && _climbCollider.CanClimbDown())
						nextClimb = Climb.Down;
					break;
				case Climb.SwingRight:
					if (NextClimbs.Contains(Climb.AcrossLeft) && (ClimbSide == DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimb = CheckLedgeAcross(DirectionFacing.Left)
							? Climb.AcrossLeft
							: Climb.Jump;
					else if (NextClimbs.Contains(Climb.Up))
						nextClimb = CheckLedgeAbove(direction);
					else if (NextClimbs.Contains(Climb.Down) && _climbCollider.CanClimbDown())
						nextClimb = Climb.Down;
					break;
				case Climb.MoveToEdge:
					if (NextClimbs.Contains(Climb.AcrossLeft) && CheckLedgeAcross(DirectionFacing.Left))
						nextClimb = Climb.AcrossLeft;
					else if (NextClimbs.Contains(Climb.AcrossRight) && CheckLedgeAcross(DirectionFacing.Right))
						nextClimb = Climb.AcrossRight;
					else
						nextClimb = Climb.Jump;
					break;
			}

			CurrentClimb = nextClimb;
			NextClimbs.Clear();

			return GetClimbingState();
		}
	}
}
