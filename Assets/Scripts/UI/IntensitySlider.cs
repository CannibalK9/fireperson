using Assets.Scripts.Player;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class IntensitySlider : MonoBehaviour
    {
        public PlayerController Player;

        private Slider _mainSlider;

        public void Start()
        {
            _mainSlider = GetComponent<Slider>();
            _mainSlider.value = PlayerPrefs.GetFloat(Variable.Intensity.ToString());
            _mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(_mainSlider.value); });
        }

        public void ValueChangeCheck(float val)
        {
			PlayerPrefs.SetFloat(Variable.Intensity.ToString(), val);
            Player.BaseIntensity = val;
        }
    }
}
