using System;
using UnityEngine;

namespace Assets.Scripts.Player.Config
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
                case Control.Jump:
					return Input.GetKey(KeyCode.Space);
				case Control.Action:
					return Input.GetKey(KeyCode.Q);
                case Control.Light:
					if (GameCursor.IsOverUi())
						return false;
					return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Mouse0);
				case Control.Ability1:
					return Input.GetKey(KeyCode.Alpha1);
				default:
					return false;
			}
		}

        public static bool GetKeyDown(Control control)
        {
			switch (control)
            {
                case Control.Light:
					if (GameCursor.IsOverUi())
						return false;
					return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0);
                default:
                    return false;
            }
        }

        public static bool GetKeyUp(Control control)
        {
			switch (control)
            {
                case Control.Light:
                    return Input.GetKeyUp(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0);
                default:
                    return false;
            }
        }
    }
}
