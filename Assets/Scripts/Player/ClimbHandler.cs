using UnityEngine;

namespace Assets.Scripts.Player
{
    public class ClimbHandler
    {
        private PlayerMotor _motor;
        private Collider2D _climbCollider;
        private Collider2D _playerCollider;

        private bool _hasMoved;
        private Vector2 _target;
        private Vector2 _player;

        public ClimbingState CurrentClimbingState { get; set; }
        public ClimbingState NextClimbingState { get; set; }
        public DirectionFacing ClimbingSide { get; set; }

        public ClimbHandler(PlayerMotor motor)
        {
            _motor = motor;
            _climbCollider = null;
            _playerCollider = _motor.Collider;
        }

        public void ClimbAnimation()
        {
            if (CurrentClimbingState == ClimbingState.None || _hasMoved)
                return;

            switch (CurrentClimbingState)
            {
                case ClimbingState.Up:
                    ClimbUp();
                    break;
                case ClimbingState.Down:
                    ClimbDown();
                    break;
                case ClimbingState.AcrossLeft:
                case ClimbingState.AcrossRight:
                    Across();
                    break;
                case ClimbingState.MoveToEdge:
                    MoveToEdge();
                    break;
                case ClimbingState.None:
                default:
                    break;
            }
            ClimbMovement();
            if (NotMoving())
                _hasMoved = true;
        }

        private void ClimbUp()
        {
            if (ClimbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(_climbCollider);
                _player = GetTopLeft(_playerCollider);
            }
            else
            {
                _target = GetTopLeft(_climbCollider);
                _player = GetTopRight(_playerCollider);
            }
        }

        private void ClimbDown()
        {
            if (ClimbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(_climbCollider);
                _player = GetBottomRight(_playerCollider);

            }
            else
            {
                _target = GetTopLeft(_climbCollider);
                _player = GetBottomLeft(_playerCollider);
            }
        }

        private void MoveToEdge()
        {
            if (ClimbingSide == DirectionFacing.Right)
            {
                _target = GetTopRight(_climbCollider);
                _player = GetBottomRight(_playerCollider);

            }
            else
            {
                _target = GetTopLeft(_climbCollider);
                _player = GetBottomLeft(_playerCollider);
            }
        }

        private void Across()
        {
            if (ClimbingSide == DirectionFacing.Right)
            {
                _target = GetTopLeft(_climbCollider);
                _player = GetBottomLeft(_playerCollider);
            }
            else
            {
                _target = GetTopRight(_climbCollider);
                _player = GetBottomRight(_playerCollider);
            }
        }

        public bool CheckLedgeAbove()
        {
            float checkWidth = 5f;
            float checkHeight = 3f;

            Vector2 origin = new Vector2(
               _playerCollider.bounds.center.x,
               _playerCollider.bounds.max.y);

            Vector2 size = new Vector2(checkWidth, 1f);

            RaycastHit2D hit = GetNearestHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.up, checkHeight, GetClimbMask()));

            if (hit)
            {
                SetClimbingParameters(hit);
                CurrentClimbingState = ClimbingState.Up;
            }
            return hit;
        }

        public bool CheckLedgeBelow(ClimbingState intendedClimbingState, DirectionFacing direction)
        {
            float checkWidth = 5f;
            float checkDepth = 3f;

            Vector2 origin = new Vector2(
                   _playerCollider.bounds.center.x,
                   _playerCollider.bounds.min.y);

            Vector2 size = new Vector2(checkWidth, 1f);
            RaycastHit2D hit;

            if (intendedClimbingState == ClimbingState.Down)
                hit = GetNearestHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.down, checkDepth, GetClimbMask()));
            else
                hit = GetCorrectSideHit(Physics2D.BoxCastAll(origin, size, 0, Vector2.down, checkDepth, GetClimbMask()), direction);

            if (hit)
            {
                SetClimbingParameters(hit);
                CurrentClimbingState = intendedClimbingState;
            }
            return hit;
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

        private RaycastHit2D GetCorrectSideHit(RaycastHit2D[] hits, DirectionFacing direction)
        {
            RaycastHit2D hit = new RaycastHit2D();
            foreach (RaycastHit2D h in hits)
            {
                DirectionFacing edge = h.collider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot")
                    ? DirectionFacing.Right
                    : DirectionFacing.Left;

                if (edge == direction)
                    hit = h;
            }
            return hit;
        }

        public bool CheckLedgeAcross(DirectionFacing direction)
        {
            if ((CurrentClimbingState == ClimbingState.Up && ClimbingSide != direction)
                || (CurrentClimbingState == ClimbingState.Down && ClimbingSide == direction)
                || (CurrentClimbingState == ClimbingState.AcrossLeft && ClimbingSide != direction)
                || (CurrentClimbingState == ClimbingState.AcrossRight && ClimbingSide != direction))
                return false;

            float checkLength = 5f;
            float checkDepth = 2f;

            float xCast = direction == DirectionFacing.Right ? _playerCollider.bounds.max.x + 1f : _playerCollider.bounds.min.x - 1f;
            Vector2 origin = new Vector2(
               xCast,
               _playerCollider.bounds.min.y - checkDepth / 2);

            Vector2 size = new Vector2(1f, checkDepth);

            Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());
            RaycastHit2D hit = new RaycastHit2D();

            foreach (RaycastHit2D h in hits)
            {
                if (h.collider != _climbCollider)
                    hit = h;
            }

            if (hit)
            {
                SetClimbingParameters(hit);
                NextClimbingState = direction == DirectionFacing.Left
                    ? ClimbingState.AcrossLeft
                    : ClimbingState.AcrossRight;
            }
            else
            {
                NextClimbingState = ClimbingState.Jump;
            }
            return true;
        }

        private LayerMask GetClimbMask()
        {
            return 1 << LayerMask.NameToLayer("Right Climb Spot")
                | 1 << LayerMask.NameToLayer("Left Climb Spot");
        }

        private void SetClimbingParameters(RaycastHit2D hit)
        {
            _motor.CancelHorizontalVelocity();
            _motor.RemovePlatformMask();
            _climbCollider = hit.collider;

            SetClimbingSide();
        }

        private void SetClimbingSide()
        {
            ClimbingSide = _climbCollider.gameObject.layer == LayerMask.NameToLayer("Right Climb Spot")
                    ? DirectionFacing.Right
                    : DirectionFacing.Left;
        }

        private bool ShouldStraightClimb()
        {
            float overhangDistance = 1f;

            return ClimbingSide == DirectionFacing.Left
                ? _motor.transform.position.x < _climbCollider.transform.position.x + overhangDistance
                : _motor.transform.position.x > _climbCollider.transform.position.x - overhangDistance;
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
                case ClimbingState.Up:
                    isTransition = CheckLedgeAbove();
                    break;
                case ClimbingState.Down:
                    isTransition = CheckLedgeBelow(ClimbingState.Down, DirectionFacing.None);
                    break;
                case ClimbingState.AcrossLeft:
                    isTransition = CheckLedgeAcross(DirectionFacing.Left);
                    break;
                case ClimbingState.AcrossRight:
                    isTransition = CheckLedgeAcross(DirectionFacing.Right);
                    break;
                case ClimbingState.None:
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
            bool notMoving = _previousPosition == null ? false : Vector2.Distance((Vector2)_previousPosition, _player) < 0.1f;
            _previousPosition = _player;
            return notMoving;
        }

        public float ClimbingSpeed = 0.5f;

        private void ClimbMovement()
        {
            _motor.Move((_target - _player) * ClimbingSpeed);
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
