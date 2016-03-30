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
            _animator.SetBool("transitionUp", false);
            _animator.SetBool("transitionDown", false);
            _animator.SetBool("transitionAcross", false);

            _nextState = PlayerMotor.SwitchClimbingState();

            switch (_nextState)
            {
                case ClimbingState.Up:
                    _animator.SetBool("transitionUp", true);
                    break;
                case ClimbingState.Down:
                    _animator.SetBool("transitionDown", true);
                    break;
                case ClimbingState.AcrossRight:
                case ClimbingState.AcrossLeft:
                    _animator.SetBool("transitionAcross", true);
                    break;
                case ClimbingState.None:
                default:
                    break;
            }

            
        }
    }
}
