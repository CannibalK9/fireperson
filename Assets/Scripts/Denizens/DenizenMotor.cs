using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using UnityEngine;

namespace Assets.Scripts.Denizens
{
	public class DenizenMotor : MonoBehaviour
	{
		public float Gravity = -25f;
		public float RunSpeed = ConstantVariables.DenizenMovementSpeed;
		public float GroundDamping = 7f;
		public float HazardWarningDistance = 1f;
		public DirectionTravelling DirectionTravelling;
		public bool CanJump;
		public bool CanMove;
		public bool StartsMoving;

		private float _normalizedHorizontalSpeed;
		private DenizenController _controller;
		private Animator _animator;
		private Vector3 _velocity;
		private bool _isJumping;
        private bool _wasGrounded;
        private bool _hitSnow;
        private FirePlace _fireplace;
		private bool _wasSliding;
        private bool _isSliding;
		private bool _transitioning;

        void Awake()
		{
			_animator = GetComponent<Animator>();
			_controller = GetComponent<DenizenController>();
		}

        void Update()
        {
            _isSliding = _controller.MovementState.IsOnSlope && _controller.MovementState.TrappedBetweenSlopes == false;
			_animator.SetBool(DenizenAnimBool.Sliding, _isSliding);
			_wasSliding = _isSliding && _wasSliding;
			if (_isSliding && _wasSliding == false)
			{
				_animator.Play(Animator.StringToHash(Animations.BeginSlide));
				_wasSliding = true;
			}
            _animator.SetBool(DenizenAnimBool.PlayerSpotted, SpotPlayer());
            _animator.SetBool(DenizenAnimBool.SatAtFireplace, _controller.SatAtFireplace);
			//_animator.SetBool(DenizenAnimBool.InSnow, );

			CheckStopMoving();

			if (_isSliding)
            {
                MoveWithVelocity();
            }
            else if (_isJumping)
            {
                JumpAcross();
            }
            else
            {
                if (_controller.SatAtFireplace || CanMove == false)
                {
                    DirectionTravelling = DirectionTravelling.None;
                }

				_animator.SetBool(DenizenAnimBool.Moving, DirectionTravelling != DirectionTravelling.None);
                DetermineMovement();
                MoveWithVelocity();
            }
        }

		private void CheckStopMoving()
		{
			if (_animator.GetBool(DenizenAnimBool.InSnow)
				|| _animator.GetBool(DenizenAnimBool.PlayerSpotted)
				|| _animator.GetBool(DenizenAnimBool.SatAtFireplace)
				)
				_animator.SetBool(DenizenAnimBool.Moving, false);
		}

        void MoveToFireplace(DirectionTravelling direction)
		{
			DirectionTravelling = direction;
			_animator.SetTrigger(DenizenAnimBool.MoveToFireplace);
        }

        void LightStove()
        {
            _fireplace.LightFully();
        }

		void StartMoving()
		{
			if (CanMove)
			{
				_animator.SetBool(DenizenAnimBool.Moving, true);
				SetTravelInDirectionFacing();
				_transitioning = false;
			}
		}

        void OnTriggerEnter2D(Collider2D col)
        {
            if (_isSliding == false &&  col.gameObject.layer == LayerMask.NameToLayer(Layers.Flash) && _controller.CanSeeFlash(col.transform.position))
            {
                DirectionTravelling = DirectionTravelling.None;

                bool isFacingRight = GetDirectionFacing() == DirectionFacing.Right;
                bool isPlRight = col.transform.position.x >= transform.position.x;

                if (isFacingRight == isPlRight)
                {
                    FlipSprite();
                    _animator.Play(Animator.StringToHash(Animations.FlashInFront));
                }
                else
                    _animator.Play(Animator.StringToHash(Animations.FlashBehind));
            }
        }

        private void DetermineMovement()
		{
            if (_controller.MovementState.IsGrounded)
			{
				_velocity.y = 0;

				if (StartsMoving == false)
				{
					StartsMoving = true;
					_wasGrounded = true;
				}
				else if (_wasGrounded == false)
				{
					SetTravelInDirectionFacing();
					_wasGrounded = true;
				}
			}
			else if (StartsMoving)
			{
				DirectionTravelling = DirectionTravelling.None;
				_wasGrounded = false;
				return;
			}
			else
				return;

			if (DirectionTravelling == DirectionTravelling.Right)
			{
				_normalizedHorizontalSpeed = 1;
				if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();
				HazardRight();
			}
			else if (DirectionTravelling == DirectionTravelling.Left)
			{
				_normalizedHorizontalSpeed = -1;
				if (GetDirectionFacing() == DirectionFacing.Right)
					FlipSprite();
				HazardLeft();
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
				if (_transitioning == false)
				{
					HazardLeft();
					HazardRight();
				}
			}
        }

