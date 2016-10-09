using Assets.Scripts.Interactable;
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
				if (_fireplace.IsLit && (HeatMessage.DistanceToMove != _fireplace.HeatIntensity || HeatMessage.HeatRange != _fireplace.HeatRayDistance))
					UpdateHeat(new HeatMessage(_fireplace.HeatIntensity, _fireplace.HeatRayDistance));
				else
					EnableCollider(_fireplace.IsLit);
			}
		}

		public void UpdateHeat(HeatMessage heatMessage)
		{
			HeatMessage = heatMessage;
			SetColliderSizes(heatMessage.HeatRange);
			EnableCollider(true);
		}

		protected abstract void SetColliderSizes(float range);
		protected abstract void EnableCollider(bool enable);
	}
}