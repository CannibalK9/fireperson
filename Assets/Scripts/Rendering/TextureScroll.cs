using UnityEngine;

namespace Assets.Scripts.Rendering
{
	public class TextureScroll : MonoBehaviour
	{
		public float ScrollSpeed;
		private Vector2 savedOffset;
		private Renderer _renderer;
		private float _startOffset;

		void Awake()
		{
			_renderer = GetComponent<Renderer>();
			_startOffset = Random.Range(0, 0.8f);
		}

		void Start()
		{
			savedOffset = _renderer.material.GetTextureOffset("_MainTex");
		}

		void Update()
		{
			float y = Mathf.Repeat(Time.time * ScrollSpeed + _startOffset, 0.8f);
			Vector2 offset = new Vector2(savedOffset.x, y);
			_renderer.material.SetTextureOffset("_MainTex", offset);
		}

		void OnDisable()
		{
			_renderer.material.SetTextureOffset("_MainTex", savedOffset);
		}
	}
}
