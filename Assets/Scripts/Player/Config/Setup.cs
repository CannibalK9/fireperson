using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Player.Config
{
	public class Setup : MonoBehaviour
	{
		[Range(1, 10)]
		public float Stability;

		[Range(1, 10)]
		public float Intensity;

		[Range(1, 10)]
		public float Control;

		[Range(1, 100)]
		public float AvailablePoints;

		public bool Tether;
		public bool Ignite;
		public bool Flash;
		public bool Steam;
		public bool Burn;
		public bool Scout;

		void Awake()
		{
			SetupVariables();
			SetupAbilities();

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
		}

		void Update()
		{
			PlayerPrefs.SetFloat(Variable.Stability.ToString(), Stability);
			PlayerPrefs.SetFloat(Variable.Intensity.ToString(), Intensity);
			PlayerPrefs.SetFloat(Variable.Control.ToString(), Control);
			PlayerPrefs.SetFloat(Variable.AvailablePoints.ToString(), AvailablePoints);
			AbilityState.SetActive(Ability.Tether, Tether);
			AbilityState.SetActive(Ability.Ignite, Ignite);
			AbilityState.SetActive(Ability.Flash, Flash);
			AbilityState.SetActive(Ability.Steam, Steam);
			AbilityState.SetActive(Ability.Burn, Burn);
			AbilityState.SetActive(Ability.Scout, Scout);
			PlayerPrefs.Save();
		}

		private static void SetupVariables()
		{
			foreach (Variable variable in Enum.GetValues(typeof(Variable)))
			{
				if (PlayerPrefs.HasKey(variable.ToString()) == false)
					PlayerPrefs.SetFloat(variable.ToString(), 1f);
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
