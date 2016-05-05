using UnityEngine;

namespace Assets.Scripts.Heat
{
	public struct HeatMessage
	{
		public RaycastHit2D Hit;
		public Vector2 Origin;
		public float CastDistance;
	}
}
