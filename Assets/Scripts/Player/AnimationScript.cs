using Assets.Scripts.CameraHandler;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player.Climbing;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class AnimationScript : MonoBehaviour
	{
		public PlayerMotor PlayerMotor;
		public PlayerController PlayerController;
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
			Climb nextState = PlayerMotor.SwitchClimbingState();

			switch (nextState)
			{
				case Climb.Up:
					_animator.SetTrigger("climbUp");
					break;
				case Climb.Flip:
					_animator.SetTrigger("flipUp");
					break;
				case Climb.Mantle:
					_animator.SetTrigger("mantle");
					break;
				case Climb.Down:
					_animator.SetTrigger("transitionDown");
					break;
				case Climb.AcrossRight:
				case Climb.AcrossLeft:
				case Climb.SwingRight:
				case Climb.SwingLeft:
					_animator.SetTrigger("transitionAcross");
					break;
				case Climb.Jump:
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

		private void Rotate()
		{
			PlayerMotor.Rotating = true;
		}

		private void AllowMovement()
		{
			_animator.speed = PlayerMotor.GetAnimationSpeed();
			PlayerMotor.MovementAllowed = true;
		}

		private void StopMovement()
		{
			_animator.speed = 1;
			PlayerMotor.MovementAllowed = false;
		}

		private void DestroyStilt()
		{
			PlayerMotor.BurnStilt();
		}

        private void CreateLight()
        {
            PlayerController.CreateLight();
        }

        private void SwitchChimney()
        {
            PlayerMotor.SwitchChimney();
        }

        private void SwitchStove()
        {
            PlayerMotor.SwitchStove();
        }

		public void Spotted()
		{
			PlayerController.Spotted();
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
