using UnityEngine;

namespace Assets.Scripts.Player
{
    public class ClimbHandler
    {
        private PlayerMotor _motor;
        private Collider2D _climbCollider;
        private Collider2D _playerCollider;

        public ClimbingState CurrentClimbingState { get; set; }
        public ClimbingState NextClimbingState { get; set; }

        public ClimbHandler(PlayerMotor motor)
        {
            _motor = motor;
            _climbCollider = null;
            _playerCollider = _motor.Collider;
        }

        public void ClimbAnimation()
        {
            if (CurrentClimbingState == ClimbingState.Up)
                ClimbUp();
            else if (CurrentClimbingState == ClimbingState.Down)
                ClimbDown();
            else if (CurrentClimbingState == ClimbingState.AcrossLeft || CurrentClimbingState == ClimbingState.AcrossRight)
                ClimbAcross();
        }

        public bool CheckClimbUp()
        {
            Vector2 origin = new Vector2(
               _playerCollider.bounds.center.x,
               _playerCollider.bounds.max.y);

            Vector2 size = new Vector2(5f, 1f);

            RaycastHit2D hit = GetNearestHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.up, 3f, GetClimbMask()));

            if (hit)
            {
                SetIsClimbing(hit);
                CurrentClimbingState = ClimbingState.Up;

                if (_climbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    _climbingSide = DirectionFacing.Right;

                    if (true)//ShouldStraightClimb())
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Right)
                            _motor.FlipSprite();
                        //_motor.Animator.Play(Animator.StringToHash("Straight Climb Up"));
                    }
                    else
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Left)
                            _motor.FlipSprite();
                        //_motor.Animator.Play(Animator.StringToHash("Flip Climb Up"));
                    }

                }
                else
                {
                    _climbingSide = DirectionFacing.Left;

                    if (true)//ShouldStraightClimb())
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Left)
                            _motor.FlipSprite();
                        //_motor.Animator.Play(Animator.StringToHash("Straight Climb Up"));
                    }
                    else
                    {
                        if (_motor.GetDirectionFacing() == DirectionFacing.Right)
                            _motor.FlipSprite();
                        //_motor.Animator.Play(Animator.StringToHash("Flip Climb Up"));
                    }
                }
            }
            return hit;
        }

        public bool CheckClimbDown()
        {
            Vector2 origin = new Vector2(
               _playerCollider.bounds.center.x,
               _playerCollider.bounds.min.y);

            Vector2 size = new Vector2(5f, 1f);

            RaycastHit2D hit = GetNearestHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.down, 3f, GetClimbMask()));

            if (hit)
            {
                SetIsClimbing(hit);
                CurrentClimbingState = ClimbingState.Down;
                if (_climbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot"))
                {
                    _climbingSide = DirectionFacing.Right;
                    if (_motor.GetDirectionFacing() == DirectionFacing.Left)
                        _motor.FlipSprite();
                }
                else
                {
                    _climbingSide = DirectionFacing.Left;
                    if (_motor.GetDirectionFacing() == DirectionFacing.Right)
                        _motor.FlipSprite();
                }
                //_motor.Animator.Play(Animator.StringToHash("Climb Down"));
            }
            return hit;
        }

        private LayerMask GetClimbMask()
        {
            return 1 << LayerMask.NameToLayer("Right Climb Spot")
                | 1 << LayerMask.NameToLayer("Left Climb Spot");
        }

        private RaycastHit2D GetNearestHit(RaycastHit2D[] hits)
        {
            RaycastHit2D hit = new RaycastHit2D();
            if (hits.Length > 0)
                hit = hits[0];

            if (hits.Length > 1)
            {
                foreach (RaycastHit2D r in hits)
                {
                    if (Vector2.Distance(hit.point, _playerCollider.transform.position)
                        > Vector2.Distance(r.point, _playerCollider.transform.position))
                        hit = r;
                }
            }
            return hit;
        }

        private void SetIsClimbing(RaycastHit2D hit)
        {
            _motor.CancelHorizontalVelocity();
            _motor.RemovePlatformMask();
            _climbCollider = hit.collider;
        }

        private bool ShouldStraightClimb()
        {
            float overhangDistance = 1f;

            return _climbingSide == DirectionFacing.Left
                ? _motor.transform.position.x < _climbCollider.transform.position.x + overhangDistance
                : _motor.transform.position.x > _climbCollider.transform.position.x - overhangDistance;
        }

        DirectionFacing _climbingSide;
        Vector2 _target;
        Vector2 _moving;

        bool _hasMoved;

        private void ClimbUp()
        {
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
            if (_hasMoved == false)
            {
                if (_climbingSide == DirectionFacing.Right)
                {
                    _target = GetTopRight(_climbCollider);
                    _moving = GetBottomLeft(_playerCollider);

                }
                else
                {
                    _target = GetTopLeft(_climbCollider);
                    _moving = GetBottomRight(_playerCollider);
                }
                ClimbMovement();

                if (NotMoving())
                    _hasMoved = true;
            }
        }

        private void ClimbAcross()
        {

        }

        public void SetNotClimbing()
        {
            CurrentClimbingState = ClimbingState.None;
            _previousPosition = null;
            _hasMoved = false;
            _motor.ReapplyPlatformMask();
        }

        public ClimbingState SwitchClimbingState()
        {
            bool isTransition = false;

            switch (NextClimbingState)
            {
                case ClimbingState.None:
                    break;
                case ClimbingState.Up:
                    isTransition = CheckClimbUp();
                    break;
                case ClimbingState.Down:
                    isTransition = CheckClimbDown();
                    break;
                case ClimbingState.AcrossRight:
                case ClimbingState.AcrossLeft:
                    isTransition = true;
                    break;
                default:
                    break;
            }

            _hasMoved = false;
            CurrentClimbingState = isTransition ? NextClimbingState : ClimbingState.None;
            NextClimbingState = ClimbingState.None;
            return CurrentClimbingState;
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
