using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerMotor : MonoBehaviour
    {
        public float Gravity = -25f;
        public float RunSpeed = 8f;
        public float GroundDamping = 1f;

        private float _normalizedHorizontalSpeed = 0;

        private Animator _animator;
        public Animator Animator
        {
            get
            {
                return _animator != null ? _animator : null;
            }
        }

        private BoxCollider2D _collider;
        public BoxCollider2D Collider
        {
            get
            {
                return _collider != null ? _collider : null;
            }
        }

        private PlayerController _controller;
        private ClimbHandler _climbHandler;
        private Vector3 _velocity;
        private LayerMask _defaultPlatformMask;
        private Transform _transform;

        void Awake()
        {
            _animator = transform.parent.parent.GetComponent<Animator>();
            _controller = GetComponent<PlayerController>();
            _collider = GetComponent<BoxCollider2D>();
            _transform = transform.parent.parent;

            _climbHandler = new ClimbHandler(this);
            _defaultPlatformMask = _controller.PlatformMask;
        }

        void Update()
        {
            if (_climbHandler.IsClimbing)
            {
                _climbHandler.ClimbAnimation();
            }
            else
            {
                HandleMovementInputs();

                if (_controller.IsGrounded)
                    _velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.deltaTime * GroundDamping);

                _velocity.y += Gravity * Time.deltaTime;
                Move(_velocity * Time.deltaTime);
                _velocity = _controller.Velocity;
            }
            _controller.HeatIce();
        }

        public void Move(Vector2 deltaMovement)
        {
            _controller._movement.Move(deltaMovement);
        }

        private void HandleMovementInputs()
        {
            if (_controller.IsGrounded == false)
            {
                _animator.SetBool("falling", true);
                _animator.SetBool("moving", false);
                return;
            }
            _animator.SetBool("falling", false);

            _velocity.y = 0;

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.2f, _controller.BoxCollider.bounds.min.y);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 2f, _controller.PlatformMask);

                _normalizedHorizontalSpeed = edgeHit ? 1 : 0;
                if (_controller.CollisionState.right && GetDirectionFacing() == DirectionFacing.Right)
                    ;//BackToWallAnimation
                else if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                var edgeRay = new Vector2(_controller.BoxCollider.bounds.min.x - 0.2f, _controller.BoxCollider.bounds.center.y);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 5f, _controller.PlatformMask);

                _normalizedHorizontalSpeed = edgeHit ? -1 : 0;
                if (_controller.CollisionState.left && GetDirectionFacing() == DirectionFacing.Left)
                    ;//BackToWallAnimation
                else if (GetDirectionFacing() == DirectionFacing.Right)
                    FlipSprite();
            }
            else
            {
                _normalizedHorizontalSpeed = 0;
            }
            SetAnimationWhenGrounded();

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _climbHandler.CheckClimbUp();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _climbHandler.CheckClimbDown();
            }
            else if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            {
                _controller.CreatePilotedLight();
                //_animator.Play(Animator.StringToHash("Create light"));
            }
        }

        public void FlipSprite()
        {
            _transform.localScale = new Vector3(-_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
        }

        private void SetAnimationWhenGrounded()
        {
            if (_controller.IsGrounded && _normalizedHorizontalSpeed == 0)
                _animator.SetBool("moving", false);
            else
            {
                _animator.SetBool("moving", true);
            }
        }

        private bool PilotedLightExists()
        {
            return GameObject.Find("PilotedLight(Clone)");
        }

        public void CancelHorizontalVelocity()
        {
            _animator.SetBool("moving", false);
            _normalizedHorizontalSpeed = 0;
            _velocity = Vector3.zero;
        }

        public DirectionFacing GetDirectionFacing()
        {
            DirectionFacing directionFacing;
            if (_transform.localScale.x > 0f)
                directionFacing = DirectionFacing.Right;
            else
                directionFacing = DirectionFacing.Left;

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
    }
}