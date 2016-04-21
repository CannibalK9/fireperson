﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private Collider2D _climbCollider;
		private readonly Collider2D _playerCollider;
        private AnimationScript _anim;

        private Vector2 _target;
		private Vector2 _player;

		public ClimbingState CurrentClimbingState { get; set; }
		public List<ClimbingState> NextClimbingStates { get; set; }
		public DirectionFacing ClimbingSide { get; set; }
		public bool MovementAllowed { get; set; }

		public ClimbHandler(PlayerMotor motor)
		{
			_motor = motor;
            _anim = _motor.Anim;
			_climbCollider = null;
			_playerCollider = _motor.Collider;
			MovementAllowed = true;
			NextClimbingStates = new List<ClimbingState>();
		}

		public void ClimbAnimation()
		{
			float climbingSpeed = 0.5f;

			switch (CurrentClimbingState)
			{
				case ClimbingState.Up:
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
			if (MovementAllowed)
				ClimbMovement(climbingSpeed);
		}

		private void ClimbUp()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = GetTopRight(_climbCollider);
				_player = GetTopLeft(_playerCollider);
			}
			else
			{
				_target = GetTopLeft(_climbCollider);
				_player = GetTopRight(_playerCollider);
			}
		}

		private void ClimbDown()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = GetTopRight(_climbCollider);
				_player = GetBottomRight(_playerCollider);

			}
			else
			{
				_target = GetTopLeft(_climbCollider);
				_player = GetBottomLeft(_playerCollider);
			}
		}

		private void MoveToEdge()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = GetTopRight(_climbCollider);
				_player = GetBottomRight(_playerCollider);

			}
			else
			{
				_target = GetTopLeft(_climbCollider);
				_player = GetBottomLeft(_playerCollider);
			}
		}

		private void Across()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = GetTopRight(_climbCollider);
				_player = GetTopLeft(_playerCollider);
			}
			else
			{
				_target = GetTopLeft(_climbCollider);
				_player = GetTopRight(_playerCollider);
			}
		}

		private void Swing()
		{
			if (ClimbingSide == DirectionFacing.Right)
			{
				_target = GetTopRight(_climbCollider);
				_player = GetBottomRight(_playerCollider);
			}
			else
			{
				_target = GetTopLeft(_climbCollider);
				_player = GetBottomLeft(_playerCollider);
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
                if (CurrentClimbingState == ClimbingState.None)
                {
                    CurrentClimbingState = ClimbingState.Up;
                    if (ShouldStraightClimb())
                        _anim.PlayAnimation("ClimbUp");
                    else
                        _anim.PlayAnimation("FlipUp");
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

			RaycastHit2D hit = intendedClimbingState == ClimbingState.Down
				? GetNearestHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.down, checkDepth, GetClimbMask()))
				: GetCorrectSideHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.down, checkDepth, GetClimbMask()), direction);

			if (hit)
			{
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
			const float checkDepth = 2f;

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
			}
            return hit;
		}

		public bool CheckLedgeSwing(DirectionFacing direction)
		{
            if (direction == _motor.GetDirectionFacing())
                _anim.SetBool("swing", false);
            else
                _anim.SetBool("swing", true);

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
			const float overhangDistance = 1f;

			return ClimbingSide == DirectionFacing.Left
				? _motor.transform.position.x < _climbCollider.transform.position.x + overhangDistance
				: _motor.transform.position.x > _climbCollider.transform.position.x - overhangDistance;
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
                else if (NextClimbingStates.Contains(ClimbingState.AcrossLeft))
                    nextClimbingState = CheckLedgeSwing(DirectionFacing.Left)
                        ? ClimbingState.SwingLeft
                        : ClimbingState.Jump;
                else if (NextClimbingStates.Contains(ClimbingState.AcrossRight))
                    nextClimbingState = CheckLedgeSwing(DirectionFacing.Right)
                        ? ClimbingState.SwingRight
                        : ClimbingState.Jump;
            }
			else if (CurrentClimbingState == ClimbingState.AcrossLeft)
            {
				if (NextClimbingStates.Contains(ClimbingState.AcrossRight))
                    nextClimbingState = CheckLedgeAcross(DirectionFacing.Right)
                        ? ClimbingState.AcrossRight
                        : ClimbingState.Jump;
                else if (NextClimbingStates.Contains(ClimbingState.Up) && CheckLedgeAbove())
                    nextClimbingState = ShouldStraightClimb() ? ClimbingState.Up : ClimbingState.Flip;
                else if (NextClimbingStates.Contains(ClimbingState.Down))
					nextClimbingState = ClimbingState.Down;
			}
			else if (CurrentClimbingState == ClimbingState.AcrossRight)
            {
				if (NextClimbingStates.Contains(ClimbingState.AcrossLeft))
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
                if (NextClimbingStates.Contains(ClimbingState.AcrossRight))
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
                if (NextClimbingStates.Contains(ClimbingState.AcrossLeft))
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
			return CurrentClimbingState;
		}

		private void ClimbMovement(float climbingSpeed)
		{
			_motor.Move((_target - _player) * climbingSpeed);
		}

		private Vector2 GetTopRight(Collider2D col)
		{
			return col.bounds.max;
		}

		private Vector2 GetTopLeft(Collider2D col)
		{
			return new Vector2(
				col.bounds.min.x,
				col.bounds.max.y);
		}

		private Vector2 GetBottomRight(Collider2D col)
		{
			return new Vector2(
				col.bounds.max.x,
				col.bounds.min.y);
		}

		private Vector2 GetBottomLeft(Collider2D col)
		{
			return col.bounds.min;
		}
	}
}
