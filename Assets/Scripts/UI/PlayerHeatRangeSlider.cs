using Assets.Scripts.Player;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class PlayerHeatRangeSlider : MonoBehaviour
    {
        public PlayerController Player;

        private Slider mainSlider;

        public void Start()
        {
            mainSlider = GetComponent<Slider>();
            mainSlider.value = PlayerPrefs.GetFloat(Variable.PlayerRange.ToString());
            mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(mainSlider.value); });
        }

        public void ValueChangeCheck(float val)
        {
            PlayerPrefs.SetFloat(Variable.PlayerRange.ToString(), val);
            Player._defaultHeatRayDistance = val;
        }
    }
}
