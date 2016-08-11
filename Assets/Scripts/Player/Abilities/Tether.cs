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
			SetColliderSizes(0);
		}

		void Update()
		{
			if (Pl == null)
			{
				ChannelingHandler.IsTethered = false;
				DestroyObject(gameObject);
				return;
			}

			var collider = _collider as CapsuleCollider;
			collider.enabled = ChannelingHandler.IsTethered;

			transform.position = Vector2.Lerp(Player.position, Pl.position, 0.5f);
			transform.LookAt(Player);
			collider.height = Vector2.Distance(Player.position, Pl.position);
		}
	}
}
