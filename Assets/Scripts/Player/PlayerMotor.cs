using System;
using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using Assets.Scripts.Movement;
using Assets.Scripts.Player.Climbing;
using Assets.Scripts.Player.Config;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class PlayerMotor : MonoBehaviour, IMotor
	{
		[Range(0f, 90f)]
		public float _slopeLimit = ConstantVariables.DefaultPlayerSlopeLimit;
		public float SlopeLimit { get { return _slopeLimit; } }

		public float Gravity = -25f;
		public float RunSpeed = 8f;
		public float GroundDamping = 1f;

		public AnimationScript Anim { get; private set; }
		public bool MovementAllowed { get; set; }
		public bool Rotating { get; set; }
		public DirectionFacing ClimbingSide { get { return _climbHandler.ClimbSide; } }
		public Transform Transform { get; set; }
		public MovementState MovementState { get; set; }
		public Collider2D Collider { get; set; }
		public Rigidbody2D Rigidbody { get; set; }

		private MovementHandler _movement;
		private ClimbHandler _climbHandler;
		private ClimbingState _climbingState;
		private Vector3 _velocity;
		private Collider2D _interactionCollider;
		private PlayerState _playerState;
		private float _normalizedHorizontalSpeed;
		private const int _rotateFrames = 30;
		private int _currentRotateFrames;

		void Awake()
		{
			Transform = transform.parent.parent;
			Anim = Transform.GetComponent<AnimationScript>();
			Collider = Transform.GetComponent<BoxCollider2D>();
			Rigidbody = Transform.GetComponent<Rigidbody2D>();
			MovementState = new MovementState();
			MovementAllowed = true;

			_movement = new MovementHandler(this);
			_climbHandler = new ClimbHandler(this);
		}

		void FixedUpdate()
		{
			if (Rotating)
			{
				RotateAroundPivot();
				return;
			}
			_playerState = HandleMovementInputs();

			switch (_playerState)
			{
				case PlayerState.WaitingForInput:
					SetHorizontalVelocity();
					MoveWithVelocity(0);
					break;
				case PlayerState.Climbing:
					MoveToClimbingPoint();
					break;
				case PlayerState.Interacting:
					_velocity = new Vector2(_interactionCollider.bounds.center.x - Collider.bounds.center.x, 0);
					MoveWithVelocity(0);
					//cancel animations here
					break;
				case PlayerState.Falling:
					SetHorizontalVelocity();
					MoveWithVelocity(Gravity);
					break;
				case PlayerState.Static:
					CancelVelocity();
					MoveWithVelocity(0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private PlayerState HandleMovementInputs()
		{
			if (MovementState.IsGrounded == false || _climbHandler.CurrentClimb == Climb.Jump)
			{
				Anim.SetBool("falling", true);
				Anim.SetBool("moving", false);
				return PlayerState.Falling;
			}
			else if (ChannelingHandler.IsChanneling && ChannelingHandler.ChannelingSet == false)
			{
				if (KeyBindings.GetKey(Control.Light))
					ChannelingHandler.Channel();
				return PlayerState.Static;
			}
			else if (IsClimbing())
				return PlayerState.Climbing;

			MovementInput();

			Anim.SetBool("falling", false);
			Anim.SetBool("moving", _normalizedHorizontalSpeed != 0);

			if (TryClimb())
				return SetMotorToClimbState();
			else if (TryInteract())
				return PlayerState.Interacting;
			else if (TryChannel())
				return PlayerState.Static;

			return PlayerState.WaitingForInput;
		}

		private void SetHorizontalVelocity()
		{
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.fixedDeltaTime * GroundDamping);
		}

		private void MoveWithVelocity(float gravity)
		{
			transform.localPosition = Vector3.zero;

			if (_velocity.x * _normalizedHorizontalSpeed > ConstantVariables.MaxHorizontalSpeed)
				_velocity.x = ConstantVariables.MaxHorizontalSpeed * _normalizedHorizontalSpeed;
			if (_velocity.y < ConstantVariables.MaxVerticalSpeed)
				_velocity.y = ConstantVariables.MaxVerticalSpeed;

			_velocity.y += gravity * Time.fixedDeltaTime;
			_movement.BoxCastMove(_velocity * Time.fixedDeltaTime);	
		}

		private void MoveToClimbingPoint()
		{
			_currentRotateFrames = 0;
			float speed = MovementAllowed
				? _climbingState.MovementSpeed
				: 0;

			if (MovementAllowed == false && _movement.IsCollidingWithNonPivot())
			{
				_climbHandler.CurrentClimb = Climb.None;
				return;
			}

			_movement.MoveLinearly(speed, Collider.GetPoint(_climbingState.PlayerPosition));
		}

		private bool IsClimbing()
		{
			return _climbHandler.CurrentClimb != Climb.None;
		}

		private PlayerState SetMotorToClimbState()
		{
			_interactionCollider = null;
			CancelVelocity();
			_climbingState = _climbHandler.GetClimbingState();
			MovementState.SetPivot(
				_climbingState.PivotCollider.GetPoint(_climbingState.PivotPosition),
				_climbingState.PivotCollider.transform.parent);

			return PlayerState.Climbing;
		}

		private void MovementInput()
		{
			if (KeyBindings.GetKey(Control.Right))
			{
				if (MovementState.RightCollision == false && NotAtEdge(DirectionTravelling.Right))
					_normalizedHorizontalSpeed = 1;
				else
					_normalizedHorizontalSpeed = 0;
				if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();
			}
			else if (KeyBindings.GetKey(Control.Left))
			{
				if (MovementState.LeftCollision == false && NotAtEdge(DirectionTravelling.Left))
					_normalizedHorizontalSpeed = -1;
				else
					_normalizedHorizontalSpeed = 0;
				if (GetDirectionFacing() == DirectionFacing.Right)
					FlipSprite();
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
			}
			if (MovementState.RightCollision || MovementState.LeftCollision)
			{
				Debug.Log("against wall");
				//BackToWallAnimation
			}
		}

		private bool NotAtEdge(DirectionTravelling direction)
		{
			float xOrigin = direction == DirectionTravelling.Right
				? Collider.bounds.max.x + 0.2f
				: Collider.bounds.min.x - 0.2f;
			var edgeRay = new Vector2(xOrigin, Collider.bounds.min.y);
			return Physics2D.Raycast(edgeRay, Vector2.down, 2f, Layers.Platforms);
		}

		private bool TryClimb()
		{
			if (KeyBindings.GetKey(Control.Up) && _climbHandler.CheckLedgeAbove(GetDirectionFacing()) != Climb.None)
			{
			}
			else if (KeyBindings.GetKey(Control.Down) &&_climbHandler.CheckLedgeBelow(Climb.Down, GetDirectionFacing()))
			{
				Anim.PlayAnimation(Animations.ClimbDown);
			}
			else if (KeyBindings.GetKey(Control.Jump) && _climbHandler.CheckLedgeBelow(Climb.MoveToEdge, GetDirectionFacing()))
			{
				Anim.PlayAnimation(Animations.MoveToEdge);
				_climbHandler.NextClimbs.Add(
					GetDirectionFacing() == DirectionFacing.Left
					? Climb.AcrossLeft
					: Climb.AcrossRight);
			}
			else 
				return false;
			
			return true;
		}

		private bool TryInteract()
		{
			if (KeyBindings.GetKey(Control.Action))
			{
				RaycastHit2D hit = CheckInteractableInFront(GetDirectionFacing());
				if (hit.collider != null)
				{
					_interactionCollider = hit.collider;

					if (hit.transform.name.Contains(Interactables.Stilt))
						Anim.PlayAnimation(Animations.DestroyStilt);
					else if (hit.transform.name.Contains(Interactables.ChimneyLid))
						Anim.PlayAnimation(Animations.OpenChimney);
					else if (hit.transform.name.Contains(Interactables.Stove))
						Anim.PlayAnimation(Animations.OpenStove);

					return true;
				}
			}
			return false;
		}

		private bool TryChannel()
		{
			if (ChannelingHandler.ChannelingSet == false && KeyBindings.GetKeyDown(Control.Light) && ChannelingHandler.IsChanneling == false)
			{
				ChannelingHandler.Channel();
				Anim.PlayAnimation(Animations.CreatePL);
				return true;
			}
			return false;
		}

		public void SetJumpingVelocity(bool forwards)
		{
			if (forwards)
				_normalizedHorizontalSpeed = GetDirectionFacing() == DirectionFacing.Right ? 1 : -1;
			else
				_normalizedHorizontalSpeed = GetDirectionFacing() == DirectionFacing.Left ? 1 : -1;

			_velocity.x = _normalizedHorizontalSpeed * 10;
			_velocity.y = 10f;
		}

        public void FlipSprite()
		{
			Vector3 target = Collider.bounds.center;
			Transform.localScale = new Vector3(-Transform.localScale.x, Transform.localScale.y, Transform.localScale.z);
			Vector3 moving = Collider.bounds.center;
			Transform.position += target - moving;
		}

		public void CancelVelocity()
		{
			Anim.SetBool("moving", false);
			_normalizedHorizontalSpeed = 0;
			_velocity = Vector3.zero;
		}

		public DirectionFacing GetDirectionFacing()
		{
			DirectionFacing directionFacing = Transform.localScale.x > 0f
				? DirectionFacing.Right
				: DirectionFacing.Left;

			return directionFacing;
		}

		private void RotateAroundPivot()
		{
			_movement.Rotate(Transform, _climbingState.PlayerPosition);
			_currentRotateFrames++;

			if (_currentRotateFrames == _rotateFrames)
			{
				Rotating = false;
				_currentRotateFrames = 0;
			}
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
			_climbingState = _climbHandler.SwitchClimbingState(direction);
			if (_climbingState.PivotCollider != null)
				MovementState.SetPivot(
					_climbingState.PivotCollider.GetPoint(_climbingState.PivotPosition),
					_climbingState.PivotCollider.transform.parent);

			return _climbingState.Climb;
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

		private RaycastHit2D CheckInteractableInFront(DirectionFacing direction)
		{
			const float checkLength = 2f;
			const float checkHeight = 0.1f;

			float xOrigin = direction == DirectionFacing.Right
				? Collider.bounds.min.x
				: Collider.bounds.max.x;

			var origin = new Vector2(
				xOrigin,
				Collider.bounds.min.y + checkHeight);

			Vector2 castDirection = GetSurfaceDirection(direction == DirectionFacing.Left ? DirectionTravelling.Left : DirectionTravelling.Right);

			return Physics2D.Raycast(origin, castDirection, checkLength, 1 << LayerMask.NameToLayer(Layers.Interactable));
		}

		public Vector3 GetSurfaceDirection(DirectionTravelling direction)
		{
			return _movement.GetSurfaceDirection(direction);
		}

		public void BurnStilt()
		{
            _interactionCollider.GetComponent<StiltDestruction>().IsBurning = true;
		}

        public void SwitchChimney()
        {
            _interactionCollider.GetComponentInChildren<ChimneyLid>().Switch();
        }

        public void SwitchStove()
        {
            _interactionCollider.GetComponent<StoveDoor>().Switch();
        }

		public float GetAnimationSpeed()
		{
			if (_climbingState == null || MovementState.GroundPivot == null)
				return 1;

			float distance = Vector2.Distance(Collider.GetPoint(_climbingState.PlayerPosition), MovementState.GroundPivot.transform.position);

			float speed = _playerState == PlayerState.Climbing
				? _climbingState.MovementSpeed
				: ConstantVariables.DefaultMovementSpeed;

			float animSpeed = 4 / (distance / speed);
			return animSpeed < 10
				? animSpeed
				: 10;
		}

		public Vector2 GetGroundPivotPosition()
		{
			return MovementState.GroundPivot.transform.position;
		}
    }
}