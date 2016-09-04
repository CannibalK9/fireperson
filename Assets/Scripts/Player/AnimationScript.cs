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
		private bool _isHanging;
		private bool _isJumping;

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
				_isHanging = false;
				_isJumping = false;
				_animator.ResetTrigger("climbUp");
				_animator.ResetTrigger("transitionDown");
				_animator.ResetTrigger("transitionAcross");
				_animator.ResetTrigger("flipUp");
				PlayerMotor.CancelClimbingState();
			}

			if (_isHanging)
				TryHangingInput();
		}

		public void SetBool(string boolName, bool value)
		{
			_animator.SetBool(boolName, value);
		}

		public bool GetBool(string boolName)
		{
			return _animator.GetBool(boolName);
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

		private void TryHangingInput()
		{
			ClimbingState nextState;
			_animator.speed = 1;
			if (PlayerMotor.TryHangingInput(out nextState))
			{
				_isHanging = false;
				if (nextState.Climb == Climb.End)
					nextState.Climb = Climb.Down;
				SetupNextState(nextState);
			}
		}

		private void ContinuousHangingInput()
		{
			_isHanging = true;
		}

		private void SetupNextState(ClimbingState nextState)
		{
			_animator.speed = 1;

			_recalculate = nextState.Recalculate;
			if (_recalculate)
			{
				PlayerMotor.MovementAllowed = false;
			}

			_isJumping = nextState.Climb == Climb.Jump;
			SetBool("isJumping", _isJumping);

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
				case Climb.Jump:
					_animator.SetTrigger("transitionAcross");
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

		private void AllowMovement()
		{
			if (_isJumping)
			{
				_animator.speed = 1;
				if (_animator.GetBool("forward"))
					PlayerMotor.SetJumpingVelocity(true);
				else
					PlayerMotor.SetJumpingVelocity(false);
			}
			else
			{
				if (_recalculate)
					_animator.speed = PlayerMotor.GetAnimationSpeed();
				PlayerMotor.MovementAllowed = true;
			}
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
