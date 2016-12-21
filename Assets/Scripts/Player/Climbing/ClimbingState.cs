using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public class ClimbingState
	{
		public Climb Climb { get; set; }
		public Collider2D PivotCollider { get; private set; }
		public float MovementSpeed { get; private set; }
		public float AnimationSpeed { get; private set; }
		public ColliderPoint PivotPosition { get; private set; }
		public ColliderPoint PlayerPosition { get; set; }
		public DirectionFacing ClimbSide { get; private set; }
		public bool IsUpright { get; private set; }
		public bool IsCorner { get; private set; }
		public bool CanClimbDown { get; private set; }

		private static ColliderPoint _target;
		private static ColliderPoint _player;
		private static int _rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);

		public ClimbingState()
		{
			Climb = Climb.None;
			PivotCollider = null;
			MovementSpeed = 1;
			PivotPosition = ColliderPoint.Centre;
			PlayerPosition = ColliderPoint.Centre;
		}

		public ClimbingState (Climb climb, Collider2D col, float movementSpeed, float animationSpeed, ColliderPoint pivot, ColliderPoint player, DirectionFacing climbSide)
		{
			Climb = climb;
			PivotCollider = col;
			MovementSpeed = movementSpeed;
			AnimationSpeed = animationSpeed;
			PivotPosition = pivot;
			PlayerPosition = player;
			ClimbSide = climbSide;
			IsUpright = col.IsUpright();
			IsCorner = col.IsCorner();
			CanClimbDown = col.CanClimbDown();
		}

		public static ClimbingState GetStaticClimbingState(ClimbingState climbingState)
		{
			if (climbingState.Climb == Climb.None)
				climbingState.Climb = Climb.Prep;

			return climbingState;
		}

		public static ClimbingState GetClimbingState(Climb climb, Collider2D climbCollider, Collider2D playerCollider, bool shouldHang = false)
		{
			if (climb == Climb.None)
			{
				return new ClimbingState();
			}

			float climbingSpeed = 1;
			float animSpeed = 1;
			DirectionFacing direction = GetClimbingSide(climbCollider);
			Climb transitionClimb = Climb.None;

			switch (climb)
			{
				case Climb.Up:
				case Climb.Flip:
					Hanging(direction);
					transitionClimb = Climb.Down;
					climbingSpeed = ConstantVariables.MoveToEdgeSpeed;
					animSpeed = GetAnimationSpeed(playerCollider, climbCollider, climbingSpeed);
					break;
				case Climb.Mantle:
					Mantle(direction);
					transitionClimb = Climb.Up;
					climbingSpeed = ConstantVariables.AcrossSpeed;
					animSpeed = GetAnimationSpeed(playerCollider, climbCollider, climbingSpeed);
					break;
				case Climb.Down:
					OffEdge(direction);
					transitionClimb = Climb.Down;
					break;
				case Climb.AcrossLeft:
				case Climb.AcrossRight:
					if (shouldHang)
					{
						Hanging(direction);
						transitionClimb = Climb.Down;
					}
					else
					{
						OffEdge(direction);
						transitionClimb = Climb.Up;
					}
					climbingSpeed = 0.2f;
					animSpeed = GetAnimationSpeed(playerCollider, climbCollider, climbingSpeed);
					break;
			}
			return new ClimbingState(transitionClimb, climbCollider, climbingSpeed, animSpeed, _target, _player, direction);
		}

		private static float GetAnimationSpeed(Collider2D playerCollider, Collider2D targetCollider, float speed)
		{
			float distance = Vector2.Distance(playerCollider.GetPoint(_player), targetCollider.GetPoint(_target));

			//1 1 60
			//5 1 12
			//1 5 300
			//1 0.5 30
			//5 0.5 6

			float animSpeed = (50 / distance) * speed;
			return animSpeed;
		}

		private static DirectionFacing GetClimbingSide(Collider2D col)
		{
			return col.gameObject.layer == _rightClimbLayer
				? DirectionFacing.Right
				: DirectionFacing.Left;
		}

		private static void Hanging(DirectionFacing direction)
		{
			if (direction == DirectionFacing.Right)
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

		private static void Mantle(DirectionFacing direction)
		{
			if (direction == DirectionFacing.Right)
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

		private static void OffEdge(DirectionFacing direction)
		{
			if (direction == DirectionFacing.Right)
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
	}
}
