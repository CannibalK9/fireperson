using Assets.Scripts.Interactable;
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
			transform.localScale = new Vector3(1 + HeatMessage.HeatRange * 2, 1, 1);
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
			}
			else
				Destroy(_heatLight);
		}
	}
}