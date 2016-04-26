using System;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public static class KeyBindings
	{
		public static bool GetKey(Control control)
		{
			switch (control)
			{
				case Control.Left:
					return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
				case Control.Right:
					return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
				case Control.Up:
					return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
				case Control.Down:
					return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
				case Control.Action:
					return Input.GetKey(KeyCode.E);
				case Control.Jump:
					return Input.GetKey(KeyCode.Space);
				case Control.Destroy:
					return Input.GetKey(KeyCode.Q);
				default:
					throw new ArgumentOutOfRangeException("control");
			}
		}
	}
}
