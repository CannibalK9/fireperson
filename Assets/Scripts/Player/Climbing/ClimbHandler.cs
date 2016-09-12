using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using UnityEngine;
using Assets.Scripts.Interactable;

namespace Assets.Scripts.Player.Climbing
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private Collider2D _climbCollider;
		private Transform _climbParent;
		private int _climbLayer;
		private readonly Collider2D _playerCollider;
		private readonly AnimationScript _anim;
		private readonly int _rightClimbLayer;
		private readonly int _leftClimbLayer;
		private ColliderPoint _target;
		private ColliderPoint _player;
		private bool _retryCheckAbove = true;
		private bool _shouldHang;
		private bool _sameEdge;

		public Climb CurrentClimb { get; set; }
		public List<Climb> NextClimbs { get; set; }
		public DirectionFacing ClimbSide { get; set; }
		public float DistanceToEdge { get; private set; }

		public ClimbHandler(PlayerMotor motor)
		{
			_motor = motor;
			_anim = _motor.Anim;
			_climbCollider = null;
			_playerCollider = _motor.Collider;
			_rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
			_leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);
			NextClimbs = new List<Climb>();
		}

		public ClimbingState GetClimbingState(bool recalculate)
		{
			if (CurrentClimb == Climb.End || recalculate == false)
			{
				_motor.ClimbingState.Climb = CurrentClimb;
				_motor.ClimbingState.Recalculate = false;
				return _motor.ClimbingState;
			}

			float climbingSpeed = ConstantVariables.DefaultMovementSpeed;

			switch (CurrentClimb)
			{
				case Climb.Up:
				case Climb.Flip:
					Hanging();
					break;
				case Climb.Mantle:
					Mantle();
					break;
				case Climb.Down:
					AtEdge();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
				case Climb.AcrossLeft:
				case Climb.AcrossRight:
					if (_shouldHang)
						Hanging();
					else
						OffEdge();
					climbingSpeed = ConstantVariables.AcrossSpeed;
					break;
				case Climb.MoveToEdge:
					OffEdge();
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					break;
			}
			 return new ClimbingState(CurrentClimb, _climbCollider, climbingSpeed, _target, _player, true);
		}

		public void CancelClimb()
		{
			if (CurrentClimb != Climb.MoveToEdge)
			{
				CurrentClimb = Climb.None;
				_climbCollider = null;
			}
		}

		private void Hanging()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.TopLeft;
			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.TopRight;
			}
		}

		private void Mantle()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.LeftFace;
			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.RightFace;
			}
		}

		private void AtEdge()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.BottomRight;

			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.BottomLeft;
			}
		}

		private void OffEdge()
		{
			if (ClimbSide == DirectionFacing.Right)
			{
				_target = ColliderPoint.TopRight;
				_player = ColliderPoint.BottomLeft;

			}
			else
			{
				_target = ColliderPoint.TopLeft;
				_player = ColliderPoint.BottomRight;
			}
		}

		private bool IsEdgeUnblocked(Collider2D originalHit)
		{
			Vector3 origin = _playerCollider.bounds.center;

			Vector2 direction = originalHit.bounds.center - origin;

			RaycastHit2D obstacleHit = Physics2D.Raycast(origin, direction, 10f, Layers.Platforms);
			Debug.DrawRay(origin, direction, Color.red);

            RaycastHit2D edgeObstructionHit = Physics2D.Raycast(new Vector2(originalHit.bounds.center.x, originalHit.bounds.max.y + 0.01f), Vector2.up, 0.1f, Layers.Platforms);

            return edgeObstructionHit == false &&
                (obstacleHit == false
                || originalHit.transform.parent == obstacleHit.collider.transform
                || Vector2.Distance(originalHit.transform.position, obstacleHit.point) < 1);
		}

		private bool IsCornerAccessible(Collider2D hit)
		{
			if (hit.IsCorner() == false || hit.IsUpright())
				return true;

			Vector2 playerPosition = hit.transform.position - _playerCollider.bounds.center;

			bool onRight = AngleDir(hit.transform.parent.up, playerPosition) < 0;
			bool above = AngleDir(hit.transform.parent.right, playerPosition) > 0;
			if (hit.IsInverted())
			{
				onRight = !onRight;
				above = !above;
			}
			return ((hit.transform.gameObject.layer == LayerMask.NameToLayer(Layers.RightClimbSpot) && (above || onRight))
				|| (hit.transform.gameObject.layer == LayerMask.NameToLayer(Layers.LeftClimbSpot)) && (above || !onRight));
		}

		private bool IsUprightAccessible(RaycastHit2D hit)
		{
			if (hit.collider.IsUpright() == false)
				return true;

			Vector2 playerPosition = hit.collider.transform.position - _playerCollider.bounds.center;

			bool onRight = AngleDir(hit.collider.transform.parent.right, playerPosition) < 0;
			if (hit.collider.transform.parent.rotation.eulerAngles.z > 180)
			{
				onRight = !onRight;
			}
			return ((hit.collider.transform.gameObject.layer == LayerMask.NameToLayer(Layers.RightClimbSpot) && onRight)
				|| (hit.collider.transform.gameObject.layer == LayerMask.NameToLayer(Layers.LeftClimbSpot)) && !onRight);
		}

		public bool CheckLedgeAbove(DirectionFacing direction, out Climb climb, bool retryCheck = true)
		{
			climb = Climb.End;

			const float checkWidth = 4f;
			const float checkHeight = 4f;

            float xValue = direction == DirectionFacing.Left ? _playerCollider.bounds.center.x - (checkWidth / 2) : _playerCollider.bounds.center.x + (checkWidth / 2);
            float yValue = _playerCollider.bounds.min.y + ConstantVariables.MaxLipHeight;

            float actualHeight = _playerCollider.bounds.size.y + checkHeight - ConstantVariables.MaxLipHeight;

			var origin = new Vector3(xValue, yValue);
			var size = new Vector2(checkWidth, 0.01f);

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, actualHeight, GetClimbMask());

			var hit = GetValidHit(hits);

			if (hit)
			{
				bool belowHeadHeight = hit.point.y < _playerCollider.bounds.max.y;

				if (belowHeadHeight && CurrentClimb == Climb.None && ShouldStraightClimb() && SpaceAboveEdge(hit.collider))
				{
					CurrentClimb = climb = Climb.Mantle;
				}
				else if (belowHeadHeight)
				{ }
				else if (hit.collider.IsCorner() || ShouldStraightClimb())
				{
					if (CurrentClimb == Climb.None)
						CurrentClimb = Climb.Up;

					climb = Climb.Up;
				}
				else
				{
					if (CurrentClimb == Climb.None)
						CurrentClimb = Climb.Flip;

					climb = Climb.Flip;
				}
			}

			if (climb == Climb.End && _retryCheckAbove && retryCheck)
			{
				_retryCheckAbove = false;
				CheckLedgeAbove(direction == DirectionFacing.Left ? DirectionFacing.Right : DirectionFacing.Left, out climb);
			}
			_retryCheckAbove = true;
			return climb != Climb.End;
		}

        private bool CheckLedgeWhileHanging()
        {
            const float checkWidth = 3f;
            const float checkHeight = 3f;

			int layer = _climbCollider.gameObject.layer;

			float xValue = layer == _leftClimbLayer ? _playerCollider.bounds.max.x - (checkWidth / 2) : _playerCollider.bounds.min.x + (checkWidth / 2);
            float yValue = _playerCollider.bounds.max.y;

            var origin = new Vector3(xValue, yValue);
            var size = new Vector2(checkWidth, 0.01f);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, Vector2.up, checkHeight, GetClimbMask());

            var hit = GetValidHit(hits);

            if (hit)
            {
				_anim.SetBool("inverted", ClimbSide == _motor.GetDirectionFacing());
				_sameEdge = false;
            }
            return hit;
        }

        public static float AngleDir(Vector2 line, Vector2 point)
		{
			return -line.x * point.y + line.y * point.x;
		}

		public bool CheckLedgeBelow(Climb intendedClimbingState, DirectionFacing direction)
		{
			const float checkWidth = 4f;

			int layer = direction == DirectionFacing.Right ? _rightClimbLayer : _leftClimbLayer;
			BoxCollider2D edge = _motor.MovementState.PivotCollider.gameObject.GetComponentsInChildren<BoxCollider2D>().SingleOrDefault(c => c.gameObject.layer == layer);

			if (edge != null) //extra check if the ground is walkable
			{
				var distance = Vector2.Distance(_motor.GetGroundPivotPosition(), edge.transform.position);
				if (distance < checkWidth && IsEdgeUnblocked(edge) && IsCornerAccessible(edge))
				{
					CurrentClimb = intendedClimbingState;
					SetClimbingParameters(edge);
					DistanceToEdge = distance;
					return true;
				}
			}
			return false;
		}

		public bool CheckLedgeAcross(DirectionFacing direction)
		{
			NextClimbs.Clear();

			const float checkLength = 6f;
			const float maxHeightAbove = 1f;
			const float maxHeightBelow = 2f;
			float checkDepth = _playerCollider.bounds.size.y + maxHeightAbove + maxHeightBelow;

			float x = direction == DirectionFacing.Left ? _playerCollider.bounds.min.x : _playerCollider.bounds.max.x;
			float y = _playerCollider.bounds.center.y - maxHeightBelow + maxHeightAbove;

			var origin = new Vector2(x, y);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());

			Transform previousClimbParent = _climbParent;
			RaycastHit2D hit = GetValidHit(hits);

			if (hit)
			{
				_shouldHang = _climbParent == previousClimbParent
					|| SpaceAboveEdge(hit.collider) == false
					|| hit.collider.bounds.center.y > _playerCollider.bounds.min.y + 1;
				_anim.SetBool("shouldHang", _shouldHang);
				_anim.SetBool("inverted", ClimbSide == direction);
			}
			return hit;
		}

		public bool CheckGrab(DirectionFacing direction)
		{
			const float checkLength = 1f;

			float x = direction == DirectionFacing.Left ? _playerCollider.bounds.min.x : _playerCollider.bounds.max.x;
			float y = _playerCollider.bounds.max.y - 0.5f;

			var origin = new Vector2(x, y);

			var size = new Vector2(0.01f, 1);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());

			RaycastHit2D hit = GetValidHit(hits);

			if (hit)
			{
				CurrentClimb = Climb.Up;
				_motor.Anim.SetAcrossTrigger();
			}
			return hit;
		}

		private RaycastHit2D GetValidHit(RaycastHit2D[] hits)
		{
			var hit = new RaycastHit2D();

			RaycastHit2D[] checkHits = _climbCollider == null
				? hits
				: hits.Where(h => h.collider != _climbCollider).ToArray();

			foreach (RaycastHit2D h in checkHits)
			{
				if (IsEdgeUnblocked(h.collider) && IsCornerAccessible(h.collider) && IsUprightAccessible(h))
				{
					hit = h;
					SetClimbingParameters(hit.collider);
					return hit;
				}
			}
			return hit;
		}

		public bool CheckReattach()
		{
			BoxCollider2D[] edges = _climbParent.GetComponentsInChildren<BoxCollider2D>();
			foreach (BoxCollider2D edge in edges)
			{
				if (_climbLayer == edge.gameObject.layer)
				{
					SetClimbingParameters(edge);
					_motor.MovementState.SetPivotCollider(edge);
					return true;
				}
			}
			return false;
		}

		private LayerMask GetClimbMask()
		{
			return 1 << _rightClimbLayer | 1 << _leftClimbLayer;
		}

		private void SetClimbingParameters(Collider2D col)
		{
			_climbCollider = col;
			_climbParent = col.transform.parent;
			_climbLayer = col.gameObject.layer;
			ClimbSide = GetClimbingSide(col);
			_anim.SetBool("falling", true);
			_anim.SetBool("corner", col.IsCorner());
			_anim.SetBool("upright", col.IsUpright());
		}

		private DirectionFacing GetClimbingSide(Collider2D col)
		{
			return col.gameObject.layer == _rightClimbLayer
				? DirectionFacing.Right
				: DirectionFacing.Left;
		}

		private bool ShouldStraightClimb()
		{
			return ClimbSide == DirectionFacing.Left
				? _motor.transform.position.x < _climbCollider.transform.position.x
				: _motor.transform.position.x > _climbCollider.transform.position.x;
		}

        private bool SpaceAboveEdge(Collider2D col)
        {
            return Physics2D.Raycast(new Vector2(col.bounds.center.x, col.bounds.max.y + 0.01f), Vector2.up, _playerCollider.bounds.size.y, Layers.Platforms) == false;
        }

		public ClimbingState SwitchClimbingState(DirectionFacing direction)
		{
			if (_climbCollider == null)
				return new ClimbingState(Climb.None, null, 1, ColliderPoint.BottomFace, ColliderPoint.BottomFace, false);

			bool recalculate = true;

			var nextClimb = CurrentClimb == Climb.End || CurrentClimb == Climb.Jump ? Climb.None : Climb.End;

			if (CurrentClimb == Climb.AcrossLeft || CurrentClimb == Climb.AcrossRight)
			{
				CurrentClimb = _shouldHang ? Climb.Down : Climb.Up;
			}

			DirectionFacing directionFacing = _motor.GetDirectionFacing();
			if (direction == DirectionFacing.None)
				direction = directionFacing;

			bool forward = direction == directionFacing || (_climbCollider.IsCorner() && CurrentClimb == Climb.Down);
			_anim.SetBool("forward", forward);

			switch (CurrentClimb)
			{
				case Climb.Mantle:
				case Climb.Flip:
				case Climb.Up:
                    if (NextClimbs.Contains(Climb.Down))
                    {
                        recalculate = false;
                        nextClimb = Climb.Down;
                    }
                    else if (NextClimbs.Contains(Climb.Up) && CheckLedgeAbove(direction, out nextClimb))
                    { }
                    else if (NextClimbs.Contains(Climb.AcrossLeft) && ClimbSide == DirectionFacing.Left)
                        nextClimb = CheckLedgeAcross(DirectionFacing.Left)
                            ? Climb.AcrossLeft
                            : Climb.Jump;
                    else if (NextClimbs.Contains(Climb.AcrossRight) && ClimbSide == DirectionFacing.Right)
                        nextClimb = CheckLedgeAcross(DirectionFacing.Right)
                            ? Climb.AcrossRight
                            : Climb.Jump;
                    else
                        MovePivotAlongSurface();

					if (nextClimb == Climb.Jump && _climbCollider.CanClimbDown() == false)
					{
						nextClimb = Climb.End;
                        MovePivotAlongSurface();
                    }
                    break;
				case Climb.Down:
					_sameEdge = true;
					if (NextClimbs.Contains(Climb.Up) && (SpaceAboveEdge(_climbCollider) || CheckLedgeWhileHanging()))
					{
                        nextClimb = Climb.Up;
                        _motor.Anim.SetBool("sameEdge", _sameEdge);
						if (_sameEdge)
                            recalculate = false;
                    }
                    else if (NextClimbs.Contains(Climb.AcrossLeft) && (_climbCollider.IsCorner() == false || ClimbSide == DirectionFacing.Left))
						if (forward)
							nextClimb = CheckLedgeAcross(DirectionFacing.Left)
								? Climb.AcrossLeft
								: Climb.Jump;
						else
						{
							nextClimb = Climb.AcrossLeft;
							recalculate = false;
						}
					else if (NextClimbs.Contains(Climb.AcrossRight) && (_climbCollider.IsCorner() == false || ClimbSide == DirectionFacing.Right))
						if (forward)
							nextClimb = CheckLedgeAcross(DirectionFacing.Right)
							? Climb.AcrossRight
							: Climb.Jump;
						else
						{
							nextClimb = Climb.AcrossRight;
							recalculate = false;
						}
					else if (NextClimbs.Contains(Climb.Down))
						nextClimb = Climb.Down;

					if ((nextClimb == Climb.Jump || nextClimb == Climb.Down) && _climbCollider.CanClimbDown() == false)
						nextClimb = Climb.End;

                    if (nextClimb == Climb.Jump && NextClimbs.Contains(Climb.Up) && (NextClimbs.Contains(Climb.AcrossLeft) || NextClimbs.Contains(Climb.AcrossRight)))
                        nextClimb = Climb.End;

                    if (nextClimb == Climb.Down)
						_motor.MovementState.IsGrounded = false;

					break;
				case Climb.MoveToEdge:
					if (NextClimbs.Contains(Climb.AcrossLeft) && CheckLedgeAcross(DirectionFacing.Left))
						nextClimb = Climb.AcrossLeft;
					else if (NextClimbs.Contains(Climb.AcrossRight) && CheckLedgeAcross(DirectionFacing.Right))
						nextClimb = Climb.AcrossRight;
					else if (NextClimbs.Contains(Climb.Down))
						nextClimb = Climb.Down;
					else if (_climbCollider.CanClimbDown())
						nextClimb = Climb.Jump;
					else
                        MovePivotAlongSurface();
                    break;
			}

			CurrentClimb = nextClimb;
			NextClimbs.Clear();

			return GetClimbingState(recalculate);
		}

        private void MovePivotAlongSurface()
        {
            _motor.MovementState.MovePivotAlongSurface(ClimbSide == DirectionFacing.Right ? DirectionTravelling.Left : DirectionTravelling.Right, _playerCollider.bounds.extents.x);
        }
	}
}
