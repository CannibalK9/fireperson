using UnityEngine;

namespace Assets.Scripts.Player
{
    public class ClimbHandler
    {
        private PlayerMotor _motor;
        private Collider2D _climbCollider;
        private Collider2D _playerCollider;

        public bool IsClimbing { get; set; }
        private bool _climbingUp;

        public ClimbHandler(PlayerMotor motor)
        {
            _motor = motor;
            _climbCollider = null;
        }

        public void ClimbAnimation()
        {
            if (_climbingUp)
                ClimbUp();
            else
                ClimbDown();
        }

        public void CheckClimbUp()
        {
            if (_playerCollider == null)
                _playerCollider = _motor.Collider;

            Vector2 origin = new Vector2(
               _playerCollider.bounds.center.x,
               _playerCollider.bounds.max.y);

            Vector2 size = new Vector2(5f, 1f);

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.up, 3f, GetClimbMask());

            if (hit)
            {
                _motor.CancelHorizontalVelocity();
                _motor.RemovePlatformMask();
                _climbCollider = hit.collider;
                IsClimbing = true;
                _climbingUp = true;

                if (_climbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    _climbingSide = DirectionFacing.Right;

                    if (IsRightOfClimbSpot())
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Right)
                            _motor.FlipSprite();
                        _motor.Animator.Play(Animator.StringToHash("Straight Climb Up"));
                    }
                    else
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Left)
                            _motor.FlipSprite();
                        _motor.Animator.Play(Animator.StringToHash("Flip Climb Up"));
                    }

                }
                else
                {
                    _climbingSide = DirectionFacing.Left;

                    if (IsRightOfClimbSpot())
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Right)
                            _motor.FlipSprite();
                        _motor.Animator.Play(Animator.StringToHash("Flip Climb Up"));
                    }
                    else
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Left)
                            _motor.FlipSprite();
                        _motor.Animator.Play(Animator.StringToHash("Straight Climb Up"));
                    }
                }
            }
        }

        public void CheckClimbDown()
        {
            if (_playerCollider == null)
                _playerCollider = _motor.Collider;

            Vector2 origin = new Vector2(
               _playerCollider.bounds.center.x,
               _playerCollider.bounds.min.y);

            Vector2 size = new Vector2(5f, 1f);

            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.down, 1f, GetClimbMask());

            if (hit)
            {
                _motor.CancelHorizontalVelocity();
                _motor.RemovePlatformMask();
                _climbCollider = hit.collider;
                IsClimbing = true;
                _climbingUp = false;
                if (_climbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    if (_motor.GetDirectionFacing() == DirectionFacing.Left)
                        _motor.FlipSprite();
                    _climbingSide = DirectionFacing.Right;
                }
                else
                {
                    if (_motor.GetDirectionFacing() == DirectionFacing.Right)
                        _motor.FlipSprite();
                    _climbingSide = DirectionFacing.Left;
                }
                _motor.Animator.Play(Animator.StringToHash("Climb Down"));
            }
        }

        private LayerMask GetClimbMask()
        {
            return 1 << LayerMask.NameToLayer("Right Climb Spot")
                | 1 << LayerMask.NameToLayer("Left Climb Spot");
        }

        private bool IsRightOfClimbSpot()
        {
            return _motor.transform.position.x > _climbCollider.transform.position.x;
        }

        DirectionFacing _climbingSide;
        Vector2 _target;
        Vector2 _moving;

        bool _hasMoved;

        private void ClimbUp()
        {
            if (IsAnimationFinished())
            {
                IsClimbing = false;
                _previousPosition = null;
                _hasMoved = false;
                _motor.ReapplyPlatformMask();
                return;
            }

            if (_hasMoved == false)
            {
                if (_climbingSide == DirectionFacing.Right)
                {
                    _target = GetTopRight(_climbCollider);
                    _moving = GetTopLeft(_playerCollider);
                }
                else
                {
                    _target = GetTopLeft(_climbCollider);
                    _moving = GetTopRight(_playerCollider);
                }

                ClimbMovement();

                if (NotMoving())
                    _hasMoved = true;
            }
        }       

        private void ClimbDown()
        {
            if (IsAnimationFinished())
            {
                IsClimbing = false;
                _motor.ReapplyPlatformMask();
                _previousPosition = null;
                _hasMoved = false;
                return;
            }
            if (_hasMoved == false)
            {
                if (_climbingSide == DirectionFacing.Right)
                {
                    _target = GetTopRight(_climbCollider);
                    _moving = GetBottomRight(_playerCollider);

                }
                else
                {
                    _target = GetTopLeft(_climbCollider);
                    _moving = GetBottomLeft(_playerCollider);
                }
                ClimbMovement();

                if (NotMoving())
                    _hasMoved = true;
            }
        }

        private Vector2? _previousPosition;

        private bool NotMoving()
        {
            bool notMoving = _previousPosition == null ? false : _previousPosition == _moving;
            _previousPosition = _moving;
            return notMoving;
        }

        public float ClimbingSpeed = 0.5f;

        private void ClimbMovement()
        {
            _motor.Move((_target - _moving) * ClimbingSpeed);
        }

        private bool IsAnimationFinished()
        {
            return _motor.Animator.IsInTransition(0);
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
    }
}
