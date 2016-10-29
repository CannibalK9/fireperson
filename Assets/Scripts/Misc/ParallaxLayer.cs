using UnityEngine;

namespace Assets.Scripts.Misc
{
	[ExecuteInEditMode]
	public class ParallaxLayer : MonoBehaviour
	{
		public float parallaxFactor;
		public void Move(Vector2 delta)
		{
			Vector2 newPos = transform.localPosition;
			newPos -= delta * parallaxFactor;
			transform.localPosition = newPos;
		}
	}
}
