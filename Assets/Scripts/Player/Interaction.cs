using UnityEngine;

namespace Assets.Scripts.Player
{
	public class Interaction
	{
		public Transform Object { get; set; }
		public Collider2D Point { get; set; }
		public bool IsInteracting { get; set; }
		public bool IsLeft { get { return Point.transform.localPosition.x < 0; } }
		public float Centre { get { return Point.bounds.center.x; } }
	}
}
