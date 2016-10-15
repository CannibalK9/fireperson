using UnityEngine;

namespace Assets.Scripts.Player
{
	public class ControlBorder : MonoBehaviour
	{
		private SpriteRenderer _renderer;

		void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
		}

		public void SetSize(float size)
		{
			transform.localScale = Vector3.one * size;
		}
	}
}
