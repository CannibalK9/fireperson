using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using UnityEngine;

namespace Assets.Scripts.Denizens
{
	public class DenizenMotor : MonoBehaviour
	{
		public float Gravity = -25f;
		public float RunSpeed = 2f;
		public float GroundDamping = 7f;
		public float HazardWarningDistance = 1f;
		public DirectionTravelling DirectionTravelling;

		private float _normalizedHorizontalSpeed = 0;

		private DenizenController _controller;
		private Animator _animator;
		private Vector3 _velocity;
		private bool _movementPaused;
		private bool _waitingToMove;

		private void BeginMoving()
		{
			_movementPaused = false;
		}

		void Awake()
		{
			_animator = GetComponent<Animator>();
			_controller = GetComponent<DenizenController>();
			DirectionTravelling = DirectionTravelling.None;
		}

		void FixedUpdate()
		{
			if (_controller.SatAtFireplace || _movementPaused)
			{
				_waitingToMove = true;
				_velocity = Vector3.zero;
				DirectionTravelling = DirectionTravelling.None;
			}
			else if (_waitingToMove)
			{
				SetTravelInDirectionFacing();
				_waitingToMove = false;
			}

			DetermineMovement();
			HandleMovement();
            SpotPlayer();
		}

		void MoveToFireplace(DirectionTravelling direction)
		{
			DirectionTravelling = direction;
		}

		private void DetermineMovement()
		{
			if (_controller.IsGrounded)
			{
				_velocity.y = 0;
				if (_controller.CollisionState.BecameGroundedThisFrame)
					SetTravelInDirectionFacing();
			}
			else
			{
				DirectionTravelling = DirectionTravelling.None;
				//SetAnimationWhenFalling();
			}

			if (DirectionTravelling == DirectionTravelling.Right)
			{
                if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();

                var hazardRay = new Vector2(
					_controller.Collider.bounds.max.x + HazardWarningDistance,
					_controller.Collider.bounds.min.y);

                if (ApproachingEdge(hazardRay) || ApproachingSnow(hazardRay) || _controller.CollisionState.Right)
                {
                    DirectionTravelling = DirectionTravelling.Left;
                    FlipSprite();
                    _normalizedHorizontalSpeed = -_normalizedHorizontalSpeed;
                }
                else
                    _normalizedHorizontalSpeed = 1;
			}
			else if (DirectionTravelling == DirectionTravelling.Left)
			{
                if (GetDirectionFacing() == DirectionFacing.Right)
                    FlipSprite();

                var hazardRay = new Vector2(
					_controller.Collider.bounds.min.x - HazardWarningDistance,
					_controller.Collider.bounds.min.y);

                if (ApproachingEdge(hazardRay) || ApproachingSnow(hazardRay) || _controller.CollisionState.Left)
                {
                    DirectionTravelling = DirectionTravelling.Right;
                    FlipSprite();
                    _normalizedHorizontalSpeed = -_normalizedHorizontalSpeed;
                }
                else
                    _normalizedHorizontalSpeed = -1;
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
			}

            SetAnimationWhenGrounded();
        }

		private void SetTravelInDirectionFacing()
		{
			DirectionTravelling = GetDirectionFacing() == DirectionFacing.Right
						? DirectionTravelling.Right
						: DirectionTravelling.Left;
		}

		private static bool ApproachingEdge(Vector2 edgeRay)
		{
			return Physics2D.Raycast(edgeRay, Vector2.down, 2f, Layers.Platforms) == false;
		}

		private bool _hitSnow;

		private bool ApproachingSnow(Vector2 snowRay)
		{
			LayerMask mask = 1 << LayerMask.NameToLayer(Layers.Ice);
			RaycastHit2D hit = Physics2D.Raycast(snowRay, Vector2.down, 0.1f, mask);
			if (hit && _hitSnow == false)
			{
				_hitSnow = true;
				_movementPaused = true;
				_animator.Play(Animator.StringToHash(Animations.Shiver));
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

		private void SetAnimationWhenGrounded()
		{
			_animator.Play(
				_normalizedHorizontalSpeed == 0
				? Animator.StringToHash(Animations.Idle)
				: Animator.StringToHash(Animations.Moving));
		}

		private void HandleMovement()
		{
			if (_controller.IsGrounded)
				_velocity.x = Mathf.SmoothDamp(
					_velocity.x,
					_normalizedHorizontalSpeed * RunSpeed,
					ref _velocity.x,
					Time.deltaTime * GroundDamping);

            if (_velocity.x > 0 && _velocity.x > RunSpeed)
                _velocity.x = RunSpeed;
            else if (_velocity.x < 0 && _velocity.x < -RunSpeed)
                _velocity.x = -RunSpeed;

			_velocity.y += Gravity * Time.deltaTime;
			_controller.Movement.BoxCastMove(_velocity * Time.deltaTime);
			_velocity = new Vector3();
		}

		private bool _playerSpotted;

		private void SpotPlayer()
		{
			Vector2 direction = GetDirectionFacing() == DirectionFacing.Right
				? Vector2.right
				: Vector2.left;

			if (_controller.SpotPlayer(direction) && _playerSpotted == false)
			{
				_playerSpotted = true;
				_movementPaused = true;
				_animator.Play(Animator.StringToHash(Animations.Gasp));
			}
			else if (_playerSpotted)
			{
				_playerSpotted = false;
				_animator.Play(Animator.StringToHash(Animations.Relief));
			}
		}

        public void BeginLightingStove(Stove stove)
        {
            _stove = stove;
            _movementPaused = true;
            _animator.Play(Animator.StringToHash(Animations.LightStove));
        }

        private Stove _stove;

        private void LightStove()
        {
            if (_stove.CanBeLitByDenizens())
                _stove.IsLit = true;
        }

        private void StartMoving()
        {
            _movementPaused = false;
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