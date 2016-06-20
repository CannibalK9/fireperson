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
			get { return _climbHandler.ClimbingSide; }
		}

        public LayerMask DefaultPlatformMask
        {
            get { return _defaultPlatformMask; }
        }

		private PlayerController _controller;
		private ClimbHandler _climbHandler;
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

			_controller.Movement.MoveWithBuilding();

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
				if (_targetCollider != null)
				{
					Vector2 player = GetDirectionFacing() == DirectionFacing.Left
						? Collider.GetBottomLeft()
						: Collider.GetBottomRight();
					LinearMovement(_targetCollider.GetBottomCenter(), player, 0.5f);
				}
			}
		}

        public void SetBuildingTransform(Transform t)
        {
            _controller.Movement.BuildingTransform = t;
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

		public void Move(Vector2 deltaMovement)
		{
			_controller.Movement.Move(deltaMovement);
		}

        public float ChannelingTime { get; set; }

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

			if (KeyBindings.GetKey(Control.Right))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.2f, _controller.BoxCollider.bounds.min.y);
				RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 2f, _controller.PlatformMask);

				_normalizedHorizontalSpeed = edgeHit && !_controller.CollisionState.Right ? 1 : 0;
				if (_controller.CollisionState.Right && GetDirectionFacing() == DirectionFacing.Right)
					;//BackToWallAnimation
				else if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();
			}
			else if (KeyBindings.GetKey(Control.Left))
			{
				var edgeRay = new Vector2(_controller.BoxCollider.bounds.min.x - 0.2f, _controller.BoxCollider.bounds.min.y);
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
				_climbHandler.CheckLedgeAbove(GetDirectionFacing());
			else if (KeyBindings.GetKey(Control.Down) && _climbHandler.CheckLedgeBelow(ClimbingState.Down, GetDirectionFacing()))
				Anim.PlayAnimation(Animations.ClimbDown);
			else if (KeyBindings.GetKey(Control.Jump) && _climbHandler.CheckLedgeBelow(ClimbingState.MoveToEdge, GetDirectionFacing()))
			{
				Anim.PlayAnimation(Animations.MoveToEdge);
				_climbHandler.NextClimbingStates.Add(
					GetDirectionFacing() == DirectionFacing.Left
					? ClimbingState.AcrossLeft
					: ClimbingState.AcrossRight);
			}
			else if (KeyBindings.GetKeyDown(Control.Light) && _controller.ActivePilotedLight == null)
            {
                ChannelingTime += Time.deltaTime;
            }
            else if (KeyBindings.GetKey(Control.Light) && _controller.ActivePilotedLight == null)
			{
                if (ChannelingTime != 0 && ChannelingTime < 3f)
                    ChannelingTime += Time.deltaTime;
				Anim.PlayAnimation(Animations.CreatePL);
			}
			else if (KeyBindings.GetKey(Control.Action))
            {
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
			var direction = DirectionFacing.None;

			if (climbingState != ClimbingState.MoveToEdge && climbingState != ClimbingState.Jump)
			{
				if (KeyBindings.GetKey(Control.Left))
				{
					_climbHandler.NextClimbingStates.Add(ClimbingState.AcrossLeft);
					direction = DirectionFacing.Left;
				}
				if (KeyBindings.GetKey(Control.Right))
				{
					_climbHandler.NextClimbingStates.Add(ClimbingState.AcrossRight);
					direction = DirectionFacing.Right;
				}
				if (KeyBindings.GetKey(Control.Up))
					_climbHandler.NextClimbingStates.Add(ClimbingState.Up);
				if (KeyBindings.GetKey(Control.Down))
					_climbHandler.NextClimbingStates.Add(ClimbingState.Down);
			}
			return _climbHandler.SwitchClimbingState(direction);
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

		public void CancelClimbingState()
		{
			_climbHandler.CurrentClimbingState = ClimbingState.None;
		}

		public void LinearMovement(Vector2 targetPoint, Vector2 movingPoint, float speed)
		{
			if (MovementAllowed)
				Move((targetPoint - movingPoint) * speed);
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