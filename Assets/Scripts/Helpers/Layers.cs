using UnityEngine;

namespace Assets.Scripts.Helpers
{
	public static class Layers
	{
		public static string Player
		{
			get { return "Player"; }
		}

		public static string OutdoorWood
		{
			get { return "OutdoorWood"; }
		}

		public static string OutdoorMetal
		{
			get { return "OutdoorMetal"; }
		}

		public static string IndoorWood
		{
			get { return "IndoorWood"; }
		}

		public static string IndoorMetal
		{
			get { return "IndoorMetal"; }
		}

		public static string PlSpot
		{
			get { return "PL Spot"; }
		}

		public static string Ice
		{
			get { return "Ice"; }
		}

		public static string Denizens
		{
			get { return "Denizens"; }
		}

		public static string RightClimbSpot
		{
			get { return "Right Climb Spot"; }
		}

		public static string LeftClimbSpot
		{
			get { return "Left Climb Spot"; }
		}

		public static string Interactable
		{
			get { return "Interactable"; }
		}

		public static string Background
		{
			get { return "Background"; }
		}

		public static string BackgroundIce
		{
			get { return "BackgroundIce"; }
		}

		public static string CameraSpot
		{
			get { return "CameraSpot"; }
		}

        public static string Steam
        {
            get { return "Steam"; }
        }

        public static LayerMask Platforms
        {
            get { return 1 << 8
                    | 1 << 10
                    | 1 << 18
                    | 1 << 19
                    | 1 << 20; }
        }
	}
}
