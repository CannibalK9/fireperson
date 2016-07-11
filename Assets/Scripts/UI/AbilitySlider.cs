using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public abstract class AbilitySlider : MonoBehaviour
	{
		Dictionary<int, Vector2> SliderPoints = new Dictionary<int, Vector2>
		{
			{ 1, new Vector2(0.1f, 0.2f) }
		};

		void Start()
		{
			Slider slider = GetComponent<Slider>();

			slider.value = GUI.HorizontalSlider(new Rect(940, 135 + (Mathf.Sin(slider.value * -.0175f) * 30), 243, 30), slider.value, 0.0f, 180.0f);
		}
	}
}
