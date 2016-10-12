using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using Assets.Scripts.Player.Abilities;
using UnityEngine;

namespace Assets.Scripts.Player.PL
{
	public class PilotedLightController : MonoBehaviour
	{
		public float CurrentStability { get; set; }
		public float CurrentIntensity { get; set; }
		public float BaseIntensity { get; set; }
		public float BaseStability { get; set; }
		public Collider2D Collider { get; set; }
		public PlayerController Player;
		public bool FirstUpdate = true;
		public CircleHeater HeatHandler;

		private Flash _flash;
		private Renderer _renderer;
		private float _previousStability;
		private float _previousIntensity;

		void Awake()
		{
			Collider = GetComponent<CircleCollider2D>();
			HeatHandler = transform.GetComponentInChildren<CircleHeater>();
			_flash = GetComponentInChildren<Flash>();
			_renderer = GetComponent<Renderer>();
			_renderer.enabled = false;
		}

		void Update()
		{
			if (ChannelingHandler.ChannelingSet == false)
			{
				BaseStability = ChannelingHandler.ChannelPercent * Player.BaseStability;
				BaseIntensity = ChannelingHandler.ChannelPercent * Player.BaseIntensity;

				CurrentStability = BaseStability;
				CurrentIntensity = BaseIntensity;
			}
			else if (FirstUpdate)
			{
				FirstUpdate = false;
				_renderer.enabled = true;
			}

			if (_previousIntensity != CurrentIntensity || _previousStability != CurrentStability)
			{
				HeatHandler.UpdateHeat(new HeatMessage(CurrentIntensity, 1 + CurrentStability / ConstantVariables.StabilityHeatRangeModifier));
				_previousStability = CurrentStability;
				_previousIntensity = CurrentIntensity;
			}
		}

		public void DecreaseVariables(float proportionOfCurrent)
		{
			CurrentIntensity = BaseIntensity * (proportionOfCurrent);
			CurrentStability = BaseStability * (proportionOfCurrent);
		}

		public void ResetVariables()
		{
			CurrentIntensity = BaseIntensity;
			CurrentStability = BaseStability;
		}

		public void EnterFireplace(FirePlace fireplace)
		{
			BaseStability += fireplace.HeatRayDistance * ConstantVariables.StabilityHeatRangeModifier;
			BaseIntensity += fireplace.HeatIntensity;
			ResetVariables();
		}

		public void LeaveFireplace(FirePlace fireplace)
		{
			BaseStability -= fireplace.HeatRayDistance * ConstantVariables.StabilityHeatRangeModifier;
			BaseIntensity -= fireplace.HeatIntensity;
			ResetVariables();
		}

		public bool IsWithinPlayerDistance()
		{
			float distance = Vector2.Distance(Player.transform.position, transform.position);
			float maxDistance = Player.DistanceFromPlayer;

			return distance < maxDistance;
		}

		public bool IsWithinScoutingDistance()
		{
			float distance = Vector2.Distance(Player.transform.position, transform.position);
			float maxDistance = Player.DistanceFromPlayer * 2;

			return distance < maxDistance;
		}

		public void Flash()
		{
			FirstUpdate = true;
			_flash.OnFlash();
		}
	}
}
