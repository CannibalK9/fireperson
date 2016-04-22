using UnityEngine;

namespace Assets.Scripts.Player
{
	public class PlayerMotor : MonoBehaviour
	{
		public float Gravity = -25f;
		public float RunSpeed = 8f;
		public float GroundDamping = 1f;

		private float _normalizedHorizontalSpeed;

        public AnimationScript Anim { get; private set; }
		public BoxCollider2D Collider { get; private set; }
		public bool AcceptInput { get; set; }

		public DirectionFacing ClimbingSide
		{
			get
			{
				return _climbHandler.ClimbingSide;
			}
		}

		private PlayerController _controller;
		private ClimbHandler _climbHandler;
		private Vector3 _velocity;
		private LayerMask _defaultPlatformMask;
		private Transform _transform;

		void Awake()
		{
			Anim = transform.parent.parent.GetComponent<AnimationScript>();
			_controller = GetComponent<PlayerController>();
			Collider = GetComponent<BoxCollider2D>();
			_transform = transform.parent.parent;

			_climbHandler = new ClimbHandler(this);
			_defaultPlatformMask = _controller.PlatformMask;
			AcceptInput = true;
		}

		void Update()
		{
			if (AcceptInput && _climbHandler.CurrentClimbingState != ClimbingState.None)
			{
				CancelVelocity();
				RemovePlatformMask();
				AcceptInput = false;
			}

            if (_climbHandler.CurrentClimbingState == ClimbingState.Jump)
            {
                SetHorizontalVelocityInAir();
                _velocity.y += Gravity * Time.deltaTime;
                Move(_velocity * Time.deltaTime);
                _velocity = _controller.Velocity;
            }

            if (AcceptInput)
			{
				ReapplyPlatformMask();

				if (_controller.IsGrounded)
				{
					HandleMovementInputs();
					SetHorizontalVelocityOnGround();
				}
				_velocity.y += Gravity * Time.deltaTime;
				Move(_velocity * Time.deltaTime);
				_velocity = _controller.Velocity;
			}
			else
			{
				_climbHandler.ClimbAnimation();
			}
			_controller.HeatIce();
		}

		public void SetHorizontalVelocityOnGround()
		{
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.deltaTime * GroundDamping);
		}

		public void SetHorizontalVelocityInAir()
		{
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * 10, ref _velocity.x, Time.deltaTime);
		}

		public void SetJumpingVelocity(bool forwards)
		{
			float velocity = 1f;

			if (forwards)
				_normalizedHorizontalSpeed = GetDirectionFacing() == DirectionFacing.Right ? 1 : -1;
			else
				_normalizedHorizontalSpeed = GetDirectionFacing() == DirectionFacing.Left ? 1 : -1;
				
			_velocity.x = Mathf.SmoothDamp(velocity, _normalizedHorizontalSpeed * 10, ref velocity, Time.deltaTime);
			_velocity.y = 1f;
		}

		public void Move(Vector2 deltaMovement)
		{
			_controller.Movement.Move(deltaMovement);
		}

		private void HandleMovementInputs()
		{
			if (_controller.IsGrounded == false)
			{
				Anim.SetBool("falling", true);
				Anim.SetBool("moving", false);
				return;
			}
			Anim.SetBool("falling", false);

			_velocity.y = 0;

			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.2f, _controller.BoxCollider.bounds.min.y);
				RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 2f, _controller.PlatformMask);

				_normalizedHorizontalSpeed = edgeHit && !_controller.CollisionState.Right ? 1 : 0;
				if (_controller.CollisionState.Right && GetDirectionFacing() == DirectionFacing.Right)
					;//BackToWallAnimation
				else if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();
			}
			else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.min.x - 0.2f, _controller.BoxCollider.bounds.center.y);
				RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 2f, _controller.PlatformMask);

				_normalizedHorizontalSpeed = edgeHit && !_controller.CollisionState.Left ? -1 : 0;
				if (_controller.CollisionState.Left && GetDirectionFacing() == DirectionFacing.Left)
					;//BackToWallAnimation
				else if (GetDirectionFacing() == DirectionFacing.Right)
					FlipSprite();
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
			}
			SetAnimationWhenGrounded();

            if (KeyBindings.GetKey(Control.Up))
                _climbHandler.CheckLedgeAbove();
            else if (KeyBindings.GetKey(Control.Down) && _climbHandler.CheckLedgeBelow(ClimbingState.Down, GetDirectionFacing()))
                Anim.PlayAnimation("ClimbDown");
            else if (KeyBindings.GetKey(Control.Jump) && _climbHandler.CheckLedgeBelow(ClimbingState.MoveToEdge, GetDirectionFacing()))
            {
                Anim.PlayAnimation("MoveToEdge");
                _climbHandler.NextClimbingStates.Add(
                    GetDirectionFacing() == DirectionFacing.Left
                    ? ClimbingState.AcrossLeft
                    : ClimbingState.AcrossRight);
            }
            else if (KeyBindings.GetKey(Control.Action))
            {
                _controller.CreatePilotedLight();
                //_animator.Play(Animator.StringToHash("Create light"));
            }
		}

		public void FlipSprite()
		{
			_transform.localScale = new Vector3(-_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
		}

		private void SetAnimationWhenGrounded()
		{
			if ( _normalizedHorizontalSpeed == 0)
				Anim.SetBool("moving", false);
			else
				Anim.SetBool("moving", true);
		}

		private bool PilotedLightExists()
		{
			return GameObject.Find("PilotedLight(Clone)");
		}

		public void CancelVelocity()
		{
			Anim.SetBool("moving", false);
			_normalizedHorizontalSpeed = 0;
			_velocity = Vector3.zero;
		}

		public DirectionFacing GetDirectionFacing()
		{
			DirectionFacing directionFacing = _transform.localScale.x > 0f
				? DirectionFacing.Right
				: DirectionFacing.Left;

			return directionFacing;
		}

		public void RemovePlatformMask()
		{
			_controller._platformMask = 0;
		}

		public void ReapplyPlatformMask()
		{
			_controller._platformMask = _defaultPlatformMask;
		}

		public ClimbingState SwitchClimbingState()
		{
			ClimbingState climbingState = _climbHandler.CurrentClimbingState;
			if (climbingState != ClimbingState.MoveToEdge && climbingState != ClimbingState.Jump)
			{
				if (KeyBindings.GetKey(Control.Left))
					_climbHandler.NextClimbingStates.Add(ClimbingState.AcrossLeft);
				if (KeyBindings.GetKey(Control.Right))
					_climbHandler.NextClimbingStates.Add(ClimbingState.AcrossRight);
				if (KeyBindings.GetKey(Control.Up))
					_climbHandler.NextClimbingStates.Add( ClimbingState.Up);
				if (KeyBindings.GetKey(Control.Down))
					_climbHandler.NextClimbingStates.Add(ClimbingState.Down);
			}
			return _climbHandler.SwitchClimbingState();
		}

		public bool TryClimbDown()
		{
			if (KeyBindings.GetKey(Control.Down))
			{
				_climbHandler.CurrentClimbingState = ClimbingState.Down;
				return true;
			}
			else
				return false;
		}

		public void AllowMovement()
		{
			_climbHandler.MovementAllowed = true;
		}

		public void StopMovement()
		{
			_climbHandler.MovementAllowed = false;
		}

        public void CancelClimbingState()
        {
            _climbHandler.CurrentClimbingState = ClimbingState.None;
        }
    }
}