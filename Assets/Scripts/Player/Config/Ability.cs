using UnityEngine;

namespace Assets.Scripts.Player.Config
{
	public enum Ability
	{
		Tether,
		Ignite,
		Flash,
		Steam,
		Burn,
		Scout,
		Tools
	}

	public static class AbilityState
	{
		public static bool IsActive(Ability ability)
		{
			return PlayerPrefs.GetInt(ability.ToString()) > 0;
		}

		public static void SetActive(Ability ability, bool isActive)
		{
			PlayerPrefs.SetInt(ability.ToString(), isActive ? 1 : 0);
		}
	}
}
