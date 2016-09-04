using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public class ClimbHandler
	{
		private readonly PlayerMotor _motor;
		private Collider2D _climbCollider;
		private Transform _climbParent;
		private readonly Collider2D _playerCollider;
		private readonly AnimationScript _anim;
		private readonly int _rightClimbLayer;
		private readonly int _leftClimbLayer;
		private ColliderPoint _target;
		private ColliderPoint _player;
		private bool _retryCheckAbove = true;
		private bool _shouldHang;

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

		private bool IsEdgeUnblocked(RaycastHit2D originalHit)
		{
			Vector3 origin = _playerCollider.bounds.center;
			Vector3 edge = originalHit.collider.bounds.center;

			Vector2 direction = edge - origin;

			RaycastHit2D hit = Physics2D.Raycast(origin, direction, 10f, Layers.Platforms);
			Debug.DrawRay(origin, direction, Color.red);

			return hit == false || Vector2.Distance(originalHit.point, hit.point) < 1;
		}

		private bool IsCornerAccessible(RaycastHit2D hit)
		{
			if (hit.collider.IsCorner() == false || hit.collider.IsUpright())
				return true;

			var side = AngleDir(hit.transform.up, hit.transform.position - _playerCollider.bounds.center);
			if (hit.collider.IsInverted())
				side = -side;
			return ((side < 0 && hit.transform.gameObject.layer == LayerMask.NameToLayer(Layers.RightClimbSpot))
				|| (side > 0 && hit.transform.gameObject.layer == LayerMask.NameToLayer(Layers.LeftClimbSpot)));
		}

		public bool CheckLedgeAbove(DirectionFacing direction, out Climb climb, bool retryCheck = true)
		{
			climb = Climb.End;

			const float checkWidth = 4f;
			const float checkHeight = 6f;

			float actualHeight = _playerCollider.bounds.size.y + checkHeight - 1;

			var origin = new Vector3(
			   _playerCollider.bounds.center.x,
			   _playerCollider.bounds.min.y + 1 + actualHeight / 2);

			var size = new Vector2(0.01f, actualHeight);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkWidth, GetClimbMask());

			var hit = GetValidHit(hits);

			if (hit)
			{
				bool belowHeadHeight = hit.point.y < _playerCollider.bounds.max.y;

				if (belowHeadHeight && CurrentClimb == Climb.None && ShouldStraightClimb())
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

		public static float AngleDir(Vector2 line, Vector2 point)
		{
			return -line.x * point.y + line.y * point.x;
		}

		public bool CheckLedgeBelow(Climb intendedClimbingState, DirectionFacing direction)
		{
			const float checkWidth = 3f;
			const float checkDepth = 0.1f;

			Vector2 origin = _motor.GetGroundPivotPosition();

			Vector2 castDirection = _motor.MovementState.GetSurfaceDirection(direction == DirectionFacing.Left ? DirectionTravelling.Left : DirectionTravelling.Right);

			float offset = castDirection.x > 0
				? _playerCollider.bounds.center.x - origin.x
				: origin.x - _playerCollider.bounds.center.x;

			//origin += offset * castDirection;
			origin -= new Vector2(0, checkDepth);

			RaycastHit2D hit = Physics2D.Raycast(origin, castDirection, checkWidth, GetClimbMask());
			Debug.DrawRay(origin, castDirection, Color.black);
			if (hit && IsEdgeUnblocked(hit))
			{
				DirectionFacing dir = hit.collider.gameObject.layer == _rightClimbLayer
					? DirectionFacing.Right
					: DirectionFacing.Left;

				if (dir == direction || hit.collider.IsUpright())
				{
					CurrentClimb = intendedClimbingState;
					SetClimbingParameters(hit.collider);
					DistanceToEdge = hit.distance;
					return true;
				}
			}
			return false;
		}

		public bool CheckLedgeAcross(DirectionFacing direction)
		{
			NextClimbs.Clear();

			const float checkLength = 5f;
			const float maxHeightAbove = 1f;
			const float maxHeightBelow = 2f;
			float checkDepth = _playerCollider.bounds.size.y + maxHeightAbove + maxHeightBelow;

			float x = direction == DirectionFacing.Left ? _playerCollider.bounds.min.x : _playerCollider.bounds.max.x;
			float y = _playerCollider.bounds.center.y - maxHeightBelow + maxHeightAbove;

			var origin = new Vector2(x, y);

			var size = new Vector2(0.01f, checkDepth);

			Vector2 castDirection = direction == DirectionFacing.Left ? Vector2.left : Vector2.right;

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0, castDirection, checkLength, GetClimbMask());

			RaycastHit2D hit = GetValidHit(hits);

			if (hit)
			{
				_shouldHang = hit.collider.bounds.center.y > _playerCollider.bounds.min.y + 1;
				_anim.SetBool("shouldHang", _shouldHang);
				_anim.SetBool("inverted", ClimbSide == direction);
			}
			return hit;
		}

		private RaycastHit2D GetValidHit(RaycastHit2D[] hits)
		{
			var hit = new RaycastHit2D();

			RaycastHit2D[] checkHits = _climbCollider == null
				? hits
				: hits.Where(h => h.transform != _climbCollider.transform).ToArray();

			foreach (RaycastHit2D h in checkHits)
			{
				if (IsEdgeUnblocked(h) && IsCornerAccessible(h))
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
				if (Vector2.Distance(edge.transform.position, _motor.MovementState.Pivot.transform.position) < 1)
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
			_climbParent = _climbCollider.transform.parent;
			SetClimbingSide();
			_anim.SetBool("falling", true);
			_anim.SetBool("corner", col.IsCorner());
		}

		private void SetClimbingSide()
		{
			if (_climbCollider.IsUpright())
			{
				DirectionFacing dir = _motor.GetDirectionFacing();
				switch (CurrentClimb)
				{
					case Climb.MoveToEdge:
					case Climb.Flip:
					case Climb.Down:
						ClimbSide = dir;
						break;
					default:
						var side = AngleDir(_climbCollider.transform.right, _climbCollider.transform.position - _playerCollider.bounds.center);
						if (_climbCollider.gameObject.layer == _leftClimbLayer)
							side = -side;
						ClimbSide = side > 0 
							? DirectionFacing.Left
							: DirectionFacing.Right;
						break;
				}
			}
			else
				ClimbSide = _climbCollider.gameObject.layer == _rightClimbLayer
					? DirectionFacing.Right
					: DirectionFacing.Left;
		}

		private bool ShouldStraightClimb()
		{
			return ClimbSide == DirectionFacing.Left
				? _motor.transform.position.x < _climbCollider.transform.position.x
				: _motor.transform.position.x > _climbCollider.transform.position.x;
		}

		public ClimbingState SwitchClimbingState(DirectionFacing direction)
		{
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
					if (NextClimbs.Contains(Climb.Down) && _climbCollider.IsUpright() == false)
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
						_motor.MoveHorizontally();

					if (nextClimb == Climb.Jump && _climbCollider.CanClimbDown() == false)
						nextClimb = Climb.End;
					break;
				case Climb.Down:
					if (NextClimbs.Contains(Climb.Up))
					{
						recalculate = false;
						nextClimb = Climb.Up;
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
						_motor.MoveHorizontally();
					break;
			}

			CurrentClimb = nextClimb;
			NextClimbs.Clear();

			return GetClimbingState(recalculate);
		}

		public bool CanClimbDown()
		{
			return _climbCollider.CanClimbDown();
		}
	}
}
