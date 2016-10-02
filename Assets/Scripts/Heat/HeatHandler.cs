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
		private FirePlace _fireplace;
		protected ParticleSystem _ps;
		protected BoxCollider2D _box;

		void Awake()
		{
			_fireplace = GetComponentInParent<FirePlace>();
			_ps = GetComponentInChildren<ParticleSystem>();

			if (this is BoxHeater)
			{
				_box = GetComponent<BoxCollider2D>();
			}
		}

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
				EnableCollider(_fireplace.IsLit);
				if (_fireplace.IsLit && (HeatMessage.DistanceToMove != _fireplace.HeatIntensity || HeatMessage.HeatRange != _fireplace.HeatRayDistance))
					UpdateHeat(new HeatMessage(_fireplace.HeatIntensity, _fireplace.HeatRayDistance));
			}
		}

		public void UpdateHeat(HeatMessage heatMessage)
		{
			HeatMessage = heatMessage;
			SetColliderSizes(heatMessage.HeatRange);
		}

		protected virtual void SetColliderSizes(float range)
		{ }

		public void EnableCollider(bool enable)
		{
			GetComponent<Collider2D>().enabled = enable;
			if (_ps != null)
			{
				if (enable)
					_ps.Play();
				else
					_ps.Stop();
			}
		}
	}
}