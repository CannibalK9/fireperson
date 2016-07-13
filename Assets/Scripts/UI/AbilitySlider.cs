using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public abstract class AbilitySlider : MonoBehaviour
	{
		bool _onPath1;
		bool _onOath2;
		bool _onPath2;

		void Start()
		{
			var slider1 = GetComponent<Slider>();
			slider1.value = GUI.VerticalSlider(new Rect(25 + (Mathf.Sin(slider1.value * 20 * -.0175f) * 30), 25, 50, 30), slider1.value, 0, 9);
		}
	}
}
