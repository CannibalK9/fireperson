using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player.PL
{
	public class PilotedLightController : MonoBehaviour, IVariableHeater
	{
		public float Stability;
		public float Intensity;
		public float Control;
		public float HeatIntensity { get { return Intensity; } }
		public float HeatRayDistance { get { return Stability; } }
		public Collider2D Collider { get; set; }
		public PlayerController Player;
		public bool FirstUpdate = true;

		private float _emberEffectTime;
		private HeatHandler _heatHandler;
		private float _distanceFromPlayer;

		void Awake()
		{
			_heatHandler = new HeatHandler(this);
			Collider = GetComponent<CircleCollider2D>();
		}

		void Start()
		{
			Stability = ChannelingHandler.PlStability();
			Intensity = ChannelingHandler.PlIntensity();
			Control = ChannelingHandler.PlControl();
			_distanceFromPlayer = Stability;
		}


		void Update()
		{
            var effect = EmberEffect.None;
            
			_emberEffectTime -= Time.deltaTime;

			if (_emberEffectTime < 0)
			{
				effect = EmberEffect.Light;
				_emberEffectTime = Random.Range(10, 60);
			}

			if (FirstUpdate)
			{
				effect = EmberEffect.Strong;
				FirstUpdate = false;
			}

            HeatIce(effect);
		}

		public void HeatIce(EmberEffect effect)
		{
			_heatHandler.OneCircleHeat(effect);
		}

		public bool IsWithinPlayerDistance()
		{
			float distance = Vector2.Distance(Player.transform.position, transform.position);
			float maxDistance = _distanceFromPlayer * ConstantVariables.DistanceFromPlayerMultiplier;

			return distance < maxDistance;
		}
	}
}
