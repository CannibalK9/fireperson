using Assets.Scripts.Interactable;
using PicoGames.VLS2D;
using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class BoxHeater : HeatHandler
	{
		private GameObject _heatLightPrefab;
		private GameObject _heatLight;
		private BoxCollider2D _col;

		void Awake()
		{
			_fireplace = GetComponentInParent<FirePlace>();
			_heatLightPrefab = (GameObject)Resources.Load("fireplaces/CircleLight");
			_col = GetComponent<BoxCollider2D>();
			EnableCollider(_fireplace == null);
		}

		protected override void SetColliderSizes(float range)
		{
			transform.localScale = new Vector3(1 + HeatMessage.Range * 2, 1, 1);
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
					_heatLight.transform.localScale = new Vector3(1 / transform.localScale.x, 1, 1);
				}
				_heatLight.GetComponent<VLSRadial>().Color = GetIntensityColour();
			}
			else
				Destroy(_heatLight);
		}
	}
}

//todo: fade in border, show border when control  changes, show dotted border when scout is set, remove heat when scouting, further scouting border