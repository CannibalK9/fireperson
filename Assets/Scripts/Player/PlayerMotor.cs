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
		public float AirDamping = 7f;

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
		public Interaction Interaction { get; set; }

		private MovementHandler _movement;
		private ClimbHandler _climbHandler;
		private PlayerIK _iks;
		private Vector3 _velocity;
		private PlayerState _playerState;
		private float _normalizedHorizontalSpeed;
		private bool _wasSliding;
		private bool _wasGrounded;
		public BoxCollider2D CrouchedCollider;
		public BoxCollider2D StandingCollider;
		private bool _isCrouched;
		private bool _isJumping;
		private bool _isHopping;
		private float _jumpTimer;

		void Awake()
		{
			Transform = transform.parent.parent;
			Anim = Transform.GetComponent<AnimationScript>();
			Rigidbody = Transform.GetComponent<Rigidbody2D>();
			MovementAllowed = true;

			StandingCollider = Transform.GetComponent<BoxCollider2D>();
			CrouchedCollider = Transform.GetComponents<BoxCollider2D>()[1];

			MovementState = new MovementState(StandingCollider.bounds.extents);
			_iks = new PlayerIK(MovementState);

			_movement = new MovementHandler(this);
			_climbHandler = new ClimbHandler(this);
			Interaction = new Interaction();
		}

		void Update()
		{
			UpdateJumpTimer();
			_playerState = HandleMovementInputs();
		}

		private void UpdateJumpTimer()
		{
			if (_isJumping)
			{
				_jumpTimer -= Time.deltaTime;
				if (_jumpTimer < 0)
					_isJumping = false;
			}
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
					_movement.BoxCastMove();
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

		public bool IsMoving()
		{
			return _normalizedHorizontalSpeed != 0;
		}

		private PlayerState HandleMovementInputs()
		{
			if (MovementState.IsOnSlope)
			{
				if (TryClimb())
				{
					MovementState.WasOnSlope = true;
					MovementState.IsGrounded = true;
					_wasGrounded = true;
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

			if (_isJumping)
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
			_isHopping = false;
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
			float damping = _isJumping ? AirDamping : GroundDamping;
			_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.fixedDeltaTime * damping);
		}

		private void MoveWithVelocity(float gravity)
		{
			transform.localPosition = Vector3.zero;

			float maxX = _isJumping ? ConstantVariables.HorizontalJumpSpeed : ConstantVariables.MaxHorizontalSpeed + CurrentClimate.Control/20;

			SetCrouched(ShouldCrouch());
			if (_isJumping == false && _isCrouched && MovementState.IsGrounded)
				maxX *= ConstantVariables.CrouchedFactor;

			if (_isJumping == false && MovementState.IsGrounded && Physics2D.Raycast(Collider.GetTopLeft() + Vector3.up * 0.01f, Vector2.right, Collider.bounds.size.x, Layers.Platforms))
			{
				maxX *= ConstantVariables.SquashedFactor;
				Anim.SetBool(PlayerAnimBool.Squashed, true);
			}
			else
				Anim.SetBool(PlayerAnimBool.Squashed, false);

			Anim.SetBool(PlayerAnimBool.Upright, _isJumping == false && MovementState.IsGrounded && MovementState.IsUpright());

			if (Mathf.Abs(_velocity.x * _normalizedHorizontalSpeed) > maxX)
				_velocity.x = maxX * _normalizedHorizontalSpeed;
			if (_isJumping == false && _velocity.y > ConstantVariables.MinVerticalSpeed)
				_velocity.y = ConstantVariables.MinVerticalSpeed;

			_velocity.y += gravity * Time.fixedDeltaTime;
			_movement.SetMovementCollisions(_velocity * Time.fixedDeltaTime, _isJumping);
			_iks.SetIks(Collider.bounds.min.y);
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
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(Collider.GetLeftFace().x - 0.5f, origin.y - 0.1f), Vector2.right, StandingCollider.size.x + 1, Layers.Platforms);
			bool shouldCrouch = hits.Any(h => h.collider != hit.collider);
			return shouldCrouch;
		}

		private void MoveToClimbingPoint()
		{
			ClimbingState cState = _climbHandler.CurrentClimbingState;

			MovementState.UpdatePivot = cState.Climb != Climb.Prep && MovementState.CharacterPoint != ColliderPoint.Centre;

			bool applyRotation = Anim.GetBool(PlayerAnimBool.Corner) && MovementState.CharacterPoint == ColliderPoint.Centre;

			if (cState.Climb == Climb.Down && _movement.IsCollidingWithNonPivot(true))
			{
				CancelClimbingState();
                CancelAnimation();
			}
			else if (_movement.MoveLinearly(cState.MovementSpeed, applyRotation, _isHopping) == false)
			{
				if (_climbHandler.CheckReattach())
					_movement.MoveLinearly(cState.MovementSpeed, applyRotation);
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
			_climbHandler.CurrentClimbingState.PlayerPosition = newPoint;
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
			_climbHandler.CurrentClimbingState.PlayerPosition = newPoint;
		}

		public float MoveToNextPivotPoint()
		{
			if (_climbHandler.NextClimbingState.PivotCollider == null)
				Debug.Log("");

			_climbHandler.CurrentClimbingState = _climbHandler.NextClimbingState;

			MovementState.SetNewPivot(_climbHandler.CurrentClimbingState);
			Anim.SetBool(PlayerAnimBool.Corner, _climbHandler.CurrentClimbingState.IsUpright);
			Anim.SetBool(PlayerAnimBool.Inverted, _climbHandler.CurrentClimbingState.ClimbSide == GetDirectionFacing());

			return _climbHandler.CurrentClimbingState.AnimationSpeed;
		}

		private bool IsClimbing()
		{
			return _climbHandler.CurrentClimbingState.Climb != Climb.None;
		}

		private PlayerState SetMotorToClimbState()
		{
			Interaction.Object = null;
			CancelVelocity();

			ClimbingState cState = _climbHandler.CurrentClimbingState;

			if (cState.Climb != Climb.Prep)
				MovementState.SetPivotCollider(cState.PivotCollider, cState.PivotPosition, cState.PlayerPosition);
			else
				MovementState.JumpInPlace();

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

		private void TryGrab()
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
					Anim.SetBool(PlayerAnimBool.Inverted, (_climbHandler.CurrentClimbingState.PivotCollider.gameObject.layer == LayerMask.NameToLayer(Layers.RightClimbSpot)) == (directionFacing == DirectionFacing.Right));
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
			else if (MovementState.IsOnSlope == false && KeyBindings.GetKey(Controls.Down) && _climbHandler.CheckLedgeBelow(Climb.Down, GetDirectionFacing(), out animation))
			{
				Anim.PlayAnimation(animation);
				SetCrouched(false);
			}
			else if (_isCrouched == false && topOfSlope == false && KeyBindings.GetKey(Controls.Jump) && _climbHandler.CheckLedgeBelow(Climb.Jump, GetDirectionFacing(), out animation))
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

			_velocity.x = _normalizedHorizontalSpeed * ConstantVariables.HorizontalJumpSpeed;
		}

		public void Hop()
		{
			//MovementState.MovePivotDown();
			//_velocity.y = -20;
			_isHopping = true;
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

		public Climb SwitchClimbingState(bool ignoreUp = false)
		{
			var direction = DirectionFacing.None;

			if (_climbHandler.CurrentClimbingState.Climb != Climb.Jump && _climbHandler.CurrentClimbingState.Climb != Climb.None)
			{
				direction = AddNextClimbs(ignoreUp);
			}

			Climb nextClimb = _climbHandler.SwitchClimbingState(direction);

			return nextClimb;
		}

		public bool TryHangingInput(out Climb nextClimb)
		{
			DirectionFacing direction = AddNextClimbs();

			nextClimb = Climb.Down;

			if (_climbHandler.NextClimbs.Any())
			{
				Climb switchedClimb = _climbHandler.SwitchClimbingState(direction, _climbHandler.CurrentClimbingState.PivotCollider != null && _climbHandler.CurrentClimbingState.IsUpright == false);

				if (switchedClimb == Climb.None)
					return false;

				if (_climbHandler.CurrentClimbingState.PivotCollider == null || switchedClimb == Climb.Down)
				{
					MovementState.UnsetPivot();
					nextClimb = Climb.None;
				}
				else
					nextClimb = switchedClimb;

				return true;
			}
			else
			{
				return false;
			}
		}

		private DirectionFacing AddNextClimbs(bool ignoreUp = false)
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
			if (ignoreUp == false && KeyBindings.GetKey(Controls.Up))
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

		public Vector2 GetGroundPivotPosition()
		{
			return MovementState.Pivot.transform.position;
		}

		public bool IsJumping()
		{
			_isJumping = _climbHandler.NextClimbingState.PivotCollider == null;
			_jumpTimer = 0.8f;
			Anim.SetBool(PlayerAnimBool.Falling, true);

			return _isJumping;
		}
    }
}