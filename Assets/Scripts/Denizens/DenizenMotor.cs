using UnityEngine;

namespace Assets.Scripts.Denizens
{
    public class DenizenMotor : MonoBehaviour
    {
        public float Gravity = -25f;
        public float RunSpeed = 3f;
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

        void Update()
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
                SetAnimationWhenGrounded();
                var hazardRay = new Vector2(
                    _controller.BoxCollider.bounds.max.x + HazardWarningDistance,
                    _controller.BoxCollider.bounds.min.y);

                if (ApproachingEdge(hazardRay) || ApproachingSnow(hazardRay) || _controller.CollisionState.Right)
                    DirectionTravelling = DirectionTravelling.Left;
                else
                    _normalizedHorizontalSpeed = 1;

                if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();
            }
            else if (DirectionTravelling == DirectionTravelling.Left)
            {
                SetAnimationWhenGrounded();
                var hazardRay = new Vector2(
                    _controller.BoxCollider.bounds.min.x - HazardWarningDistance,
                    _controller.BoxCollider.bounds.min.y);

                if (ApproachingEdge(hazardRay) || ApproachingSnow(hazardRay) || _controller.CollisionState.Left)
                    DirectionTravelling = DirectionTravelling.Right;
                else
                    _normalizedHorizontalSpeed = -1;

                if (GetDirectionFacing() == DirectionFacing.Right)
                    FlipSprite();
            }
            else
            {
                _normalizedHorizontalSpeed = 0;
            }
        }

        private void SetTravelInDirectionFacing()
        {
            DirectionTravelling = GetDirectionFacing() == DirectionFacing.Right
                        ? DirectionTravelling.Right
                        : DirectionTravelling.Left;
        }

        private bool ApproachingEdge(Vector2 edgeRay)
        {
            return Physics2D.Raycast(edgeRay, Vector2.down, 1f, _controller.PlatformMask) == false;
        }

        private bool _hitSnow;

        private bool ApproachingSnow(Vector2 snowRay)
        {
            LayerMask mask = 1 << LayerMask.NameToLayer("Melting");
            RaycastHit2D hit = Physics2D.Raycast(snowRay, Vector2.down, 0.1f, mask);
            if (hit && _hitSnow == false)
            {
                _hitSnow = true;
                _movementPaused = true;
                _animator.Play(Animator.StringToHash("Shiver"));
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
            if (_controller.IsGrounded && _normalizedHorizontalSpeed == 0)
                _animator.Play(Animator.StringToHash("Idle"));
            else
                _animator.Play(Animator.StringToHash("Idle"));
        }

        private void HandleMovement()
        {
            if (_controller.IsGrounded)
                _velocity.x = Mathf.SmoothDamp(
                    _velocity.x,
                    _normalizedHorizontalSpeed * RunSpeed,
                    ref _velocity.x,
                    Time.deltaTime * GroundDamping);

            _velocity.y += Gravity * Time.deltaTime;
            _controller.Movement.Move(_velocity * Time.deltaTime);
            _velocity = _controller.Velocity;
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
                _animator.Play(Animator.StringToHash("Gasp"));
            }
            else if (_playerSpotted)
            {
                _playerSpotted = false;
                _animator.Play(Animator.StringToHash("Relief"));
            }
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