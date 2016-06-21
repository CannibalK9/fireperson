using Assets.Scripts.Player;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class StabilitySlider : MonoBehaviour
    {
        public PlayerController Player;

        private Slider _mainSlider;

        public void Start()
        {
            _mainSlider = GetComponent<Slider>();
            _mainSlider.value = PlayerPrefs.GetFloat(Variable.Stability.ToString());
            _mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(_mainSlider.value); });
        }

        public void ValueChangeCheck(float val)
        {
			PlayerPrefs.SetFloat(Variable.Stability.ToString(), val);
            Player.BaseStability = val;
        }
    }
}
