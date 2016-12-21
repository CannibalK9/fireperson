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
		private bool _shouldFlip;
		private bool _isHanging;
		private bool _ignoreMovement;

		void Awake()
		{
			_animator = GetComponent<Animator>();
		}

		 void Start()
        {
            CameraScript.Player = transform;
        }

		void Update()
		{
			if (PlayerMotor.IsMoving())
			{
				Reset();
			}

			if (_isHanging)
				TryHangingInputWithoutAudio();
		}

		private void Reset()
		{
			_animator.speed = 1;
			_isHanging = false;
			_animator.ResetTrigger("climbUp");
			_animator.ResetTrigger("transitionDown");
			_animator.ResetTrigger("transitionAcross");
			_animator.ResetTrigger("flipUp");
			_animator.ResetTrigger("mantle");
			_animator.ResetTrigger("stopClimbing");

			PlayerMotor.CancelClimbingState();
			InteractionComplete();
		}

		public void SetBool(string boolName, bool value)
		{
			_animator.SetBool(boolName, value);
		}

		public bool GetBool(string boolName)
		{
			return _animator.GetBool(boolName);
		}

		public void SetAcrossTrigger()
		{
			_animator.SetTrigger("transitionAcross");
		}

		public void ResetAcrossTrigger()
		{
			_animator.ResetTrigger("transitionAcross");
		}

		public void PlayAnimation(string anim)
		{
			_animator.Play(Animator.StringToHash(anim));
		}

		public void SwitchClimbingState()
		{
			SwitchClimbingState(false);
		}

		public void SwitchClimbingStateIgnoreUp()
		{
			SwitchClimbingState(true);
		}

		private void SwitchClimbingState(bool ignoreUp)
		{
			_ignoreMovement = false;
			Climb nextClimb = PlayerMotor.SwitchClimbingState(ignoreUp);
			SetupNextState(nextClimb);
			_shouldFlip = true;
		}

		private void TryHangingInput()
		{
			TryHangingInputWithoutAudio();
		}

		private void TryHangingInputWithoutAudio()
		{
			Climb nextClimb;
			_animator.speed = 1;
			_isHanging = true;
			if (PlayerMotor.TryHangingInput(out nextClimb))
			{
				_isHanging = false;
				if (nextClimb == Climb.None)
					nextClimb = Climb.Down;
				SetupNextState(nextClimb);
			}
		}

		private void ContinuousHangingInput()
		{
			_isHanging = true;
		}

		private void SetupNextState(Climb nextClimb)
		{
			_animator.speed = 1;

			switch (nextClimb)
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
				case Climb.None:
				case Climb.Hang:
					_animator.SetTrigger("stopClimbing");
					break;
			}
		}

		private void InteractionComplete()
		{
			PlayerMotor.Interaction.IsInteracting = false;
		}

		private void FlipSpriteTowardsEdge()
		{
			if (PlayerMotor.ClimbingSide == PlayerMotor.GetDirectionFacing())
				PlayerMotor.FlipSprite();
			SetBool(PlayerAnimBool.Forward, true);
		}

		private void FlipSpriteAwayFromEdge()
		{
			if (PlayerMotor.ClimbingSide != PlayerMotor.GetDirectionFacing())
				PlayerMotor.FlipSprite();
			SetBool(PlayerAnimBool.Forward, true);
		}

		private void FlipSprite()
		{
			if (_shouldFlip)
				PlayerMotor.FlipSprite();
			SetBool(PlayerAnimBool.Forward, true);
		}

		public void FlipSpriteRight()
		{
			if (PlayerMotor.GetDirectionFacing() == DirectionFacing.Left)
				PlayerMotor.FlipSprite();
			SetBool(PlayerAnimBool.Forward, true);
		}

		public void FlipSpriteLeft()
		{
			if (PlayerMotor.GetDirectionFacing() == DirectionFacing.Right)
				PlayerMotor.FlipSprite();
			SetBool(PlayerAnimBool.Forward, true);
		}

		private void Hop()
		{
			_animator.speed = 1;
			PlayerMotor.Hop();
		}

		private void MoveHorizontally()
		{
			_animator.speed = 1;
			PlayerMotor.MoveHorizontally();
		}

		private void MoveVertically()
		{
			_animator.speed = 1;
			if (_ignoreMovement == false)
				PlayerMotor.MoveVertically();
		}

		private void IgnoreMovement()
		{
			_ignoreMovement = true;
		}

		private void MoveToNextPivotPoint()
		{
			bool isJumping = PlayerMotor.IsJumping();
			SetBool(PlayerAnimBool.IsJumping, isJumping);
			if (isJumping)
			{
				_animator.speed = 1;
				if (_animator.GetBool(PlayerAnimBool.Forward))
					PlayerMotor.SetJumpingVelocity(true);
				else
					PlayerMotor.SetJumpingVelocity(false);
			}
			else
			{
				_animator.speed = PlayerMotor.MoveToNextPivotPoint();
			}
		}

		private void SwitchStilt()
		{
			PlayerMotor.SwitchStilt();
		}

		private void CreateLight()
        {
            PlayerController.CreateLight();
        }

		private void StopChanneling()
		{
			PlayerController.StopChanneling();
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
                CameraScript.Player = col.transform;
            }
        }

        void OnTriggerExit2D(Collider2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer(Layers.CameraSpot))
            {
                CameraScript.Player = transform;
            }
        }
    }
}
