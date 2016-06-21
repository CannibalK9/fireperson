using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private Collider2D _climbCollider;
		private readonly Collider2D _playerCollider;
		private readonly AnimationScript _anim;
		private readonly int _rightClimbLayer;
		private readonly int _leftClimbLayer;
		private Vector2 _target;
		private Vector2 _player;

		public ClimbingState CurrentClimbingState { get; set; }
		public List<ClimbingState> NextClimbingStates { get; set; }
		public DirectionFacing ClimbingSide { get; set; }

		public ClimbHandler(PlayerMotor motor)
		{
			_motor = motor;
			_anim = _motor.Anim;
			_climbCollider = null;
			_playerCollider = _motor.Collider;
			_rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
			_leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);
			NextClimbingStates = new List<ClimbingState>();
		}

		public void ClimbAnimation()
		{
			float climbingSpeed = 0.5f;

			switch (CurrentClimbingState)
			{
				case ClimbingState.Up:
				case ClimbingState.Flip:
					ClimbUp();
					break;
				case ClimbingState.Down:
					ClimbDown();
					climbingSpeed = 1f;
					break;
				case ClimbingState.AcrossLeft:
				case ClimbingState.AcrossRight:
					Across();
					climbingSpeed = 0.2f;
					break;
				case ClimbingState.SwingLeft:
				case ClimbingState.SwingRight:
					Swing();
					climbingSpeed = 0.4f;
					break;
				case ClimbingState.MoveToEdge:
					MoveToEdge();
					climbingSpeed = 1f;
					break;
			}
			_motor.LinearMovement(_target, _player, climbingSpeed);
		}

		private void ClimbUp()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = _climbCollider.GetTopRight();
				_player = _playerCollider.GetTopLeft();
			}
			else
			{
				_target = _climbCollider.GetTopLeft();
				_player = _playerCollider.GetTopRight();
			}
		}

		private void ClimbDown()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = _climbCollider.GetTopRight();
				_player = _playerCollider.GetBottomRight();

			}
			else
			{
				_target = _climbCollider.GetTopLeft();
				_player = _playerCollider.GetBottomLeft();
			}
		}

		private void MoveToEdge()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = _climbCollider.GetTopRight();
				_player = _playerCollider.GetBottomRight();

			}
			else
			{
				_target = _climbCollider.GetTopLeft();
				_player = _playerCollider.GetBottomLeft();
			}
		}

		private void Across()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = _climbCollider.GetTopRight();
				_player = _playerCollider.GetTopLeft();
			}
			else
			{
				_target = _climbCollider.GetTopLeft();
				_player = _playerCollider.GetTopRight();
			}
		}

		private void Swing()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = _climbCollider.GetTopRight();
				_player = _playerCollider.GetBottomRight();
			}
			else
			{
				_target = _climbCollider.GetTopLeft();
				_player = _playerCollider.GetBottomLeft();
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

			return hit && originalHit.collider.transform.parent == hit.collider.transform;
		}

		public bool CheckLedgeAbove(DirectionFacing direction)
		{
			const float checkWidth = 5f;
			const float checkHeight = 4f;

			float actualHeight = _playerCollider.bounds.size.y + checkHeight - 1;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x,
			   _playerCollider.bounds.min.y + 1 + actualHeight / 2);

			var size = new Vector2(0.01f, checkHeight);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkWidth, GetClimbMask());

			var hit = GetValidHit(hits);

			if (hit)
			{
				if (ShouldStraightClimb() == false && hit.collider.IsCorner())
					return false;

				if (CurrentClimbingState == ClimbingState.None)
				{
					if (hit.point.y < _playerCollider.bounds.max.y)
					{
						CurrentClimbingState = ClimbingState.Mantle;
						_anim.PlayAnimation(Animations.Mantle);
					}
					else
					{
						CurrentClimbingState = ClimbingState.Up;
						_anim.PlayAnimation(ShouldStraightClimb() ? Animations.ClimbUp : Animations.FlipUp);
					}
				}
			}
			return hit;
		}

		public bool CheckLedgeBelow(ClimbingState intendedClimbingState, DirectionFacing direction)
		{
			const float checkWidth = 5f;
			const float checkDepth = 3f;

			var origin = new Vector2(
				   _playerCollider.bounds.center.x,
				   _playerCollider.bounds.min.y - checkDepth / 2);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkWidth, GetClimbMask());

			var hit = GetValidHit(hits);

			if (hit)
			{
				if (intendedClimbingState == ClimbingState.Down && hit.collider.CanClimbDown() == false
					|| intendedClimbingState == ClimbingState.MoveToEdge && hit.collider.CanCross() == false)
					return false;
				CurrentClimbingState = intendedClimbingState;
				SetClimbingParameters(hit);
			}
			return hit;
		}

		public bool CheckLedgeAcross(DirectionFacing direction)
		{
			NextClimbingStates.Clear();

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
				ClimbingSide = _motor.transform.position.x < _climbCollider.transform.position.x
					? DirectionFacing.Left
					: DirectionFacing.Right;
			else
				ClimbingSide = _climbCollider.gameObject.layer == _rightClimbLayer
					? DirectionFacing.Right
					: DirectionFacing.Left;
		}

		private bool ShouldStraightClimb()
		{
			return ClimbingSide == DirectionFacing.Left
				? _motor.transform.position.x < _climbCollider.transform.position.x
				: _motor.transform.position.x > _climbCollider.transform.position.x;
		}

		public ClimbingState SwitchClimbingState(DirectionFacing direction)
		{
			if (NextClimbingStates.Count == 0)
				_climbCollider = null;

			var nextClimbingState = ClimbingState.None;
			if (direction == DirectionFacing.None)
				direction = _motor.GetDirectionFacing();

			switch (CurrentClimbingState)
			{
				case ClimbingState.Mantle:
				case ClimbingState.Flip:
				case ClimbingState.Up:
					if (NextClimbingStates.Contains(ClimbingState.Down))
						nextClimbingState = ClimbingState.Down;
					else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove(direction))
						nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
					else if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && (ClimbingSide == DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimbingState = CheckLedgeAcross(DirectionFacing.Left)
							? ClimbingState.AcrossLeft
							: ClimbingState.Jump;
					else if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && ClimbingSide == DirectionFacing.Right)
						nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
							? ClimbingState.AcrossRight
							: ClimbingState.Jump;
					break;
				case ClimbingState.Down:
					if (NextClimbingStates.Contains(ClimbingState.Up))
						nextClimbingState = ClimbingState.Up;
					else if (NextClimbingStates.Contains(ClimbingState.AcrossLeft)
							&& (_climbCollider.IsCorner() == false
								|| DirectionFacing.Left == direction))
						nextClimbingState = CheckLedgeSwing(DirectionFacing.Left)
							? ClimbingState.SwingLeft
							: ClimbingState.Jump;
					else if (NextClimbingStates.Contains(ClimbingState.AcrossRight)
							&& (_climbCollider.IsCorner() == false
								|| DirectionFacing.Right == direction))
						nextClimbingState = CheckLedgeSwing(DirectionFacing.Right)
							? ClimbingState.SwingRight
							: ClimbingState.Jump;
					break;
				case ClimbingState.AcrossLeft:
					if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && (ClimbingSide != DirectionFacing.Right || _climbCollider.IsUpright()))
						nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
							? ClimbingState.AcrossRight
							: ClimbingState.Jump;
					else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove(direction))
						nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
					else if (NextClimbingStates.Contains(ClimbingState.Down) && _climbCollider.CanClimbDown())
						nextClimbingState = ClimbingState.Down;
					break;
				case ClimbingState.AcrossRight:
					if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && (ClimbingSide != DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimbingState = CheckLedgeAcross(DirectionFacing.Left)
							? ClimbingState.AcrossLeft
							: ClimbingState.Jump;
					else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove(direction))
						nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
					else if (NextClimbingStates.Contains(ClimbingState.Down) && _climbCollider.CanClimbDown()) 
						nextClimbingState = ClimbingState.Down;
					break;
				case ClimbingState.SwingLeft:
					if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && (ClimbingSide != DirectionFacing.Right || _climbCollider.IsUpright()))
						nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
							? ClimbingState.AcrossRight
							: ClimbingState.Jump;
					else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove(direction))
						nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
					else if (NextClimbingStates.Contains(ClimbingState.Down) && _climbCollider.CanClimbDown())
						nextClimbingState = ClimbingState.Down;
					break;
				case ClimbingState.SwingRight:
					if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && (ClimbingSide != DirectionFacing.Left || _climbCollider.IsUpright()))
						nextClimbingState = CheckLedgeAcross(DirectionFacing.Left)
							? ClimbingState.AcrossLeft
							: ClimbingState.Jump;
					else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove(direction))
						nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
					else if (NextClimbingStates.Contains(ClimbingState.Down) && _climbCollider.CanClimbDown())
						nextClimbingState = ClimbingState.Down;
					break;
				case ClimbingState.MoveToEdge:
					if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && CheckLedgeAcross(DirectionFacing.Left))
						nextClimbingState = ClimbingState.AcrossLeft;
					else if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && CheckLedgeAcross(DirectionFacing.Right))
						nextClimbingState = ClimbingState.AcrossRight;
					else
						nextClimbingState = ClimbingState.Jump;
					break;
			}

			CurrentClimbingState = nextClimbingState;
			NextClimbingStates.Clear();

			return CurrentClimbingState;
		}
	}
}
