using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Environment
{
	public class BuildingSway : MonoBehaviour
	{
		private List<Rigidbody2D> _buildings;
		public float _timer;
		public bool _right;
		public float Thrust = 5f;

		void Awake()
		{
			_buildings = new List<Rigidbody2D>();
			foreach (Transform t in transform)
			{
				if (t.tag == "windless")
					continue;

				var r = t.GetComponent<Rigidbody2D>();
				if (r != null)
					_buildings.Add(r);
			}
		}

		void Update()
		{
			if (_timer < 0)
			{
				_timer = Random.Range(2, 7);
				_right = !_right;
			}
			Vector2 force = Thrust * (_right ? Vector2.right : Vector2.left);

			foreach (Rigidbody2D r in _buildings)
			{
				r.AddForce(force);
			}
			_timer -= Time.deltaTime;
		}
	}
}
