using UnityEngine;

namespace Assets.Scripts.Player
{
    public class AnimationScript : MonoBehaviour
    {
        public PlayerMotor PlayerMotor;
        private Animator _animator;
        private ClimbingState _nextState;

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void SwitchClimbingState()
        {
            _nextState = PlayerMotor.SwitchClimbingState();

            switch (_nextState)
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
                case ClimbingState.None:
                default:
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
            PlayerMotor.SetHorizontalVelocity(3);
        }
    }
}
