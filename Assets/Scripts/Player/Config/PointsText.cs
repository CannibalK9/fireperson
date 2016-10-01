using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player.Config
{
	public class PointsText : MonoBehaviour
	{
		private Text _text;

		void Awake()
		{
			_text = GetComponent<Text>();
		}

		void Update()
		{
			_text.text = string.Format("Points spent: {0} / {1}",
				PlayerPrefs.GetInt(IntVariable.SpentPoints.ToString()),
				PlayerPrefs.GetInt(IntVariable.AvailablePoints.ToString()));
		}
	}
}
