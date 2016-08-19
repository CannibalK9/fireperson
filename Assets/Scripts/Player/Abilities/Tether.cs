using Assets.Scripts.Heat;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
	public class Tether : HeatHandler
	{
		public Transform Player { get; set; }
		public Transform Pl { get; set; }

		void Start()
		{
			UpdateHeat(new HeatMessage(1, 0));
		}

		void Update()
		{
			if (Pl == null)
			{
				ChannelingHandler.IsTethered = false;
				DestroyObject(gameObject);
				return;
			}

			EnableCollider(ChannelingHandler.IsTethered);

			transform.position = Vector2.Lerp(Player.position, Pl.position, 0.5f);

			Vector3 diff = Player.position - transform.position;
			diff.Normalize();
			float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);

			var col = GetComponent<BoxCollider2D>();
			col.size = new Vector2(col.size.x, Vector2.Distance(Player.position, Pl.position) / 3);
		}
	}
}
