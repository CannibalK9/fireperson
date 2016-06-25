using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class ClimbingState
	{
		public Climb Climb { get; private set; }
		public Collider2D PivotCollider { get; private set; }
		public float MovementSpeed { get; private set; }
		public ColliderPoint PivotPosition { get; private set; }
		public ColliderPoint PlayerPosition { get; private set; }

		public ClimbingState (Climb climb, Collider2D col, float speed, ColliderPoint pivot, ColliderPoint player)
		{
			Climb = climb;
			PivotCollider = col;
			MovementSpeed = speed;
			PivotPosition = pivot;
			PlayerPosition = player;
		}
	}
}
