using Assets.Scripts.Interactable;
using PicoGames.VLS2D;
using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class CircleHeater : HeatHandler
	{
		private GameObject _heatLightPrefab;
		private GameObject _heatLight;
		private CircleCollider2D _col;

		void Awake()
		{
			_fireplace = GetComponentInParent<FirePlace>();
			_col = GetComponent<CircleCollider2D>();
			_heatLightPrefab = (GameObject)Resources.Load("fireplaces/CircleLight");
			EnableCollider(_fireplace == null);
		}

		protected override void SetColliderSizes(float range)
		{
			transform.localScale = Vector3.one * (HeatMessage.Range);
		}

		protected override void EnableCollider(bool enable)
		{
			_col.enabled = enable;

			if (enable)
			{
				if (_heatLight == null)
				{
					_heatLight = (GameObject)Instantiate(_heatLightPrefab, transform.position, transform.rotation);
					_heatLight.transform.parent = transform;
					_heatLight.transform.localScale = Vector3.one;
				}
				_heatLight.GetComponent<VLSRadial>().Color = GetIntensityColour();
			}
			else
				Destroy(_heatLight);
		}
	}
}
