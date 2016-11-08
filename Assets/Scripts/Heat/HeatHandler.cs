using Assets.Scripts.Interactable;
using Destructible2D;
using UnityEngine;

namespace Assets.Scripts.Heat
{
	public abstract class HeatHandler : MonoBehaviour
	{
		public HeatMessage HeatMessage { get; set; }
		private GameObject _steam;
        private GameObject _emberLight;
        private GameObject _emberStrong;
		protected FirePlace _fireplace;

		void Start()
		{
			_steam = (GameObject)Resources.Load("particles/steam");
			_emberLight = (GameObject)Resources.Load("particles/emberLight");
			_emberStrong = (GameObject)Resources.Load("particles/emberStrong");
		}

		void Update()
		{
			if (_fireplace != null)
			{
				if (_fireplace.IsLit && (HeatMessage.Intensity != _fireplace.HeatIntensity || HeatMessage.Range != _fireplace.HeatRayDistance))
					UpdateHeat(new HeatMessage(_fireplace.HeatIntensity, _fireplace.HeatRayDistance));
				else
					EnableCollider(_fireplace.IsLit);
			}
		}

		public void UpdateHeat(HeatMessage heatMessage)
		{
			HeatMessage = heatMessage;
			SetColliderSizes(heatMessage.Range);
			EnableCollider(true);
		}

		protected Color GetIntensityColour()
		{
			float hue = 50f - HeatMessage.Intensity / 2;
			return Color.HSVToRGB(hue / 360, 0.5f, 0.75f);
		}

		protected abstract void SetColliderSizes(float range);
		protected abstract void EnableCollider(bool enable);
	}
}