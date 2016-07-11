using Assets.Scripts.Player;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ControlSlider : MonoBehaviour
    {
        public PlayerController Player;

        private Slider _mainSlider;

        public void Start()
        {
            _mainSlider = GetComponent<Slider>();

			_mainSlider.value = PlayerPrefs.GetFloat(Variable.Control.ToString());
            _mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(_mainSlider.value); });
        }

		public void ValueChangeCheck(float val)
		{
			PlayerPrefs.SetFloat(Variable.Control.ToString(), val);
			Player.BaseControl = val;
		}

		void OnGUI()
		{
			_mainSlider.value = GUI.HorizontalSlider(new Rect(940, 135 + (Mathf.Sin(_mainSlider.value * -.0175f) * 30), 243, 30), _mainSlider.value, 0.0f, 180.0f);
		}
    }
}
