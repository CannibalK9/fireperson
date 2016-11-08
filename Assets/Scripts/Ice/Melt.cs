using System;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Ice
{
	public class Melt : MonoBehaviour
	{
		void Awake()
		{
			var collider = GetComponentsInChildren<Transform>().First(t => t.name.Equals("Collider"));

			if (collider != null && collider.GetComponent<MeltCollider>() == null)
				collider.gameObject.AddComponent<MeltCollider>();
		}
	}
}
