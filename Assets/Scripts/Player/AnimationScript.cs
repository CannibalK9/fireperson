using Assets.Scripts.CameraHandler;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class AnimationScript : MonoBehaviour
	{
		public PlayerMotor PlayerMotor;
        public SmoothCamera2D CameraScript;
		private Animator _animator;

		void Awake()
		{
			_animator = GetComponent<Animator>();
		}

        void Start()
        {
            CameraScript.Target = transform;
        }

		void Update()
		{
			if (_animator.GetCurrentAnimatorStateInfo(0).IsName(Animations.Idle) 
                || _animator.GetCurrentAnimatorStateInfo(0).IsName(Animations.Falling))
			{
				_animator.ResetTrigger("climbUp");
				_animator.ResetTrigger("transitionDown");
				_animator.ResetTrigger("transitionAcross");
				_animator.ResetTrigger("jump");
				_animator.ResetTrigger("flipUp");
				AllowMovement();
				AcceptInput();
				PlayerMotor.CancelClimbingState();
			}
		}

		public void SetBool(string boolName, bool value)
		{
			_animator.SetBool(boolName, value);
		}

		public void PlayAnimation(string anim)
		{
			_animator.Play(Animator.StringToHash(anim));
		}

		private void SwitchClimbingState()
		{
			ClimbingState nextState = PlayerMotor.SwitchClimbingState();

			switch (nextState)
			{
				case ClimbingState.Up:
					_animator.SetTrigger("climbUp");
					break;
				case ClimbingState.Flip:
					_animator.SetTrigger("flipUp");
					break;
				case ClimbingState.Down:
					_animator.SetTrigger("transitionDown");
					break;
				case ClimbingState.AcrossRight:
				case ClimbingState.AcrossLeft:
				case ClimbingState.SwingRight:
				case ClimbingState.SwingLeft:
					_animator.SetTrigger("transitionAcross");
					break;
				case ClimbingState.Jump:
					_animator.SetTrigger("jump");
					break;
			}
		}

		private void TryClimbDown()
		{
			if (PlayerMotor.TryClimbDown())
				_animator.SetTrigger("transitionDown");
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

		private void FlipSprite()
		{
			PlayerMotor.FlipSprite();
		}

		private void ApplyJumpVelocity()
		{
			PlayerMotor.SetJumpingVelocity(true);
		}

		private void ApplyJumpVelocityBackwards()
		{
			PlayerMotor.SetJumpingVelocity(false);
		}

		private void AllowMovement()
		{
			PlayerMotor.MovementAllowed = true;
		}

		private void StopMovement()
		{
			PlayerMotor.MovementAllowed = false;
		}

		private void AcceptInput()
		{
			PlayerMotor.AcceptInput = true;
		}

		private void IgnoreInput()
		{
			PlayerMotor.AcceptInput = false;
		}

		private void DestroyStilt()
		{
			PlayerMotor.BurnStilt();
		}

        private void CreateLight()
        {
            PlayerMotor.CreateLight();
        }

        private void SwitchChimney()
        {
            PlayerMotor.SwitchChimney();
        }

        private void SwitchStove()
        {
            PlayerMotor.SwitchStove();
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer(Layers.CameraSpot))
            {
                CameraScript.Target = col.transform;
            }
        }

        void OnTriggerExit2D(Collider2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer(Layers.CameraSpot))
            {
                CameraScript.Target = transform;
            }
        }
    }
}
