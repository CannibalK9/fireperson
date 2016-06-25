using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
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
		public bool MovementAllowed { get; set; }

		public DirectionFacing ClimbingSide
		{
			get { return _climbHandler.ClimbSide; }
		}

        public LayerMask DefaultPlatformMask
        {
            get { return _defaultPlatformMask; }
        }

		private PlayerController _controller;
		private ClimbHandler _climbHandler;
		private ClimbingState _climbingState;
		private Vector3 _velocity;
		private LayerMask _defaultPlatformMask;
		private Transform _transform;
		private Collider2D _targetCollider;

		void Awake()
		{
			_transform = transform.parent.parent;
			Anim = _transform.GetComponent<AnimationScript>();
			_controller = GetComponent<PlayerController>();
			Collider = GetComponent<BoxCollider2D>();

			_climbHandler = new ClimbHandler(this);
			AcceptInput = true;
		}

		void Update()
		{
			if (_climbHandler.CurrentClimb == Climb.Jump)
			{
				SetHorizontalVelocityInAir();
				MoveWithVelocity();
				return;
			}
			else if (AcceptInput)
			{
				if (_controller.MovementState.IsGrounded)
				{
					HandleMovementInputs();

					if (_climbHandler.CurrentClimb != Climb.None)
					{
						CancelVelocity();
						AcceptInput = false;
						_climbingState = _climbHandler.GetClimbingState();
						_controller.MovementState.SetPivot(
							_climbingState.PivotCollider.GetPoint(_climbingState.PivotPosition),
							_climbingState.PivotCollider);
					}
					else if (AcceptInput && MovementAllowed)
					{
						SetHorizontalVelocityOnGround();
						MoveWithVelocity();
						return;
					}
				}
				else
					MoveWithVelocity();
			}
			if (AcceptInput == false && MovementAllowed)
			{
				if (_targetCollider == null)
				{
					_controller.Movement.MoveLinearly(
					  _climbingState.MovementSpeed,
					  _controller.BoxCollider.GetPoint(_climbingState.PlayerPosition));
				}
				else
				{
					_controller.Movement.MoveLinearly(
					  0.5f,
					  _controller.BoxCollider.GetBottomCenter());
				}
			}
		}

		private void MoveWithVelocity()
		{
			_velocity.y += Gravity * Time.deltaTime;
			Move(_velocity * Time.deltaTime);
			_velocity = _controller.Velocity;
		}

        private void SetHorizontalVelocityOnGround()
		{
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.deltaTime * GroundDamping);
		}

		private void SetHorizontalVelocityInAir()
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

		private void Move(Vector2 deltaMovement)
		{
			_controller.Movement.BoxCastMove(deltaMovement);
		}

		private void HandleMovementInputs()
		{
			if (_controller.MovementState.IsGrounded == false)
			{
				Anim.SetBool("falling", true);
				Anim.SetBool("moving", false);
				return;
			}
			Anim.SetBool("falling", false);

			_velocity.y = 0;

			if (KeyBindings.GetKey(Control.Right))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.2f, _controller.BoxCollider.bounds.min.y);
				RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 2f, Layers.Platforms);

				_normalizedHorizontalSpeed = edgeHit && !_controller.MovementState.RightCollision ? 1 : 0;
				if (_controller.MovementState.RightCollision && GetDirectionFacing() == DirectionFacing.Right)
					;//BackToWallAnimation
				else if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();
			}
			else if (KeyBindings.GetKey(Control.Left))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.min.x - 0.2f, _controller.BoxCollider.bounds.min.y);
				RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 2f, Layers.Platforms);

				_normalizedHorizontalSpeed = edgeHit && !_controller.MovementState.LeftCollision ? -1 : 0;
				if (_controller.MovementState.LeftCollision && GetDirectionFacing() == DirectionFacing.Left)
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
				_climbHandler.CheckLedgeAbove(GetDirectionFacing());
			else if (KeyBindings.GetKey(Control.Down) && _climbHandler.CheckLedgeBelow(Climb.Down, GetDirectionFacing()))
				Anim.PlayAnimation(Animations.ClimbDown);
			else if (KeyBindings.GetKey(Control.Jump) && _climbHandler.CheckLedgeBelow(Climb.MoveToEdge, GetDirectionFacing()))
			{
				Anim.PlayAnimation(Animations.MoveToEdge);
				_climbHandler.NextClimbs.Add(
					GetDirectionFacing() == DirectionFacing.Left
					? Climb.AcrossLeft
					: Climb.AcrossRight);
			}
			else if (ChannelingHandler.ChannelingSet == false && KeyBindings.GetKeyDown(Control.Light))
            {
				MovementAllowed = false;
				Anim.PlayAnimation(Animations.CreatePL);
			}
			else if (ChannelingHandler.ChannelingSet == false && KeyBindings.GetKey(Control.Light))
			{
                ChannelingHandler.Channel();
			}
			else if (KeyBindings.GetKey(Control.Action))
            {
				MovementAllowed = false;
                RaycastHit2D hit = CheckInteractableInFront(GetDirectionFacing());
                if (hit == false)
                    return;

                if (hit.transform.name.Contains(Interactables.Stilt))
                    Anim.PlayAnimation(Animations.DestroyStilt);
                else if (hit.transform.name.Contains(Interactables.ChimneyLid))
                    Anim.PlayAnimation(Animations.OpenChimney);
                else if (hit.transform.name.Contains(Interactables.Stove))
                    Anim.PlayAnimation(Animations.OpenStove);
            }
        }

        public void CreateLight()
        {
            _controller.CreateLight();
        }

        public void FlipSprite()
		{
			Vector3 target = Collider.bounds.center;
			_transform.localScale = new Vector3(-_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
			Vector3 moving = Collider.bounds.center;
			_transform.position += target - moving;
		}

		private void SetAnimationWhenGrounded()
		{
			Anim.SetBool(
				"moving",
				_normalizedHorizontalSpeed != 0);
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

		public Climb SwitchClimbingState()
		{
			Climb climb = _climbHandler.CurrentClimb;
			var direction = DirectionFacing.None;

			if (climb != Climb.MoveToEdge && climb != Climb.Jump)
			{
				if (KeyBindings.GetKey(Control.Left))
				{
					_climbHandler.NextClimbs.Add(Climb.AcrossLeft);
					direction = DirectionFacing.Left;
				}
				if (KeyBindings.GetKey(Control.Right))
				{
					_climbHandler.NextClimbs.Add(Climb.AcrossRight);
					direction = DirectionFacing.Right;
				}
				if (KeyBindings.GetKey(Control.Up))
					_climbHandler.NextClimbs.Add(Climb.Up);
				if (KeyBindings.GetKey(Control.Down))
					_climbHandler.NextClimbs.Add(Climb.Down);
			}
			ClimbingState climbingState = _climbHandler.SwitchClimbingState(direction);
			if (climbingState.PivotCollider != null)
				_controller.MovementState.SetPivot(
					climbingState.PivotCollider.GetPoint(climbingState.PivotPosition),
					climbingState.PivotCollider);

			return climbingState.Climb;
		}

		public bool TryClimbDown()
		{
			if (KeyBindings.GetKey(Control.Down))
			{
				_climbHandler.CurrentClimb = Climb.Down;
				return true;
			}
			else
				return false;
		}

		public void CancelClimbingState()
		{
			_climbHandler.CancelClimb();
		}

		public RaycastHit2D CheckInteractableInFront(DirectionFacing direction)
		{
			const float checkLength = 2f;
			const float checkDepth = 3f;

			var origin = direction == DirectionFacing.Left
				? new Vector2(
				   Collider.bounds.center.x - checkLength/2,
				   Collider.bounds.min.y)
				   : new Vector2(
						Collider.bounds.center.x + checkLength/2,
						Collider.bounds.min.y);

			var size = new Vector2(checkLength, checkDepth);

			RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.down, 0.01f, 1 << LayerMask.NameToLayer(Layers.Interactable));

			if (hit)
			{
				_targetCollider = hit.collider;
			}
			return hit;
		}

		public void BurnStilt()
		{
            _targetCollider.GetComponent<StiltDestruction>().IsBurning = true;
		}

        public void SwitchChimney()
        {
            _targetCollider.GetComponentInChildren<ChimneyLid>().Switch();
        }

        public void SwitchStove()
        {
            _targetCollider.GetComponent<StoveDoor>().Switch();
        }
    }
}