using UnityEngine;

namespace Assets.Scripts.Misc
{
	[ExecuteInEditMode]
	public class ParallaxLayer : MonoBehaviour
	{
		public float ParallaxFactor;
		public bool X;
		public bool Y;
		public void Move(Vector2 delta)
		{
			Vector2 newPos = transform.position;
			newPos -= delta * ParallaxFactor;
			if (X && !Y)
				transform.position = new Vector2(newPos.x, transform.position.y);
			else if (!X && Y)
				transform.position = new Vector2(transform.position.x, newPos.y);
			else
				transform.position = newPos;
		}
	}
}
