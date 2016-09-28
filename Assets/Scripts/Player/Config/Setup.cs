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
		public float AvailablePoints;

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
		}

		void Update()
		{
			Stability = PlayerPrefs.GetFloat(Variable.Stability.ToString());
			Intensity = PlayerPrefs.GetFloat(Variable.Intensity.ToString());
			Control = PlayerPrefs.GetFloat(Variable.Control.ToString());
			AvailablePoints = PlayerPrefs.GetFloat(Variable.AvailablePoints.ToString());

			Tether = AbilityState.IsActive(Ability.Tether);
			Ignite = AbilityState.IsActive(Ability.Ignite);
			Flash = AbilityState.IsActive(Ability.Flash);
			Steam = AbilityState.IsActive(Ability.Steam);
			Burn = AbilityState.IsActive(Ability.Burn);
			Scout = AbilityState.IsActive(Ability.Scout);
			Tools = AbilityState.IsActive(Ability.Tools);
		}

		private static void SetupVariables()
		{
			foreach (Variable variable in Enum.GetValues(typeof(Variable)))
			{
				//if (PlayerPrefs.HasKey(variable.ToString()) == false)
					PlayerPrefs.SetFloat(variable.ToString(), 75f);
			}
		}

		private static void SetupAbilities()
		{
			foreach (Ability ability in Enum.GetValues(typeof(Ability)))
			{
				if (PlayerPrefs.HasKey(ability.ToString()) == false)
					PlayerPrefs.SetInt(ability.ToString(), 0);
			}
		}
	}
}
