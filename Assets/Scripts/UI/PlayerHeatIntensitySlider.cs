using Assets.Scripts.Player;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class PlayerHeatIntensitySlider : MonoBehaviour
    {
        public PlayerController Player;

        private Slider mainSlider;

        public void Start()
        {
            mainSlider = GetComponent<Slider>();
            mainSlider.value = PlayerPrefs.GetFloat(Variable.PlayerIntensity.ToString());
            mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(mainSlider.value); });
        }

        public void ValueChangeCheck(float val)
        {
            PlayerPrefs.SetFloat(Variable.PlayerIntensity.ToString(), val);
            Player._heatIntensity = val;
        }
    }
}
