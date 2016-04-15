using UnityEngine;

namespace Assets.Scripts.Player
{
	public class AnimationScript : MonoBehaviour
	{
		public PlayerMotor PlayerMotor;
		private Animator _animator;

		void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		private void SwitchClimbingState()
		{
            ClimbingState nextState = PlayerMotor.SwitchClimbingState();

			switch (nextState)
			{
				case ClimbingState.Up:
					_animator.SetTrigger("climbUp");
					break;
				case ClimbingState.Down:
					_animator.SetTrigger("transitionDown");
					break;
				case ClimbingState.AcrossRight:
				case ClimbingState.AcrossLeft:
					_animator.SetTrigger("transitionAcross");
					break;
				case ClimbingState.Jump:
					_animator.SetTrigger("jump");
					break;
			}
		}

		private void FlipSpriteTowardsEdge()
		{
			if (PlayerMotor.ClimbingSide == PlayerMotor.GetDirectionFacing())
				PlayerMotor.FlipSprite();
		}

		private void FlipSpriteAwayFromEdge()
		{
			if (PlayerMotor.ClimbingSide != PlayerMotor.GetDirectionFacing())
				PlayerMotor.FlipSprite();
		}

		private void ApplyJumpVelocity()
		{
			PlayerMotor.SetHorizontalVelocity(1, true);
		}

		private void ApplyJumpVelocityBackwards()
		{
			PlayerMotor.SetHorizontalVelocity(1, false);
		}

		private void AllowMovement()
		{
			PlayerMotor.AllowMovement();
		}

		private void StopMovement()
		{
			PlayerMotor.StopMovement();
		}
	}
}
