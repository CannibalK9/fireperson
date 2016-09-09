using System;
using System.Linq;
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
		public DirectionFacing ClimbingSide { get { return _climbHandler.ClimbSide; } }
		public Transform Transform { get; set; }
		public MovementState MovementState { get; set; }
		public Collider2D Collider { get; set; }
		public Rigidbody2D Rigidbody { get; set; }
		public ClimbingState ClimbingState { get; set; }

		private MovementHandler _movement;
		private ClimbHandler _climbHandler;
		private Vector3 _velocity;
		private Transform _interactionObject;
		private PlayerState _playerState;
		private float _normalizedHorizontalSpeed;
        private float _interactionMovementSpeed;

		void Awake()
		{
			Transform = transform.parent.parent;
			Anim = Transform.GetComponent<AnimationScript>();
			Collider = Transform.GetComponent<BoxCollider2D>();
			Rigidbody = Transform.GetComponent<Rigidbody2D>();
			MovementState = new MovementState(Collider.bounds.extents);
			MovementAllowed = true;

			_movement = new MovementHandler(this);
			_climbHandler = new ClimbHandler(this);
		}

		void FixedUpdate()
		{
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
                    MoveToInteractionPoint();
					break;
				case PlayerState.Falling:
					SetHorizontalVelocity();
					MoveWithVelocity(Gravity);
					break;
				case PlayerState.Static:
					CancelVelocity();
					MoveWithVelocity(0);
					break;
				case PlayerState.Jumping:
					MoveWithVelocity(0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private PlayerState HandleMovementInputs()
		{
			if (_climbHandler.CurrentClimb == Climb.Jump)
				return PlayerState.Jumping;
			else if (MovementState.IsGrounded == false)
			{
				Anim.SetBool("falling", true);
				Anim.SetBool("moving", false);
				Anim.SetBool("upright", false);
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
			else if (TryClimb())
				return SetMotorToClimbState();
			else if (TryInteract())
				return PlayerState.Interacting;
			else if (TryChannel())
				return PlayerState.Static;

			Anim.SetBool("falling", false);
			Anim.SetBool("moving", _normalizedHorizontalSpeed != 0);

			MovementInput();

			return PlayerState.WaitingForInput;
		}

		private void SetHorizontalVelocity()
		{
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.fixedDeltaTime * GroundDamping);
		}

		private void MoveWithVelocity(float gravity)
		{
			transform.localPosition = Vector3.zero;

			float maxX = ConstantVariables.MaxHorizontalSpeed;

			if (Physics2D.Raycast(Collider.GetTopLeft() + Vector3.up * 0.1f, Vector2.right, Collider.bounds.size.x, Layers.Platforms))
			{
				maxX = ConstantVariables.SquashedSpeed;
				Anim.SetBool("squashed", true);
			}
			else
				Anim.SetBool("squashed", false);

			if (_velocity.x * _normalizedHorizontalSpeed > maxX)
				_velocity.x = maxX * _normalizedHorizontalSpeed;
			if (_velocity.y < ConstantVariables.MaxVerticalSpeed)
				_velocity.y = ConstantVariables.MaxVerticalSpeed;

			_velocity.y += gravity * Time.fixedDeltaTime;
			bool isKinetic = _playerState == PlayerState.Jumping;
			_movement.BoxCastMove(_velocity * Time.fixedDeltaTime, isKinetic);	
		}

		private void MoveToClimbingPoint()
		{
			float speed = MovementAllowed
				? ClimbingState.MovementSpeed
				: 0;

			bool isCorner = Anim.GetBool("corner") && (MovementState.CharacterPoint == ColliderPoint.TopLeft || MovementState.CharacterPoint == ColliderPoint.TopRight);

			if (_climbHandler.CurrentClimb == Climb.Down && _movement.IsCollidingWithNonPivot())
			{
				CancelClimbingState();
                CancelAnimation();
			}
			else if (_movement.MoveLinearly(speed, isCorner) == false)
			{
				if (_climbHandler.CheckReattach())
					_movement.MoveLinearly(speed, isCorner);
				else
				{
					CancelClimbingState();
                    CancelAnimation();
				}
			}
		}

        private void MoveToInteractionPoint()
        {
            if (_movement.IsCollidingWithNonPivot())
            {
                CancelAnimation();
            }
            else
            {
                if (MovementAllowed)
                    _movement.MoveLinearly(_interactionMovementSpeed);
                else
                    MoveWithVelocity(0);
            }
        }

		public void UpdateClimbingSpeed(float speed)
		{
			ClimbingState.MovementSpeed = speed;
		}

		public void MoveHorizontally()
		{
			ColliderPoint newPoint;

			switch (MovementState.CharacterPoint)
			{
				case ColliderPoint.TopLeft:
					newPoint = ColliderPoint.TopRight;
					break;
				case ColliderPoint.TopRight:
					newPoint = ColliderPoint.TopLeft;
					break;
				case ColliderPoint.BottomLeft:
					newPoint = ColliderPoint.BottomRight;
					break;
				case ColliderPoint.BottomRight:
					newPoint = ColliderPoint.BottomLeft;
					break;
				case ColliderPoint.LeftFace:
					newPoint = ColliderPoint.RightFace;
					break;
				case ColliderPoint.RightFace:
					newPoint = ColliderPoint.LeftFace;
					break;
				default:
					throw new ArgumentOutOfRangeException(MovementState.CharacterPoint.ToString());
			}

			MovementState.CharacterPoint = newPoint;
		}

		public void MoveVertically()
		{
			ColliderPoint newPoint;

			switch (MovementState.CharacterPoint)
			{
				case ColliderPoint.TopLeft:
					newPoint = ColliderPoint.BottomLeft;
					break;
				case ColliderPoint.TopRight:
					newPoint = ColliderPoint.BottomRight;
					break;
				case ColliderPoint.BottomLeft:
					newPoint = ColliderPoint.TopLeft;
					break;
				case ColliderPoint.BottomRight:
					newPoint = ColliderPoint.TopRight;
					break;
				case ColliderPoint.TopFace:
					newPoint = ColliderPoint.BottomFace;
					break;
				case ColliderPoint.BottomFace:
					newPoint = ColliderPoint.TopFace;
					break;
				case ColliderPoint.LeftFace:
					newPoint = ColliderPoint.BottomLeft;
					break;
				case ColliderPoint.RightFace:
					newPoint = ColliderPoint.BottomRight;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			MovementState.CharacterPoint = newPoint;
		}

		private bool IsClimbing()
		{
			return _climbHandler.CurrentClimb != Climb.None;
		}

		private PlayerState SetMotorToClimbState()
		{
			_interactionObject = null;
			CancelVelocity();
			ClimbingState = _climbHandler.GetClimbingState(true);
			MovementState.SetPivot(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);

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

			Debug.DrawRay(edgeRay, MovementState.GetSurfaceDownDirection(), Color.blue);
			return Physics2D.Raycast(edgeRay, MovementState.GetSurfaceDownDirection(), 1.5f, Layers.Platforms);
		}

		private bool TryClimb()
		{
			Climb climb;
			if (KeyBindings.GetKey(Control.Up) && _climbHandler.CheckLedgeAbove(GetDirectionFacing(), out climb, false))
			{
				switch (climb)
				{
					case Climb.Up:
						Anim.PlayAnimation(Animations.ToEdge);
						break;
					case Climb.Flip:
						Anim.PlayAnimation(Animations.InverseToEdge);
						break;
					case Climb.Mantle:
						Anim.PlayAnimation(Animations.Mantle);
						break;
				}
			}
			else if (KeyBindings.GetKey(Control.Down) && _climbHandler.CheckLedgeBelow(Climb.Down, GetDirectionFacing()))
			{
				Anim.PlayAnimation(_climbHandler.DistanceToEdge > ConstantVariables.DistanceToTriggerRollDown
						? Animations.RollDown
						: Animations.ClimbDown);
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
				if (hit)
				{
					_interactionObject = hit.collider.transform.parent;
                    MovementState.SetPivot(hit.collider, ColliderPoint.Centre, MovementState.CharacterPoint);
                    _interactionMovementSpeed = GetAnimationSpeed();

					string interactName = _interactionObject.name;

					if (interactName.Contains(Interactables.Stilt))
						InteractWithStilt();
					else if (interactName.Contains(Interactables.ChimneyLid))
						Anim.PlayAnimation(Animations.OpenChimney);
					else if (interactName.Contains(Interactables.Stove))
						Anim.PlayAnimation(Animations.OpenStove);

					return true;
				}
			}
			else if (KeyBindings.GetKey(Control.Ability1) && ChannelingHandler.ChannelingSet)
			{
				ChannelingHandler.IsTethered = !ChannelingHandler.IsTethered;
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

			_velocity.x = _normalizedHorizontalSpeed * 20;
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

		public ClimbingState SwitchClimbingState()
		{
			Climb climb = _climbHandler.CurrentClimb;
			var direction = DirectionFacing.None;

			if (climb != Climb.MoveToEdge && climb != Climb.Jump && climb != Climb.End)
			{
				direction = AddNextClimbs();
			}

			ClimbingState = _climbHandler.SwitchClimbingState(direction);
			if (ClimbingState.PivotCollider != null && ClimbingState.Climb != Climb.Jump && ClimbingState.Climb != Climb.End && ClimbingState.Recalculate)
				MovementState.SetPivot(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);

			return ClimbingState;
		}

		public bool TryHangingInput(out ClimbingState climbingState)
		{
			_climbHandler.CurrentClimb = Climb.Down;
			DirectionFacing direction = AddNextClimbs();
			
			if (_climbHandler.NextClimbs.Any())
			{
				ClimbingState = _climbHandler.SwitchClimbingState(direction);

				if (ClimbingState.Recalculate == false && ClimbingState.Climb == Climb.End)
				{
					climbingState = ClimbingState;
					return false;
				}

				if (ClimbingState.PivotCollider != null && ClimbingState.Climb != Climb.End && ClimbingState.Recalculate)
					MovementState.SetPivot(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);
				else if (ClimbingState.PivotCollider == null || ClimbingState.Climb == Climb.End)
					MovementState.UnsetPivot();

				climbingState = ClimbingState;
				return true;
			}
			else
			{
				climbingState = ClimbingState;
				return false;
			}
		}

		public bool TryAnchoredInput(out ClimbingState climbingState)
		{
			_climbHandler.CurrentClimb = Climb.Up;
			DirectionFacing direction = AddNextClimbs();

			if (_climbHandler.NextClimbs.Any())
			{
				ClimbingState = _climbHandler.SwitchClimbingState(direction);

				if (ClimbingState.Recalculate == false && ClimbingState.Climb == Climb.End)
				{
					climbingState = ClimbingState;
					return false;
				}

				if (ClimbingState.PivotCollider != null && ClimbingState.Climb != Climb.End && ClimbingState.Recalculate)
					MovementState.SetPivot(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);
				else if (ClimbingState.PivotCollider == null)
					MovementState.UnsetPivot();

				climbingState = ClimbingState;
				return true;
			}
			else
			{
				climbingState = ClimbingState;
				return false;
			}
		}

		private DirectionFacing AddNextClimbs()
		{
			var direction = DirectionFacing.None;

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

			return direction;
		}

		public void CancelClimbingState()
		{
			_climbHandler.CancelClimb();
		}

        private void CancelAnimation()
        {
            Anim.PlayAnimation(Animations.Idle);
            MoveWithVelocity(0);
            Transform.rotation = new Quaternion();
        }

		private RaycastHit2D CheckInteractableInFront(DirectionFacing direction)
		{
			const float checkLength = 2f;

			float xOrigin = direction == DirectionFacing.Right
				? Collider.bounds.min.x
				: Collider.bounds.max.x;

			var origin = new Vector2(
				xOrigin,
				Collider.bounds.min.y);

			Vector2 castDirection = MovementState.GetSurfaceDirection(direction == DirectionFacing.Left ? DirectionTravelling.Left : DirectionTravelling.Right);

			return Physics2D.BoxCast(origin, Vector2.one, 0, castDirection, checkLength, 1 << LayerMask.NameToLayer(Layers.Interactable));
		}

		public void InteractWithStilt()
		{
			var stilt = _interactionObject.transform.parent.GetComponent<Stilt>();
			if (stilt.IsExtended)
			{
				Anim.PlayAnimation(Animations.LowerStilt);
			}
			else if (stilt.IsExtended == false && AbilityState.IsActive(Ability.Tools))
			{
				Anim.PlayAnimation(Animations.RaiseStilt);
			}
		}

		public void SwitchStilt()
		{
			var stilt = _interactionObject.transform.parent.GetComponent<Stilt>();
			stilt.IsExtended = !stilt.IsExtended;
		}

		public void SwitchChimney()
        {
            _interactionObject.GetComponentInChildren<ChimneyLid>().Switch();
        }

        public void SwitchStove()
        {
            _interactionObject.GetComponent<StoveDoor>().Switch();
        }

		public float GetClimbingAnimationSpeed()
		{
			if (ClimbingState == null || MovementState.Pivot == null)
				return 1;

            return GetAnimationSpeed();
		}

        private float GetAnimationSpeed()
        {
			float distance = Vector2.Distance(Collider.GetPoint(MovementState.CharacterPoint), MovementState.Pivot.transform.position);

            float speed = _playerState == PlayerState.Climbing
                ? ClimbingState.MovementSpeed
                : ConstantVariables.DefaultMovementSpeed;

            float animSpeed = 4 / (distance / speed);
            return animSpeed < 10
                ? animSpeed
                : 10;
        }

		public Vector2 GetGroundPivotPosition()
		{
			return MovementState.Pivot.transform.position;
		}
    }
}