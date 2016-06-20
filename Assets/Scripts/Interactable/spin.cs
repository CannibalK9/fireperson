using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class spin : MonoBehaviour
	{
		void Update()
		{
			transform.Rotate(0, 0, 0.2f);
		}
	}
}
