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
		private bool _recalculate;
		private bool _shouldFlip;

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
			ClimbingState nextState = PlayerMotor.SwitchClimbingState();
			SetupNextState(nextState);
			_shouldFlip = true;
		}

		private void TryClimbDown()
		{
			ClimbingState nextState;
			if (PlayerMotor.TryClimbDown(out nextState))
			{
				if (nextState.Climb == Climb.End)
					nextState.Climb = Climb.Down;
				SetupNextState(nextState);
				_shouldFlip = false;
				if ((nextState.Climb == Climb.SwingLeft || nextState.Climb == Climb.SwingRight || nextState.Climb == Climb.Jump) && _animator.GetBool("swing") == false)
				{
					_shouldFlip = true;
					FlipSprite();
				}
			}
			else
				_animator.SetTrigger("vault");
		}

		private void SetupNextState(ClimbingState nextState)
		{
			_animator.speed = 1;

			_recalculate = nextState.Recalculate;
			if (_recalculate)
			{
				PlayerMotor.MovementAllowed = false;
			}

			switch (nextState.Climb)
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
			if (_shouldFlip)
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

		private void ApplySwingVelocity()
		{
			PlayerMotor.SetSwingVelocity();
		}

		private void AllowMovement()
		{
			if (_recalculate)
				_animator.speed = PlayerMotor.GetAnimationSpeed();
			PlayerMotor.MovementAllowed = true;
		}

		private void MoveHorizontally()
		{
			_animator.speed = 1;
			PlayerMotor.MoveHorizontally();
			PlayerMotor.UpdateClimbingSpeed(0.75f);
		}

		private void MoveVertically()
		{
			_animator.speed = 1;
			PlayerMotor.MoveVertically();
			PlayerMotor.UpdateClimbingSpeed(0.75f);
		}

		private void SwitchStilt()
		{
			PlayerMotor.SwitchStilt();
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
