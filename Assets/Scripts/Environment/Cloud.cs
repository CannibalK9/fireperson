using UnityEngine;

namespace Assets.Scripts.Environment
{
	public class Cloud : MonoBehaviour
	{
		private float _speed;
		private float _endY;
		private float _startY;
		private Camera _camera;

		void Awake()
		{
			_camera = Camera.main;
			_startY = transform.position.y;
			_endY = _camera.transform.position.y;
			_speed = Random.Range(0.01f, 0.05f);
		}

		void Update()
		{
			transform.Translate(0, -_speed, 0, Space.World);
			float scale = 1 - ((_startY - transform.position.y) / (_startY - _endY));
			transform.localScale = new Vector3(scale, scale, 0);

			if (transform.position.y < _camera.transform.position.y)
				Destroy(gameObject);
		}
	}
}
