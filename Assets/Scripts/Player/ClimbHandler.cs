using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Movement;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private Collider2D _climbCollider;
		private readonly Collider2D _playerCollider;
		private readonly AnimationScript _anim;

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

		public bool CheckLedgeAbove()
		{
			const float checkWidth = 5f;
			const float checkHeight = 4f;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x,
			   _playerCollider.bounds.max.y);

			var size = new Vector2(checkWidth, 1f);

			RaycastHit2D hit = GetNearestHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.up, checkHeight, GetClimbMask()));

			if (hit)
			{
				SetClimbingParameters(hit);
				if (ShouldStraightClimb() == false && hit.collider.name.Contains("corner"))
					return false;

				if (CurrentClimbingState == ClimbingState.None)
				{
					CurrentClimbingState = ClimbingState.Up;
					_anim.PlayAnimation(ShouldStraightClimb() ? "ClimbUp" : "FlipUp");
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
				   _playerCollider.bounds.min.y);

			var size = new Vector2(checkWidth, 1f);

			RaycastHit2D hit = GetCorrectSideHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.down, checkDepth, GetClimbMask()), direction);

			if (hit)
			{
				if (intendedClimbingState == ClimbingState.Down && hit.collider.name.Contains("less")
					|| intendedClimbingState == ClimbingState.MoveToEdge && hit.collider.name.Contains("jumpless"))
					return false;
				CurrentClimbingState = intendedClimbingState;
				SetClimbingParameters(hit);
			}
			return hit;
		}

		private RaycastHit2D GetNearestHit(RaycastHit2D[] hits)
		{
			var hit = new RaycastHit2D();
			if (hits.Length > 0)
				hit = hits[0];

			if (hits.Length > 1)
			{
				foreach (RaycastHit2D r in hits)
				{
					if (Vector2.Distance(hit.point, _playerCollider.transform.position)
						> Vector2.Distance(r.point, _playerCollider.transform.position))
						hit = r;
				}
			}
			return hit;
		}

		private RaycastHit2D GetCorrectSideHit(IEnumerable<RaycastHit2D> hits, DirectionFacing direction)
		{
			var hit = new RaycastHit2D();
			foreach (RaycastHit2D h in hits)
			{
				DirectionFacing edge = h.collider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot")
					? DirectionFacing.Right
					: DirectionFacing.Left;

				if (edge == direction)
					hit = h;
			}
			return hit;
		}

		public bool CheckLedgeAcross(DirectionFacing direction)
		{
			NextClimbingStates.Clear();

			const float checkLength = 10f;
			const float checkDepth = 4f;

			var origin = new Vector2(
			   _playerCollider.bounds.center.x,
			   _playerCollider.bounds.min.y);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());
			var hit = new RaycastHit2D();

			foreach (RaycastHit2D h in hits.Where(h => h.collider != _climbCollider))
			{
				hit = h;
				SetClimbingParameters(hit);
                return hit;
			}
			return hit;
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
			var hit = new RaycastHit2D();

			foreach (RaycastHit2D h in hits.Where(h => h.collider != _climbCollider))
			{
				hit = h;
				SetClimbingParameters(hit);
			}
			return hit;
		}

		private LayerMask GetClimbMask()
		{
			return 1 << LayerMask.NameToLayer("Right Climb Spot")
				| 1 << LayerMask.NameToLayer("Left Climb Spot");
		}

		private void SetClimbingParameters(RaycastHit2D hit)
		{			
			_climbCollider = hit.collider;
			SetClimbingSide();
			_anim.SetBool("falling", true);
		}

		private void SetClimbingSide()
		{
			ClimbingSide = _climbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot")
					? DirectionFacing.Right
					: DirectionFacing.Left;
		}

		private bool ShouldStraightClimb()
		{
			return ClimbingSide == DirectionFacing.Left
				? _motor.transform.position.x < _climbCollider.transform.position.x
				: _motor.transform.position.x > _climbCollider.transform.position.x;
		}

		public ClimbingState SwitchClimbingState()
		{
			var nextClimbingState = ClimbingState.None;

			if (CurrentClimbingState == ClimbingState.Up || CurrentClimbingState == ClimbingState.Flip)
			{
				if (NextClimbingStates.Contains(ClimbingState.Down))
					nextClimbingState = ClimbingState.Down;
				else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove())
					nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
				else if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && ClimbingSide == DirectionFacing.Left)
					nextClimbingState = CheckLedgeAcross(DirectionFacing.Left)
						? ClimbingState.AcrossLeft
						: ClimbingState.Jump;
				else if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && ClimbingSide == DirectionFacing.Right)
					nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
						? ClimbingState.AcrossRight
						: ClimbingState.Jump;
			}
			else if (CurrentClimbingState == ClimbingState.Down)
			{
				if (NextClimbingStates.Contains(ClimbingState.Up))
					nextClimbingState = ClimbingState.Up;
				else if (NextClimbingStates.Contains(ClimbingState.AcrossLeft)
					&& (_climbCollider.name.Contains("corner") == false
					|| DirectionFacing.Left == _motor.GetDirectionFacing()))
					nextClimbingState = CheckLedgeSwing(DirectionFacing.Left)
						? ClimbingState.SwingLeft
						: ClimbingState.Jump;
				else if (NextClimbingStates.Contains(ClimbingState.AcrossRight)
					&& (_climbCollider.name.Contains("corner") == false
					|| DirectionFacing.Right == _motor.GetDirectionFacing()))
					nextClimbingState = CheckLedgeSwing(DirectionFacing.Right)
						? ClimbingState.SwingRight
						: ClimbingState.Jump;
			}
			else if (CurrentClimbingState == ClimbingState.AcrossLeft)
			{
				if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && ClimbingSide != DirectionFacing.Right)
					nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
						? ClimbingState.AcrossRight
						: ClimbingState.Jump;
				else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove())
					nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
				else if (NextClimbingStates.Contains(ClimbingState.Down)) //check tag hereish, don't drop always
					nextClimbingState = ClimbingState.Down;
			}
			else if (CurrentClimbingState == ClimbingState.AcrossRight)
			{
				if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && ClimbingSide != DirectionFacing.Left)
					nextClimbingState = CheckLedgeAcross(DirectionFacing.Left)
						? ClimbingState.AcrossLeft
						: ClimbingState.Jump;
				else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove())
					nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
				else if (NextClimbingStates.Contains(ClimbingState.Down))
					nextClimbingState = ClimbingState.Down;
			}
			else if (CurrentClimbingState == ClimbingState.SwingLeft)
			{
				if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && ClimbingSide != DirectionFacing.Right)
					nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
						? ClimbingState.AcrossRight
						: ClimbingState.Jump;
				else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove())
					nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
				else if (NextClimbingStates.Contains(ClimbingState.Down))
					nextClimbingState = ClimbingState.Down;
			}
			else if (CurrentClimbingState == ClimbingState.SwingRight)
			{
				if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && ClimbingSide != DirectionFacing.Left)
					nextClimbingState = CheckLedgeAcross(DirectionFacing.Left)
						? ClimbingState.AcrossLeft
						: ClimbingState.Jump;
				else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove())
					nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
				else if (NextClimbingStates.Contains(ClimbingState.Down))
					nextClimbingState = ClimbingState.Down;
			}
			else if (CurrentClimbingState == ClimbingState.MoveToEdge)
			{
				if (NextClimbingStates.Contains(ClimbingState.AcrossLeft) && CheckLedgeAcross(DirectionFacing.Left))
					nextClimbingState = ClimbingState.AcrossLeft;
				else if (NextClimbingStates.Contains(ClimbingState.AcrossRight) && CheckLedgeAcross(DirectionFacing.Right))
					nextClimbingState = ClimbingState.AcrossRight;
				else
					nextClimbingState = ClimbingState.Jump;
			}

			CurrentClimbingState = nextClimbingState;
			NextClimbingStates.Clear();

			if (_climbCollider != null)
			{
				_motor.BuildingTransform = _climbCollider.transform.parent.parent;
			}

			return CurrentClimbingState;
		}
	}
}