		private void HazardLeft()
		{
			var hazardRay = new Vector2(
				_controller.Collider.bounds.min.x - HazardWarningDistance,
				_controller.Collider.bounds.min.y);

			HazardHandler(hazardRay);
		}

		private void HazardRight()
		{
			var hazardRay = new Vector2(
					_controller.Collider.bounds.max.x + HazardWarningDistance,
					_controller.Collider.bounds.min.y);

			HazardHandler(hazardRay);
		}

		private void HazardHandler(Vector2 hazardRay)
		{
			if (ApproachingEdge(hazardRay))
				_animator.SetTrigger(DenizenAnimBool.AtEdge);
			else if (ApproachingSnow(hazardRay))
				_animator.SetTrigger(DenizenAnimBool.AtSnow);
			else if (_controller.MovementState.RightCollision || _controller.MovementState.LeftCollision)
				_animator.SetTrigger(DenizenAnimBool.AtWall);
			else
				return;

			if (CanMove)
			{
				DirectionTravelling = DirectionTravelling.None;
				_transitioning = true;
			}
		}

		public void SetTravelInDirectionFacing()
		{
			if (CanMove)
				DirectionTravelling = GetDirectionFacing() == DirectionFacing.Right
					? DirectionTravelling.Right
					: DirectionTravelling.Left;
		}

		private bool ApproachingEdge(Vector2 edgeRay)
		{
			if (Physics2D.Raycast(edgeRay, _controller.MovementState.GetSurfaceDownDirection(), 2f, Layers.Platforms))
				return false;

            if (CanJump == false)
                return true;

			const float checkLength = 6f;
			const float checkDepth = 4f;

			var origin = new Vector2(
			   _controller.Collider.bounds.center.x,
			   _controller.Collider.bounds.min.y);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = GetDirectionFacing() == DirectionFacing.Left ? Vector2.left : Vector2.right;
			LayerMask mask = GetDirectionFacing() == DirectionFacing.Left
                ? 1 << LayerMask.NameToLayer(Layers.RightClimbSpot)
                : 1 << LayerMask.NameToLayer(Layers.LeftClimbSpot);

			RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, castDirection, checkLength, mask);

			if (hit)
			{
			DirectionTravelling = DirectionTravelling.None;
				_isJumping = true;
				_controller.MovementState.SetPivot(hit.collider, ColliderPoint.TopFace, ColliderPoint.BottomFace);
				_animator.SetTrigger(DenizenAnimBool.Jump);
				return false;
			}
			return true;
		}

		private bool ApproachingSnow(Vector2 snowRay)
		{
			LayerMask mask = 1 << LayerMask.NameToLayer(Layers.Ice);
			RaycastHit2D hit = Physics2D.Raycast(snowRay, Vector2.down, 0.1f, mask);
			if (hit && _hitSnow == false)
			{
				_hitSnow = true;
			}
			else
			{
				_hitSnow = false;
			}
			return hit;
		}

		private void FlipSprite()
		{
			transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}

		private void MoveWithVelocity()
		{
			if (_controller.MovementState.IsGrounded)
				_velocity.x = Mathf.SmoothDamp(
					_velocity.x,
					_normalizedHorizontalSpeed * RunSpeed,
					ref _velocity.x,
					Time.deltaTime * GroundDamping);

			_velocity.y += Gravity * Time.deltaTime;

			if (Mathf.Abs(_velocity.x * _normalizedHorizontalSpeed) > RunSpeed)
				_velocity.x = RunSpeed * _normalizedHorizontalSpeed;
			if (_velocity.y < ConstantVariables.MaxVerticalSpeed)
				_velocity.y = ConstantVariables.MaxVerticalSpeed;

			_controller.Movement.BoxCastMove(_velocity * Time.deltaTime, false);
			if (_controller.MovementState.IsGrounded == false)
	            _animator.Play(Animator.StringToHash(Animations.Falling));
        }

        private void JumpAcross()
		{
			if (_controller.Movement.MoveLinearly(ConstantVariables.AcrossSpeed))
				_isJumping = _animator.GetCurrentAnimatorStateInfo(0).IsName(Animations.Jump);
			else
			{
				_animator.SetTrigger(DenizenAnimBool.CancelJump);
				_isJumping = false;
			}
		}

		private bool SpotPlayer()
		{
			Vector2 direction = GetDirectionFacing() == DirectionFacing.Right
				? Vector2.right
				: Vector2.left;

            return _controller.SpotPlayer(direction);
        }

        public void BeginLighting(FirePlace fireplace)
        {
            _fireplace = fireplace;
			DirectionTravelling = DirectionTravelling.None;
            _animator.SetTrigger(DenizenAnimBool.LightStove);
        }

        private enum DirectionFacing
		{
			Right,
			Left
		}

		private DirectionFacing GetDirectionFacing()
		{
			return transform.localScale.x > 0f
				? DirectionFacing.Right
				: DirectionFacing.Left;
		}
	}
}