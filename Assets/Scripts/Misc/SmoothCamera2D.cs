using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.CameraHandler
{
	public class SmoothCamera2D : MonoBehaviour
	{
		public float MovementDampTime = 0.15f;
		public float SizeDampTime = 0.15f;
		public Transform Player { get; set; }
		public Transform CameraSpot { get; set; }
		public Transform Pl { get; set; }
		public float Height { get; set; }

		private Vector2 _velocity = Vector2.zero;
		private float _speed;
		private Camera _camera;

		void Awake()
		{
			_camera = GetComponent<Camera>();
			Height = 16;
		}

		void Update()
		{
			Vector2 point = transform.position;

			if (CameraSpot != null)
			{
				point = CameraSpot.position;
				if (Pl != null)
				{

				}
			}
			else
			{
				if (Player == null)
				{
					FindObjectOfType<AnimationScript>();
				}

				if (Player != null)
				{
					if (Pl != null)
					{
						point = Vector2.Lerp(Player.position, Pl.position, 0.5f);
						Height = 16 + Vector2.Distance(Player.position, Pl.position);
					}
					else
					{
						point = Player.position;
						Height = 16;
					}
				}
			}
			transform.position = Vector2.SmoothDamp(transform.position, point, ref _velocity, MovementDampTime);
			_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, Height / 2, ref _speed, SizeDampTime);
		}
	}
}