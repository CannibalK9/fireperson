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
		public float AirDamping = 10f;

		public AnimationScript Anim { get; private set; }
		public bool MovementAllowed { get; set; }
		public DirectionFacing ClimbingSide { get { return _climbHandler.ClimbSide; } }
		public Transform Transform { get; set; }
		public MovementState MovementState { get; set; }
		public Collider2D Collider
		{
			get
			{
				return _isCrouched ? CrouchedCollider : StandingCollider;
			}
			set { }
		}
		public Rigidbody2D Rigidbody { get; set; }
		public ClimbingState ClimbingState { get; set; }
		public Interaction Interaction { get; set; }

		private MovementHandler _movement;
		private ClimbHandler _climbHandler;
		private Vector3 _velocity;
		private PlayerState _playerState;
		private float _normalizedHorizontalSpeed;
		private bool _wasSliding;
		private bool _wasGrounded;
		public BoxCollider2D CrouchedCollider;
		public BoxCollider2D StandingCollider;
		private bool _isCrouched;

		void Awake()
		{
			Transform = transform.parent.parent;
			Anim = Transform.GetComponent<AnimationScript>();
			Rigidbody = Transform.GetComponent<Rigidbody2D>();
			MovementAllowed = true;

			StandingCollider = Transform.GetComponent<BoxCollider2D>();
			CrouchedCollider = Transform.GetComponents<BoxCollider2D>()[1];

			MovementState = new MovementState(StandingCollider.bounds.extents);

			_movement = new MovementHandler(this);
			_climbHandler = new ClimbHandler(this);
			Interaction = new Interaction();
		}

		void Update()
		{
			_playerState = HandleMovementInputs();
		}

		void FixedUpdate()
		{ 
			switch (_playerState)
			{
				case PlayerState.WaitingForInput:
					MoveWithVelocity(0);
					AcceptMovementInput();
					SetHorizontalVelocity();
					_movement.BoxCastMove();
					break;
				case PlayerState.Sliding:
					SetSliding();
					SetHorizontalVelocity();
					MoveWithVelocity(0);
					_movement.BoxCastMove();
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
					_movement.BoxCastMove();
					TryGrab();
					break;
				case PlayerState.Static:
					CancelVelocity();
					MoveWithVelocity(0);
					_movement.BoxCastMove();
					break;
				case PlayerState.Jumping:
                    MoveWithVelocity(0);
					_movement.BoxCastMove();
					TryGrab();
					break;
				default:
					MoveWithVelocity(0);
					_movement.BoxCastMove();
					break;
            }
		}

		private PlayerState HandleMovementInputs()
		{
			if (MovementState.IsOnSlope)
			{
				if (TryClimb())
				{
					MovementState.WasOnSlope = true;
					MovementState.IsGrounded = true;
					MovementState.IsOnSlope = false;
					_wasSliding = false;
					Anim.SetBool(PlayerAnimBool.Sliding, false);
					return SetMotorToClimbState();
				}
				else
					return PlayerState.Sliding;
			}
			else
			{
				if (_wasSliding)
					_normalizedHorizontalSpeed = 0;
				_wasSliding = false;
				Anim.SetBool(PlayerAnimBool.Sliding, false);
			}

			if (_climbHandler.CurrentClimb == Climb.Jump)
			{
				return PlayerState.Jumping;
			}
			else if (MovementState.IsGrounded)
			{
				if (_wasGrounded == false)
				{
					_wasGrounded = true;
					return PlayerState.WaitingForInput;
				}
				else if (Interaction.IsInteracting)
					return PlayerState.Interacting;
				else if (ChannelingHandler.IsChanneling && ChannelingHandler.ChannelingSet == false)
				{
					if (KeyBindings.GetKey(Controls.Light))
						ChannelingHandler.Channel();
					return PlayerState.Static;
				}
				else if (IsClimbing())
					return PlayerState.Climbing;
				else if (TryClimb())
					return SetMotorToClimbState();
				else if (TryInteract())
				{
					Interaction.IsInteracting = true;
					return PlayerState.Interacting;
				}
				else if (TryChannel())
					return PlayerState.Static;
				else
				{
					return PlayerState.WaitingForInput;
				}
			}
			else
			{
				_wasGrounded = false;

				if (Anim.GetBool(PlayerAnimBool.Falling) == false)
					Anim.PlayAnimation(Animations.Falling);
				Anim.SetBool(PlayerAnimBool.Falling, true);
				Anim.SetBool(PlayerAnimBool.Moving, false);
				Anim.SetBool(PlayerAnimBool.Upright, false);
				_normalizedHorizontalSpeed = 0;
				return PlayerState.Falling;
			}
		}

		private void AcceptMovementInput()
		{
			MovementInput();
			Anim.SetBool(PlayerAnimBool.Falling, false);
			Anim.SetBool(PlayerAnimBool.Moving, _normalizedHorizontalSpeed != 0);
		}

		private void SetSliding()
		{
			Anim.SetBool(PlayerAnimBool.Sliding, true);
			if (_wasSliding == false)
			{
				bool slidingRight = MovementState.NormalDirection == DirectionFacing.Right;
				bool facingRight = GetDirectionFacing() == DirectionFacing.Right;
				Anim.SetBool(PlayerAnimBool.Forward, slidingRight == facingRight);
				_normalizedHorizontalSpeed = slidingRight ? 1 : -1;
				if (slidingRight == facingRight)
					Anim.PlayAnimation(Animations.SlideForward);
				else
					Anim.PlayAnimation(Animations.SlideBackward);

				_wasSliding = true;
			}
			else
			{
				bool slidingRight = MovementState.NormalDirection == DirectionFacing.Right;
				if (KeyBindings.GetKey(Controls.Left))
				{
					Anim.FlipSpriteLeft();
					Anim.SetBool(PlayerAnimBool.Forward, !slidingRight);
				}
				else if (KeyBindings.GetKey(Controls.Right))
				{
					Anim.FlipSpriteRight();
					Anim.SetBool(PlayerAnimBool.Forward, slidingRight);
				}
			}
		}

		private void SetHorizontalVelocity()
		{
			float damping = MovementState.IsGrounded ? GroundDamping : AirDamping;
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.fixedDeltaTime * damping);
		}

		private void MoveWithVelocity(float gravity)
		{
			transform.localPosition = Vector3.zero;

			float maxX = ConstantVariables.MaxHorizontalSpeed + CurrentClimate.Control/20;

			SetCrouched(ShouldCrouch());
			if (_isCrouched)
				maxX *= ConstantVariables.CrouchedFactor;

			if (Physics2D.Raycast(Collider.GetTopLeft() + Vector3.up * 0.01f, Vector2.right, Collider.bounds.size.x, Layers.Platforms))
			{
				maxX *= ConstantVariables.SquashedFactor;
				Anim.SetBool(PlayerAnimBool.Squashed, true);
			}
			else
				Anim.SetBool(PlayerAnimBool.Squashed, false);

			Anim.SetBool(PlayerAnimBool.Upright, MovementState.IsUpright());

			if (Mathf.Abs(_velocity.x * _normalizedHorizontalSpeed) > maxX)
				_velocity.x = maxX * _normalizedHorizontalSpeed;
			if (_velocity.y < ConstantVariables.MaxVerticalSpeed)
				_velocity.y = ConstantVariables.MaxVerticalSpeed;

			_velocity.y += gravity * Time.fixedDeltaTime;
			_movement.SetMovementCollisions(_velocity * Time.fixedDeltaTime, _playerState == PlayerState.Jumping);
		}

		private void SetCrouched(bool shouldCrouch)
		{
			Anim.SetBool(PlayerAnimBool.Crouched, shouldCrouch);
			_isCrouched = shouldCrouch;
			StandingCollider.enabled = !shouldCrouch;
			CrouchedCollider.enabled = shouldCrouch;
		}

		private bool ShouldCrouch()
		{
			Vector2 origin = Collider.GetBottomCenter() + Vector3.up * (ConstantVariables.MaxLipHeight + 0.1f);
			Vector2 size = new Vector2(StandingCollider.size.x + 0.2f, 0.01f);
			float distance = StandingCollider.size.y - ConstantVariables.MaxLipHeight - 0.1f;
			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, distance, Layers.Platforms);
			Debug.DrawLine(origin, origin + (Vector2.up * distance), Color.magenta);
			bool shouldCrouch = hits.Any();// && hits.All(hit => hit.point.y > Collider.bounds.min.y + CrouchedCollider.size.y);
			return shouldCrouch;
		}

		private void MoveToClimbingPoint()
		{
			float speed = MovementAllowed
				? ClimbingState.MovementSpeed
				: 0;

			bool applyRotation = Anim.GetBool(PlayerAnimBool.Corner) && MovementState.CharacterPoint == ColliderPoint.Centre;

			if (_climbHandler.CurrentClimb == Climb.Down && _movement.IsCollidingWithNonPivot(true))
			{
				CancelClimbingState();
                CancelAnimation();
			}
			else if (_movement.MoveLinearly(speed, applyRotation) == false)
			{
				if (_climbHandler.CheckReattach())
					_movement.MoveLinearly(speed, applyRotation);
				else
				{
					CancelClimbingState();
                    CancelAnimation();
				}
			}
		}

        private void MoveToInteractionPoint()
        {
			if (_movement.IsCollidingWithNonPivot(false))
			{
				CancelAnimation();
				return;
			}

			if (Mathf.Abs(Interaction.Centre - Collider.bounds.center.x) > 0.2f)
				_normalizedHorizontalSpeed = Interaction.Centre > Collider.bounds.center.x ? 1 : -1;
			else
				_normalizedHorizontalSpeed = 0;

			SetHorizontalVelocity();
			MoveWithVelocity(0);
		}

		public void UpdateClimbingSpeed(float speed)
		{
			ClimbingState.MovementSpeed = speed;
		}

		public void MoveHorizontally()
		{
			ColliderPoint newPoint;

			if (MovementState.CharacterPoint == ColliderPoint.Centre)
				newPoint = MovementState.CharacterPoint;

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
                    newPoint = ColliderPoint.BottomFace;
                    break;
            }

			MovementState.CharacterPoint = newPoint;
		}

		public void MoveVertically()
		{
			ColliderPoint newPoint = MovementState.CharacterPoint;
			bool cornerRecovery = newPoint == ColliderPoint.Centre;

			if (cornerRecovery)
				newPoint = MovementState.PreviousCharacterPoint;

			switch (newPoint)
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
                    newPoint = ColliderPoint.BottomFace;
                    break; ;
			}
			if (cornerRecovery || (MovementState.PivotCollider.IsUpright() && (newPoint == ColliderPoint.TopLeft || newPoint == ColliderPoint.TopRight)))
				MovementState.SetPivotCollider(MovementState.PivotCollider, MovementState.TargetPoint, newPoint);
			else
				MovementState.CharacterPoint = newPoint;
		}

		private bool IsClimbing()
		{
			return _climbHandler.CurrentClimb != Climb.None;
		}

		private PlayerState SetMotorToClimbState()
		{
			Interaction.Object = null;
			CancelVelocity();
			if (_climbHandler.CurrentClimb != Climb.Jump)
			{
				ClimbingState = _climbHandler.GetClimbingState(true);
				MovementState.SetPivotCollider(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);
			}
			return PlayerState.Climbing;
		}

		private void MovementInput()
		{
			if (KeyBindings.GetKey(Controls.Anchor))
			{
				_normalizedHorizontalSpeed = 0;
			}
			else if (KeyBindings.GetKey(Controls.Right))
			{
				if (MovementState.RightCollision == false && MovementState.RightEdge == false)
					_normalizedHorizontalSpeed = 1;
				else
					_normalizedHorizontalSpeed = 0;
				if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();
			}
			else if (KeyBindings.GetKey(Controls.Left))
			{
				if (MovementState.LeftCollision == false && MovementState.LeftEdge == false)
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
				//AtWallAnimation
			}
		}

		private void TryGrab() //forward is not being set correctly, all else seems to work!
		{
			bool grabbing = false;
			DirectionFacing directionFacing = GetDirectionFacing();
			if (KeyBindings.GetKey(Controls.Left))
			{
				Anim.SetBool(PlayerAnimBool.IsGrabbing, true);
				Anim.SetBool(PlayerAnimBool.Inverted, directionFacing != DirectionFacing.Left);
				if (_climbHandler.CheckGrab(DirectionFacing.Left))
				{
					MovementState.IsGrounded = true;
					MovementAllowed = true;
					grabbing = true;
				}
			}
			else if (KeyBindings.GetKey(Controls.Right))
			{
				Anim.SetBool(PlayerAnimBool.IsGrabbing, true);
				Anim.SetBool(PlayerAnimBool.Inverted, directionFacing != DirectionFacing.Right);
				if (_climbHandler.CheckGrab(DirectionFacing.Right))
				{
					MovementState.IsGrounded = true;
					MovementAllowed = true;
					grabbing = true;
				}
			}
			else
			{
				if (_climbHandler.CheckGrab(holdingUp: KeyBindings.GetKey(Controls.Up)))
				{
					MovementState.IsGrounded = true;
					MovementAllowed = true;
					Anim.SetBool(PlayerAnimBool.IsGrabbing, true);
					Anim.SetBool(PlayerAnimBool.Inverted, (ClimbingState.PivotCollider.gameObject.layer == LayerMask.NameToLayer(Layers.RightClimbSpot)) == (directionFacing == DirectionFacing.Right));
					grabbing = true;
				}
			}
			Anim.SetBool(PlayerAnimBool.IsGrabbing, grabbing);
			_wasGrounded = grabbing;
			if (grabbing)
				SetMotorToClimbState();
		}

		private bool TryClimb()
		{
			bool topOfSlope = MovementState.IsOnSlope
				? topOfSlope = MovementState.NormalDirection != GetDirectionFacing()
				: false;

			Climb climb;
			string animation = "";
			if (_isCrouched == false && KeyBindings.GetKey(Controls.Up) && _climbHandler.CheckLedgeAbove(GetDirectionFacing(), out climb, true))
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
			else if (topOfSlope == false && KeyBindings.GetKey(Controls.Down) && _climbHandler.CheckLedgeBelow(Climb.Down, GetDirectionFacing(), out animation))
			{
				Anim.PlayAnimation(animation);
				SetCrouched(false);
			}
			else if (_isCrouched == false && topOfSlope == false && KeyBindings.GetKey(Controls.Jump) && _climbHandler.CheckLedgeBelow(Climb.MoveToEdge, GetDirectionFacing(), out animation))
				Anim.PlayAnimation(animation);
			else
				return false;

			Transform.GetComponent<AudioSource>().Play();
			return true;
		}

		private bool TryInteract()
		{
			if (KeyBindings.GetKey(Controls.Action))
			{
				RaycastHit2D hit = CheckInteractableInFront(GetDirectionFacing());
				if (hit)
				{
					Interaction.Point = hit.collider;
					Interaction.Object = hit.collider.transform.parent;

					var stilt = Interaction.Object.GetComponentInParent<Stilt>();
					if (stilt != null)
					{
						return InteractWithStilt(stilt);
					}
					var chimney = Interaction.Object.GetComponentInChildren<ChimneyLid>();
					if (chimney != null)
					{
						InteractWithChimney(chimney);
						return true;
					}
					var stove = Interaction.Object.GetComponentInChildren<Stove>();
					if (stove != null)
					{
						InteractWithStove(stove);
						return true;
					}
				}
			}
			return false;
		}

		private bool TryChannel()
		{
			if (ChannelingHandler.ChannelingSet == false
				&& ChannelingHandler.IsChanneling == false
				&& KeyBindings.GetKeyDown(Controls.Light)
				&& Pointer.IsPointerOverUIObject() == false)
			{
				ChannelingHandler.StartChanneling();
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

		public void Hop()
		{
			MovementState.MovePivotDown();
			_climbHandler.CurrentClimb = Climb.Jump;
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
			Anim.SetBool(PlayerAnimBool.Moving, false);
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
				MovementState.SetPivotCollider(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);

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
					MovementState.SetPivotCollider(ClimbingState.PivotCollider, ClimbingState.PivotPosition, ClimbingState.PlayerPosition);
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

		private DirectionFacing AddNextClimbs()
		{
			var direction = DirectionFacing.None;

			if (KeyBindings.GetKey(Controls.Left))
			{
				_climbHandler.NextClimbs.Add(Climb.AcrossLeft);
				direction = DirectionFacing.Left;
			}
			else if (KeyBindings.GetKey(Controls.Right))
			{
				_climbHandler.NextClimbs.Add(Climb.AcrossRight);
				direction = DirectionFacing.Right;
			}
			if (KeyBindings.GetKey(Controls.Up))
				_climbHandler.NextClimbs.Add(Climb.Up);
			if (KeyBindings.GetKey(Controls.Down))
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

			Vector2 castDirection = MovementState.GetSurfaceDirection(direction == DirectionFacing.Left ? DirectionTravelling.Left : DirectionTravelling.Right);

			return Physics2D.BoxCast(Collider.bounds.center, new Vector2(0.1f, Collider.bounds.size.y), 0, castDirection, checkLength, 1 << LayerMask.NameToLayer(Layers.Interactable));
		}

		public bool InteractWithStilt(Stilt stilt)
		{
			if (stilt.IsExtended)
			{
				Anim.PlayAnimation(Animations.LowerStilt);
				return true;
			}
			else if (stilt.IsExtended == false && AbilityState.IsActive(Ability.Tools))
			{
				Anim.PlayAnimation(Animations.RaiseStilt);
				return true;
			}
			return false;
		}

		public void InteractWithChimney(ChimneyLid chimney)
		{
			if (chimney.IsAccessible)
			{
				if (Interaction.IsLeft)
					Anim.PlayAnimation(Animations.CloseChimneyFromLeft);
				else
					Anim.PlayAnimation(Animations.CloseChimneyFromRight);
			}
			else
			{
				if (Interaction.IsLeft)
					Anim.PlayAnimation(Animations.OpenChimneyFromLeft);
				else
					Anim.PlayAnimation(Animations.OpenChimneyFromRight);
			}
		}

		public void InteractWithStove(Stove stove)
		{
			if (stove.IsAccessible)
			{
				if (Interaction.IsLeft)
					Anim.PlayAnimation(Animations.CloseStoveFromLeft);
				else
					Anim.PlayAnimation(Animations.CloseStoveFromRight);
			}
			else
			{
				Anim.PlayAnimation(Animations.OpenStoveFromLeft);
			}
		}

		public void SwitchStilt()
		{
			var stilt = Interaction.Object.GetComponentInParent<Stilt>();
			stilt.IsExtended = !stilt.IsExtended;
		}

		public void SwitchChimney()
        {
			Interaction.Object.GetComponentInChildren<ChimneyLid>().Switch();
        }

        public void SwitchStove()
        {
			Interaction.Object.GetComponent<StoveDoor>().Switch();
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

			float animSpeed = 0.6f;// distance / speed;
            return animSpeed < 5
                ? animSpeed
                : 5;
        }

		public Vector2 GetGroundPivotPosition()
		{
			return MovementState.Pivot.transform.position;
		}
    }
}