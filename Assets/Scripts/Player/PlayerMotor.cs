using System;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerMotor : MonoBehaviour
    {
        public float Gravity = -25f;
        public float RunSpeed = 8f;
        public float GroundDamping = 1f;

        private float _normalizedHorizontalSpeed = 0;
        private int _animationPhase = 0;

        private PlayerController _controller;
        private Animator _animator;
        private Vector3 _velocity;
        public event Action<RaycastHit2D> onControllerCollidedEvent;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<PlayerController>();

            onControllerCollidedEvent += onControllerCollider;
            _controller.onTriggerEnterEvent += onTriggerEnterEvent;
            _controller.onTriggerExitEvent += onTriggerExitEvent;
        }

        void onControllerCollider(RaycastHit2D hit)
        {
            if (hit.normal.y == 1f)
            return;

            //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
        }


        void onTriggerEnterEvent(Collider2D col)
        {
            Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
        }


        void onTriggerExitEvent(Collider2D col)
        {
            Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
        }

        void Update()
        {
            if (_animationPhase != 0)
            {
                switch (_animationPhase)
                {
                    case 1:
                        ClimbUp_1();
                        break;
                    case 2:
                        ClimbUp_2();
                        break;
                    case 3:
                        ClimbUp_3();
                        break;
                    case 4:
                        ClimbDown_1();
                        break;
                    case 5:
                        ClimbDown_2();
                        break;
                    case 6:
                        ClimbDown_3();
                        break;
                }
            }
            else
            {
                HandleMovementInputs();

                if (_controller.IsGrounded)
                    _velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, ref _velocity.x, Time.deltaTime * GroundDamping);

                _velocity.y += Gravity * Time.deltaTime;
                _controller._movement.Move(_velocity * Time.deltaTime);

                if (onControllerCollidedEvent != null)
                {
                    foreach (RaycastHit2D hit in _controller.RaycastHitsThisFrame)
                        onControllerCollidedEvent(hit);
                }

                _velocity = _controller.Velocity;
            }
            _controller.HeatIce();
        }

        private void HandleMovementInputs()
        {
            if (_controller.IsGrounded)
                _velocity.y = 0;

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.2f, _controller.BoxCollider.bounds.min.y);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 1f, _controller.PlatformMask);

                _normalizedHorizontalSpeed = edgeHit ? 1 : 0;
                if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                var edgeRay = new Vector2(_controller.BoxCollider.bounds.min.x - 0.2f, _controller.BoxCollider.bounds.min.y);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 1f, _controller.PlatformMask);

                _normalizedHorizontalSpeed = edgeHit ? -1 : 0;
                if (GetDirectionFacing() == DirectionFacing.Right)
                    FlipSprite();
            }
            else
            {
                _normalizedHorizontalSpeed = 0;
            }
            SetAnimationWhenGrounded();

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                ClimbUp();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                ClimbDown();
            }
            else if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            {
                _controller.CreatePilotedLight();
                //_animator.Play(Animator.StringToHash("Create light"));
            }
        }

        private void FlipSprite()
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        private void SetAnimationWhenGrounded()
        {
            if (_controller.IsGrounded && _normalizedHorizontalSpeed == 0) { }
            //_animator.Play(Animator.StringToHash("Idle"));
            else { }
                //_animator.Play(Animator.StringToHash("Run"));
        }

        private bool PilotedLightExists()
        {
            return GameObject.Find("PilotedLight(Clone)");
        }

        private Collider2D ClimbCollider = null;

        private void ClimbUp()
        {
            Vector2 origin = new Vector2(
               _controller.BoxCollider.bounds.center.x,
               _controller.BoxCollider.bounds.max.y);

            Vector2 size = new Vector2(4f, 1f);
            LayerMask mask =
                1 << LayerMask.NameToLayer("Right Climb Spot")
                | 1 << LayerMask.NameToLayer("Left Climb Spot");

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.up, 3f, mask);

            if (hit)
            {
                ClimbCollider = hit.collider;
                if (ClimbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    CancelHorizontalVelocity();
                    climbingSide = DirectionFacing.Right;
                    _animator.Play(Animator.StringToHash("Right Climb Up"));
                }
                else
                {
                    CancelHorizontalVelocity();
                    climbingSide = DirectionFacing.Left;
                    _animator.Play(Animator.StringToHash("Right Climb Up"));
                }
            }
        }

        private void ClimbDown()
        {
            Vector2 origin = new Vector2(
               _controller.BoxCollider.bounds.center.x,
               _controller.BoxCollider.bounds.min.y);

            Vector2 size = new Vector2(4f, 1f);
            LayerMask mask =
                1 << LayerMask.NameToLayer("Right Climb Spot")
                | 1 << LayerMask.NameToLayer("Left Climb Spot");

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.down, 1f, mask);

            if (hit)
            {
                ClimbCollider = hit.collider;
                if (ClimbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    CancelHorizontalVelocity();
                    climbingSide = DirectionFacing.Right;
                    _animator.Play(Animator.StringToHash("Right Climb Down"));
                }
                else
                {
                    CancelHorizontalVelocity();
                    climbingSide = DirectionFacing.Left;
                    _animator.Play(Animator.StringToHash("Right Climb Down"));
                }
            }
        }

        private void CancelHorizontalVelocity()
        {
            _normalizedHorizontalSpeed = 0;
            _velocity = Vector3.zero;
        }

        //Animation events
        private void SetAnimationPhase(int phase)
        {
            _animationPhase = phase;
        }

        DirectionFacing climbingSide;

        private void ClimbUp_1()
        {
            if (climbingSide == DirectionFacing.Right)
                ClimbMovement(GetTopRight(ClimbCollider), GetTopLeft(_controller.BoxCollider));
            else
                ClimbMovement(GetTopLeft(ClimbCollider), GetTopRight(_controller.BoxCollider));
        }

        private void ClimbUp_2()
        {
            if (climbingSide == DirectionFacing.Right)
                ClimbMovement(GetTopRight(ClimbCollider), GetBottomLeft(_controller.BoxCollider));
            else
                ClimbMovement(GetTopLeft(ClimbCollider), GetBottomRight(_controller.BoxCollider));
        }

        private void ClimbUp_3()
        {
            if (climbingSide == DirectionFacing.Right)
                ClimbMovement(GetTopLeft(ClimbCollider), GetBottomLeft(_controller.BoxCollider));
            else
                ClimbMovement(GetTopRight(ClimbCollider), GetBottomRight(_controller.BoxCollider));
        }

        private void ClimbDown_1()
        {
            if (climbingSide == DirectionFacing.Right)
                ClimbMovement(GetTopRight(ClimbCollider), GetBottomRight(_controller.BoxCollider));
            else
                ClimbMovement(GetTopLeft(ClimbCollider), GetBottomLeft(_controller.BoxCollider));
        }

        private void ClimbDown_2()
        {
            if (climbingSide == DirectionFacing.Right)
                ClimbMovement(GetTopRight(ClimbCollider), GetBottomLeft(_controller.BoxCollider));
            else
                ClimbMovement(GetTopLeft(ClimbCollider), GetBottomRight(_controller.BoxCollider));
        }

        private void ClimbDown_3()
        {
            if (climbingSide == DirectionFacing.Right)
                ClimbMovement(GetTopRight(ClimbCollider), GetTopLeft(_controller.BoxCollider));
            else
                ClimbMovement(GetTopLeft(ClimbCollider), GetTopRight(_controller.BoxCollider));
        }

        private void ClimbMovement(Vector2 ledge, Vector2 player)
        {
            _controller._movement.Move((ledge - player) * 0.1f);
        }

        private Vector2 GetTopRight(Collider2D col)
        {
            return col.bounds.max;
        }

        private Vector2 GetTopLeft(Collider2D col)
        {
            return new Vector2(
                col.bounds.min.x,
                col.bounds.max.y);
        }

        private Vector2 GetBottomRight(Collider2D col)
        {
            return new Vector2(
                col.bounds.max.x,
                col.bounds.min.y);
        }

        private Vector2 GetBottomLeft(Collider2D col)
        {
            return col.bounds.min;
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