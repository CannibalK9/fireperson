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
		public float HeatIntensity { get { return Intensity; } }
		public float HeatRayDistance { get { return Stability; } }
		public Collider2D Collider { get; set; }
		public PlayerController Player;
		public bool FirstUpdate = true;
		public CircleHeater HeatHandler;

		private float _distanceFromPlayer;
		private Flash _flash;
		private Renderer _renderer;

		void Awake()
		{
			Collider = GetComponent<CircleCollider2D>();
			HeatHandler = transform.GetComponentInChildren<CircleHeater>();
			_flash = GetComponentInChildren<Flash>();
			_renderer = GetComponent<Renderer>();
			_renderer.enabled = false;
		}

		void Start()
		{
			_distanceFromPlayer = Player.Control * ConstantVariables.DistanceFromPlayerMultiplier;
		}

		void Update()
		{
			if (ChannelingHandler.ChannelingSet == false)
			{
				Stability = ChannelingHandler.ChannelPercent * Player.BaseStability;
				Intensity = ChannelingHandler.ChannelPercent * Player.BaseIntensity;
			}
			else if (FirstUpdate)
			{
				FirstUpdate = false;
				_renderer.enabled = true;
			}

			HeatHandler.UpdateHeat(new HeatMessage(Intensity / 50, 1 + Stability / 10));
		}

		public bool IsWithinPlayerDistance()
		{
			float distance = Vector2.Distance(Player.transform.position, transform.position);
			float maxDistance = _distanceFromPlayer;

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
