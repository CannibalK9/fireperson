using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player.Abilities;
using UnityEngine;

namespace Assets.Scripts.Player.PL
{
	public class PilotedLightController : MonoBehaviour
	{
		public float Stability;
		public float Intensity;
		public float Control;
		public float HeatIntensity { get { return Intensity; } }
		public float HeatRayDistance { get { return Stability; } }
		public Collider2D Collider { get; set; }
		public PlayerController Player;
		public bool FirstUpdate = true;
		public HeatHandler HeatHandler;

		private float _emberEffectTime;
		private float _distanceFromPlayer;
		private Flash _flash;

		void Awake()
		{
			Collider = GetComponent<CircleCollider2D>();
			HeatHandler = transform.GetComponentInChildren<HeatHandler>();
			_flash = GetComponentInChildren<Flash>();
		}

		void Start()
		{
			Stability = ChannelingHandler.PlStability();
			Intensity = ChannelingHandler.PlIntensity();
			Control = ChannelingHandler.PlControl();
			_distanceFromPlayer = Control;
			HeatIce();
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
				//effect = EmberEffect.Strong;
				FirstUpdate = false;
			}
		}
		public void HeatIce()
		{
			HeatHandler.UpdateHeat(new HeatMessage(HeatIntensity / 100, Stability));
		}

		public bool IsWithinPlayerDistance()
		{
			float distance = Vector2.Distance(Player.transform.position, transform.position);
			float maxDistance = _distanceFromPlayer * ConstantVariables.DistanceFromPlayerMultiplier;

			return distance < maxDistance;
		}

		public bool IsWithinScoutingDistance()
		{
			float distance = Vector2.Distance(Player.transform.position, transform.position);
			float maxDistance = _distanceFromPlayer * ConstantVariables.DistanceFromPlayerMultiplier * 2;

			return distance < maxDistance;
		}

		public void Flash()
		{
			FirstUpdate = true;
			_flash.OnFlash();
		}
	}
}
