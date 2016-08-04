using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class AbilitySlider : MonoBehaviour
	{
		bool _onPath1;
		bool _onPath2;
		bool _onPath3;
		int _value;

		List<Vector2> _intersections;
		List<Vector2> _straight;
		List<Vector2> _curved;

		LineRenderer l = new LineRenderer();
		

		List<Slider> _sliders = new List<Slider>();
		Slider _slider1;

		void Start()
		{
			//int strightPoints = 15;
			//for (int i = 1; i <= strightPoints; i++)
			//{
			//	_straight.Add(new Vector2(0, i / strightPoints));
			//}

			//int curvedPoints = 27;
			//for (int i = 0; i < curvedPoints; i++)
			//{
			//	_curved.Add(_straight[0] + new Vector2(Mathf.Sin(_slider1.value * 20 * -.0175f) * 30, i / curvedPoints));
			//}

			//_sliders = GetComponents<Slider>().ToList();
			//_slider1 = _sliders[0];
			//foreach (Slider slider in _sliders)
			//{
			//	slider.onValueChanged.AddListener(delegate { ValueChangeCheck(slider); });
			//}
		}

		void OnGUI()
		{
			//if (_slider1.isActiveAndEnabled)
			//{
			//	float x = Input.GetAxis("Mouse X");
			//	float y = Input.GetAxis("Mouse Y");

			//	var direction = new Vector2(x, y);
			//	_onPath1 = Vector2.Angle(direction, Vector2.left) > Vector2.Angle(direction, Vector2.up);
			//	_slider1.value = _onPath1
			//		? GUI.VerticalSlider(new Rect(25 + (Mathf.Sin(_slider1.value * 20 * -.0175f) * 30), 25, 50, 30), _slider1.value, 0, 9)
			//		: GUI.VerticalSlider(new Rect(25, 25, 50, 30), _slider1.value, 0, 5);
			//}
		}

		public void ValueChangeCheck(Slider slider)
		{
			_value = (int) _sliders.Sum(s => s.value);
			if (_value > PlayerPrefs.GetInt(Variable.AvailablePoints.ToString()))
			{
				slider.value -= 1;
			}
		}
	}
}
