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

		private float _normalizedHorizontalSpeed;
		private DenizenController _controller;
		private Animator _animator;
		private Vector3 _velocity;
		private bool _isJumping;
		private bool _hasFirstLanded;
        private bool _wasGrounded;
        private bool _hitSnow;
        private FirePlace _fireplace;
        private bool _isSliding;

        void Awake()
		{
			_animator = GetComponent<Animator>();
			_controller = GetComponent<DenizenController>();
		}

        void Update()
        {
            _isSliding = _controller.MovementState.IsOnSlope && _controller.MovementState.TrappedBetweenSlopes == false;
            _animator.SetBool(DenizenAnimBool.Sliding, _isSliding);
            _animator.SetBool(DenizenAnimBool.PlayerSpotted, SpotPlayer());
            _animator.SetBool(DenizenAnimBool.SatAtFireplace, _controller.SatAtFireplace);

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
                if (_controller.SatAtFireplace)
                {
                    DirectionTravelling = DirectionTravelling.None;
                }

                if (CanMove)
                {
                    DetermineMovement();
                    MoveWithVelocity();
                }
            }
        }

        void MoveToFireplace(DirectionTravelling direction)
		{
			DirectionTravelling = direction;
			_animator.SetBool(DenizenAnimBool.MoveToFireplace, true);
        }

        void ChangeDirection()
        {
            DirectionTravelling = DirectionTravelling == DirectionTravelling.Right ? DirectionTravelling.Left : DirectionTravelling.Right;
            FlipSprite();
            _normalizedHorizontalSpeed = -_normalizedHorizontalSpeed;
        }

        void LightStove()
        {
            _fireplace.LightFully();
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

				if (_hasFirstLanded == false)
				{
					_hasFirstLanded = true;
					_wasGrounded = true;
				}
				else if (_wasGrounded == false)
				{
					SetTravelInDirectionFacing();
					_wasGrounded = true;
				}
			}
			else if (_hasFirstLanded)
			{
				DirectionTravelling = DirectionTravelling.None;
				_wasGrounded = false;
			}
			else
				return;

			if (DirectionTravelling == DirectionTravelling.Right)
			{
                _normalizedHorizontalSpeed = 1;
				if (GetDirectionFacing() == DirectionFacing.Left)
					FlipSprite();

				var hazardRay = new Vector2(
					_controller.Collider.bounds.max.x + HazardWarningDistance,
					_controller.Collider.bounds.min.y);

                HazardHandler(hazardRay);
			}
			else if (DirectionTravelling == DirectionTravelling.Left)
			{
                _normalizedHorizontalSpeed = -1;
				if (GetDirectionFacing() == DirectionFacing.Right)
					FlipSprite();

				var hazardRay = new Vector2(
					_controller.Collider.bounds.min.x - HazardWarningDistance,
					_controller.Collider.bounds.min.y);

                HazardHandler(hazardRay);
            }
            else
			{
				_normalizedHorizontalSpeed = 0;
			}
            _animator.SetBool(DenizenAnimBool.Moving, _normalizedHorizontalSpeed != 0);
        }

        private void HazardHandler(Vector2 hazardRay)
        {
            if (_animator.GetBool(DenizenAnimBool.Sliding))
                return;

            if (ApproachingEdge(hazardRay))
                _animator.SetTrigger(DenizenAnimBool.AtEdge);
            else if (ApproachingSnow(hazardRay))
                _animator.SetTrigger(DenizenAnimBool.AtSnow);
            else if (_controller.MovementState.RightCollision || _controller.MovementState.LeftCollision)
                _animator.SetTrigger(DenizenAnimBool.AtWall);
        }

		public void SetTravelInDirectionFacing()
		{
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

			Vector2 castDirection = DirectionTravelling == DirectionTravelling.Left ? Vector2.left : Vector2.right;
			LayerMask mask = DirectionTravelling == DirectionTravelling.Left
                ? 1 << LayerMask.NameToLayer(Layers.RightClimbSpot)
                : 1 << LayerMask.NameToLayer(Layers.LeftClimbSpot);

			RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, castDirection, checkLength, mask);

			if (hit)
			{
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
				DirectionTravelling = DirectionTravelling.None;
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

			if (_velocity.x * _normalizedHorizontalSpeed > RunSpeed)
				_velocity.x = RunSpeed * _normalizedHorizontalSpeed;
			if (_velocity.y < ConstantVariables.MaxVerticalSpeed)
				_velocity.y = ConstantVariables.MaxVerticalSpeed;

			_controller.Movement.BoxCastMove(_velocity * Time.deltaTime, false);
            _animator.SetBool(DenizenAnimBool.Falling, _controller.MovementState.IsGrounded == false);
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