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

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            if (_animationPhase != 0)
            {
                switch(_animationPhase)
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

                _velocity = _controller.Velocity;
            }
            _controller.HeatIce();
        }

        private void HandleMovementInputs()
        {
            if (_controller.IsGrounded == false)
                return;

            _velocity.y = 0;

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                var edgeRay = new Vector2(_controller.BoxCollider.bounds.max.x + 0.2f, _controller.BoxCollider.bounds.min.y);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 1f, _controller.PlatformMask);

                _normalizedHorizontalSpeed = edgeHit ? 1 : 0;
                if (_controller.CollisionState.right && GetDirectionFacing() == DirectionFacing.Right)
                    ;//BackToWallAnimation
                else if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                var edgeRay = new Vector2(_controller.BoxCollider.bounds.min.x - 0.2f, _controller.BoxCollider.bounds.min.y);
                RaycastHit2D edgeHit = Physics2D.Raycast(edgeRay, Vector2.down, 0.5f, _controller.PlatformMask);

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
            if (_controller.IsGrounded && _normalizedHorizontalSpeed == 0)
                _animator.Play(Animator.StringToHash("Idle"));
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

            Vector2 size = new Vector2(5f, 1f);

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.up, 3f, GetClimbMask());

            if (hit)
            {
                CancelHorizontalVelocity();
                ClimbCollider = hit.collider;
                if (ClimbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    SetAnimationPhase(1);
                    _climbingSide = DirectionFacing.Right;
                    _animator.Play(Animator.StringToHash("Right Climb Up_1"));
                }
                else
                {
                    SetAnimationPhase(1);
                    _climbingSide = DirectionFacing.Left;
                    _animator.Play(Animator.StringToHash("Right Climb Up_1"));
                }
            }
        }

        private void ClimbDown()
        {
            Vector2 origin = new Vector2(
               _controller.BoxCollider.bounds.center.x,
               _controller.BoxCollider.bounds.min.y);

            Vector2 size = new Vector2(5f, 1f);

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.down, 1f, GetClimbMask());

            if (hit)
            {
                CancelHorizontalVelocity();
                ClimbCollider = hit.collider;
                if (ClimbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    SetAnimationPhase(4);
                    _climbingSide = DirectionFacing.Right;
                    _animator.Play(Animator.StringToHash("Right Climb Down_1"));
                }
                else
                {
                    SetAnimationPhase(4);
                    _climbingSide = DirectionFacing.Left;
                    _animator.Play(Animator.StringToHash("Right Climb Down_1"));
                }
            }
        }

        private LayerMask GetClimbMask()
        {
            return 1 << LayerMask.NameToLayer("Right Climb Spot")
                | 1 << LayerMask.NameToLayer("Left Climb Spot");
        }

        private void CancelHorizontalVelocity()
        {
            _normalizedHorizontalSpeed = 0;
            _velocity = Vector3.zero;
        }

        DirectionFacing _climbingSide;
        Vector2 _target;
        Vector2 _moving;

        private void SetAnimationPhase(int phase)
        {
            _animationPhase = phase;
        }

        private void ClimbUp_1()
        {
            if (_climbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(ClimbCollider);
                _moving = GetTopLeft(_controller.BoxCollider);
            }
            else
            {
                _target = GetTopLeft(ClimbCollider);
                _moving = GetTopRight(_controller.BoxCollider);
            }
            ClimbMovement();

            if (NotMoving())
            {
                _animator.Play(Animator.StringToHash("Right Climb Up_2"));
                SetAnimationPhase(2);
                _previousPosition = null;
            }
        }

        private void ClimbUp_2()
        {
            if (_climbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(ClimbCollider);
                _moving = GetBottomLeft(_controller.BoxCollider);
            }
            else
            {
                _target = GetTopLeft(ClimbCollider);
                _moving = GetBottomRight(_controller.BoxCollider);
            }
            ClimbMovement();

            if (NotMoving())
            {
                _animator.Play(Animator.StringToHash("Right Climb Up_3"));
                SetAnimationPhase(3);
                _previousPosition = null;
            }
        }

        private void ClimbUp_3()
        {
            if (_climbingSide == DirectionFacing.Right)
            {
                _target = GetTopLeft(ClimbCollider);
                _moving = GetBottomLeft(_controller.BoxCollider);
            }
            else
            {
                _target = GetTopRight(ClimbCollider);
                _moving = GetBottomRight(_controller.BoxCollider);
            }
            ClimbMovement();

            if (NotMoving())
            {
                _animator.Play(Animator.StringToHash("Idle"));
                SetAnimationPhase(0);
                _previousPosition = null;
            }
        }

        private void ClimbDown_1()
        {
            if (_climbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(ClimbCollider);
                _moving = GetBottomRight(_controller.BoxCollider);

            }
            else
            {
                _target = GetTopLeft(ClimbCollider);
                _moving = GetBottomLeft(_controller.BoxCollider);
            }
            ClimbMovement();

            if (NotMoving())
            {
                _animator.Play(Animator.StringToHash("Right Climb Down_2"));
                SetAnimationPhase(5);
                _previousPosition = null;
            }
        }

        private void ClimbDown_2()
        {
            if (_climbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(ClimbCollider);
                _moving = GetBottomLeft(_controller.BoxCollider);
            }
            else
            {
                _target = GetTopLeft(ClimbCollider);
                _moving = GetBottomRight(_controller.BoxCollider);
            }
            ClimbMovement();

            if (NotMoving())
            {
                _animator.Play(Animator.StringToHash("Right Climb Down_3"));
                SetAnimationPhase(6);
                _previousPosition = null;
            }
        }

        private void ClimbDown_3()
        {
            if (_climbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(ClimbCollider);
                _moving = GetTopLeft(_controller.BoxCollider);
            }
            else
            {
                _target = GetTopLeft(ClimbCollider);
                _moving = GetTopRight(_controller.BoxCollider);
            }
            ClimbMovement();

            if (NotMoving())
            {
                _animator.Play(Animator.StringToHash("Idle"));
                SetAnimationPhase(0);
                _previousPosition = null;
            }
        }

        private Vector2? _previousPosition;

        private bool NotMoving()
        {
            bool notMoving = _previousPosition == null ? false :_previousPosition == _moving;
            _previousPosition = _moving;
            return notMoving;
        }

        private void ClimbMovement()
        {
            _controller._movement.Move((_target - _moving) * 0.5f);
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