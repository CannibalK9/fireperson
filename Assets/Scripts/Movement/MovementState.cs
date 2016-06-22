using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementState
	{
		public bool IsGrounded { get; set; }
		public bool IsOnSlope { get; set; }
		public GameObject GroundPivot { get; set; }

		public MovementState()
		{
			GroundPivot = new GameObject();
		}

		public void Reset()
		{
			IsGrounded = false;
			IsOnSlope = false;
		}
	}
}
