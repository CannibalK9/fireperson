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

		void Awake()
		{
			SetupVariables();
			SetupAbilities();

			Stability = PlayerPrefs.GetFloat(Variable.Stability.ToString());
			Intensity = PlayerPrefs.GetFloat(Variable.Intensity.ToString());
			Control = PlayerPrefs.GetFloat(Variable.Control.ToString());
			AvailablePoints = PlayerPrefs.GetFloat(Variable.AvailablePoints.ToString());

			Tether = PlayerPrefs.GetInt(Ability.Tether.ToString()) > 0;
		}

		void Update()
		{
			PlayerPrefs.SetFloat(Variable.Stability.ToString(), Stability);
			PlayerPrefs.SetFloat(Variable.Intensity.ToString(), Intensity);
			PlayerPrefs.SetFloat(Variable.Control.ToString(), Control);
			PlayerPrefs.SetFloat(Variable.AvailablePoints.ToString(), AvailablePoints);
			PlayerPrefs.SetInt(Ability.Tether.ToString(), Tether ? 1 : 0);
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
