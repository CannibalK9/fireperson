using System;
using UnityEngine;

namespace Assets.Scripts.Player.Config
{
	public class Setup : MonoBehaviour
	{
		[Range(1, 101)]
		public float Stability;

		[Range(1, 101)]
		public float Intensity;

		[Range(1, 101)]
		public float Control;

		[Range(1, 75)]
		public int AvailablePoints;

		public bool Tether;
		public bool Ignite;
		public bool Flash;
		public bool Steam;
		public bool Burn;
		public bool Scout;
		public bool Tools;

		void Awake()
		{
			SetupVariables();
			SetupAbilities();
			PlayerPrefs.SetInt(IntVariable.SpentPoints.ToString(), 0);
			if (PlayerPrefs.GetInt(IntVariable.AvailablePoints.ToString()) < 5)
				PlayerPrefs.SetInt(IntVariable.AvailablePoints.ToString(), 5);
			AvailablePoints = PlayerPrefs.GetInt(IntVariable.AvailablePoints.ToString());
			PlayerPrefs.Save();
		}

		void Update()
		{
			Stability = PlayerPrefs.GetFloat(FloatVariable.Stability.ToString());
			Intensity = PlayerPrefs.GetFloat(FloatVariable.Intensity.ToString());
			Control = PlayerPrefs.GetFloat(FloatVariable.Control.ToString());

			Tether = AbilityState.IsActive(Ability.Tether);
			Ignite = AbilityState.IsActive(Ability.Ignite);
			Flash = AbilityState.IsActive(Ability.Flash);
			Steam = AbilityState.IsActive(Ability.Steam);
			Burn = AbilityState.IsActive(Ability.Burn);
			Scout = AbilityState.IsActive(Ability.Scout);
			Tools = AbilityState.IsActive(Ability.Tools);

			PlayerPrefs.SetInt(IntVariable.AvailablePoints.ToString(), AvailablePoints);
			PlayerPrefs.Save();
		}

		private static void SetupVariables()
		{
			foreach (FloatVariable variable in Enum.GetValues(typeof(FloatVariable)))
			{
				PlayerPrefs.SetFloat(variable.ToString(), 0f);
			}
		}

		private static void SetupAbilities()
		{
			foreach (Ability ability in Enum.GetValues(typeof(Ability)))
			{
				PlayerPrefs.SetInt(ability.ToString(), 0);
			}
		}
	}
}
