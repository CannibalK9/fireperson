using UnityEngine;

namespace Assets.Scripts.Heat
{
	public abstract class HeatHandler : MonoBehaviour
	{
		public HeatMessage HeatMessage { get; set; }
		private GameObject _steam;
        private GameObject _emberLight;
        private GameObject _emberStrong;
		protected ParticleSystem _ps;

		protected float _defaultWidth;
		protected Vector3 _defaultScale;

		void Awake()
		{
			var col = GetComponent<Collider2D>();
			var circleCol = col as CircleCollider2D;
			if (circleCol != null)
				_defaultWidth = circleCol.bounds.size.x;
			else
			{
				var boxCol = col as BoxCollider2D;
				_defaultWidth = boxCol.size.x;
			}
			_defaultScale = transform.localScale;
			_ps = GetComponentInChildren<ParticleSystem>();
			UpdateHeat(new HeatMessage(1, 2f));
		}

		void Start()
		{
			_steam = (GameObject)Resources.Load("particles/steam");
			_emberLight = (GameObject)Resources.Load("particles/emberLight");
			_emberStrong = (GameObject)Resources.Load("particles/emberStrong");
		}

		public void UpdateHeat(HeatMessage heatMessage)
		{
			HeatMessage = heatMessage;
			SetColliderSizes(heatMessage.HeatRange);
		}

		protected virtual void SetColliderSizes(float additionalRange)
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