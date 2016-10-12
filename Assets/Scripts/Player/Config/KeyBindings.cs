using System;
using UnityEngine;

namespace Assets.Scripts.Player.Config
{
	public static class KeyBindings
	{
		public static bool GetKey(Controls control)
		{
			switch (control)
			{
				case Controls.Left:
					return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
				case Controls.Right:
					return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
				case Controls.Up:
					return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
				case Controls.Down:
					return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
                case Controls.Jump:
					return Input.GetKey(KeyCode.Space);
				case Controls.Anchor:
					return Input.GetKey(KeyCode.X);
				case Controls.Action:
					return Input.GetKey(KeyCode.Q);
                case Controls.Light:
					return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Mouse0);
				case Controls.Ability1:
					return Input.GetKey(KeyCode.Alpha1);
				default:
					return false;
			}
		}

        public static bool GetKeyDown(Controls control)
        {
			switch (control)
            {
                case Controls.Light:
					return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0);
                default:
                    return false;
            }
        }

        public static bool GetKeyUp(Controls control)
        {
			switch (control)
            {
                case Controls.Light:
                    return Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Mouse0);
				case Controls.Ability1:
					return Input.GetKeyUp(KeyCode.Alpha1);
                default:
                    return false;
            }
        }
    }
}
