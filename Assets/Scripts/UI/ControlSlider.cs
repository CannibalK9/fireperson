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
    }
}
