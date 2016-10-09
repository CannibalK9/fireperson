using Assets.Scripts.Heat;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
	public class Tether : HeatHandler
	{
		public Transform Player { get; set; }
		public Transform Pl { get; set; }

		private GameObject _heatLightPrefab;
		private GameObject _heatLight;
		private BoxCollider2D _col;

		void Awake()
		{
			_heatLightPrefab = (GameObject)Resources.Load("fireplaces/TetherLight");
			_col = GetComponent<BoxCollider2D>();
			HeatMessage = new HeatMessage(1, 0);
		}

		void Update()
		{
			if (Pl == null)
			{
				ChannelingHandler.IsTethered = false;
				DestroyObject(gameObject);
				return;
			}

			transform.position = Vector2.Lerp(Player.position, Pl.position, 0.5f);

			Vector3 diff = Player.position - transform.position;
			diff.Normalize();
			float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);

			_col.size = new Vector2(_col.size.x, Vector2.Distance(Player.position, Pl.position));

			_col.enabled = ChannelingHandler.IsTethered;

			if (ChannelingHandler.IsTethered)
			{
				if (_heatLight == null)
				{
					_heatLight = (GameObject)Instantiate(_heatLightPrefab, transform.position, transform.rotation);
					_heatLight.transform.parent = transform;
				}
				_heatLight.transform.localScale = new Vector3(_col.size.x, _col.size.y / 2, 1);
			}
			else
				Destroy(_heatLight);
		}

		protected override void SetColliderSizes(float range)
		{
		}

		protected override void EnableCollider(bool enable)
		{
			
		}
	}
}
