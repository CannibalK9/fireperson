using UnityEngine;

namespace Assets.Scripts.Denizens
{
    public class DenizenMotor : MonoBehaviour
    {
        public float Gravity = -25f;
        public float RunSpeed = 3f;
        public float GroundDamping = 7f;
        public float HazardWarningDistance = 1f;
        public DirectionTravelling directionTravelling;

        private float _normalizedHorizontalSpeed = 0;

        private DenizenController _controller;
        private Animator _animator;
        private Vector3 _velocity;
        private bool _satAtFirePlace;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<DenizenController>();
            directionTravelling = DirectionTravelling.None;
        }

        void Update()
        {
            if (_controller.SatAtFireplace)
            {
                _satAtFirePlace = true;
                _velocity = Vector3.zero;
                directionTravelling = DirectionTravelling.None;
            }
            else if (_satAtFirePlace)
            {
                SetTravelInDirectionFacing();
                _satAtFirePlace = false;
            }

            DetermineMovement();
            HandleMovement();
        }

        void MoveToFireplace(DirectionTravelling direction)
        {
            directionTravelling = direction;
        }

        private void DetermineMovement()
        {
            if (_controller.IsGrounded)
            {
                _velocity.y = 0;
                //SetAnimationWhenGrounded();
                if (_controller.CollisionState.becameGroundedThisFrame)
                    SetTravelInDirectionFacing();
            }
            else
            {
                directionTravelling = DirectionTravelling.None;
                //SetAnimationWhenFalling();
            }

            if (directionTravelling == DirectionTravelling.Right)
            {
                var hazardRay = new Vector2(
                    _controller.BoxCollider.bounds.max.x + HazardWarningDistance,
                    _controller.BoxCollider.bounds.min.y);

                if (ApproachingEdge(hazardRay) || ApproachingSnow(hazardRay) || _controller.CollisionState.right)
                    directionTravelling = DirectionTravelling.Left;
                else
                    _normalizedHorizontalSpeed = 1;

                if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();
            }
            else if (directionTravelling == DirectionTravelling.Left)
            {
                var hazardRay = new Vector2(
                    _controller.BoxCollider.bounds.min.x - HazardWarningDistance,
                    _controller.BoxCollider.bounds.min.y);

                if (ApproachingEdge(hazardRay) || ApproachingSnow(hazardRay) || _controller.CollisionState.left)
                    directionTravelling = DirectionTravelling.Right;
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
            directionTravelling = GetDirectionFacing() == DirectionFacing.Right
                        ? DirectionTravelling.Right
                        : DirectionTravelling.Left;
        }

        private bool ApproachingEdge(Vector2 edgeRay)
        {
            return Physics2D.Raycast(edgeRay, Vector2.down, 1f, _controller.PlatformMask) == false;
        }

        private bool ApproachingSnow(Vector2 snowRay)
        {
            LayerMask mask = 1 << LayerMask.NameToLayer("Melting");
            return Physics2D.Raycast(snowRay, Vector2.down, 0.1f, mask);
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
                _animator.Play(Animator.StringToHash("Run"));
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

        private enum DirectionFacing
        {
            Right,
            Left
        }

        private DirectionFacing GetDirectionFacing()
        {
            DirectionFacing directionFacing;
            if (transform.localScale.x > 0f)
                directionFacing = DirectionFacing.Right;
            else
                directionFacing = DirectionFacing.Left;

            return directionFacing;
        }
    }
}