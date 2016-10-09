using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class PlayerHeater : HeatHandler
	{
		private GameObject _heatLightPrefab;
		private GameObject _heatLight;
		private BoxCollider2D _col;

		void Awake()
		{
			_heatLightPrefab = (GameObject)Resources.Load("fireplaces/PlayerLight");
			_col = GetComponent<BoxCollider2D>();
			EnableCollider(true);
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
				}
				_heatLight.transform.localScale = new Vector3(0.5f, 1.5f + HeatMessage.HeatRange * 1.2f, 1);
			}
			else
				Destroy(_heatLight);
		}
	}
}