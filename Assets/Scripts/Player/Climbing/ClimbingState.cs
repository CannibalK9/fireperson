using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public class ClimbingState
	{
		public Climb Climb { get; set; }
		public Collider2D PivotCollider { get; private set; }
		public float MovementSpeed { get; set; }
		public ColliderPoint PivotPosition { get; private set; }
		public ColliderPoint PlayerPosition { get; private set; }
		public bool Recalculate { get; set; }

		public ClimbingState (Climb climb, Collider2D col, float speed, ColliderPoint pivot, ColliderPoint player, bool recalculate)
		{
			Climb = climb;
			PivotCollider = col;
			MovementSpeed = speed;
			PivotPosition = pivot;
			PlayerPosition = player;
			Recalculate = recalculate;
		}
	}
}
