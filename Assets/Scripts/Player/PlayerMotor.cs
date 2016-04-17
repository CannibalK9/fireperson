using UnityEngine;

namespace Assets.Scripts.Player
{
	public class PlayerMotor : MonoBehaviour
	{
		public float Gravity = -25f;
		public float RunSpeed = 8f;
		public float GroundDamping = 1f;

		private float _normalizedHorizontalSpeed;

		public Animator Animator { get; private set; }
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
			Animator = transform.parent.parent.GetComponent<Animator>();
			_controller = GetComponent<PlayerController>();
			Collider = GetComponent<BoxCollider2D>();
			_transform = transform.parent.parent;

			_climbHandler = new ClimbHandler(this);
			_defaultPlatformMask = _controller.PlatformMask;
            AcceptInput = true;
		}

		void Update()
		{
			if (Animator == null)
				Animator = transform.parent.parent.GetComponent<Animator>();

            if (AcceptInput && _climbHandler.CurrentClimbingState != ClimbingState.None)
            {
                CancelVelocity();
                RemovePlatformMask();
                AcceptInput = false;
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

		public void SetHorizontalVelocity(float velocity, bool forwards)
		{
			if (forwards)
				_normalizedHorizontalSpeed = GetDirectionFacing() == DirectionFacing.Right ? 1 : -1;
			else
				_normalizedHorizontalSpeed = GetDirectionFacing() == DirectionFacing.Left ? 1 : -1;
				
			_velocity.x = Mathf.SmoothDamp(velocity, _normalizedHorizontalSpeed * 10, ref velocity, Time.deltaTime);
		}

		public void Move(Vector2 deltaMovement)
		{
			_controller.Movement.Move(deltaMovement);
		}

		private void HandleMovementInputs()
		{
			if (_controller.IsGrounded == false)
			{
				Animator.SetBool("falling", true);
				Animator.SetBool("moving", false);
				return;
			}
			Animator.SetBool("falling", false);

			_velocity.y = 0;

			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.8f, _controller.BoxCollider.bounds.min.y);
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
				RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 5f, _controller.PlatformMask);

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

			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				if (_climbHandler.CheckLedgeAbove())
					Animator.Play(Animator.StringToHash("ClimbUp"));
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				if (_climbHandler.CheckLedgeBelow(ClimbingState.Down, DirectionFacing.None))
					Animator.Play(Animator.StringToHash("ClimbDown"));
			}
			else if (Input.GetKeyDown(KeyCode.Space))
			{
				if (_climbHandler.CheckLedgeBelow(ClimbingState.MoveToEdge, GetDirectionFacing()))
				{
					Animator.Play(Animator.StringToHash("MoveToEdge"));
					_climbHandler.NextClimbingState = GetDirectionFacing() == DirectionFacing.Left
                        ? ClimbingState.AcrossLeft
                        : ClimbingState.AcrossRight;
				}
			}
			else if (Input.GetKeyDown(KeyCode.E))
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
			if (_controller.IsGrounded && _normalizedHorizontalSpeed == 0)
				Animator.SetBool("moving", false);
			else
			{
				Animator.SetBool("moving", true);
			}
		}

		private bool PilotedLightExists()
		{
			return GameObject.Find("PilotedLight(Clone)");
		}

		public void CancelVelocity()
		{
            Animator.SetBool("moving", false);
			_normalizedHorizontalSpeed = 0;
			_velocity = Vector3.zero;
		}

		public DirectionFacing GetDirectionFacing()
		{
			DirectionFacing directionFacing;
			if (_transform.localScale.x > 0f)
				directionFacing = DirectionFacing.Right;
			else
				directionFacing = DirectionFacing.Left;

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
			if (_climbHandler.CurrentClimbingState == ClimbingState.Jump)
			{
				_climbHandler.NextClimbingState = ClimbingState.None;
			}
            else if (_climbHandler.CurrentClimbingState != ClimbingState.MoveToEdge)
			{
				if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
					_climbHandler.NextClimbingState = ClimbingState.AcrossLeft;
				else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
					_climbHandler.NextClimbingState = ClimbingState.AcrossRight;
				else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
					_climbHandler.NextClimbingState = ClimbingState.Up;
				else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
					_climbHandler.NextClimbingState = ClimbingState.Down;
			}
			return _climbHandler.SwitchClimbingState();
		}

        public bool TryClimbDown()
        {
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
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
	}
}