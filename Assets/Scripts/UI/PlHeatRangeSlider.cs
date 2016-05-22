using Assets.Scripts.Player;
using Assets.Scripts.Player.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class PlHeatRangeSlider : MonoBehaviour
    {
        public PilotedLightController Pl;

        private Slider mainSlider;

        public void Start()
        {
            mainSlider = GetComponent<Slider>();
            mainSlider.value = PlayerPrefs.GetFloat(Variable.PlRange.ToString());
            mainSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(mainSlider.value); });
        }

        public void ValueChangeCheck(float val)
        {
            PlayerPrefs.SetFloat(Variable.PlRange.ToString(), val);

            if (Pl == null)
            {
                GameObject pilotedLight = GameObject.Find("PilotedLight(Clone)");
                if (pilotedLight != null)
                    Pl = pilotedLight.GetComponent<PilotedLightController>();
            }
            if (Pl != null)
                Pl._heatRayDistance = val;
        }
    }
}
