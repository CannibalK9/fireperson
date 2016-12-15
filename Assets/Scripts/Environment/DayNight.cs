using Assets.Scripts.Misc;
using UnityEngine;

namespace Assets.Scripts.Environment
{
	[ExecuteInEditMode]
	public class DayNight : Singleton<DayNight>
	{
		protected DayNight() { }

		[Range(1f, 100f)]
		public float Day;

		void Awake()
		{
		}

		void Update()
		{
			RenderSettings.ambientIntensity = (Day / 100) * 1.2f;
		}
	}
}
