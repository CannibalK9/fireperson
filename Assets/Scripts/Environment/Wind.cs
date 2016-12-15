using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class Wind : Singleton<Wind>
    {
		public float MinPower = 0;
		public float MaxPower = 5;

		public float MinTime = 2;
		public float MaxTime = 7;

		public bool Right;
		public bool Alternate;
		public Vector2 Force { get; set; }

		private float _windPower;
		private float _timer;

		protected Wind() { }

		void Awake()
		{
			Force = Vector2.zero;
		}

		void Update()
		{
			if (_timer < 0)
			{
				_windPower = Random.Range(MinPower, MaxPower);
				_timer = Random.Range(MinTime, MaxTime);

				if (Alternate)
					Right = !Right;

				Force = _windPower * (Right ? Vector2.right : Vector2.left);
			}

			_timer -= Time.deltaTime;
		}
	}
}
