using UnityEngine;

namespace Assets.Scripts.Helpers
{
	public static class Layers
	{
        public static string Default
        {
            get { return "Default"; }
        }

        public static string Player
		{
			get { return "Player"; }
		}

		public static string PL
		{
			get { return "PL"; }
		}

		public static string Cliffs
		{
			get { return "Cliffs"; }
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

		public static string Heat
		{
			get { return "Heat"; }
		}

		public static string Flash
		{
			get { return "Flash"; }
		}

		public static LayerMask Platforms
        {
            get { return 1 << LayerMask.NameToLayer(Layers.IndoorMetal)
                    | 1 << LayerMask.NameToLayer(Layers.IndoorWood)
					| 1 << LayerMask.NameToLayer(Layers.OutdoorMetal)
					| 1 << LayerMask.NameToLayer(Layers.OutdoorWood)
					| 1 << LayerMask.NameToLayer(Layers.Ice)
					| 1 << LayerMask.NameToLayer(Layers.Cliffs); }
        }
	}
}
