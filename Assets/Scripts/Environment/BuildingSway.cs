using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Environment
{
	public class BuildingSway : MonoBehaviour
	{
		private List<Rigidbody2D> _buildings;

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
			Vector2 force = Wind.Instance.Force;

			foreach (Rigidbody2D r in _buildings)
			{
				r.AddForce(force);
			}
		}
	}
}
