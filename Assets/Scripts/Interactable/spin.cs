using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class spin : MonoBehaviour
	{
		public bool Spin;
		public bool Left;
		[Range(0.01f, 5f)]
		public float Speed;

		void Update()
		{
			var speed = Speed;
			if (Left)
				speed = -Speed;
			if (Spin)
				transform.Rotate(0, 0, Speed);
		}
	}
}
